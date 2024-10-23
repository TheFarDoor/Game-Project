using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEngine.AI;
using UnityEngine;
using UnityEngine.UIElements;

public class Enemy : MonoBehaviour
{
    [Header("Details")]
    public string enemy_name;
    public string description;
    public float e_Starting_health;
    public float e_Starting_mana;


    [Header("Status")]
    public bool isDefeated = false; // Bool to track if player has beaten this enemy
    public bool inBattle;

    [Header("FOV Settings")]
    [Range(1, 10)]
    public float viewRadius = 5; // How far the enemy can see (default is 5)
    [Range(10,160)]
    public float viewAngle = 40; // How wide the enemy can see (default is 40)
    public LayerMask playerMask; // LayerMask of player
    public LayerMask obstacleMask; // LayerMask for obstacles such as walls
    [Range(0.0f, 1.0f)]
    public float decisionDelay = 0.2f; // delay time in between checks for player (0.2f by default)

    [Header("References")]
    public NavMeshAgent agent;
    [SerializeField] private GameObject gameManager;
    public GameObject assginedArena;
    

    public void Start(){
        gameManager = GameObject.Find("Game Manager");
        StartCoroutine("HandleBehaviour", decisionDelay);
    }

    IEnumerator HandleBehaviour(float delay){
        // code below runs every x seconds where x is the delay given
        while(true){
            yield return new WaitForSeconds(delay); 
            if (!inBattle){ // if the player is not already in a battle
                if(!isDefeated && CheckForPlayer() != null){ // if there is a player in range and the player hasnt beaten this enenmy already
                    // Load up a battle
                    Transform playerTransform = CheckForPlayer();
                    StartCoroutine(gameManager.GetComponent<BattleManager>().StartBattle(playerTransform, this.transform));
                }    
            }
        }
    }

    // Check for targets in visible range. Returns transform of player if found else it returns null
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

    // CODE FOR BATTLE
    public void Start_E_Turn(){
        BattleManager battleManager = gameManager.transform.GetComponent<BattleManager>();
        Deck deck = this.GetComponent<Deck>();
        if(deck.UserHand.Count == 0){ // if enemy hand empty
            battleManager.SwitchTurn();
        }
        else{
            Transform eZone = assginedArena.transform.Find("Zones/EZone");
            foreach(Card card in deck.UserHand){
                if(battleManager.E_Mana > deck.UserHand[0].Cost){
                    foreach(Transform slot in eZone){
                        if(!battleManager.SlotOccupied(slot)){
                            StartCoroutine(battleManager.SummonMonster(this.gameObject, deck.UserHand[0], slot));
                            battleManager.SwitchTurn();
                        }
                    }  
                }   
            }
            battleManager.SwitchTurn();  
        }
    }

    void OnDrawGizmos(){
        Gizmos.DrawWireSphere(transform.position, viewRadius);
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, transform.position + ((Quaternion.AngleAxis((viewAngle/2), transform.up) * transform.forward).normalized * viewRadius));
        Gizmos.DrawLine(transform.position, transform.position + ((Quaternion.AngleAxis(-(viewAngle/2), transform.up) * transform.forward).normalized * viewRadius));
    }
}
