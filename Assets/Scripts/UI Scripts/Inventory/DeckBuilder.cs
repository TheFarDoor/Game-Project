using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
using UnityEngine.InputSystem;

public class DeckBuilder : MonoBehaviour
{
    [Header("UI References")]
    public GameObject Inventory;
    public GameObject DeckList;
    public GameObject cardPrefab;
    public TextMeshProUGUI deckCountText;
    public bool Vis;

    [Header("Deck Settings")]
    public Deck playerDeck;
    private int maxDeckSize = 20;

    private InputActions controls;

    void Awake()
    {
        controls = new InputActions();
        controls.Player.Inventory.performed += OnToggleDeck;
    }

    void OnEnable()
    {
        controls.Enable();
    }

    void OnDisable()
    {
        controls.Disable();
    }

    void Start()
    {
        // Find UI elements based on the hierarchy
        DeckList = this.transform.Find("InventoryUI/CardDeck/CardDeckScrollView/Viewport/DeckContent").gameObject;
        Inventory = this.transform.Find("InventoryUI/CardCollection/CardCollectionScrollView/Viewport/CollectionContent").gameObject;
        deckCountText = this.transform.Find("InventoryUI/CardDeck/DeckCount").GetComponent<TextMeshProUGUI>();

        // Get references to labels and set them
        TextMeshProUGUI deckLabel = this.transform.Find("InventoryUI/CardDeck/DeckLabel").GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI collectionLabel = this.transform.Find("InventoryUI/CardCollection/CollectionLabel").GetComponent<TextMeshProUGUI>();
        deckLabel.text = "Battle Deck";
        collectionLabel.text = "Card Collection";

        // Hide UI by default
        Transform inventoryUI = this.transform.Find("InventoryUI");
        inventoryUI.gameObject.SetActive(false);
        Vis = false;

        // Get the player's deck
        playerDeck = GameObject.FindWithTag("Player").GetComponent<Deck>();
        
        UpdateDeckCount(playerDeck.deckList.Count);
    }

    private void OnToggleDeck(InputAction.CallbackContext context)
    {
        if (Vis)
        {
            CloseInventory();
        }
        else
        {
            OpenInventory();
        }
    }

    private void OpenInventory()
    {
        Vis = true;
        // Find and activate the main InventoryUI parent
        Transform inventoryUI = this.transform.Find("InventoryUI");
        inventoryUI.gameObject.SetActive(true);

        // Ensure the content holders are active
        DeckList.SetActive(true);
        Inventory.SetActive(true);

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        
        ViewCards(playerDeck.collectionList, Inventory.transform, true);
        ViewCards(playerDeck.deckList, DeckList.transform, false);
        UpdateDeckCount(playerDeck.deckList.Count);
    }

    private void CloseInventory()
    {
        Vis = false;
        // Find and deactivate the main InventoryUI parent
        Transform inventoryUI = this.transform.Find("InventoryUI");
        inventoryUI.gameObject.SetActive(false);

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    void ViewCards(List<Card> listCard, Transform container, bool isInventory)
    {
        foreach (Transform child in container)
        {
            Destroy(child.gameObject);
        }

        foreach (Card card in listCard)
        {
            GameObject cardInstance = Instantiate(cardPrefab, container);
            DeckUICard deckUICard = cardInstance.GetComponent<DeckUICard>();
            
            if (deckUICard != null)
            {
                deckUICard.SetCardData(card, isInventory, this);
            }
        }
    }

    public void AddCardToDeck(Card card)
    {
        if (playerDeck.deckList.Count < maxDeckSize)
        {
            playerDeck.deckList.Add(card);
            playerDeck.collectionList.Remove(card);
            UpdateUI();
        }
    }

    public void RemoveCardFromDeck(Card card)
    {
        playerDeck.deckList.Remove(card);
        playerDeck.collectionList.Add(card);
        UpdateUI();
    }

    private void UpdateUI()
    {
        ViewCards(playerDeck.collectionList, Inventory.transform, true);
        ViewCards(playerDeck.deckList, DeckList.transform, false);
        UpdateDeckCount(playerDeck.deckList.Count);
    }

    private void UpdateDeckCount(int currentCount)
    {
        deckCountText.text = $"{currentCount}/{maxDeckSize}";
    }
}