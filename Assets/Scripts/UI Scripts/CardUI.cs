using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using JetBrains.Annotations;

public class CardUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [Header("References")]
    public Deck_Orig playerDeck; // Reference to the player deck

    [Header("Card UI References")]
    public Image cardImage; // The image UI component for the card
    public TextMeshProUGUI cardNameText; // Text UI component for card name
    public TextMeshProUGUI descriptionText; // Text UI component for card description
    public TextMeshProUGUI damageText; // Text UI component for card damage
    public TextMeshProUGUI defenceText; // Text UI component for card defence
    public TextMeshProUGUI costText; // Text UI component for card cost


    [Header("Card Data reference")]
    public Card cardData;

    [Header("Other")]
    public bool thisCardSelected; // bool to track if the card 
    public Vector3 defaultScale = new Vector3(1, 1, 1);

    public void Update(){
        if (BattleManager.Instance.selectedCard == this){
            thisCardSelected = true;
        }
        else{
            thisCardSelected = false;
        }
    }

    public void SetCardData(Card card)
    {
        cardData = card;

        cardNameText = this.transform.Find("CardPanel/MonsterName").GetComponent<TextMeshProUGUI>();
        descriptionText = this.transform.Find("CardPanel/MonsterDesc").GetComponent<TextMeshProUGUI>();
        damageText = this.transform.Find("CardPanel/MonsterAtk").GetComponent<TextMeshProUGUI>();
        defenceText = this.transform.Find("CardPanel/MonsterDef").GetComponent<TextMeshProUGUI>();
        costText = this.transform.Find("CardPanel/MonsterCost").GetComponent<TextMeshProUGUI>();

        // Update UI components with card data
        cardNameText.text = card.CardName;
        descriptionText.text = card.Description;
        damageText.text = "DMG: " + card.Damage.ToString();
        defenceText.text = "DEF: " + card.Health.ToString();
        costText.text = "MANA: " + card.Cost.ToString();

        // Assuming you have a sprite in the card model (modify as per your setup)
        if (card.Model != null)
        {
            
        }

        // assign default colour
        this.GetComponent<Image>().color = BattleManager.Instance.default_CardUIColour;
    }

    public void OnPointerEnter(PointerEventData eventData){
        if(!thisCardSelected){
            this.GetComponent<Image>().color = BattleManager.Instance.hovered_CardUIColour;
            this.transform.localScale = defaultScale * BattleManager.Instance.cardUIHoverScale;
        }
    }

    public void OnPointerExit(PointerEventData eventData){
        if (!thisCardSelected){
           this.GetComponent<Image>().color = BattleManager.Instance.default_CardUIColour;
           this.transform.localScale = defaultScale;  
        }        
    }

    public void OnPointerClick(PointerEventData eventData){
        if (BattleManager.Instance.currentTurn == BattleManager.Turn.Player && !thisCardSelected && eventData.button == PointerEventData.InputButton.Left){
            this.GetComponent<Image>().color = BattleManager.Instance.selected_CardUIColour;
            this.transform.localScale = defaultScale * BattleManager.Instance.cardUIHoverScale;
            BattleManager.Instance.UpdateSelectedCardAndMonster(this, null); // tell battleManager that this card is selected
        }
    }
}