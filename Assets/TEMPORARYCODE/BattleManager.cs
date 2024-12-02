using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using TMPro;
using System;
using System.Threading;
using UnityEditor;
using UnityEngine.AI;
using UnityEngine.TextCore.Text;


public class BattleManager : MonoBehaviour
{
    public enum Turn{
        Player,
        Enemy,
    }

    public enum BattleState{ // possible battle states
        NoBattleOngoing, // There is no battle happening
        Initializing, // Setting up/Initializing a battle
        Drawing, // drawing a card or cards
        Placing, // placing a card
        Attacking, // attacking with a monster
        SwitchingTurn, // switching turn
        CardSelected, // a card is selected
        MonsterSelected, // a monster is selected
        Idle, // nothing is happening/selected
        EnemyTurn, // its the enemies turn
        BattleEnd, // End of the BattleEnd
    }

    // VARIABLES
    public static BattleManager Instance; // Instance of the BattleManager

    public BattleState currentBattleState; // tracks current battle state

    [Header("Player + Enemy Ref"), Space(20)] // reference to player and enemy
    GameObject player;
    GameObject enemy;

    [Header("Turn Tracking"), Space(20)]
    public Turn currentTurn; // tracks whos turn it is
    public bool firstTurn; // Tracks if its the first turn

    [Header("Arena + Position References"), Space(20)]
    public GameObject Arena; // reference to the Arena which the battle will take place on
    public Transform Arena_playerPosition; // playerPosition on the arena
    public Transform Arena_enemyPosition; // enemyPosition on the arena

    // Stores position + rotation data of the player and enemy prior to being moved to the arena for battle
    private Vector3 previousPlayerPosition;
    private Quaternion previousPlayerRotation;
    private Vector3 previousEnemyPosition;
    private Quaternion previousEnemyRotation;

    [Header("Arena Card zones"), Space(20)]
    public string playerCardZoneTag = "CardZone_P"; // player card slots
    public string enemyCardZoneTag = "CardZone_E"; // enemy card slots

    public Transform Arena_playerCardSlots;
    public Transform Arena_enemyCardSlots;


    [Header("Decks/Card Data"), Space(20)] // Cards List information below which includes a deck, the hand and used cards for the player and enemy
    public int startingCardAmount = 5;
    public float cardDrawDelay = 0.1f;
    [Space(8)]
    public List<Card> playerDeck;
    public List<Card> playerHand;
    public List<Card> playerUsedCards;
    [Space(8)]
    public List<Card> enemyDeck;
    public List<Card> enemyHand;
    public List<Card> enemyUsedCards;

    [Header("HP + Mana"), Space(20)]
    // Mana and Health bar references
    public Slider healthBar;
    public TextMeshProUGUI healthText;
    public Slider manaBar;
    public TextMeshProUGUI manaText;
    [Space(8)]
    public float P_maxHp; // max player Health
    public float P_Health; // current player health
    public float P_maxMana; // max player Mana
    public float P_Mana; // current player mana
    [Space(5)]
    public float E_maxHp; // max enemy Health
    public float E_Health; // current enemy health
    public float E_maxMana; //  max enemy Mana
    public float E_Mana; // current enemy mana

    [Header("Selected Items"), Space(20)]
    public CardUI selectedCard;
    public GameObject selectedMonster;

    [Header("Cameras"), Space(20)]
    public Camera mainCam; // main player camera
    public Camera arenaCam; // arena camera
    [Range(1, 5)] public float arenaCamSpeed = 2.0f; // arena camera rotate speed

    [Header("Card UI"), Space(20)]
    public Transform cardUIHolder;
    [Range(1.0f, 1.5f)] public float cardUIHoverScale; // how much bigger should the card get when hovered
    public Color default_CardUIColour; // normal Card UI colour
    public Color hovered_CardUIColour; // card colour when hovered
    public Color selected_CardUIColour; // card colour when selected

    [Header("Animation Durations"), Space(20)]
    [Range(0.5f,2)] public float attackLerpSpeed = 1.0f;



