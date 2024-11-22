using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using TMPro;

public class BattleManager : MonoBehaviour
{
    public enum Turn{
        Player,
        Enemy,
    }

    public enum BattleState{ // possible battle states
        Initializing, // Setting up/Initializing a battle
        Drawing, // drawing a card or cards
        Placing, // placing a card
        Attacking, // attacking with a monster
        CardSelected, // a card is selected
        MonsterSelected, // a monster is selected
        Idle, // nothing is happening/selected
        EnemyTurn, // its the enemies turn
        BattleEnd, // End of the BattleEnd
    }

    // VARIABLES
    public static BattleManager Instance; // Instance of the BattleManager

    public BattleState currentBattleState; // tracks current battle state

    [Header("Player + Enemy Ref"), Space(10)] // reference to player and enemy
    GameObject player;
    GameObject enemy;

    [Header("Turn Tracking"), Space(10)]
    public Turn currentTurn; // tracks whos turn it is
    public bool firstTurn; // Tracks if its the first turn

    [Header("Arena + Position References"), Space(10)]
    public GameObject Arena; // reference to the Arena which the battle will take place on
    public Transform Arena_playerPosition; // playerPosition on the arena
    public Transform Arena_enemyPosition; // enemyPosition on the arena
    public float cardPlacementOffset; // How much to elevate the monster or card above its spawn location

    // Stores position + rotation data of the player and enemy prior to being moved to the arena for battle
    [SerializeField] private Vector3 previousPlayerPosition;
    [SerializeField] private Quaternion previousPlayerRotation;
    [SerializeField] private Vector3 previousEnemyPosition;
    [SerializeField] private Quaternion previousEnemyRotation;

    [Header("Arena zone tag names"), Space(10)]
    public string playerCardZone = "CardZone_P";
    public string enemyCardZone = "CardZone_E";

    [Header("Decks/Card Data"), Space(10)] // Cards List information below which includes a deck, the hand and used cards for the player and enemy
    public int StartingCardAmount = 5;
    [Space(8)]
    public List<Card> playerDeck;
    public List<Card> playerHand;
    public List<Card> playerUsedCard;
    [Space(8)]
    public List<Card> enemyDeck;
    public List<Card> enemyHand;
    public List<Card> enemyUsedCard;

    [Header("HP + Mana"), Space(10)]
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

    [Header("Selected Items"), Space(10)]
    public CardUI selectedCard;
    public GameObject selectedMonster;

    [Header("Cameras"), Space(10)]
    public Camera mainCam; // main player camera
    public Camera arenaCam; // arena camera

