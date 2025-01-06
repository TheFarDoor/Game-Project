using System;
using System.Collections;
using UnityEngine;

public class Enemy : MonoBehaviour
{

    [Header("Details")]
    public string enemy_name;
    public string enemy_description;
    public float e_Starting_health;
    public float e_Starting_mana;

    [Header("FOV Settings"), Space(10)]
    [Range(1, 10)] public float viewRadius = 5; // How far the enemy can see (default is 5)
    [Range(10,160)] public float viewAngle = 40; // How wide the enemy can see (default is 40)
    public LayerMask playerMask; // LayerMask of player
    public LayerMask obstacleMask; // LayerMask for obstacles such as walls
    [Range(0.0f, 1.0f)] public float decisionDelay = 0.2f; // delay time in between checks for player (0.2f by default)

    [Header("Status"), Space(10)]
    public bool hasBeenDefeated = false; // boolean to track if player has beaten this enemy
    public bool inBattle = false;
    public bool hasSeenPlayer = false; // track if player is seen

    [Header("Arena"), Space(10)]
    public GameObject assignedArena;

    public void Start(){
        StartCoroutine(PlayerChecker());
    }

    public IEnumerator PlayerChecker(){
        while(hasBeenDefeated == false && hasSeenPlayer == false){
            yield return new WaitForSeconds(GameManager.Instance.enemySearchDelay);

            Transform playerTransform = CheckForPlayer();
            if(playerTransform != false){
                hasSeenPlayer = true;
                inBattle = true;
                StartCoroutine(BattleManager.Instance.InitializeBattle(playerTransform, this.transform));
            }
        }
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

    void OnDrawGizmos(){ // visualizes the fov of the enemy in the Scene
        Gizmos.DrawWireSphere(transform.position, viewRadius);
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, transform.position + ((Quaternion.AngleAxis((viewAngle/2), transform.up) * transform.forward).normalized * viewRadius));
        Gizmos.DrawLine(transform.position, transform.position + ((Quaternion.AngleAxis(-(viewAngle/2), transform.up) * transform.forward).normalized * viewRadius));
    }
}
