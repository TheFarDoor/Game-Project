using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class pickUp : MonoBehaviour
{
    public Card card;
    private Deck playerDeck;

    public float rotationSpeed = 50f;
    public float heightRotation = 0.25f;
    public float heightRotationSpeed = 1.5f;

    private Vector3 startPosition;
    private bool isPlayerNearby = false;

    public GameObject interactTextPrefab; // Assign a prefab for the "Click E" text in the Inspector
    private GameObject interactTextInstance;

    private InputActions inputActions;

    // Start is called before the first frame update
    void Start()
    {
        playerDeck = GameObject.FindWithTag("Player").GetComponent<Deck>();
        startPosition = transform.position; // Initial position of the model/card

        inputActions = new InputActions();
        inputActions.Player.Enable();
        inputActions.Player.Interaction.performed += OnInteract; // Bind interaction action
    }

    void OnDestroy()
    {
        inputActions.Player.Interaction.performed -= OnInteract;
    }

    void Update()
    {
        // Floating and rotating animation
        transform.Rotate(0, rotationSpeed * Time.deltaTime, 0);
        
        float newHeight = startPosition.y + Mathf.Sin(Time.time * heightRotationSpeed) * heightRotation;
        transform.position = new Vector3(startPosition.x, newHeight, startPosition.z);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            isPlayerNearby = true;

            // Display interaction UI
            if (interactTextPrefab != null && interactTextInstance == null)
            {
                interactTextInstance = Instantiate(interactTextPrefab, transform.position + Vector3.up * 2f, Quaternion.identity, transform);
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            isPlayerNearby = false;

            // Hide interaction UI
            if (interactTextInstance != null)
            {
                Destroy(interactTextInstance);
                interactTextInstance = null;
            }
        }
    }

    private void OnInteract(InputAction.CallbackContext context)
    {
        if (isPlayerNearby)
        {
            // Ensure pick-up has a card assigned
            if (card == null)
            {
                Debug.LogWarning("No card assigned to this pick-up.");
                return; // Exit if no card assigned
            }

            // Check if the player already has this card
            if (!playerDeck.collectionList.Contains(card))
            {
                // Add the card to the player's deck
                playerDeck.deckList.Add(card);
                Debug.Log("Card added to user collection: " + card.name);
            }
            else
            {
                Debug.Log("Player already has this card: " + card.name);
            }

            // Destroy the card object
            Destroy(transform.parent.gameObject);
        }
    }
}
