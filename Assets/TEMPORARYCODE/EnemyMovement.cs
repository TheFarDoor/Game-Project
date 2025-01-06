using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem.Switch;

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
    public PatrolType pt;

    [Header("Move Stats"), Space(20)]
    public float chaseSpeed;
    public float investigateSpeed;
    public float normalSpeed;

    [Header("Patrol Variables"), Space(20)]
    public Vector3 patrolA;
    public Vector3 patrolB;
    public Vector3 patorlArea;

    [Header("FOV Settings"), Space(10)]
    [Range(1, 15)] public float viewRadius = 5; // How far the enemy can see (default is 5)
    [Range(10,160)] public float viewAngle = 40; // How wide the enemy can see (default is 40)
    public LayerMask playerMask; // LayerMask of player
    public LayerMask obstacleMask; // LayerMask for obstacles such as walls

    [Space(10)]
    public NavMeshAgent agent;
    public Transform playerPos;



    public void Start(){
        agent = this.GetComponent<NavMeshAgent>();
        viewRadius = this.GetComponent<Enemy>().viewRadius * 1.8f;
        viewAngle = this.GetComponent<Enemy>().viewAngle * 1.25f;

        patrolA = transform.position + patrolA;
        patrolB = transform.position + patrolB;
    }

    public void Update(){
        if(!this.GetComponent<Enemy>().inBattle){
            playerPos = CheckForPlayer();
            Debug.Log(playerPos);
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
            agent.ResetPath();
        }
    }

    public void Patrol(){
        switch(pt){
            case PatrolType.LookAround:
                LookAround();
                break;

            case PatrolType.MoveAtoB:
                MoveBetweenPoints();
                break;

            case PatrolType.RandomRoam:
                break;
        }
    }

    public void ChasePlayer(){
        agent.SetDestination(playerPos.position);
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

        Gizmos.DrawWireSphere(transform.position, viewRadius);
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, transform.position + ((Quaternion.AngleAxis((viewAngle/2), transform.up) * transform.forward).normalized * viewRadius));
        Gizmos.DrawLine(transform.position, transform.position + ((Quaternion.AngleAxis(-(viewAngle/2), transform.up) * transform.forward).normalized * viewRadius));
    }
}
