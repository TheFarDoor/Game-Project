using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class Deck : MonoBehaviour
{
    [Header("Random Deck")]
    [SerializeField] private int fireTypesCount; // number of fire types in this deck
    [SerializeField] private int rockTypesCount; // number of rock types in this deck
    [SerializeField] private int spellsCount; // number of spells in this deck
    public bool randomDeck;

    [Header("Decks+Hand")]
    [SerializeField] private List<Card> userHand; // cards in the players hand
    [SerializeField] private List<Card> userDeck; // cards in the players deck
    [SerializeField] private List<Card> userCardCollection; // cards collected by user
    [SerializeField] private List<Card> usedCardPile;

    [Header("References")]
    [SerializeField] private CardsManager cardManager; // reference to the cardManager Script to use GenerateRandomDeck method
    

    public List<Card> UserDeck => userDeck;
    public List<Card> UserHand => userHand;
    public List<Card> UserCardCollection => userCardCollection;
    public List<Card> UsedCardPile => usedCardPile;

    IEnumerator Start(){
        yield return new WaitUntil(() => GameObject.Find("Game Manager") == true); // wait until cardHolder gameObject exsists
        cardManager = GameObject.Find("Game Manager").GetComponent<CardsManager>();

        if (randomDeck){
            RandomizeDeck();
        }
    }

    public void RandomizeDeck(){
        userDeck = cardManager.GenerateRandomDeck(fireTypesCount, rockTypesCount, spellsCount); // uses the GenerateRandomDeck function to generate a deck with the number of each card type given
        ShuffleDeck();
    }

    public void DrawCardsToHand(int numToDraw){ // draw specified amount of cards
        List<Card> cardsToTake = userDeck.Take(Mathf.Min(numToDraw, userDeck.Count)).ToList(); // Take amount of card you can
        userHand.AddRange(cardsToTake); // add to hand list
        userDeck.RemoveRange(0, cardsToTake.Count); // remove from deck list
    }

    public void ShuffleDeck(){
        userDeck = userDeck.OrderBy(x => Random.value).ToList(); // Shuffle the deck randomly
    }
}
