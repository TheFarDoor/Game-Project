using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class BattleManager : MonoBehaviour
{
    public Transform playerBattlePos;
    public Transform enemyBattlePos;

     private Vector3 previousPlayerPos;
    private Quaternion previousPlayerRot;
    private Vector3 previousEnemyPos;
    private Quaternion previousEnemyRot;

    private GameObject player;
    private GameObject enemy;

    [SerializeField] private bool battle;

    public void StartBattle(Transform p, Transform e){ // p is player transform and e is enemy transform thats passed in

        // get player and enemy transform reference and save their position+rotation data prior to battle
        player = p.gameObject;
        enemy = e.gameObject;

        previousPlayerPos = player.transform.position;
        previousPlayerRot = player.transform.rotation;

        previousEnemyPos = enemy.transform.position;
        previousEnemyRot = enemy.transform.rotation;

        enemy.GetComponent<Enemy>().inBattle = true;
        player.GetComponent<PlayerMovement>().enabled = false; // turn off player movement script to stop movement

        // Move players to battle area
        player.transform.SetPositionAndRotation(playerBattlePos.position, playerBattlePos.rotation);
        enemy.transform.SetPositionAndRotation(enemyBattlePos.position, enemyBattlePos.rotation);

        StartCoroutine(EndBattle());
    }

    public IEnumerator EndBattle(){
        yield return new WaitForSeconds(10);

        enemy.GetComponent<Enemy>().isDefeated = true;

        enemy.transform.SetPositionAndRotation(previousEnemyPos, previousEnemyRot);
        player.transform.SetPositionAndRotation(previousPlayerPos, previousPlayerRot);
        
        yield return new WaitForSeconds(0.3f);
        enemy.GetComponent<Enemy>().inBattle = false;
        player.GetComponent<PlayerMovement>().enabled = true; // turn on player movement script to allow movement

        player = enemy = null; // reset references to gameobjects
    }
}
