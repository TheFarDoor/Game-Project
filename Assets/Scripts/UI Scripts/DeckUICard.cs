using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

public class DeckUICard : MonoBehaviour, IPointerClickHandler
{
    [Header("Card UI References")]
    private Card cardData;
    private bool isInventory; // True if the card is in the inventory, false if in the deck
    private DeckBuilder deckBuilder;

    public TextMeshProUGUI cardNameText;  // Card name UI reference
    public TextMeshProUGUI descriptionText; // Card description UI reference

    public void SetCardData(Card card, bool inInventory, DeckBuilder builder)
    {
        cardData = card;
        isInventory = inInventory;
        deckBuilder = builder;

        // Update UI with card information
        cardNameText.text = card.CardName;
        descriptionText.text = card.Description;
    }

    // Handle card click event
    public void OnPointerClick(PointerEventData eventData)
    {
        if (isInventory)
        {
            // Add card to deck if clicked in inventory
            deckBuilder.AddCardToDeck(cardData);
        }
        else
        {
            // Remove card from deck if clicked in deck
            deckBuilder.RemoveCardFromDeck(cardData);
        }
    }
}