using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using UnityEngine.InputSystem;

public class CardUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("References")]
    public Deck playerDeck; // Reference to the player deck

    [Header("Card UI References")]
    public Image cardImage; // The image UI component for the card
    public TextMeshProUGUI cardNameText; // Text UI component for card name
    public TextMeshProUGUI descriptionText; // Text UI component for card description
    public TextMeshProUGUI damageText; // Text UI component for card damage
    public TextMeshProUGUI defenceText; // Text UI component for card defence
    public TextMeshProUGUI costText; // Text UI component for card cost

    [Header("Dragging")]
    public Vector3 originalPositionPreDrag;
    public List<Vector3> playerCardSlots = new List<Vector3>();


    [Header("Card Data reference")]
    public Card cardData;

    [Header("Other")]
    public bool thisCardSelected; // bool to track if the card 
    public Vector3 defaultScale = new Vector3(1, 1, 1);


    RaycastHit hit; // store information from raycast hit
    Vector3 snapTo = Vector3.zero;
    Transform snappedSlot = null;


    public void Update(){
        if (BattleManager.Instance.A_selectedCard == this){
            thisCardSelected = true;
        }
        else{
            thisCardSelected = false;
        }

        if (playerCardSlots.Count <= 0){
            foreach(Transform slot in BattleManager.Instance.Arena_A_CardSlots){
                playerCardSlots.Add(slot.position);
            }
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
        if(!thisCardSelected && !BattleManager.Instance.draggingCard){
            this.GetComponent<Image>().color = BattleManager.Instance.hovered_CardUIColour;
            this.transform.localScale = defaultScale * BattleManager.Instance.cardUIHoverScale;
        }
    }

    public void OnPointerExit(PointerEventData eventData){
        if (!thisCardSelected && !BattleManager.Instance.draggingCard){
           this.GetComponent<Image>().color = BattleManager.Instance.default_CardUIColour;
           this.transform.localScale = defaultScale;  
        }        
    }

    public void OnPointerClick(PointerEventData eventData){
        if (BattleManager.Instance.currentTurn == BattleManager.Turn.A && !thisCardSelected && eventData.button == PointerEventData.InputButton.Left){
            this.GetComponent<Image>().color = BattleManager.Instance.selected_CardUIColour;
            this.transform.localScale = defaultScale * BattleManager.Instance.cardUIHoverScale;
            BattleManager.Instance.UpdateSelectedCardAndMonster(this, null); // tell battleManager that this card is selected
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (BattleManager.Instance.currentTurn == BattleManager.Turn.A){
            originalPositionPreDrag = this.transform.position;
            BattleManager.Instance.UpdateSelectedCardAndMonster(this, null);
            this.GetComponent<Image>().color = BattleManager.Instance.selected_CardUIColour;
            this.transform.localScale = defaultScale * BattleManager.Instance.cardUIHoverScale;
            BattleManager.Instance.draggingCard = true;
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (BattleManager.Instance.currentTurn == BattleManager.Turn.A){
            this.transform.position = Mouse.current.position.ReadValue();

            Ray ray = BattleManager.Instance.Arena.transform.Find("Cam").GetComponent<Camera>().ScreenPointToRay(new Vector3(0,0)); // ray aimed at where mouse if pointing
            Physics.Raycast(ray, out hit); // Cast ray

            int lm_arena = 1 << LayerMask.NameToLayer("Arena_Interact"); // get layermask of specific layer
            Collider[] slotsNearby = Physics.OverlapSphere(hit.point, BattleManager.Instance.dragSnapDistance, lm_arena); // physics spear to get overlaping colliders

            List<Collider> pSlotsNearMouse = new List<Collider>();
            
            foreach(Collider col in slotsNearby){
                if(col.CompareTag(BattleManager.Instance.A_CardzoneTag) && col.transform.parent.childCount !<= 2){
                    pSlotsNearMouse.Add(col);
                }
            }

            if(pSlotsNearMouse.Count != 0){
                float closestDistance = -1.0f;
                int closestSlotIndex = 0;
                for(int i=0; i<pSlotsNearMouse.Count; i++){
                    float dist = Vector3.Distance(hit.point, pSlotsNearMouse[i].transform.position);
                    if(closestDistance < 0){closestDistance = dist; closestSlotIndex = i;}
                    if(dist < closestDistance){closestDistance = dist; closestSlotIndex = i;}
                }

                snapTo = BattleManager.Instance.Arena.transform.Find("Cam").GetComponent<Camera>().WorldToScreenPoint(slotsNearby[closestSlotIndex].transform.position);
                snappedSlot = slotsNearby[closestSlotIndex].transform;

                if(this.transform.position != snapTo){
                    this.transform.position = snapTo;
                    return;
                }
            }

            this.transform.position = Mouse.current.position.ReadValue();
            snapTo = Vector3.zero;
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {   
        if (BattleManager.Instance.currentTurn == BattleManager.Turn.A){
            if(snappedSlot != null){
                Debug.Log(snappedSlot.name);
                BattleManager.Instance.PlaceOrUseCard(BattleManager.Instance.A_selectedCard.cardData, snappedSlot.parent, true);
            }
            else{
                this.transform.position = originalPositionPreDrag;
                BattleManager.Instance.UpdateSelectedCardAndMonster(null, null);
                BattleManager.Instance.draggingCard = false;
                this.GetComponent<Image>().color = BattleManager.Instance.default_CardUIColour;
                this.transform.localScale = defaultScale;   
            }  
        }
    }

    public void OnDrawGizmos(){
        if(hit.point == null){return;}
        Gizmos.DrawWireSphere(hit.point, BattleManager.Instance.dragSnapDistance);
    }
}