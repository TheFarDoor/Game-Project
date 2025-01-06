using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyMovement : MonoBehaviour
{

    public enum PatrolType{
        LookAround,
        MoveAtoB,
        RandomRoam
    }

    public enum State{
        Idle,
        Chasing,
        Patrolling
    }

    public State defaultState;
    [Space(40)]
    public State currentState;
    public PatrolType patrolType;

    [Header("Move Stats"), Space(20)]
    public float chaseSpeed;
    public float investigateSpeed;
    public float normalSpeed;

    [Header("Patrol Variables"), Space(20)]
    public Vector3 patrolA;
    public Vector3 patrolB;
    public Vector3 original_IdleArea;
    public Vector3 rpp_Point;
    public Vector3 patorlArea;
    public float pat_radius;

    [Header("FOV Settings"), Space(10)]
    [Range(1, 15)] public float viewRadius = 5; // How far the enemy can see (default is 5)
    [Range(10,160)] public float viewAngle = 40; // How wide the enemy can see (default is 40)
    public LayerMask playerMask; // LayerMask of player
    public LayerMask obstacleMask; // LayerMask for obstacles such as walls

    [Space(10)]
    public NavMeshAgent agent;
    public Transform playerPos;
    public LayerMask terrainMask;



    public void Start(){
        agent = this.GetComponent<NavMeshAgent>();
        viewRadius = this.GetComponent<Enemy>().viewRadius * 1.8f;
        viewAngle = this.GetComponent<Enemy>().viewAngle * 1.25f;

        patrolA = transform.position + patrolA;
        patrolB = transform.position + patrolB;

        original_IdleArea = this.transform.position;
    }

    public void Update(){
        if(agent.enabled == false){Debug.Log("Not allowed"); return;}
        if(this.GetComponent<Enemy>() && !this.GetComponent<Enemy>().inBattle){
            playerPos = CheckForPlayer();
            if(playerPos != null){
                currentState = State.Chasing;
            }
            else{
                currentState = defaultState;
            }

            switch(currentState){ // handle movement based on state
                case State.Idle:
                    break;

                case State.Chasing:
                    ChasePlayer();
                    break;

                case State.Patrolling:
                    Patrol();
                    break;
            }
        }
        else{
            if(Vector3.Distance(transform.position, original_IdleArea) >= 0.5f){
                agent.SetDestination(original_IdleArea);
                return;
            }
            agent.ResetPath();
        }
    }

    public void Patrol(){
        switch(patrolType){
            case PatrolType.LookAround:
                LookAround();
                break;

            case PatrolType.MoveAtoB:
                MoveBetweenPoints();
                break;

            case PatrolType.RandomRoam:
                RandomPatrolPoints();
                break;
        }
    }

    public void ChasePlayer(){
        agent.SetDestination(playerPos.position);
    }

    public void RandomPatrolPoints(){
        if(rpp_Point != null){
            agent.SetDestination(rpp_Point);
        }
        else{
            Vector2 randPoint = Random.insideUnitCircle * patorlArea;
            Vector3 randPos = new Vector3(randPoint.x + patorlArea.x, patorlArea.y + 100f, randPoint.y + patorlArea.z);

            Ray ray = new Ray(randPos, Vector3.down);
            if(Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, terrainMask)){
                rpp_Point = hit.point;
            }
        }
    }

    public void MoveBetweenPoints(){
        if(Vector3.Distance(this.transform.position, patrolA) <= 0.5f){
            agent.SetDestination(patrolB);
        }
        else if(Vector3.Distance(this.transform.position, patrolB) <= 0.5f){
            agent.SetDestination(patrolA);
        }

        if(!agent.hasPath){
            agent.SetDestination(patrolA);
        }
    }

    public void LookAround(){ // just stand still
        return;
    }

    private Transform CheckForPlayer(){
        Collider[] targetsInView = Physics.OverlapSphere(transform.position, viewRadius, playerMask); // cast a overlap sphere and store detected colliders in array
        
        if (targetsInView.Length != 0){ // if player detected
            Transform playerTarget = targetsInView[0].transform;
            Vector3 playerDirection = (playerTarget.position - transform.position).normalized;
            if (Vector3.Angle(transform.forward, playerDirection) < viewAngle/2){ // check if player is within view range 
                float distanceToPlayer = Vector3.Distance(transform.position, playerTarget.position);
                if(!Physics.Raycast(transform.position, playerDirection, distanceToPlayer, obstacleMask)){ // use raycast to make sure there is no obstacle in the way
                    Debug.DrawRay(transform.position, playerDirection * distanceToPlayer, Color.red, 0.15f);
                    return playerTarget;
                }
            }
        }
        return null;
    }

    void OnDrawGizmos(){
        Gizmos.color = Color.magenta;

        Gizmos.DrawWireSphere(transform.position + patrolA, 1f);
        Gizmos.DrawWireSphere(transform.position + patrolB, 1f);

        Gizmos.DrawWireSphere(transform.position + patorlArea, pat_radius);

        Gizmos.DrawWireSphere(transform.position, viewRadius);
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, transform.position + ((Quaternion.AngleAxis((viewAngle/2), transform.up) * transform.forward).normalized * viewRadius));
        Gizmos.DrawLine(transform.position, transform.position + ((Quaternion.AngleAxis(-(viewAngle/2), transform.up) * transform.forward).normalized * viewRadius));
    }
}