    // METHODS
    private void Awake(){ // ensuring there is only one gameobject with the battlemanager script at any given time
        if (Instance == null){
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else{
            Destroy(gameObject);
        }
    }

    public void Start(){
        mainCam = GameObject.Find("/Player/Main Camera").GetComponent<Camera>();
        SetBattleState(BattleState.NoBattleOngoing);
    }

    // Update is called once per frame
    void Update()
    {   
        // TESTING !!!!!! TEMP CODE !!!!!!!!!!!!
        if(Input.GetKeyDown(KeyCode.F)){EndBattle();} 
        if(Input.GetKeyDown(KeyCode.G)){RemoveMana(1.0f, true);} 
        if(Input.GetKeyDown(KeyCode.R)){RemoveHealth(1.0f, true);}
        if(Input.GetKeyDown(KeyCode.T)){ManaRegen(1.0f, true);}
        if(Input.GetKeyDown(KeyCode.Y)){SwitchTurn();}

        // Nessasary code below
        if (currentBattleState != BattleState.Initializing){
           HandleBattleLogic();
        }
    }

    public void HandleBattleLogic(){
        switch(currentBattleState){
            case BattleState.Idle:
                GameObject I_clickedObject = CheckMouseClick();

                if(I_clickedObject == null){ // If there was no click or an invalid click then do nothing
                    return;
                }

                if(I_clickedObject.transform.parent.tag == playerCardZoneTag){
                    I_clickedObject = I_clickedObject.transform.parent.gameObject;
                    if(I_clickedObject.transform.childCount == 3){
                        MonsterStatus monstStatus = I_clickedObject.GetComponent<MonsterStatus>();
                        if(monstStatus.canAttack == true && monstStatus.attack > 0){
                            UpdateSelectedCardAndMonster(null, I_clickedObject.transform.GetChild(2).gameObject);
                        }
                    }
                }
                break;

            case BattleState.CardSelected:
                GameObject C_clickedObject = CheckMouseClick();
                
                if(C_clickedObject == null){ // If there was no click or an invalid click then do nothing
                    return;
                }

                if(C_clickedObject.transform.parent.tag == playerCardZoneTag){
                    if(C_clickedObject.transform.parent.childCount == 2){
                        Transform CardSlot = C_clickedObject.transform.parent;
                        PlaceOrUseCard(selectedCard.cardData, CardSlot, true);
                    }
                }   
                break;

            case BattleState.MonsterSelected:
                GameObject M_clickedObject = CheckMouseClick();
                
                if(M_clickedObject == null){ // If there was no click or an invalid click then do nothing
                    return;
                }

                if (M_clickedObject.transform.parent.tag == enemyCardZoneTag){
                    M_clickedObject = M_clickedObject.transform.parent.gameObject;
                    StartCoroutine(AttackWithMonster(selectedMonster, M_clickedObject));
                }

                break;
        }
    }

    public IEnumerator AttackWithMonster(GameObject attackingMonster, GameObject enemyMonster){
        if (currentBattleState == BattleState.Attacking) yield break; // prevent coroutine running twice

        SetBattleState(BattleState.Attacking);

        enemyMonster = enemyMonster.transform.GetChild(2).gameObject;

        Vector3 startPos = attackingMonster.transform.position; // attacking monster start position
        Vector3 startForwardVec = attackingMonster.transform.forward; // attacking monster starting forward direction

        // rotating monster before attack
        yield return StartCoroutine(RotateTowards(attackingMonster, enemyMonster.transform.position, 0.1f));
        
        // moving monster for attack and rotating monster back to original position
        yield return StartCoroutine(MonsterAttkLerp(attackingMonster, enemyMonster, startPos));

        // rotating monster after attack
        yield return StartCoroutine(RotateTowards(attackingMonster, startPos + (startForwardVec * 20f), 0.1f));

        attackingMonster.transform.parent.GetComponent<MonsterStatus>().UpdateAttackBool(false);

        DestroyDeadMonster(attackingMonster, enemyMonster);

        // remove selections and set state to idle
        UpdateSelectedCardAndMonster(null, null);
        SetBattleState(BattleState.Idle);
    }

    public IEnumerator MonsterAttkLerp(GameObject objAttacking, GameObject enemyMonster, Vector3 attkStartPos){
        
        bool movingToAttack = true; // bool to track if monster is moving to attack for moving back from attack
        bool attackComplete = false; // bool to track if attack is complete

        float t = 0f;
        while (!attackComplete){
            t += (movingToAttack? 1: -1) * Time.deltaTime * attackLerpSpeed; // increment the t value based on time and rotation speed. The direction of the movement is based on the bool.
            t = Mathf.Clamp01(t); // ensure t stays between 0 and 1

            Vector3 attkEndPos = attkStartPos + ((enemyMonster.transform.position - attkStartPos).normalized * (enemyMonster.transform.position - attkStartPos).magnitude * 0.9f);
            objAttacking.transform.position = Vector3.Lerp(attkStartPos, attkEndPos, t);
            

            if (t > 0.99f && movingToAttack){
                MonsterAttkCalculation(objAttacking, enemyMonster);
                movingToAttack = false;
            }

            if (t < 0.01f && !movingToAttack){
                attackComplete = true;
            }

            yield return null;
        }

        objAttacking.transform.position = attkStartPos;
    }

    public IEnumerator RotateTowards(GameObject objToRot, Vector3 targetVec, float rotSpeed){
        Quaternion targetRotation = Quaternion.LookRotation((targetVec - objToRot.transform.position).normalized); // Get the target rotation (look towards the target direction)
        Debug.DrawRay(objToRot.transform.position, (targetVec - objToRot.transform.position).normalized * 1000f, Color.red, 2f);

        float t = 0f; // Lerp t value, ranges from 0 to 1

        // Gradually rotate the object towards the target rotation using Lerp
        while (t < 1f || objToRot.transform.rotation != targetRotation){

            t += Time.deltaTime * rotSpeed; // Increment the t value based on time and rotation speed
            t = Mathf.Clamp01(t); // Ensure t stays between 0 and 1

            // Lerp between the current rotation and target rotation
            objToRot.transform.rotation = Quaternion.Lerp(objToRot.transform.rotation, targetRotation, t);

            if (Quaternion.Angle(objToRot.transform.rotation, targetRotation) < 0.1f){
                break;
            }

            yield return null; // Wait for the next frame
        }

        // Make sure it faces exactly the target direction after the loop
        objToRot.transform.rotation = targetRotation;
    }

    public void MonsterAttkCalculation(GameObject attkMonst, GameObject defMonst){
        MonsterStatus Astats = attkMonst.transform.parent.GetComponent<MonsterStatus>();
        MonsterStatus Dstats = defMonst.transform.parent.GetComponent<MonsterStatus>();

        int D_HP = Dstats.health - Astats.attack;
        int A_HP = Astats.health - Dstats.attack;

        if(D_HP <= 0){
            defMonst.GetComponent<MeshRenderer>().enabled = false;
            Dstats.ClearStats();
        }
        else{
            Dstats.UpdateStats(Dstats.monstName, Dstats.attack, D_HP);
        }

        if(A_HP <= 0){
            attkMonst.GetComponent<MeshRenderer>().enabled = false;
            Astats.ClearStats();
        }
        else{
            Astats.UpdateStats(Astats.monstName, Astats.attack, A_HP);
        }
    }

    public void DestroyDeadMonster(GameObject attkMonst, GameObject defMonst){
        MonsterStatus monA = attkMonst.transform.parent.GetComponent<MonsterStatus>();
        MonsterStatus monB = defMonst.transform.parent.GetComponent<MonsterStatus>();

        if(monA.health <= 0){Destroy(attkMonst);}
        if(monB.health <= 0){Destroy(defMonst);}
    }

    public bool PlaceOrUseCard(Card cardToUse, Transform cardSlot, bool ForPlayer){
        if (RemoveMana(cardToUse.Cost, ForPlayer)){ // check if there is enough mana and remove mana based on card cost
            SetBattleState(BattleState.Placing);
            // Spawn model or place card
            GameObject placedCard = Instantiate(cardToUse.Model, cardSlot.GetChild(0).position + new Vector3(0, GetObjectHeight(cardToUse.Model) * 1.1f, 0), cardSlot.rotation);
            placedCard.transform.SetParent(cardSlot.transform, worldPositionStays: true); // assign placed card to slot

            // update text for monster stats on arena
            cardSlot.GetComponent<MonsterStatus>().UpdateStats(cardToUse.CardName, cardToUse.Damage, cardToUse.Health);
            cardSlot.GetComponent<MonsterStatus>().UpdateAttackBool(firstTurn? false: true);

            // loop through user
            foreach (Card card in (ForPlayer? playerHand: enemyHand)){
                if(card == cardToUse){
                    (ForPlayer? playerUsedCards: enemyUsedCards).Add(card);
                    (ForPlayer? playerHand: enemyHand).Remove(card);
                    break;
                }
            }

            if(ForPlayer){
                foreach (Transform cardUI in cardUIHolder){
                    if(cardUI.GetComponent<CardUI>().cardData == cardToUse){
                        Destroy(cardUI.gameObject);
                        break;
                    }
                }    
            }

            if (ForPlayer){
            UpdateSelectedCardAndMonster(null, null);
            SetBattleState(BattleState.Idle);
            }

            return true;
        }

        if (ForPlayer){
            UpdateSelectedCardAndMonster(null, null);
            SetBattleState(BattleState.Idle);
        }

        return false;
    }

    private float GetObjectHeight(GameObject obj)
    {
        // Get the Renderer of the object
        Renderer renderer = obj.GetComponent<Renderer>();
        if (renderer != null)
        {
            // Get the bounds of the renderer in world space
            Bounds bounds = renderer.bounds;
            
            // Calculate the height by subtracting the min and max Y values of the bounds
            float height = bounds.size.y; // This gives you the height of the object in world space
            return height;
        }
        else
        {
            Debug.LogWarning("Object does not have a Renderer.");
            return 0f; // Return 0 if no Renderer is found
        }
    }

    public GameObject CheckMouseClick(){ // check what is clicked when mouse is clicked
        if(Input.GetMouseButtonDown(0)){ // left click
            Ray ray = arenaCam.ScreenPointToRay(Input.mousePosition); // ray aimed at where mouse if pointing
            RaycastHit hit; // store information from raycast hit

            if(Physics.Raycast(ray, out hit, Mathf.Infinity)){ // If the raycase with infinite range hits something
                return hit.collider.gameObject; // return the tag and gameobject of the thing that is hit by the ray
            }

            return null; // return null is nothing is hit  
        }
        else if(Input.GetMouseButtonDown(1)){ // if right click then deselect card and/or monster
            UpdateSelectedCardAndMonster(null, null);
            SetBattleState(BattleState.Idle); // update current battle state as nothing is selected
            return  null; // return null as the right click was to deselect card and/or monster
        }

        return null; // return null if not mouse click
    }

    public void UpdateSelectedCardAndMonster(CardUI clickedCard, GameObject clickedMonster){

        // reset old ui card if one was selected before
        if (selectedCard != null){
            selectedCard.gameObject.GetComponent<Image>().color = default_CardUIColour;
            selectedCard.gameObject.transform.localScale = selectedCard.defaultScale;
            selectedCard.GetComponent<CardUI>().thisCardSelected = false;
        }

        // update selected card with new clicked card or selected monster with new clicked monster
        selectedCard = clickedCard; // assign selected card
        selectedMonster = clickedMonster; // assign selectedMonster

        switch((selectedCard == null, selectedMonster == null)){// update battle state
            case (false, true): // card selected
                SetBattleState(BattleState.CardSelected); 
                break;
            case (true, false): // monster selected
                SetBattleState(BattleState.MonsterSelected); 
                break;
            case (true, true): // none selected
                SetBattleState(BattleState.Idle); 
                break;
        }
    }

    public void SetBattleState(BattleState newState){ // Update/Set battle State
        currentBattleState = newState;
    }

    public void SwitchTurn(){ // switches turns
        SetBattleState(BattleState.SwitchingTurn);
        if(firstTurn){firstTurn = false;}
        switch(currentTurn){
            case Turn.Player:
                Debug.Log("Switchting to enemy Turn");
                currentTurn = Turn.Enemy;
                ManaRegen(1.0f, false);
                StartCoroutine(DrawCards(1, false));
                ResetMonsterAttackBools(false);
                StartCoroutine(EnemyBattleLogic());
                break;

            case Turn.Enemy:
                Debug.Log("Switchting to player Turn");
                currentTurn = Turn.Player;
                ManaRegen(1.0f, true);
                StartCoroutine(DrawCards(1, true));
                ResetMonsterAttackBools(true);
                SetBattleState(BattleState.Idle);
                break;
        }
    }

    public IEnumerator InitializeBattle(Transform p, Transform e){
        if (currentBattleState == BattleState.Initializing) yield break; // prevent coroutine running twice
        Debug.Log("INITIALIZING BATTLE");
        GameManager.Instance.SetState(GameManager.GameState.InBattle);
        SetBattleState(BattleState.Initializing); // set battle state to initializing

        // get ui refernces
        yield return new WaitUntil(() => GameObject.Find("/Canvas-Cam/BattleUI").activeInHierarchy);
        healthBar = GameObject.Find("/Canvas-Cam/BattleUI/HealthHolder/HealthBar").GetComponent<Slider>();
        healthText = GameObject.Find("/Canvas-Cam/BattleUI/HealthHolder/Health").GetComponent<TextMeshProUGUI>();
        manaBar = GameObject.Find("/Canvas-Cam/BattleUI/ManaHolder/ManaBar").GetComponent<Slider>();
        manaText = GameObject.Find("/Canvas-Cam/BattleUI/ManaHolder/Mana").GetComponent<TextMeshProUGUI>();
        cardUIHolder = GameObject.Find("/Canvas-Cam/BattleUI/CardContainer").transform;

        // Set References
        player = p.gameObject;
        enemy = e.gameObject;

        Arena = enemy.GetComponent<Enemy>().assignedArena; // get arena assigned to enemy
        arenaCam = Arena.transform.Find("Cam").GetComponent<Camera>(); // get arena camera
        Arena_playerPosition = Arena.transform.Find("PlayerPos"); // get arena position for player
        Arena_enemyPosition = Arena.transform.Find("EnemyPos"); // get arena position for enemy
        Arena_playerCardSlots = Arena.transform.Find("Zones/PZone"); // player placeable card zones
        Arena_enemyCardSlots = Arena.transform.Find("Zones/EZone"); // enemy placeable card zones

        
        previousPlayerPosition = player.transform.position; // save player position prior to battle
        previousPlayerRotation = player.transform.rotation; // save player rotation prior to battle
        previousEnemyPosition = enemy.transform.position; // save enemy position prior to battle
        previousEnemyRotation = enemy.transform.rotation; // save enemy rotation prior to battle

        // Stop player movement and move the player + enemy to the arena
        player.GetComponent<PlayerMovement>().enabled = false; // turn off player movement script to stop movement
        player.transform.SetPositionAndRotation(Arena_playerPosition.position, Arena_playerPosition.rotation);
        enemy.transform.SetPositionAndRotation(Arena_enemyPosition.position, Arena_enemyPosition.rotation);

        // Initialize HP and Mana
        InitializeHealthAndMana(enemy.GetComponent<Enemy>().e_Starting_health, enemy.GetComponent<Enemy>().e_Starting_mana);

        // Switch Cameras
        mainCam.gameObject.SetActive(false); // disable player cam
        arenaCam.gameObject.SetActive(true); // enable arena cam

        // Make cursor visible and free to move
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        // Set up decks
        playerDeck = new List<Card>(player.GetComponent<Deck>().deckList); // copy playerdeck
        enemyDeck = new List<Card>(enemy.GetComponent<Deck>().deckList); // copy enemydeck
        yield return StartCoroutine(DrawCards(startingCardAmount, false)); // draw cards for enemy at start
        yield return StartCoroutine(DrawCards(startingCardAmount, true)); // draw cards for player at start
        this.transform.GetComponent<CardsManager>().DisplayCards(playerHand); // refresh card ui

        firstTurn = true; // set first turn to true so a attack cant happen on the first turn

        currentTurn = UnityEngine.Random.Range(0,2) == 0? Turn.Player : Turn.Enemy; // Select a random character to start by selecting a random number between 0 or 1

        switch(currentTurn){ // based on the current turn do something
            case Turn.Player: // its player turn
                SetBattleState(BattleState.Idle); // set battle state to idle
                break;

            case Turn.Enemy: // its enemy turn
                StartCoroutine(EnemyBattleLogic()); // handle enemy turn
                break;
        }
    }

    public void EndBattle(){
        if (currentBattleState == BattleState.BattleEnd) return; // prevent coroutine from running twice
        
        SetBattleState(BattleState.BattleEnd); // set battlestate to battleend

        StopAllCoroutines();

        enemy.GetComponent<Enemy>().hasBeenDefeated = true;

        // Enable player movement and move the player + enemy back to their original positions prior to battle
        Debug.DrawLine(previousPlayerPosition, previousPlayerPosition + Vector3.up * 10f, Color.magenta, Mathf.Infinity);
        player.GetComponent<CharacterController>().enabled = false;
        player.transform.SetPositionAndRotation(previousPlayerPosition, previousPlayerRotation);
        player.GetComponent<CharacterController>().enabled = true;
        enemy.transform.SetPositionAndRotation(previousEnemyPosition, previousEnemyRotation);
        player.GetComponent<PlayerMovement>().enabled = true; // enable player movement

        GameManager.Instance.SetState(GameManager.GameState.Exploring);

        // Switch Cameras
        mainCam.gameObject.SetActive(true); // enable player cam
        arenaCam.gameObject.SetActive(false); // disable arena cam
       
        // make cursor locked to the middle and make the cursor invisible
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        
        // Reset References (these cant be set to null)
        previousPlayerPosition = Vector3.zero;
        previousPlayerRotation = Quaternion.identity;
        previousEnemyPosition = Vector3.zero;
        previousEnemyRotation = Quaternion.identity;

        // Reset References
        player = enemy = Arena = null;
        Arena_playerPosition = Arena_enemyPosition = null;
    }

    public IEnumerator DrawCards(int numToDraw, bool ForPlayer){
        if (currentBattleState == BattleState.Drawing) yield break; // prevent coroutine running twice

        SetBattleState(BattleState.Drawing);

        switch(ForPlayer){
            case true:
                numToDraw = Mathf.Min(numToDraw, playerDeck.Count);
                yield return StartCoroutine(MoveCardsToAnotherList(playerDeck, playerHand, numToDraw));  
                break;
            
            case false:
                numToDraw = Mathf.Min(numToDraw, enemyDeck.Count);
                yield return StartCoroutine(MoveCardsToAnotherList(enemyDeck, enemyHand, numToDraw));
                break;
        }
        SetBattleState(BattleState.Idle);
    }

    private IEnumerator MoveCardsToAnotherList(List<Card> fromList, List<Card> toList, int numToMove){
        for (int i = 0; i < numToMove; i++)
        {
            if (fromList.Count > 0)  // Make sure there's still a card to move
            {
                Card card = fromList[0];  // Take the first card
                fromList.RemoveAt(0);     // Remove it from the original list
                toList.Add(card);         // Add it to the target list

                yield return new WaitForSeconds(cardDrawDelay); // Delay between card moves
            }
        }
    }

    public void ResetMonsterAttackBools(bool forPlayer){
        Transform slotsHolder = Arena.transform.Find("Zones");

        switch(forPlayer){
            case true:
                slotsHolder = slotsHolder.Find("PZone");
                foreach(Transform slot in slotsHolder){
                    MonsterStatus monstStat = slot.GetComponent<MonsterStatus>();
                    if(slot.childCount == 3 && monstStat.canAttack == false){
                        monstStat.canAttack = true;
                    }
                }
                return;

            case false:
                slotsHolder = slotsHolder.Find("EZone");
                foreach(Transform slot in slotsHolder){
                    MonsterStatus monstStat = slot.GetComponent<MonsterStatus>();
                    if(slot.childCount == 3 && monstStat.canAttack == false){
                        monstStat.canAttack = true;
                    }
                }
                return;
        }
    }

    public bool CheckSlotsForMonsters(bool forPlayer){ // check if slots are currently empty
        Transform slotsHolder = Arena.transform.Find("Zones");

        switch(forPlayer){
            case true:
                slotsHolder = slotsHolder.Find("PZone");
                foreach(Transform slot in slotsHolder){
                    if(slot.childCount == 3){
                        return false;
                    }
                }
                return true;

            case false:
                slotsHolder = slotsHolder.Find("EZone");
                foreach(Transform slot in slotsHolder){
                    if(slot.childCount == 3){
                        return false;
                    }
                }
                return true;
        }
    }


    // ENEMY BATTLE code
    public IEnumerator EnemyBattleLogic(){
        if (currentBattleState == BattleState.EnemyTurn) yield break; // prevent coroutine running twice

        SetBattleState(BattleState.EnemyTurn);

        Transform eZone = Arena.transform.Find("Zones/EZone");

        bool slotsFull = true;
        foreach (Transform slot in eZone){
            if(slot.childCount == 2){
                slotsFull = false;
                break;
            }
        }

        if(slotsFull) yield break; // if slots are fulls

        float timer = 0;
        int cardIndex = 0;
        while(cardIndex < enemyHand.Count){
            if(timer > 20){break;}
            Card card = enemyHand[cardIndex];
            bool placedCard = false;
            foreach (Transform slot in eZone){
                if(slot.childCount == 2){
                    if(PlaceOrUseCard(card, slot, false)){
                        placedCard = true;
                    }
                    break;
                }
            }
            
            cardIndex = (placedCard? 0: cardIndex++);
            timer ++;
        }

        yield return new WaitForSeconds(3f);
        Debug.Log("EndingTurn");
        SwitchTurn();
        Debug.Log("EndedTurn");
    }


    // METHODS FOR UPDATING HEALTH AND MANA BELOW
    public void InitializeHealthAndMana(float E_StartHp, float E_StartMana){
        P_Health = healthBar.value = healthBar.maxValue = P_maxHp;
        P_Mana = manaBar.value = manaBar.maxValue = P_maxMana;

        E_Health = E_maxHp = E_StartHp;
        E_Mana = E_maxMana = E_StartMana;

        Mathf.Clamp(P_Health, 0, P_maxHp);
        Mathf.Clamp(P_Mana, 0, P_maxMana);
        Mathf.Clamp(E_Health, 0, E_maxHp);
        Mathf.Clamp(E_Mana, 0, E_maxMana);

        UpdateManaHealthText();
    }

    public bool RemoveMana(float manaCost, bool IsThisForPlayer) // Use mana and update the mana slider. Returns true if theres enough mana otherwise returns false
    { // the bool IsThisForPlayer indicated if player mana is updated or enemy mana is updated
        switch(IsThisForPlayer){
            case true: // this is to update player mana
                float potential_PMana = P_Mana - manaCost;
                if (potential_PMana < 0){ // check if there is enough mana to use
                    Debug.Log("Player Not enough Mana");
                    return false;
                }

                manaBar.value = P_Mana = potential_PMana; 
                UpdateManaHealthText();
                return true;

            case false: // this is to update enemy mana
                float potential_EMana = E_Mana - manaCost;
                if (potential_EMana < 0){ // check if there is enough mana to use
                    Debug.Log("Enemy Not enough Mana");
                    return false;
                }
                E_Mana = potential_EMana; // update mana value
                return true;
        }
    }

    public void ManaRegen(float manaToRegen, bool IsThisForPlayer){
        switch(IsThisForPlayer){
            case true:
                if((P_Mana + manaToRegen) > P_maxMana){
                    manaBar.value = P_Mana = P_maxMana; 
                    UpdateManaHealthText();
                    break;
                }

                manaBar.value = P_Mana = (P_Mana + manaToRegen);
                UpdateManaHealthText();
                break;

            case false:
                if((E_Mana + manaToRegen) > E_maxMana){
                    E_Mana = E_maxMana;
                    break;
                }

                E_Mana = (E_Mana + manaToRegen);
                break;
        }
    }

    public void RemoveHealth(float damageTaken, bool IsThisForPlayer){ // take damage and update the health slider
        switch(IsThisForPlayer){
            case true: // this is to update player mana
                float potential_PHealth = P_Health - damageTaken;
                if (potential_PHealth < 0){ // check if health will go below 0
                    EndBattle(); // handle game end
                    break;
                }

                healthBar.value = P_Health = potential_PHealth; 
                UpdateManaHealthText();
                break;

            case false: // this is to update enemy mana
                float potential_EHealth = E_Health - damageTaken;
                if (potential_EHealth < 0){ // check if there is enough mana to use
                    EndBattle(); // handle game end
                    break;
                }
                E_Health -= damageTaken; // update mana value
                break;
        }
    }

    public void UpdateManaHealthText(){
        manaText.text = "Mana: " + P_Mana.ToString("F0") + " / " + P_maxMana.ToString("F0"); // update mana text
        healthText.text = "Health: " + P_Health.ToString("F0") + " / " + P_maxHp.ToString("F0"); // update health text
    }
}

    
