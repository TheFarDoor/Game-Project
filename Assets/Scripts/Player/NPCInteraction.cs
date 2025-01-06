using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using System.Collections;


public class NPCInteraction : MonoBehaviour
{
    public GameObject dialogueUI;        // Reference to the World Space Canvas
    public TMP_Text dialogueText;        // TextMeshPro text for displaying messages
    public string[] messages;            // Array of NPC messages

    public Transform player;             // Reference to the player's transform
    public float interactionRange = 3f;  // Maximum range for interaction

    private Animator animator;           // Reference to the NPC's Animator
    private int messageIndex = 0;        // Keeps track of the current message index
    private bool isDialogueActive = false; // Checks if dialogue is currently active
    private InputActions inputActions;   // Reference to the InputActions

    private Coroutine distanceCheckCoroutine; // Coroutine to monitor player distance

    private void Awake()
    {
        animator = GetComponent<Animator>();

        // Initialize InputActions
        inputActions = new InputActions();

        // Bind the Interaction action
        inputActions.Player.Interaction.performed += OnInteract;
    }

    private void OnEnable()
    {
        inputActions.Player.Enable();
    }

    private void OnDisable()
    {
        inputActions.Player.Disable();
    }

    private void OnInteract(InputAction.CallbackContext context)
    {
        // Check if the player is within range of the NPC
        float distance = Vector3.Distance(transform.position, player.position);
        if (distance <= interactionRange)
        {
            // Trigger the animation each time the interaction input is performed
            if (animator != null)
            {
                animator.SetTrigger("Talk");
            }

            // Handle dialogue logic
            if (!isDialogueActive)
            {
                StartDialogue();
            }
            else
            {
                NextMessage();
            }
        }
    }

    private void StartDialogue()
    {
        if (messages.Length > 0)
        {
            isDialogueActive = true;
            messageIndex = 0;
            dialogueUI.SetActive(true); // Activate the dialogue UI
            dialogueText.text = messages[messageIndex]; // Display the first message

            // Start monitoring the player's distance
            if (distanceCheckCoroutine == null)
            {
                distanceCheckCoroutine = StartCoroutine(CheckPlayerDistance());
            }
        }
    }

    private void NextMessage()
    {
        messageIndex++;
        if (messageIndex < messages.Length)
        {
            dialogueText.text = messages[messageIndex]; // Show the next message
        }
        else
        {
            EndDialogue();
        }
    }

    private void EndDialogue()
    {
        isDialogueActive = false;
        dialogueUI.SetActive(false); // Hide the dialogue UI
        messageIndex = 0; // Reset the message index for the next interaction

        // Stop monitoring the player's distance
        if (distanceCheckCoroutine != null)
        {
            StopCoroutine(distanceCheckCoroutine);
            distanceCheckCoroutine = null;
        }
    }

    private IEnumerator CheckPlayerDistance()
    {
        while (isDialogueActive)
        {
            float distance = Vector3.Distance(transform.position, player.position);
            if (distance > interactionRange)
            {
                Debug.Log("Player moved too far. Dialogue UI resetting.");
                EndDialogue();
                yield break; // Exit the coroutine
            }
            yield return null; // Wait for the next frame
        }
    }
}
