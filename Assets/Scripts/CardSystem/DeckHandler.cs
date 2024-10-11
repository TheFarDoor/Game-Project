using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeckHandler : MonoBehaviour
{
    [SerializeField] bool cardsSpawned = false;
    public int Decksize = 20;
    public List<Card> InspectorDeckList = new List<Card>(); 
    Dictionary<int, Card> Deck = new Dictionary<int, Card>();

    IEnumerator Start(){
        yield return new WaitUntil(() => cardsSpawned == true);
        GameObject cHolder = GameObject.Find("CardHolder");
        for (int i=0; i<Decksize; i++){
            InspectorDeckList.Add(cHolder.transform.GetChild(Random.Range(0, cHolder.transform.childCount)).GetComponent<Card>());
        }
        for (int i=0; i<InspectorDeckList.Count; i++){
            Deck[i] = InspectorDeckList[i];
        }
    }

    void Update(){
        if (GameObject.Find("CardHolder") && GameObject.Find("CardHolder").transform.childCount != 0){
            cardsSpawned = true;
        }
    }

    public void Shuffle(){
        for (int i=Deck.Count - 1; i > 0; i--){
            int j = Random.Range(0, i+1);

            Card temp = Deck[i];
            Deck[i] = Deck[j];
            Deck[j] = temp;
        }
    }

    private void ShowDictionary(){
        string visualDeck = "";
        for (int i = 0; i < Deck.Count; i++){
            visualDeck += "\n " + i + " : " + Deck[i];
            Debug.Log("\n " + i + " : " + (Deck[i] == null? "Empty" : Deck[i]));
        }
    }
}
