using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class pickUp : MonoBehaviour
{

    public Card card;

    private Deck playerDeck;

    // Start is called before the first frame update
    void Start()
    {
        playerDeck = GameObject.FindWithTag("Player").GetComponent<Deck>();
    }

    // Update is called once per frame
    /*
    void Update()
    {

        
    }
    */


    void OnTriggerEnter(Collider other)
    {
        // Check if the object colliding is the player
        if (other.gameObject.CompareTag("Player"))
        {
            // Check if the player already has this card
            if (!playerDeck.UserCardCollection.Contains(card))
            {
                // Add the card to the player's User Card Collection
                playerDeck.UserCardCollection.Add(card);
                Debug.Log("Card added to user collection: " + card.name);
            }
            else
            {
                Debug.Log("Player already has this card: " + card.name);
            }

            Destroy(gameObject);
        }
    }
}
