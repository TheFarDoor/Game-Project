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
    public float P_Health;
    public float P_Mana;
    [Space(10)]
    public float E_Health;
    public float E_Mana;
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
    [Header("Arena")]
    public Vector3 spawnOffset;

    public void Update(){
        if(battleOngoing && Input.GetKeyDown(KeyCode.Q)){
            StartCoroutine(EndBattle());
        }

        if(battleOngoing){
            if (playerTurn){
                MouseOverDetector();
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
        //playerTurn = UnityEngine.Random.Range(0,2) == 0;
        playerTurn = true;

        // Assign health and mana values
        P_Health = player.GetComponent<HealthSystem>().CurrentHealth;
        P_Mana = player.GetComponent<ManaSystem>().CurrentMana;

        E_Health = enemy.GetComponent<Enemy>().e_Starting_health;
        E_Health = enemy.GetComponent<Enemy>().e_Starting_mana;

        // Switch Cameras
        player.transform.Find("Main Camera").gameObject.SetActive(false); // disable player cam
        Arena.transform.Find("Cam").gameObject.SetActive(true); // enable arena cam

        // Make cursor visible and free to move
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        // Draw cards to hand for each person
        player.GetComponent<Deck>().DrawCardsToHand(StartingCardAmount);
        enemy.GetComponent<Deck>().DrawCardsToHand(StartingCardAmount);


        UpdateCardUI();

        battleOngoing = true;
    }

    private void UpdateCardUI(){
        this.transform.GetComponent<CardsManager>().DisplayCards(player.GetComponent<Deck>().UserHand);
    }

    private void CheckClick(){
        if (Input.GetMouseButtonDown(0) && !EventSystem.current.IsPointerOverGameObject()){ // If you left click and are not over ui element
            SummonMonster();
        }
        if (Input.GetMouseButtonDown(1) && !EventSystem.current.IsPointerOverGameObject()){ // If you right click and are not over a ui element then deselect card
            UpdateSelectedCard(null, null);
        }
    }

    private void SummonMonster(){ // handles Monster Summoning
        GameObject clickedObject = MouseOverDetector(); // Find out what mouse is currently hovering over
        if(clickedObject && clickedObject.tag == "CardZone_P" && selectedCard){ // if mouse over valid object and a card is selected
            if((P_Mana - selectedCard.Cost) > 0 && clickedObject.transform.childCount == 0){ // if player has enough mana for card and there isnt already a spawned monster
                Deck deckScript = player.GetComponent<Deck>();
                GameObject spawnedMosnter = GameObject.Instantiate(selectedCard.Model, clickedObject.transform.position + spawnOffset, clickedObject.transform.rotation);
                spawnedMosnter.transform.parent = clickedObject.transform;

                // Remove the card you used from the hand list and place it in the used cards list and update ui
                Card usedCard = deckScript.UserHand.Find(card => card.Id == selectedCard.Id);
                deckScript.UsedCardPile.Add(usedCard);
                deckScript.UserHand.Remove(usedCard);
                UpdateCardUI();

                // Update Mana
                P_Mana -= selectedCard.Cost; // update battleManager Tracker
                player.GetComponent<ManaSystem>().RemoveMana(selectedCard.Cost); // Update val for ui
            }
            UpdateSelectedCard(null, null); // Deselect Card if there is already a spawed monster or lack of mana
        }
    }

    private GameObject MouseOverDetector(){ // Use ray to find out what mouse is over and return gameObject if mouse over interactable
        Ray ray = Arena.transform.Find("Cam").GetComponent<Camera>().ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        LayerMask arenaLayerMask = 1 << LayerMask.NameToLayer("Arena_Interact");
        if(Physics.Raycast(ray, out hit, Mathf.Infinity, arenaLayerMask)){
            return hit.collider.gameObject;
        }
        return null;
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
