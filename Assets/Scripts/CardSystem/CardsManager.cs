using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class CardsManager : MonoBehaviour
{
    public GameObject cardUIPrefab;  // Reference to your card UI prefab
    public Transform cardParent;     // Parent object (like a Panel or Canvas) to hold the cards

    public List<Card> allFireCards;
    public List<Card> allRockCards;

    public List<Card> allSpells;

    public void Awake() {
        LoadCard();
    }

    private void LoadCard() {
        // Loading all the cards from the Resources Folder
        allFireCards = Resources.LoadAll<Card>("Cards/Fire").ToList();
        allRockCards = Resources.LoadAll<Card>("Cards/Rock").ToList();
        allSpells = Resources.LoadAll<Card>("Cards/Spell").ToList();
    }

    public List<Card> GetRandomCards(List<Card> listOfCards, int numOfCards){
        List<Card> tempCards = new List<Card>();
        if (numOfCards > listOfCards.Count){
            Debug.Log("More cards requested than available");
        }

        // Get Random card from list and add it
        for (int i=0; i < numOfCards; i++){
            int RandomIndex = Random.Range(0, listOfCards.Count);
            tempCards.Add(listOfCards[RandomIndex]);
        }

        return tempCards;
    }

    public List<Card> GenerateRandomDeck(int fType, int rType, int spells){
        List<Card> tempDeck = new List<Card>();

        // Get Specified number of specific type of card. Example: line below gets fType number of fire cards
        tempDeck.AddRange(GetRandomCards(allFireCards, fType));
        tempDeck.AddRange(GetRandomCards(allRockCards, rType));
        tempDeck.AddRange(GetRandomCards(allSpells, spells));

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

            // Get the UI components (you need to define a script to handle this, explained below)
            CardUI cardUIComponent = cardUI.GetComponent<CardUI>();
            if (cardUIComponent != null)
            {
                cardUIComponent.SetCardData(card); // Pass the card data to the UI
            }
        }
    }
}
