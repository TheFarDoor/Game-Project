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

    [Header("Deck+Hand")]
    [SerializeField] private List<Card> userHand; // cards in the players hand
    [SerializeField] private List<Card> userDeck; // cards in the players deck

    [SerializeField] private CardsManager cardManager; // reference to the cardManager Script to use GenerateRandomDeck method

    public List<Card> UserDeck => userDeck;
    public List<Card> UserHand => userHand;

    IEnumerator Start(){
        yield return new WaitUntil(() => GameObject.Find("Game Manager") == true); // wait until cardHolder gameObject exsists
        cardManager = GameObject.Find("Game Manager").GetComponent<CardsManager>();

        Debug.Log("Bab");
        if (randomDeck){
            RandomizeDeck();
        }
    }

    public void RandomizeDeck(){
        Debug.Log("randoming");
        userDeck = cardManager.GenerateRandomDeck(fireTypesCount, rockTypesCount, spellsCount); // uses the GenerateRandomDeck function to generate a deck with the number of each card type given
        Shuffle();
        Debug.Log("Done");
    }

    public void Shuffle(){
        userDeck = userDeck.OrderBy(x => Random.value).ToList(); // Shuffle the deck randomly
    }
}
