using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CardUI : MonoBehaviour
{
    public Deck playerDeck;      // Reference to the player deck

    public Image cardImage;      // The image UI component for the card
    public TextMeshProUGUI cardNameText;    // Text UI component for card name
    public TextMeshProUGUI descriptionText; // Text UI component for card description
    public TextMeshProUGUI damageText;      // Text UI component for card damage
    public TextMeshProUGUI defenceText;     // Text UI component for card defence
    public TextMeshProUGUI costText;        // Text UI component for card cost

    private Card thisCard;

    public Card ThisCard => thisCard;

    public void SetCardData(Card card)
    {
        thisCard = card;

        cardNameText = this.transform.Find("CardPanel/MonsterName").GetComponent<TextMeshProUGUI>();
        descriptionText = this.transform.Find("CardPanel/MonsterDesc").GetComponent<TextMeshProUGUI>();
        damageText = this.transform.Find("CardPanel/MonsterAtk").GetComponent<TextMeshProUGUI>();
        defenceText = this.transform.Find("CardPanel/MonsterDef").GetComponent<TextMeshProUGUI>();
        costText = this.transform.Find("CardPanel/MonsterCost").GetComponent<TextMeshProUGUI>();

        // Update UI components with card data
        cardNameText.text = card.CardName;
        descriptionText.text = card.Description;
        damageText.text = "DMG: " + card.Damage.ToString();
        defenceText.text = "DEF: " + card.Defence.ToString();
        costText.text = "MANA: " + card.Cost.ToString();

        // Assuming you have a sprite in the card model (modify as per your setup)
        if (card.Model != null)
        {
            cardImage.sprite = card.Model.GetComponent<SpriteRenderer>().sprite;
        }
    }
}