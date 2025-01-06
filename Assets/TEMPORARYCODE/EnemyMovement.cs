using System.Collections;
using System.Collections.Generic;
using UnityEngine;
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

    public State currentState;
    public PatrolType pt;

    [Header("Move Stats"), Space(20)]
    public float chaseSpeed;
    public float investigateSpeed;
    public float normalSpeed;

    [Header("Patrol Variables"), Space(20)]
    public Vector3 patrolA;
    public Vector3 partrolB;
    public Vector3 patorlArea;

    [Header("FOV Settings"), Space(10)]
    [Range(1, 10)] public float viewRadius = 5; // How far the enemy can see (default is 5)
    [Range(10,160)] public float viewAngle = 40; // How wide the enemy can see (default is 40)
    public LayerMask playerMask; // LayerMask of player
    public LayerMask obstacleMask; // LayerMask for obstacles such as walls


    public void Start(){

    }

    public void Update(){
        switch(currentState){
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

    public void Patrol(){
        switch(pt){
            case PatrolType.LookAround:
                LookAround();
                break;

            case PatrolType.MoveAtoB:
                break;

            case PatrolType.RandomRoam:
                break;
        }
    }

    public void ChasePlayer(){

    }

    public void LookAround(){

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
}
