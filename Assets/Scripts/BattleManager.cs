using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class BattleManager : MonoBehaviour
{
    private Transform playerBattlePos;
    private Transform enemyBattlePos;

    // variables to track postions and rotation prior to the battle
    private Vector3 previousPlayerPos;
    private Quaternion previousPlayerRot;
    private Vector3 previousEnemyPos;
    private Quaternion previousEnemyRot;

    // references to player and enemy gameobjects
    private GameObject player;
    private GameObject enemy;

    private bool playerTurn; // bool to  track if its the players turn or not
    public bool battle; // bool to track if battle is happening

    public GameObject Arena;

    [Header("Battle Parameters")]
    public int P_Health;
    public int P_Mana;
    [Space(10)]
    public int E_Health;
    public int E_Mana;

    public void StartBattle(Transform p, Transform e){ // p is player transform and e is enemy transform thats passed in

        // get player and enemy gameObject reference
        player = p.gameObject;
        enemy = e.gameObject;

        // get arena postions
        Arena = enemy.GetComponent<Enemy>().assginedArena;
        playerBattlePos = Arena.transform.Find("PlayerPos");
        enemyBattlePos = Arena.transform.Find("EnemyPos");

        // save their position+rotation data prior to battle
        previousPlayerPos = player.transform.position;
        previousPlayerRot = player.transform.rotation;

        previousEnemyPos = enemy.transform.position;
        previousEnemyRot = enemy.transform.rotation;

        enemy.GetComponent<Enemy>().inBattle = true;
        player.GetComponent<PlayerMovement>().enabled = false; // turn off player movement script to stop movement

        // Move players to battle area
        player.transform.SetPositionAndRotation(playerBattlePos.position, playerBattlePos.rotation);
        enemy.transform.SetPositionAndRotation(enemyBattlePos.position, enemyBattlePos.rotation);

        // Assign a random person to start first
        playerTurn = UnityEngine.Random.Range(0,2) == 0? true : false;

        // Switch Cameras
        player.transform.Find("Main Camera").gameObject.SetActive(false); // disable player cam
        Arena.transform.Find("Cam").gameObject.SetActive(true); // enable arena cam

        battle = true;

        TurnHandler();
        StartCoroutine(EndBattle());
    }

    private void TurnHandler(){
        if (playerTurn){
            player.GetComponent<PlayerBattleLogic>().Start_P_Turn();
        }
        else{
            enemy.GetComponent<Enemy>();
        }
    }

    public IEnumerator EndBattle(){
        yield return new WaitForSeconds(10);

        enemy.GetComponent<Enemy>().isDefeated = true;

        enemy.transform.SetPositionAndRotation(previousEnemyPos, previousEnemyRot);
        player.transform.SetPositionAndRotation(previousPlayerPos, previousPlayerRot);
        
        yield return new WaitForSeconds(0.3f);
        enemy.GetComponent<Enemy>().inBattle = false;
        player.GetComponent<PlayerMovement>().enabled = true; // turn on player movement script to allow movement

        // Switch Cameras
        player.transform.Find("Main Camera").gameObject.SetActive(true); // enable player cam
        Arena.transform.Find("Cam").gameObject.SetActive(false); // disable arena cam

        player = enemy = null; // reset references to gameobjects

        battle = false;
    }

    void switchTurn() {
        playerTurn = !playerTurn;
        TurnHandler();
    }

    public bool GameEndChecker(){ // return true if one person has lost all their hp
        if (P_Health <= 0 || E_Health <= 0){
            return true;
        }
        return false;
    }
}
