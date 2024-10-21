using UnityEngine;
using UnityEngine.UI;

public class CardUI : MonoBehaviour
{
    public Image cardImage;      // The image UI component for the card
    public Text cardNameText;    // Text UI component for card name
    public Text descriptionText; // Text UI component for card description
    public Text damageText;      // Text UI component for card damage
    public Text costText;        // Text UI component for card cost

    public void SetCardData(Card card)
    {
        // Update UI components with card data
        cardNameText.text = card.GetCardName();
        descriptionText.text = card.GetCardDescription();
        damageText.text = "Damage: " + card.GetCardDamage().ToString();
        costText.text = "Cost: " + card.GetCardCost().ToString();

        // Assuming you have a sprite in the card model (modify as per your setup)
        if (card.GetCardModel() != null)
        {
            cardImage.sprite = card.GetCardModel().GetComponent<SpriteRenderer>().sprite;
        }
    }
}