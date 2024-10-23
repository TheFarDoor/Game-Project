using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Unity.VisualScripting;
using TMPro;
using UnityEditor;
using UnityEngine.Networking;

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

    [Header("Battle Status")]
    public bool firstTurn; 
    public bool playerTurn; // bool to  track if its the players turn or not
    public bool battleOngoing; // bool to track if battle is happening
    public GameObject endTurnBtn;

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

    [Header("Battle handling")]
    public Card hoveredCard = null;
    public Card selectedCard;
    public UnityEngine.UI.Image selectedCardUI;
    [Space(10)]
    public GameObject selectedMonster;
    public bool monstersFighting;

    [Header("TextReferences")]
    public TextMeshProUGUI turnT;
    public TextMeshProUGUI SMT;
    public TextMeshProUGUI SCT;
    public TextMeshProUGUI TT;
    public TextMeshProUGUI eHP;
    private float starteHP;


    [Header("Arena")]
    public Vector3 spawnOffset;

    public void Start(){ // Get references
        endTurnBtn = GameObject.Find("/Canvas/BattleUI/End Turn");
        turnT = GameObject.Find("/Canvas/BattleUI/TutorialTextHolder/TurnText").GetComponent<TextMeshProUGUI>();
        SMT = GameObject.Find("/Canvas/BattleUI/TutorialTextHolder/SelectedM").GetComponent<TextMeshProUGUI>();
        SCT = GameObject.Find("/Canvas/BattleUI/TutorialTextHolder/SelectedC").GetComponent<TextMeshProUGUI>();
        TT = GameObject.Find("/Canvas/BattleUI/TutorialTextHolder/ToolTip").GetComponent<TextMeshProUGUI>();
        eHP = GameObject.Find("/Canvas/BattleUI/TutorialTextHolder/EHP").GetComponent<TextMeshProUGUI>();
    }

    public void Update(){
        if(battleOngoing && Input.GetKeyDown(KeyCode.Q)){
            StartCoroutine(EndBattle());
        }

        if (endTurnBtn){
            endTurnBtn.SetActive(playerTurn);
        }
        
        if(battleOngoing){
            if (playerTurn && !monstersFighting){
                MouseOverDetector();
                CheckClick();
            }
            else{
                enemy.GetComponent<Enemy>().Start_E_Turn();
            } 
        }

        if(battleOngoing){
            GameEndChecker();  
        }

        HandleBattleText();
    }

    public void HandleBattleText(){
        turnT.text = "Turn: " + (playerTurn? "player": "enemy");
        SMT.text = "Selected Monster: " + (selectedMonster? selectedMonster.name: "None");
        SCT.text = "Selected Card: "  + (selectedCard? selectedCard.name: "None");
        if(firstTurn){
            TT.text = "Tip: You cannot attack on your first turn after summoning!";   
        }
        eHP.text = starteHP != 0? "Enemy Hp: " + E_Health + "/" + starteHP: "Loading Enemy Hp";
    }
    // Initialize start of battle
    public IEnumerator StartBattle(Transform p, Transform e){ // p is player transform and e is enemy transform thats passed in

        endTurnBtn.GetComponent<Button>().onClick.AddListener(SwitchTurn); // Assign switching turn to end turn button onclick

        // get player and enemy gameObject reference
        player = p.gameObject;
        enemy = e.gameObject;

        if(player.GetComponent<Deck>().UserDeck.Count == 0){
            player.GetComponent<Deck>().UserDeck.AddRange(GameObject.Find("Game Manager").GetComponent<CardsManager>().GenerateRandomDeck(3, 2, 0));
        }

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

        // Assign health and mana values
        P_Health = player.GetComponent<HealthSystem>().CurrentHealth;
        P_Mana = player.GetComponent<ManaSystem>().CurrentMana;

        E_Health = enemy.GetComponent<Enemy>().e_Starting_health;
        E_Mana = enemy.GetComponent<Enemy>().e_Starting_mana;
        starteHP = E_Health;

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
        yield return new WaitUntil(() => enemy.GetComponent<Deck>().UserHand.Count > 0 && player.GetComponent<Deck>().UserHand.Count > 0);
        firstTurn = true;
        battleOngoing = true;

    }

    private void UpdateCardUI(){
        this.transform.GetComponent<CardsManager>().DisplayCards(player.GetComponent<Deck>().UserHand);
    }

    private void CheckClick(){
        if (Input.GetMouseButtonDown(0) && !EventSystem.current.IsPointerOverGameObject()){ // If you left click and are not over ui element
            if(selectedCard && !selectedMonster){
                PlayerSummonSelectedMonster();    
            }
            else{
                if(!firstTurn){
                    UpdateSelectedCard(null, null); // deselect any cardUI
                    MonsterSelectorAttack();  
                }
            }
        }
        if (Input.GetMouseButtonDown(1) && !EventSystem.current.IsPointerOverGameObject()){ // If you right click and are not over a ui element then deselect card
            UpdateSelectedCard(null, null);
        }
    }

    public void MonsterFight(GameObject monster1, GameObject monster2){ // calculates the battles results between monsters and returns a tuple that states if monsters died
        monstersFighting = true;
        SummonedMonsterStats m1_Stats = monster1.transform.parent.GetComponent<SummonedMonsterStats>();
        SummonedMonsterStats m2_Stats = monster2.transform.parent.GetComponent<SummonedMonsterStats>();

        m2_Stats.health -= m1_Stats.atk;
        m1_Stats.health -= m2_Stats.atk;
        if (m1_Stats.health <= 0){
            MonsterDead(monster1);
        }
        if (m2_Stats.health <= 0){
            MonsterDead(monster2);
        }
        monstersFighting = false;
    }

    public void MonsterDead(GameObject deadMonster){
        deadMonster.transform.parent.GetComponent<SummonedMonsterStats>().Reset();
        Destroy(deadMonster);
    }

    private void MonsterSelectorAttack(){
        GameObject clickedObject = MouseOverDetector(); // Find out what mouse is currently hovering over and get gameObject
        if(clickedObject){ // if a interactible section of the arena is clicked
            if((clickedObject.tag == "CardZone_P" && clickedObject.transform.childCount !=0 ) || clickedObject.tag == "SummonedMonster"){ // if the player clicks on a friendly summoned monster or the zone on which a friendly monster is summoned
                selectedMonster = clickedObject.tag == "CardZone_P"? clickedObject.transform.GetChild(0).gameObject: clickedObject; // set player selected monster to the one clicked
            }
            else if(clickedObject.tag == "CardZone_E" && clickedObject.transform.childCount !=0 && selectedMonster){
                GameObject monsterToAttack = clickedObject.tag == "CardZone_E"? clickedObject.transform.GetChild(0).gameObject: clickedObject; // set enemy monster to attack that player clicks
                MonsterFight(selectedMonster, monsterToAttack);
            }
            else if(clickedObject.tag == "Arena_EZone" && selectedMonster){ // if you click on the enemy with a selected monster
                Transform enemyMonsterZone = Arena.transform.Find("Zones/EZone");
                for (int i=0; i < enemyMonsterZone.childCount; i++){ // loop through all the monster zones
                    if(enemyMonsterZone.GetChild(i).childCount != 0){ // if a zone contains a monster return as you cant directly attack the enemy
                        return;
                    }
                }
                E_Health -= selectedMonster.transform.parent.GetComponent<SummonedMonsterStats>().atk;
                GameEndChecker();
            }
            else{
                TT.text = "Tip: Click on a friendly monster and then click on an enemy monster to attack!";
                selectedMonster = null;
            }
        }
        else{
            selectedMonster = null;
        }
    }

    private void PlayerSummonSelectedMonster(){ // handles Monster Summoning for player
        GameObject clickedObject = MouseOverDetector(); // Find out what mouse is currently hovering over and get gameObeject
        if(clickedObject && clickedObject.tag == "CardZone_P" && selectedCard){ // if mouse over valid object and a card is selected
            if((P_Mana - selectedCard.Cost) > 0 && clickedObject.transform.childCount == 0){ // if player has enough mana for card and there isnt already a spawned monster
                Deck deckScript = player.GetComponent<Deck>();
                StartCoroutine(SummonMonster(player, selectedCard, clickedObject.transform));
            }
            TT.text = "Tip: You dont have enough mana or there is already a monster!";
            UpdateSelectedCard(null, null); // Deselect Card if there is already a spawed monster or lack of mana
        }
        TT.text = "Tip: Select a card to summon!";
    }

    public IEnumerator SummonMonster(GameObject user, Card cardToSpawn, Transform slotToSpawn){ // handles generic monster spawning
        Deck deckScript = user.GetComponent<Deck>(); // Get user seck script

        GameObject spawnedMosnter = GameObject.Instantiate(cardToSpawn.Model, slotToSpawn.transform.position + spawnOffset, slotToSpawn.rotation); // summon card model from card data
        spawnedMosnter.transform.parent = slotToSpawn; // make model's parent the slot its spawned at
        spawnedMosnter.tag = "SummonedMonster"; // assign monster tag
        slotToSpawn.GetComponent<SummonedMonsterStats>().SetStats(cardToSpawn.Damage, cardToSpawn.Health); // Set stats

        // Remove the card you used from the hand list and place it in the used cards list and update ui
        Card usedCard = deckScript.UserHand.Find(card => card.Id == cardToSpawn.Id);
        if(usedCard != null){
            deckScript.UsedCardPile.Add(usedCard);
            deckScript.UserHand.Remove(usedCard);
        }
        yield return new WaitForSeconds(0.2f);
        UpdateCardUI();

        // Update Mana
        if(user.tag == "Player"){
            P_Mana -= cardToSpawn.Cost; // update battleManager player mana tracker
            player.GetComponent<ManaSystem>().RemoveMana(cardToSpawn.Cost); // Update val for ui    
        }
        else{
            E_Mana -= cardToSpawn.Cost; // update battleManager player mana tracker
        }

        TT.text = "Tip: Click on a friendly monster and then click on an enemy monster to attack!";
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

    void ClearBoard(){
        Transform Zones = Arena.transform.Find("Zones");
        foreach(Transform zone in Zones){
            foreach(Transform slot in zone){
                if(slot.childCount != 0){
                    Destroy(slot.GetChild(0).gameObject);
                }
            }
        }
    }

    // Handle the end of the battle
    public IEnumerator EndBattle(){

        battleOngoing = false;

        enemy.GetComponent<Enemy>().isDefeated = true; // Make enemy defeated to true so you dont fight it start after you are sent back

        // move enemy and player back to original positions
        enemy.transform.SetPositionAndRotation(previousEnemyPos, previousEnemyRot);
        player.transform.SetPositionAndRotation(previousPlayerPos, previousPlayerRot);
        
        yield return new WaitForSeconds(0.3f); // delay
        enemy.GetComponent<Enemy>().inBattle = false; // make enemy aware its not in battle
        player.GetComponent<PlayerMovement>().enabled = true; // turn on player movement script to allow movement

        // Clear the Board
        ClearBoard();

        // Switch Cameras
        player.transform.Find("Main Camera").gameObject.SetActive(true); // enable player cam
        Arena.transform.Find("Cam").gameObject.SetActive(false); // disable arena cam

        player = enemy = null; // reset references to gameobjects
        
        // Make cursor invisble and locked to middle
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    public bool SlotOccupied(Transform zone){
        return zone.childCount > 0;
    }

    public void SwitchTurn() {
        if(playerTurn == true){ // if it is player turn then next turn is enemy so add mana to enemy
            E_Mana += 1;
            player.GetComponent<Deck>().DrawCardsToHand(1);
        }
        else{ // if not add mana to player
            P_Mana += 1;
            player.GetComponent<ManaSystem>().RecoverMana(1);
            enemy.GetComponent<Deck>().DrawCardsToHand(1);
        }
        playerTurn = !playerTurn; // switch turn
        if(firstTurn){firstTurn = false;} // if its the first turn and we switch then its no longer the first turn
    }

    public void GameEndChecker(){ // return true if one person has lost all their hp
        if (P_Health <= 0 || E_Health <= 0){
            StartCoroutine(EndBattle());
        }
    }
}
