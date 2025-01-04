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

    public GameObject interactTextPrefab;
    private GameObject interactTextInstance;

    private InputActions inputActions;

    void Start()
    {
        playerDeck = GameObject.FindWithTag("Player").GetComponent<Deck>();
        startPosition = transform.position;

        inputActions = new InputActions();
        inputActions.Player.Enable();
        inputActions.Player.Interaction.performed += OnInteract;
    }

    void OnDestroy()
    {
        inputActions.Player.Interaction.performed -= OnInteract;
    }

    void Update()
    {
        transform.Rotate(0, rotationSpeed * Time.deltaTime, 0);
        
        float newHeight = startPosition.y + Mathf.Sin(Time.time * heightRotationSpeed) * heightRotation;
        transform.position = new Vector3(startPosition.x, newHeight, startPosition.z);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            isPlayerNearby = true;

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
            if (card == null)
            {
                Debug.LogWarning("No card assigned to this pick-up.");
                return;
            }

            if (!playerDeck.collectionList.Contains(card))
            {
                playerDeck.deckList.Add(card);
                Debug.Log("Card added to user collection: " + card.name);
            }
            else
            {
                Debug.Log("Player already has this card: " + card.name);
            }

            Destroy(transform.parent.gameObject);
        }
    }
}
