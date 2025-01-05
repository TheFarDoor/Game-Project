using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

public class DeckUICard : MonoBehaviour, IPointerClickHandler
{
    [Header("Card UI References")]
    public Card cardData;
    private bool isInventory; // True if the card is in the inventory, false if in the deck
    private DeckBuilder deckBuilder;

    public TextMeshProUGUI cardNameText;  // Card name UI reference
    public TextMeshProUGUI descriptionText; // Card description UI reference
    public TextMeshProUGUI cardDamageText; // Card damage UI reference
    public TextMeshProUGUI cardDefenceText; // Card defence UI reference
    public TextMeshProUGUI cardCostText; // Card cost UI reference

    public void SetCardData(Card card, bool inInventory, DeckBuilder builder)
    {
        cardData = card;
        isInventory = inInventory;
        deckBuilder = builder;

        cardNameText = this.transform.Find("CardPanel/MonsterName").GetComponent<TextMeshProUGUI>();
        descriptionText = this.transform.Find("CardPanel/MonsterDesc").GetComponent<TextMeshProUGUI>();
        cardDamageText = this.transform.Find("CardPanel/MonsterAtk").GetComponent<TextMeshProUGUI>();
        cardDefenceText = this.transform.Find("CardPanel/MonsterDef").GetComponent<TextMeshProUGUI>();
        cardCostText = this.transform.Find("CardPanel/MonsterCost").GetComponent<TextMeshProUGUI>();

        // Update UI with card information
        cardNameText.text = card.CardName;
        descriptionText.text = card.Description;
        cardDamageText.text = "DMG: " + card.Damage.ToString();
        cardDefenceText.text = "DEF: " + card.Health.ToString();
        cardCostText.text = "MANA: " + card.Cost.ToString();
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