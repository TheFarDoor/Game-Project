using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class Deck : MonoBehaviour
{
    [SerializeField] private int fireTypesCount; // number of fire types in this deck
    [SerializeField] private int rockTypesCount; // number of rock types in this deck
    [SerializeField] private int spellsCount; // number of spells in this deck

    [SerializeField] private List<Card> UserDeck; // the deck itself

    [SerializeField] private CardsManager cardManager; // reference to the cardManager Script to use GenerateRandomDeck method

    IEnumerator Start(){
        yield return new WaitUntil(() => GameObject.Find("GameManager") == true); // wait until cardHolder gameObject exsists
        cardManager = GameObject.Find("GameManager").GetComponent<CardsManager>();
        UserDeck = cardManager.GenerateRandomDeck(fireTypesCount, rockTypesCount, spellsCount); // uses the GenerateRandomDeck function to generate a deck with the number of each card type given
        Shuffle();
    }

    public void Shuffle(){
        UserDeck = UserDeck.OrderBy(x => Random.value).ToList(); // Shuffle the deck randomly
    }
}
