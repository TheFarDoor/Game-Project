using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using JetBrains.Annotations;

public class CardUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [Header("References")]
    public Deck playerDeck;      // Reference to the player deck
    public BattleManager battleManager;   // Reference to battle manager

    [Header("Card UI References")]
    public Image cardImage;      // The image UI component for the card
    public TextMeshProUGUI cardNameText;    // Text UI component for card name
    public TextMeshProUGUI descriptionText; // Text UI component for card description
    public TextMeshProUGUI damageText;      // Text UI component for card damage
    public TextMeshProUGUI defenceText;     // Text UI component for card defence
    public TextMeshProUGUI costText;        // Text UI component for card cost


    [Header("Card Data reference")]
    private Card thisCard;

    public Card ThisCard => thisCard;

    [Header("Other")]
    public bool thisCardSelected;

    public void Awake(){
        battleManager = GameObject.Find("/Game Manager").GetComponent<BattleManager>();
    }  

    public void Update(){
        if (battleManager.selectedCard == thisCard){
            thisCardSelected = true;
        }
        else{
            thisCardSelected = false;
        }
    }

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

    public void OnPointerEnter(PointerEventData eventData){
        battleManager.hoveredCard = thisCard;
        if(!thisCardSelected){
            this.GetComponent<Image>().color = battleManager.hoverd_CardUIColor;  
        }
        
    }

    public void OnPointerExit(PointerEventData eventData){
        battleManager.hoveredCard = null;
        if (!thisCardSelected){
           this.GetComponent<Image>().color = battleManager.original_CardUIColor;  
        }        
    }

    public void OnPointerClick(PointerEventData eventData){
        if(!thisCardSelected){
            battleManager.UpdateSelectedCard(thisCard, this.GetComponent<Image>());
        }
    }

}