using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class CardsManager : MonoBehaviour
{
    [Header("BattleUI")]
    public GameObject cardUIPrefab;  // Reference to your card UI prefab
    public Transform cardParent;     // Parent object (like a Panel or Canvas) to hold the cards

    [Header("Cards List")]
    public List<Card> allFireCards;
    public List<Card> allRockCards;
    public List<Card> allSpells;


    public void Awake() {
        LoadCard();
    }

    public void Start(){
        GameObject canvas = GameObject.Find("/Canvas");
        cardParent = canvas.transform.Find("BattleUI/CardContainer").transform;
    }

    private void LoadCard() {

        try{
            // Loading all the cards from the Resources Folder
            allFireCards = Resources.LoadAll<Card>("Cards/Fire").ToList();
            allRockCards = Resources.LoadAll<Card>("Cards/Rock").ToList();
            allSpells = Resources.LoadAll<Card>("Cards/Spell").ToList();
        }
        catch(Exception e)
        {
            return;
        }

    }

    public List<Card> GetRandomCards(List<Card> listOfCards, int numOfCards, int IdTracker){
        List<Card> tempCards = new List<Card>();

        // Get Random card from list and add it
        for (int i=1; i < numOfCards + 1; i++){
            int RandomIndex = UnityEngine.Random.Range(0, listOfCards.Count);
            Card cardTemplate = listOfCards[RandomIndex];

            Card newCardInstance = Card.CreateInstance(cardTemplate);
            newCardInstance.SetId(IdTracker + i);
            newCardInstance.name = cardTemplate.CardName;
            tempCards.Add(newCardInstance);
        }

        return tempCards;
    }

    public List<Card> GenerateRandomDeck(int fType, int rType, int spells){
        List<Card> tempDeck = new List<Card>();

        // Get Specified number of specific type of card. Example: line below gets fType number of fire cards
        tempDeck.AddRange(GetRandomCards(allFireCards, fType, tempDeck.Count));
        tempDeck.AddRange(GetRandomCards(allRockCards, rType, tempDeck.Count));
        tempDeck.AddRange(GetRandomCards(allSpells, spells, tempDeck.Count));

        return tempDeck;
    }

    public void DisplayCards(List<Card> cards){
        // Clear the existing UI (if any)
        foreach (Transform child in cardParent)
        {
            Destroy(child.gameObject);
        }

        foreach (Card card in cards)
        {
            // Instantiate a new card UI
            GameObject cardUI = Instantiate(cardUIPrefab, cardParent);

            // Get the UI components 
            CardUI cardUIComponent = cardUI.GetComponent<CardUI>();
            if (cardUIComponent != null)
            {
                cardUIComponent.SetCardData(card); // Pass the card data to the UI
            }
        }
    }
}
