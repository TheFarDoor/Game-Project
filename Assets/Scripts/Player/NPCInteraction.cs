using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using TMPro;


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

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }


    private void Update()
    {
        // Check the distance between the player and the NPC
        float distance = Vector3.Distance(transform.position, player.position);

        if (distance <= interactionRange)
        {
            // Check for interaction input
            if (Input.GetKeyDown(KeyCode.E))
            {
                // Trigger the animation each time E is pressed
                if (animator != null)
                {
                    animator.SetTrigger("Talk"); // Trigger the "Talk" animation
                }

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
        else
        {
            // End dialogue if the player moves out of range
            if (isDialogueActive)
            {
                EndDialogue();
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
    }
}
