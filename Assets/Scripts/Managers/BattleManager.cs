using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using TMPro;
using System;
using UnityEngine.InputSystem;
using Palmmedia.ReportGenerator.Core.Common;
using System.IO.Compression;
using JetBrains.Annotations;
using Unity.VisualScripting;
using System.Collections.Specialized;

public class BattleManager : MonoBehaviour
{
    public enum Turn{
        A,
        B,
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
        EnemyTurn,
        EndingBattle, // End of the BattleEnd
    }

    // VARIABLES
    public static BattleManager Instance; // Instance of the BattleManager

    public BattleState currentBattleState; // tracks current battle state

    [Header("Player + Enemy Ref"), Space(20)] // reference to player and enemy
    public GameObject A;
    public GameObject B;

    [Header("Turn Tracking"), Space(20)]
    public Turn currentTurn; // tracks whos turn it is
    public bool firstTurn; // Tracks if its the first turn

    [Header("Arena + Position References"), Space(20)]
    public GameObject Arena; // reference to the Arena which the battle will take place on
    public Transform Arena_A_Position; // playerPosition on the arena
    public Transform Arena_B_Position; // enemyPosition on the arena

    // Stores position + rotation data of the player and enemy prior to being moved to the arena for battle
    private Vector3 previous_A_Position;
    private Quaternion previous_A_Rotation;
    private Vector3 previous_B_Position;
    private Quaternion previous_B_Rotation;

    [Header("Arena Card zones"), Space(20)]
    public string A_CardzoneTag = "A CardZone"; // player card slots
    public string B_CardzoneTag = "B CardZone"; // enemy card slots

    public Transform Arena_A_CardSlots;
    public Transform Arena_B_CardSlots;

    [Space(10)]

    [Header("Decks/Card Data"), Space(20)] // Cards List information below which includes a deck, the hand and used cards for the player and enemy
    public int startingCardAmount = 5;
    public float cardDrawDelay = 0.1f;
    [Space(8)]
    public List<Card> A_Deck;
    public List<Card> A_Hand;
    public List<Card> A_Used;
    [Space(8)]
    public List<Card> B_Deck;
    public List<Card> B_Hand;
    public List<Card> B_Used;

    [Header("HP + Mana"), Space(20)]
    // Mana and Health bar references
    public Slider healthBar;
    public TextMeshProUGUI healthText;
    public Slider manaBar;
    public TextMeshProUGUI manaText;
    [Space(8)]
    public float A_maxHp; // max player Health
    public float A_Health; // current player health
    public float A_maxMana; // max player Mana
    public float A_Mana; // current player mana
    [Space(5)]
    public float B_maxHp; // max enemy Health
    public float B_Health; // current enemy health
    public float B_maxMana; //  max enemy Mana
    public float B_Mana; // current enemy mana
    [Space(8)]
    public float sliderLerpTime = 0.05f;

    [Header("Selected Items"), Space(20)]
    public CardUI A_selectedCard;
    public GameObject A_selectedMonster;
    
    public CardUI B_selectedCard;
    public GameObject B_selectedMonster;

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
    [Space(8)]
    public float dragSnapDistance = 0.5f;
    public bool draggingCard = false;
    public float cardSnapLerpTime = 0.25f;

    [Header("Animation Durations"), Space(20)]
    [Range(0.5f,2)] public float attackLerpSpeed = 1.0f;

    // INFORMATION OF SLOTS
    public Dictionary<string, Tuple<bool, Card, float, bool>> InformationSlots = new Dictionary<string, Tuple<bool, Card, float, bool>>(); // tuple (occupied, Card, can attack)


    [Header("PlayerInput"), Space(10)]
    public PlayerInput controls;

    void OnEnable(){
        controls.actions["Left Click"].performed += ctx => HandleLeftClick();
        controls.actions["Right Click"].performed += ctx => HandleRightClick();
    }

