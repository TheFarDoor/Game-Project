using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class CardsManager : MonoBehaviour
{
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
        while (tempCards.Count < numOfCards){
            if ((numOfCards - tempCards.Count) >= listOfCards.Count){
                tempCards.AddRange(listOfCards.OrderBy(x => Random.value).Take(listOfCards.Count).ToList());
                Debug.Log(tempCards.Count);
            }
            else if((numOfCards - tempCards.Count) < listOfCards.Count){
                tempCards.AddRange(listOfCards.OrderBy(x => Random.value).Take(numOfCards - tempCards.Count).ToList());
                Debug.Log(tempCards.Count);
            }
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
}
