using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.Unity.VisualStudio.Editor;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

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

    public bool playerTurn; // bool to  track if its the players turn or not
    public bool battleOngoing; // bool to track if battle is happening

    public GameObject Arena;

    [Header("Battle Parameters")]
    public int P_Health;
    public int P_Mana;
    [Space(10)]
    public int E_Health;
    public int E_Mana;
    [Space(10)]
    public int StartingCardAmount = 5;

    [Header("Card UI Colors")]
    public Color original_CardUIColor;
    public Color hoverd_CardUIColor;
    public Color selected_CardUIColor;

    [Header("BattleUI handling")]
    public Card hoveredCard = null;
    public Card selectedCard;
    public UnityEngine.UI.Image selectedCardUI;
    [Header("PlayTurnState")]
    public bool cardSelectionPhase;
    public bool cardPlacePhase;

    public void Update(){
        if(battleOngoing && Input.GetKeyDown(KeyCode.Q)){
            StartCoroutine(EndBattle());
        }

        if(battleOngoing){
            if (playerTurn){
                CheckClick();
            }
            else{
                enemy.GetComponent<Enemy>();
            } 
        }
    }

    // Initialize start of battle
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
        playerTurn = UnityEngine.Random.Range(0,2) == 0;

        // Switch Cameras
        player.transform.Find("Main Camera").gameObject.SetActive(false); // disable player cam
        Arena.transform.Find("Cam").gameObject.SetActive(true); // enable arena cam

        // Make cursor visible and free to move
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        player.GetComponent<Deck>().DrawCardsToHand(StartingCardAmount);
        enemy.GetComponent<Deck>().DrawCardsToHand(StartingCardAmount);

        this.transform.GetComponent<CardsManager>().DisplayCards(player.GetComponent<Deck>().UserHand);

        battleOngoing = true;
    }

    private void CheckClick(){
        if (Input.GetMouseButtonDown(0)){
            if (!EventSystem.current.IsPointerOverGameObject()){
                WhatWasClicked();
            }
        }
        if (Input.GetMouseButtonDown(1)){
            Debug.Log("right clicked");
            if (!EventSystem.current.IsPointerOverGameObject()){
                Debug.Log("Deselect Card");
                UpdateSelectedCard(null, null);
            }
        }
    }

    private void WhatWasClicked(){
        Ray ray = Arena.transform.Find("Cam").GetComponent<Camera>().ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if(Physics.Raycast(ray, out hit)){
            Debug.Log("Clicked on object: " + hit.collider.gameObject.name);
        }
    }

    // Set card that is selected by player
    public void UpdateSelectedCard(Card newSelectedCard, UnityEngine.UI.Image newSelectedCardUI){
        selectedCard = newSelectedCard;
        if (selectedCardUI){ // checking to make sure its not null before assingment of colour
            selectedCardUI.color = original_CardUIColor; // reset old selected card to original colour
        }
        selectedCardUI = newSelectedCardUI;
        if (selectedCardUI){ // checking to make sure its not null before assingment of colour
            selectedCardUI.color = selected_CardUIColor; // make new selected card the selected colour
        }
    }

    // Handle the end of the battle
    public IEnumerator EndBattle(){

        enemy.GetComponent<Enemy>().isDefeated = true; // Make enemy defeated to true so you dont fight it start after you are sent back

        // move enemy and player back to original positions
        enemy.transform.SetPositionAndRotation(previousEnemyPos, previousEnemyRot);
        player.transform.SetPositionAndRotation(previousPlayerPos, previousPlayerRot);
        
        yield return new WaitForSeconds(0.3f); // delay
        enemy.GetComponent<Enemy>().inBattle = false; // make enemy aware its not in battle
        player.GetComponent<PlayerMovement>().enabled = true; // turn on player movement script to allow movement

        // Switch Cameras
        player.transform.Find("Main Camera").gameObject.SetActive(true); // enable player cam
        Arena.transform.Find("Cam").gameObject.SetActive(false); // disable arena cam

        player = enemy = null; // reset references to gameobjects
        
        // Make cursor invisble and locked to middle
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        
        battleOngoing = false;
    }

    void switchTurn() {
        playerTurn = !playerTurn;
    }

    public bool GameEndChecker(){ // return true if one person has lost all their hp
        if (P_Health <= 0 || E_Health <= 0){
            return true;
        }
        return false;
    }
}