    [Header("Card UI"), Space(10)]
    public Transform cardUIHolder;
    [Range(1.0f, 1.5f)] public float cardUIHoverScale; // how much bigger should the card get when hovered
    public Color default_CardUIColour; // normal Card UI colour
    public Color hovered_CardUIColour; // card colour when hovered
    public Color selected_CardUIColour; // card colour when selected


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
    }

    // Update is called once per frame
    void Update()
    {   
        // TESTING !!!!!! TEMP CODE !!!!!!!!!!!!
        if(Input.GetKeyDown(KeyCode.Q)){StartCoroutine(EndBattle());} 
        if(Input.GetKeyDown(KeyCode.E)){RemoveMana(1.0f, true);} 
        if(Input.GetKeyDown(KeyCode.R)){RemoveHealth(1.0f, true);}
        if(Input.GetKeyDown(KeyCode.T)){ManaRegen(1.0f, true);}

        // Nessasary code below
        if (currentBattleState != BattleState.Initializing){
           HandleInput(); 
        }
    }

    public void HandleInput(){
        switch(currentBattleState){
            case BattleState.Idle:
                CheckMouseClick();
                break;
            case BattleState.CardSelected:
                if(CheckMouseClick().Item1 == playerCardZone){
                    if(CheckMouseClick().Item2.transform.childCount == 0){
                        Transform CardSlot = CheckMouseClick().Item2.transform;
                        PlaceOrUseCard(selectedCard, CardSlot, true);
                    }
                }   
                break;
            case BattleState.MonsterSelected:
                if (CheckMouseClick().Item1 == ""){
                    
                }
                break;
            case BattleState.Drawing:
                break;
            case BattleState.Placing:
                break;
            case BattleState.Attacking:
                break;
        }
    }

    public void PlaceOrUseCard(CardUI cardToUse, Transform cardSlot, bool ForPlayer){
        SetBattleState(BattleState.Placing);

        Card cardData = cardToUse.cardData; // get cardData from cardUI

        if (RemoveMana(cardData.Cost, ForPlayer)){ // check if there is enough mana and remove mana based on card cost
            // Spawn model or place card
            GameObject placedCard = Instantiate(cardData.Model, cardSlot.position + new Vector3(0, cardPlacementOffset, 0), cardSlot.rotation);
            placedCard.transform.parent = cardSlot;

            // update text for monster stats on arena
            int slotIndex = cardSlot.GetSiblingIndex();
            TextMeshProUGUI monsterStatText = Arena.transform.Find("Canvas-Arena/PlacedCardStats/Player").GetChild(slotIndex).GetComponent<TextMeshProUGUI>();
            monsterStatText.text = MonsterStatTextTemplate(cardData.Damage, cardData.Health);

            // loop through user
            foreach (Card card in playerHand){
                if(card == cardData){
                    playerUsedCard.Add(card);
                    playerHand.Remove(card);
                    break;
                }
            }

            foreach (Transform cardUI in cardUIHolder){
                if(cardUI.gameObject == cardToUse.gameObject){
                    Destroy(cardUI.gameObject);
                    break;
                }
            }
        }
        UpdateSelectedCard(null);
        SetBattleState(BattleState.Idle);
    }

    public string MonsterStatTextTemplate(float attack, float hp){
        return "Attack:  " + attack.ToString("F0") + "\n" + "Health:  " + hp.ToString("F0");
    }

    public (string, GameObject) CheckMouseClick(){ // check what is clicked when mouse is clicked
        if(Input.GetMouseButtonDown(0)){ // left click
            Ray ray = arenaCam.ScreenPointToRay(Input.mousePosition); // ray aimed at where mouse if pointing
            RaycastHit hit; // store information from raycast hit

            if(Physics.Raycast(ray, out hit, Mathf.Infinity)){ // If the raycase with infinite range hits something
                return (hit.collider.tag , hit.collider.gameObject); // return the tag and gameobject of the thing that is hit by the ray
            }

            return (null, null); // return null is nothing is hit  
        }
        else if(Input.GetMouseButtonDown(1)){ // if right click then deselect card and/or monster
            UpdateSelectedCard(null);
            selectedMonster = null;
            SetBattleState(BattleState.Idle); // update current battle state as nothing is selected
            return (null, null); // return null as the right click was to deselect card and/or monster
        }

        return (null, null); // return null if not mouse click
    }

    public void UpdateSelectedCard(CardUI clickedCard){
        // reset old ui card if one was selected before
        if (selectedCard != null){
            selectedCard.gameObject.GetComponent<Image>().color = default_CardUIColour;
            selectedCard.gameObject.transform.localScale = selectedCard.defaultScale;
            selectedCard.GetComponent<CardUI>().thisCardSelected = false;
        }

        // update selected card with new clicked card
        selectedCard = clickedCard; // assign selected card
        selectedMonster = null; // deselected monster
        SetBattleState(BattleState.CardSelected); // update battle state 
    }

    public void UpdateSelectedMonster(GameObject clickedMonster){
        selectedCard = null; // deselect card
        selectedMonster = clickedMonster; // assign selected monster
        SetBattleState(BattleState.MonsterSelected); // update battle state 
    }

    public void SetBattleState(BattleState newState){ // Update/Set battle State
        currentBattleState = newState;
    }

    public void SwitchTurn(){ // switches turns
        switch(currentTurn){
            case Turn.Player:
                currentTurn = Turn.Enemy;
                break;
            case Turn.Enemy:
                currentTurn = Turn.Player;
                break;
        }
    }

    public IEnumerator InitializeBattle(Transform p, Transform e){
        currentBattleState = BattleState.Initializing; // set battle state to initializing
        GameManager.Instance.SetState(GameManager.GameState.InBattle);

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

        yield return new WaitForSeconds(0.1f); // temporary code !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
        playerHand.AddRange(player.GetComponent<Deck>().deckList);
        this.transform.GetComponent<CardsManager>().DisplayCards(playerHand);

        firstTurn = true; // set first turn to true so a attack cant happen on the first turn

        currentTurn = UnityEngine.Random.Range(0,2) == 0? Turn.Player : Turn.Enemy; // Select a random character to start by selecting a random number between 0 or 1

        currentBattleState = BattleState.Idle; // Set battle state to idle once initialization is completed
    }

    public IEnumerator EndBattle(){
        enemy.GetComponent<Enemy>().hasBeenDefeated = true;

        currentBattleState = BattleState.BattleEnd; // set battlestate to battleend

        // Enable player movement and move the player + enemy back to their original positions prior to battle
        player.transform.SetPositionAndRotation(previousPlayerPosition, previousPlayerRotation);
        Debug.Log("Teleporting Player");
        enemy.transform.SetPositionAndRotation(previousEnemyPosition, previousEnemyRotation);
        Debug.Log("Teleporting Enemy");
        player.GetComponent<PlayerMovement>().enabled = true; // enable player movement
        Debug.Log("Ended Battle");

        GameManager.Instance.SetState(GameManager.GameState.Exploring);

        // Switch Cameras
        mainCam.gameObject.SetActive(true); // enable player cam
        arenaCam.gameObject.SetActive(false); // disable arena cam
       
        // make cursor locked to the middle and make the cursor invisible
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        
        yield return new WaitUntil(() => Vector3.Distance(player.transform.position, previousPlayerPosition) < 0.01f && Vector3.Distance(enemy.transform.position, previousEnemyPosition) < 0.01f); // wait until player and enemy are moved

        // Reset References (these cant be set to null)
        previousPlayerPosition = Vector3.zero;
        previousPlayerRotation = Quaternion.identity;
        previousEnemyPosition = Vector3.zero;
        previousEnemyRotation = Quaternion.identity;

        // Reset References
        player = enemy = Arena = null;
        Arena_playerPosition = Arena_enemyPosition = null;
    }

    // METHODS FOR UPDATING HEALTH AND MANA BELOW
    public void InitializeHealthAndMana(float E_StartHp, float E_StartMana){
        P_Health = healthBar.maxValue = healthBar.value = P_maxHp;
        P_Mana = manaBar.maxValue = manaBar.value = P_maxMana;

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
                    Debug.Log("Not enough Mana");
                    return false;
                }

                manaBar.value = P_Mana = potential_PMana; 
                UpdateManaHealthText();
                return true;

            case false: // this is to update enemy mana
                float potential_EMana = E_Mana - manaCost;
                if (potential_EMana < 0){ // check if there is enough mana to use
                    Debug.Log("Not enough Mana");
                    return false;
                }
                E_Mana -= manaCost; // update mana value
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
                    manaBar.value = E_Mana = E_maxMana;
                    UpdateManaHealthText();
                    break;
                }

                manaBar.value = E_Mana = (E_Mana + manaToRegen);
                UpdateManaHealthText();
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
