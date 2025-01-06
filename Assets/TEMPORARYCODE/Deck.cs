using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Deck : MonoBehaviour
{

    public bool randomDeck;
    [Header("CardCount")]
    public int fireTypeCount;
    public int rockTypeCount;
    public int airTypeCount;
    public int waterTypeCount;

    [Header("Lists"), Space(5)]
    public List<Card> deckList;
    public List<Card> collectionList;

    public void Start(){
        if (randomDeck){
            RandomizeDeck();
        }        
    }

    public void RandomizeDeck(){
        deckList = CardsManager.Instance.GenerateRandomDeck(fireTypeCount, rockTypeCount, airTypeCount, waterTypeCount); // uses the GenerateRandomDeck function to generate a deck with the number of each card type given
        ShuffleDeck();
    }

    public void ShuffleDeck(){
        deckList = deckList.OrderBy(x => Random.value).ToList(); // Shuffle the deck randomly
    }
}