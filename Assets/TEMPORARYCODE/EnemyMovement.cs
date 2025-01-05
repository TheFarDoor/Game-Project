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
}
