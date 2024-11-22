using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

public class DeckBuilder : MonoBehaviour
{
    public GameObject Inventory;
    public GameObject DeckList;
    public GameObject cardPrefab; // Prefab for the card UI
    public bool Vis;
    public Deck_Orig playerDeck;
    public TextMeshProUGUI cardText;

    void Start()
    {
        DeckList = this.transform.Find("IHolder/DeckList").gameObject;
        Inventory = this.transform.Find("IHolder/Inventory").gameObject;

        DeckList.SetActive(false);
        Inventory.SetActive(false);

        playerDeck = GameObject.Find("/Player").GetComponent<Deck_Orig>(); // Get the player's deck
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.B)) // Open inventory with the 'B' key
        {
            if (Vis)
            {
                Vis = false;
                Cursor.visible = false;
                Cursor.lockState = CursorLockMode.Locked;
                DeckList.SetActive(Vis);
                Inventory.SetActive(Vis);
            }
            else
            {
                Vis = true;
                DeckList.SetActive(Vis);
                Inventory.SetActive(Vis);
                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.None;
                ViewCards(playerDeck.UserCardCollection, Inventory.transform, true); // Show cards in inventory
                ViewCards(playerDeck.UserDeck, DeckList.transform, false); // Show cards in deck
            }
        }
    }

    // View cards in the UI (Inventory or Deck)
    void ViewCards(List<Card> listCard, Transform container, bool isInventory)
    {
        foreach (Transform slot in container.transform)
        {
            Destroy(slot.gameObject); // Clear the current UI
        }

        foreach (var card in listCard)
        {
            GameObject newCardUI = Instantiate(cardPrefab, container); // Create card UI prefab
            DeckUICard deckUICard = newCardUI.GetComponent<DeckUICard>();
            if (deckUICard != null)
            {
                deckUICard.SetCardData(card, isInventory, this); // Set card data and interaction type (inventory or deck)
            }
        }
    }

    // Add card from inventory to deck
    public void AddCardToDeck(Card card)
    {
        playerDeck.UserDeck.Add(card);
        playerDeck.UserCardCollection.Remove(card);
        UpdateUI();
    }

    // Remove card from deck and return it to inventory
    public void RemoveCardFromDeck(Card card)
    {
        playerDeck.UserDeck.Remove(card);
        playerDeck.UserCardCollection.Add(card);
        UpdateUI();
    }

    // Refresh UI when cards are added/removed
    void UpdateUI()
    {
        ViewCards(playerDeck.UserCardCollection, Inventory.transform, true);
        ViewCards(playerDeck.UserDeck, DeckList.transform, false);
    }
}