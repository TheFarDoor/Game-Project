using UnityEngine;
using UnityEngine.UI;

public class CardUI : MonoBehaviour
{
    public Image cardImage;      // The image UI component for the card
    public Text cardNameText;    // Text UI component for card name
    public Text descriptionText; // Text UI component for card description
    public Text damageText;      // Text UI component for card damage
    public Text manaCostText;        // Text UI component for card cost

    public void SetCardData(Card card)
    {
        // Update UI components with card data
        cardNameText.text = card.CardName;
        descriptionText.text = card.Description;
        damageText.text = "Damage: " + card.Damage.ToString();
        manaCostText.text = "Cost: " + card.Cost.ToString();

        // Extract sprite from the prefab (if the card prefab contains a SpriteRenderer)
        GameObject cardModel = card.Model;
        if (cardModel != null)
        {
            SpriteRenderer spriteRenderer = cardModel.GetComponent<SpriteRenderer>();
            if (spriteRenderer != null)
            {
                cardImage.sprite = spriteRenderer.sprite; // Assign the sprite to the UI Image
            }
            else
            {
                Debug.LogWarning("The card model does not have a SpriteRenderer component.");
            }
        }
        else
        {
            Debug.LogWarning("The card model is null.");
        }
    }
}