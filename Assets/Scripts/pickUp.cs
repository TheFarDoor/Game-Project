using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class pickUp : MonoBehaviour
{
    public Card card;

    private Deck playerDeck;

    public float rotationSpeed = 50f;
    public float heightRotation = 0.25f;
    public float heightRotationSpeed = 1.5f;

    private Vector3 startPosition;

    // Start is called before the first frame update
    void Start()
    {
        playerDeck = GameObject.FindWithTag("Player").GetComponent<Deck>();
        startPosition = transform.position; // initial position of the model/card
    }

    // Update is called once per frame
    void Update()
    {
        transform.Rotate(0, rotationSpeed * Time.deltaTime, 0);
        
        float newHeight = startPosition.y + Mathf.Sin(Time.time * heightRotationSpeed) * heightRotation;
        transform.position = new Vector3(startPosition.x, newHeight, startPosition.z);

    }
    

    void OnTriggerEnter(Collider other)
    {
        // Check if the object colliding is the player
        if (other.gameObject.CompareTag("Player"))
        {
            // Ensure pick-up has a card assigned
            if (card == null)
            {
                Debug.LogWarning("No card assigned to this pick-up.");
                return;  // Exit if no card assigned
            }
            // Checks if player already has this card
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