    // METHODS
    private void Awake(){ // ensuring there is only one gameobject with the battlemanager script at any given time
        if (Instance == null){
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else{
            Destroy(gameObject);
        }

        controls = GameObject.Find("/Player").GetComponent<PlayerInput>();
    }

    public void Start(){
        mainCam = GameObject.Find("/Player/Main Camera").GetComponent<Camera>();
        SetBattleState(BattleState.NoBattleOngoing);
    }

    // Update is called once per frame
    void Update()
    {   
        // TESTING !!!!!! TEMP CODE !!!!!!!!!!!!
        // if(Input.GetKeyDown(KeyCode.F)){EndBattle();} 
        // if(Input.GetKeyDown(KeyCode.G)){RemoveMana(1.0f, true);} 
        // if(Input.GetKeyDown(KeyCode.R)){RemoveHealth(5.0f, true);}
        // if(Input.GetKeyDown(KeyCode.T)){ManaRegen(1.0f, true);}
        // if(Input.GetKeyDown(KeyCode.Y)){SwitchTurn();}
    }

    public IEnumerator AttackWithMonster(GameObject attackingMonster, GameObject enemyMonster){
        if (currentBattleState == BattleState.Attacking) yield break; // prevent coroutine running twice

        SetBattleState(BattleState.Attacking);

        Vector3 startPos = attackingMonster.transform.position; // attacking monster start position
        Vector3 startForwardVec = attackingMonster.transform.forward; // attacking monster starting forward direction

        // rotating monster before attack
        yield return StartCoroutine(RotateTowards(attackingMonster, enemyMonster.transform.position, 0.1f));
        
        // moving monster for attack and rotating monster back to original position
        yield return StartCoroutine(MonsterAttkLerp(attackingMonster, enemyMonster, startPos, false));

        // rotating monster after attack
        yield return StartCoroutine(RotateTowards(attackingMonster, startPos + (startForwardVec * 20f), 0.1f));

        attackingMonster.transform.parent.GetComponent<SlotStatus>().UpdateAttackBool(false);

        DestroyDeadMonster(attackingMonster, enemyMonster);

        // remove selections and set state to idle
        UpdateSelectedCardAndMonster(null, null);
        SetBattleState(BattleState.Idle);
    }

    public IEnumerator MonsterAttkLerp(GameObject monstAttacking, GameObject monstDefending, Vector3 attkStartPos, bool atkDir){
        
        bool movingToAttack = true; // bool to track if monster is moving to attack for moving back from attack
        bool attackComplete = false; // bool to track if attack is complete

        float t = 0f;
        while (!attackComplete){
            t += (movingToAttack? 1: -1) * Time.deltaTime * attackLerpSpeed; // increment the t value based on time and rotation speed. The direction of the movement is based on the bool.
            t = Mathf.Clamp01(t); // ensure t stays between 0 and 1

            Vector3 attkEndPos = attkStartPos + ((monstDefending.transform.position - attkStartPos).normalized * (monstDefending.transform.position - attkStartPos).magnitude * 0.9f);
            monstAttacking.transform.position = Vector3.Lerp(attkStartPos, attkEndPos, t);
            

            if (t > 0.99f && movingToAttack){
                if(!atkDir){
                    MonsterAttkCalculation(monstAttacking.transform.parent.gameObject, monstDefending.transform.parent.gameObject);
                }
                movingToAttack = false;
            }

            if (t < 0.01f && !movingToAttack){
                attackComplete = true;
            }

            yield return null;
        }

        monstAttacking.transform.position = attkStartPos;
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

    public void MonsterAttkCalculation(GameObject attkMonstP, GameObject defMonstP){
        Tuple<bool, Card, float, bool> Astats = InformationSlots[attkMonstP.name];
        Tuple<bool, Card, float, bool> Dstats = InformationSlots[defMonstP.name];

        float D_HP = Dstats.Item3 - Astats.Item2.Damage;
        float A_HP = Astats.Item3 - Dstats.Item2.Damage;

        if(D_HP <= 0){
            defMonstP.transform.GetChild(0).gameObject.SetActive(false);
            defMonstP.GetComponent<SlotStatus>().ClearTuple();
        }
        else{
            defMonstP.GetComponent<SlotStatus>().UpdateHp(D_HP);
        }

        if(A_HP <= 0){
            attkMonstP.transform.GetChild(0).gameObject.SetActive(false);
            attkMonstP.GetComponent<SlotStatus>().ClearTuple();
        }
        else{
           attkMonstP.GetComponent<SlotStatus>().UpdateHp(A_HP);
        }
    }

    public void DestroyDeadMonster(GameObject attkMonstP, GameObject defMonstP){
        Tuple<bool, Card, float, bool> monA = InformationSlots[attkMonstP.transform.parent.name];
        Tuple<bool, Card, float, bool> monB = InformationSlots[defMonstP.transform.parent.name];

        if(monA.Item3 <= 0){Destroy(attkMonstP);}
        if(monB.Item3 <= 0){Destroy(defMonstP);}
    }

    public bool PlaceOrUseCard(Card cardToUse, Transform cardSlot, bool ForPlayer){
        if (RemoveMana(cardToUse.Cost, ForPlayer)){ // check if there is enough mana and remove mana based on card cost
            SetBattleState(BattleState.Placing);
            // Spawn model or place card
            GameObject placedCard = Instantiate(cardToUse.Model, cardSlot.position + new Vector3(0, GetObjectHeight(cardToUse.Model) * 1.1f, 0), cardSlot.rotation);
            placedCard.transform.SetParent(cardSlot.transform, worldPositionStays: true); // assign placed card to slot

            // update text for monster stats on arena
            Debug.Log(cardSlot.name);
            cardSlot.GetComponent<SlotStatus>().UpdateTuple(true, cardToUse, cardToUse.Health, firstTurn? false: true);

            // loop through user
            foreach (Card card in (ForPlayer? A_Hand: B_Hand)){
                if(card == cardToUse){
                    (ForPlayer? A_Used: B_Used).Add(card);
                    (ForPlayer? A_Hand: B_Hand).Remove(card);
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
            return 1f; // Return 0 if no Renderer is found
        }
    }

    public void HandleLeftClick(){
       if(currentTurn == Turn.A){
            GameObject clicked = null;
            Ray ray = arenaCam.ScreenPointToRay(Mouse.current.position.ReadValue()); // ray aimed at where mouse if pointing
            RaycastHit hit; // store information from raycast hit

            int lm = LayerMask.GetMask("Arena_Interact");

            if(Physics.Raycast(ray, out hit, Mathf.Infinity, lm)){ // If the raycase with infinite range hits something
                clicked = hit.collider.gameObject; // return the tag and gameobject of the thing that is hit by the ray
            }

            if(clicked == null){UpdateSelectedCardAndMonster(null, null);}

            switch(currentBattleState){
                case BattleState.Idle:
                    if(clicked == null){return;}
                    if(clicked.tag == A_CardzoneTag){
                        if(clicked.transform.childCount == 0){return;}
                        if(InformationSlots[clicked.name].Item1){
                            UpdateSelectedCardAndMonster(null, clicked.transform.GetChild(0).gameObject);
                        }
                    }
                    break;

                case BattleState.CardSelected:
                    if(clicked.tag == A_CardzoneTag && clicked.transform.childCount == 0){
                        PlaceOrUseCard(A_selectedCard.cardData, clicked.transform, true);
                    }
                    break;

                case BattleState.MonsterSelected:
                    if(clicked.tag == B_CardzoneTag){
                        if(clicked.transform.childCount == 0){return;}
                        if(InformationSlots[clicked.name].Item1){
                            StartCoroutine(AttackWithMonster(A_selectedMonster, clicked.transform.GetChild(0).gameObject));
                        }
                    }
                    else if(clicked.tag == B_CardzoneTag){
                        if(clicked.transform.childCount == 0){UpdateSelectedCardAndMonster(null, null); return;}
                        if(InformationSlots[clicked.name].Item1){
                            UpdateSelectedCardAndMonster(null, clicked.transform.GetChild(0).gameObject);
                        }
                    }
                    
                    if(clicked.tag == "B Pos"){
                        if(CheckSlotsForMonsters(false)){
                            AttackDirectly(clicked.transform, false);    
                        }
                    }
                    break;
            }
        }
    }

    public void HandleRightClick(){
        UpdateSelectedCardAndMonster(null, null);
        SetBattleState(BattleState.Idle); // update current battle state as nothing is selected
    }

    public void UpdateSelectedCardAndMonster(CardUI clickedCard, GameObject clickedMonster){

        // reset old ui card if one was selected before
        if (A_selectedCard != null){
            A_selectedCard.gameObject.GetComponent<Image>().color = default_CardUIColour;
            A_selectedCard.gameObject.transform.localScale = A_selectedCard.defaultScale;
            A_selectedCard.GetComponent<CardUI>().thisCardSelected = false;
        }

        // update selected card with new clicked card or selected monster with new clicked monster
        A_selectedCard = clickedCard; // assign selected card
        A_selectedMonster = clickedMonster; // assign selectedMonster

        switch((A_selectedCard == null, A_selectedMonster == null)){// update battle state
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
        if(currentBattleState == BattleState.SwitchingTurn){return;}
        SetBattleState(BattleState.SwitchingTurn);
        if(firstTurn){firstTurn = false;}
        switch(currentTurn){
            case Turn.A:
                UpdateSelectedCardAndMonster(null, null); // deselect any select items
                Debug.Log("Switchting to enemy Turn");
                currentTurn = Turn.B;
                ManaRegen(1.0f, false);
                StartCoroutine(DrawCards(1, false));
                ResetMonsterAttackBools(false);
                StartCoroutine(EnemyBattleLogic());
                CheckWin();
                Debug.Log("Completed switch to enemy Turn");
                break;

            case Turn.B:
                Debug.Log("Switchting to player Turn");
                currentTurn = Turn.A;
                ManaRegen(1.0f, true);
                StartCoroutine(DrawCards(1, true));
                ResetMonsterAttackBools(true);
                CheckWin();
                SetBattleState(BattleState.Idle);
                Debug.Log("Completed switch to player Turn");
                break;
        }
    }

    public void CheckWin(){
        if((A_Deck.Count == 0 && A_Hand.Count == 0)|| (B_Deck.Count == 0 && B_Hand.Count == 0 && CheckSlotsForMonsters(false))){
            EndBattle();
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
        A = p.gameObject;
        B = e.gameObject;

        Arena = B.GetComponent<Enemy>().assignedArena; // get arena assigned to enemy
        arenaCam = Arena.transform.Find("Cam").GetComponent<Camera>(); // get arena camera
        Arena_A_Position = Arena.transform.Find("A Position"); // get arena position for player
        Arena_B_Position = Arena.transform.Find("B Position"); // get arena position for enemy
        Arena_A_CardSlots = Arena.transform.Find("Zones/A Zone"); // player placeable card zones
        Arena_B_CardSlots = Arena.transform.Find("Zones/B Zone"); // enemy placeable card zones

        
        previous_A_Position = A.transform.position; // save player position prior to battle
        previous_A_Rotation = A.transform.rotation; // save player rotation prior to battle
        previous_B_Position = B.transform.position; // save enemy position prior to battle
        previous_B_Rotation = B.transform.rotation; // save enemy rotation prior to battle

        A.GetComponent<PlayerInputHandler>().enabled = false;

        // Stop player movement and move the player + enemy to the arena
        A.GetComponent<CharacterController>().enabled = false;
        A.transform.SetPositionAndRotation(Arena_A_Position.position, Arena_A_Position.rotation);
        A.GetComponent<CharacterController>().enabled = true;
        B.transform.SetPositionAndRotation(Arena_B_Position.position, Arena_B_Position.rotation);

        // Initialize HP and Mana
        InitializeHealthAndMana(B.GetComponent<Enemy>().e_Starting_health, B.GetComponent<Enemy>().e_Starting_mana);

        // Switch Cameras
        mainCam.gameObject.SetActive(false); // disable player cam
        arenaCam.gameObject.SetActive(true); // enable arena cam

        // Make cursor visible and free to move
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        // Set up decks
        A_Deck = new List<Card>(A.GetComponent<Deck>().deckList); // copy playerdeck
        B_Deck = new List<Card>(B.GetComponent<Deck>().deckList); // copy enemydeck
        yield return StartCoroutine(DrawCards(startingCardAmount, false)); // draw cards for enemy at start
        yield return StartCoroutine(DrawCards(startingCardAmount, true)); // draw cards for player at start
        this.transform.GetComponent<CardsManager>().DisplayCards(A_Hand); // refresh card ui

        firstTurn = true; // set first turn to true so a attack cant happen on the first turn

        currentTurn = UnityEngine.Random.Range(0,2) == 0? Turn.A : Turn.B; // Select a random character to start by selecting a random number between 0 or 1

        switch(currentTurn){ // based on the current turn do something
            case Turn.A: // its player turn
                SetBattleState(BattleState.Idle); // set battle state to idle
                break;

            case Turn.B: // its enemy turn
                StartCoroutine(EnemyBattleLogic()); // handle enemy turn
                break;
        }
    }

    public void EndBattle(){
        if (currentBattleState == BattleState.EndingBattle) return; // prevent coroutine from running twice
        
        SetBattleState(BattleState.EndingBattle); // set battlestate to battleend

        StopAllCoroutines();

        if(B_Deck.Count == 0 || B_Health <= 0){
            B.GetComponent<Enemy>().hasBeenDefeated = true;    
        }

        // Enable player movement and move the player + enemy back to their original positions prior to battle
        Debug.DrawLine(previous_A_Position, previous_A_Position + Vector3.up * 10f, Color.magenta, Mathf.Infinity);
        A.GetComponent<CharacterController>().enabled = false;
        // Vector3 dir = (previous_B_Position - previous_A_Position).normalized;
        // float dist = Vector3.Distance(previous_A_Position, previous_B_Position);
        // Vector3 newPos = previous_A_Position + (dir * dist);
        A.transform.SetPositionAndRotation(previous_A_Position, previous_A_Rotation);
        A.GetComponent<CharacterController>().enabled = true;
        B.transform.SetPositionAndRotation(previous_B_Position, previous_B_Rotation);

        GameManager.Instance.SetState(GameManager.GameState.Exploring);

        // Switch Cameras
        mainCam.gameObject.SetActive(true); // enable player cam
        arenaCam.gameObject.SetActive(false); // disable arena cam
       
        // make cursor locked to the middle and make the cursor invisible
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        
        A.GetComponent<PlayerInputHandler>().enabled = true;

        // Reset References (these cant be set to null)
        previous_A_Position = Vector3.zero;
        previous_A_Rotation = Quaternion.identity;
        previous_B_Position = Vector3.zero;
        previous_B_Rotation = Quaternion.identity;

        // Reset References
        A = B = Arena = null;
        Arena_A_Position = Arena_B_Position = null;
    }

    public IEnumerator DrawCards(int numToDraw, bool ForPlayer){
        if (currentBattleState == BattleState.Drawing) yield break; // prevent coroutine running twice

        SetBattleState(BattleState.Drawing);

        switch(ForPlayer){
            case true:
                numToDraw = Mathf.Min(numToDraw, A_Deck.Count);
                yield return StartCoroutine(MoveCardsToAnotherList(A_Deck, A_Hand, numToDraw));  
                break;
            
            case false:
                numToDraw = Mathf.Min(numToDraw, B_Deck.Count);
                yield return StartCoroutine(MoveCardsToAnotherList(B_Deck, B_Hand, numToDraw));
                break;
        }
        CardsManager.Instance.DisplayCards(A_Hand);
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
        Transform slotsHolder = Arena.transform.GetChild(3);

        switch(forPlayer){
            case true:
                slotsHolder = slotsHolder.Find("A Zone");
                foreach(Transform slot in slotsHolder){
                    Tuple<bool, Card, float, bool> info = InformationSlots[slot.name];
                    if(info.Item1 && !info.Item4){
                        slot.GetComponent<SlotStatus>().UpdateAttackBool(true);
                    }
                }
                return;

            case false:
                slotsHolder = slotsHolder.Find("B Zone");
                foreach(Transform slot in slotsHolder){
                    Tuple<bool, Card, float, bool> info = InformationSlots[slot.name];
                    if(info.Item1 && !info.Item4){
                        slot.GetComponent<SlotStatus>().UpdateAttackBool(true);
                    }
                }
                return;
        }
    }

    public bool CheckSlotsForMonsters(bool forPlayer){ // check if slots are currently empty
        switch(forPlayer){
            case true:
                foreach(Transform slot in Arena_A_CardSlots){
                    if(InformationSlots[slot.name].Item1){
                        return false;
                    }
                }
                return true;

            case false:
                foreach(Transform slot in Arena_B_CardSlots){
                    if(InformationSlots[slot.name].Item1){
                        return false;
                    }
                }
                return true;
        }
    }

    public void AttackDirectly(Transform slotWithMonst, bool forPlayer){
        if(!forPlayer){
            StartCoroutine(MonsterAttkLerp(A_selectedMonster, Arena_B_Position.gameObject, A_selectedMonster.transform.position, true));
            RemoveHealth(InformationSlots[A_selectedMonster.transform.parent.name].Item2.Damage, false);
            A_selectedMonster.transform.parent.GetComponent<SlotStatus>().UpdateAttackBool(false);
            UpdateSelectedCardAndMonster(null, null);
        }
        else{
            StartCoroutine(MonsterAttkLerp(slotWithMonst.GetChild(0).gameObject, Arena_A_Position.gameObject, slotWithMonst.GetChild(0).position, true));
            RemoveHealth(InformationSlots[slotWithMonst.name].Item2.Damage, true);
            slotWithMonst.GetComponent<SlotStatus>().UpdateAttackBool(false);
        }
    }


    // ENEMY BATTLE code
    public IEnumerator EnemyBattleLogic(){
        if(currentBattleState == BattleState.EnemyTurn){yield break;}
        SetBattleState(BattleState.EnemyTurn);

        int steps = 0;
        while(B_Mana > 0 && steps < 2){
            for(int i=0; i<B_Hand.Count; i++){
                for(int j=0; j<Arena_B_CardSlots.childCount; j++){
                    if(!InformationSlots[Arena_B_CardSlots.GetChild(j).name].Item1){
                        try{
                        PlaceOrUseCard(B_Hand[i], Arena_B_CardSlots.GetChild(j).transform, false);
                        }
                        catch(Exception){
                            break;
                        }
                    }
                }
            }
            steps++;
        }

        if(CheckSlotsForMonsters(true)){
            for(int j=0; j<Arena_B_CardSlots.childCount; j++){
                if(InformationSlots[Arena_B_CardSlots.GetChild(j).name].Item1 && InformationSlots[Arena_B_CardSlots.GetChild(j).name].Item4){
                    AttackDirectly(Arena_B_CardSlots.GetChild(j), true);
                }
            }
        }
        else{
           for(int j=0; j<Arena_B_CardSlots.childCount; j++){
                if(InformationSlots[Arena_B_CardSlots.GetChild(j).name].Item1 && InformationSlots[Arena_B_CardSlots.GetChild(j).name].Item4){
                    for(int k=0; k<Arena_A_CardSlots.childCount; k++){
                        if(InformationSlots[Arena_A_CardSlots.GetChild(k).name].Item1){
                            AttackWithMonster(Arena_B_CardSlots.GetChild(j).gameObject, Arena_A_CardSlots.GetChild(k).gameObject);    
                        }                        
                    }
                }
            } 
        }

        yield return new WaitForSeconds(1f);
        SwitchTurn();
        Debug.Log("EndedTurn");
    }


    // METHODS FOR UPDATING HEALTH AND MANA BELOW
    public void InitializeHealthAndMana(float E_StartHp, float E_StartMana){
        A_Health = healthBar.value = healthBar.maxValue = A_maxHp;
        A_Mana = manaBar.value = manaBar.maxValue = A_maxMana;

        B_Health = B_maxHp = E_StartHp;
        B_Mana = B_maxMana = E_StartMana;

        Mathf.Clamp(A_Health, 0, A_maxHp);
        Mathf.Clamp(A_Mana, 0, A_maxMana);
        Mathf.Clamp(B_Health, 0, B_maxHp);
        Mathf.Clamp(B_Mana, 0, B_maxMana);

        UpdateManaHealthText();
    }

    public bool RemoveMana(float manaCost, bool IsThisForPlayer) // Use mana and update the mana slider. Returns true if theres enough mana otherwise returns false
    { // the bool IsThisForPlayer indicated if player mana is updated or enemy mana is updated
        switch(IsThisForPlayer){
            case true: // this is to update player mana
                float potential_PMana = A_Mana - manaCost;
                if (potential_PMana < 0){ // check if there is enough mana to use
                    Debug.Log("Player Not enough Mana");
                    return false;
                }
                StartCoroutine(ResourceSliderLerp(manaBar, potential_PMana));
                A_Mana = potential_PMana; 
                UpdateManaHealthText();
                return true;

            case false: // this is to update enemy mana
                float potential_EMana = B_Mana - manaCost;
                if (potential_EMana < 0){ // check if there is enough mana to use
                    Debug.Log("Enemy Not enough Mana");
                    return false;
                }
                B_Mana = potential_EMana; // update mana value
                return true;
        }
    }

    public void ManaRegen(float manaToRegen, bool IsThisForPlayer){
        switch(IsThisForPlayer){
            case true:
                if((A_Mana + manaToRegen) > A_maxMana){
                    StartCoroutine(ResourceSliderLerp(manaBar, A_maxMana));
                    A_Mana = A_maxMana; 
                    UpdateManaHealthText();
                    break;
                }

                StartCoroutine(ResourceSliderLerp(manaBar, A_Mana + manaToRegen));
                A_Mana = (A_Mana + manaToRegen);
                UpdateManaHealthText();
                break;

            case false:
                if((B_Mana + manaToRegen) > B_maxMana){
                    B_Mana = B_maxMana;
                    break;
                }

                B_Mana = (B_Mana + manaToRegen);
                break;
        }
    }

    public void RemoveHealth(float damageTaken, bool IsThisForPlayer){ // take damage and update the health slider
        switch(IsThisForPlayer){
            case true: // this is to update player mana
                float potential_PHealth = A_Health - damageTaken;
                if (potential_PHealth <= 0){ // check if health will go below 0
                    EndBattle(); // handle game end
                    break;
                }
                StartCoroutine(ResourceSliderLerp(healthBar, potential_PHealth));
                A_Health = potential_PHealth; 
                UpdateManaHealthText();
                break;

            case false: // this is to update enemy mana
                float potential_EHealth = B_Health - damageTaken;
                if (potential_EHealth <= 0){ // check if there is enough mana to use
                    EndBattle(); // handle game end
                    break;
                }
                B_Health -= damageTaken; // update mana value
                break;
        }
    }

    public void UpdateManaHealthText(){
        manaText.text =  A_Mana.ToString("F0") + " / " + A_maxMana.ToString("F0"); // update mana text
        healthText.text =  A_Health.ToString("F0") + " / " + A_maxHp.ToString("F0"); // update health text
    }

    public IEnumerator ResourceSliderLerp(Slider rSlider, float newValue){
        float timer = 0f;
        float startValue = rSlider.value;
        while(rSlider.value != newValue){
            timer += Time.deltaTime;
            rSlider.value = Mathf.SmoothStep(startValue, newValue, timer/sliderLerpTime);
            yield return null;
        }
    }
}

    
