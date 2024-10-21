using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    // Initializes Public Variables
    public Camera playerCamera;

    public float walkSpeed = 6f; // walking speed
    public float runSpeed = 12f; // running speed
    public float crouchSpeed = 2f; // crouching speed

    public float jumpPower = 4f; // jumping power
    public float gravity = 11f; // gravity

    public float lookSpeed = 2f; // speed to look around with the mouse
    

    public float defaultHeight = 2f; // height of player
    public float crouchHeight = 1f; // height when crouching

    public float stamina = 100f; // stamina
    public float staminaTime = 3f; // time for sprint
    public float staminaRecovery = 2f; // time to 

    // Initialize Private Variables
    private Vector3 moveDirection = Vector3.zero; // stores/keeps the direction which the player moves

    private CharacterController characterController; // refers to the CharacterController component in unity

    private bool canMove = true; // controls if the player can move or not


    void Start()
    {
        characterController = GetComponent<CharacterController>(); // Initializes the reference to CharacterController in unity
        Cursor.lockState = CursorLockMode.Locked; // locks the mouse cursor to the game window
        Cursor.visible = false; // makes the mouse cursor invisible
    }

    void Update()
    {
        Vector3 forward = transform.forward; // Gets the forward direction of the player current view
        Vector3 right = transform.right;     // Gets the right direction of the player current view

        bool isRunning = Input.GetKey(KeyCode.LeftShift); // Checks if the player is currently running if they're holding shift
        float speed = isRunning ? runSpeed : walkSpeed; // Gives the speed for the players movement by checking if the player is running or walking

        float inputVertical = Input.GetAxis("Vertical"); // Gets the input for forward/backward movement of the player
        float inputHorizontal = Input.GetAxis("Horizontal"); // Gets the input for left/right movement of the player

        float curSpeedX = canMove ? speed * inputVertical : 0; // calculate the movement speed of forward/backward
        float curSpeedY = canMove ? speed * inputHorizontal : 0; // calculate movement speed of left/right

        float movementDirectionY = moveDirection.y; // The Y direction (height direction)
        moveDirection = (forward * curSpeedX) + (right * curSpeedY); // finds the direction to move the player and at what speed


        

        if (isRunning && stamina > 0)
        {
            // Decrease stamina over time when the player is running
            stamina -= 100f / staminaTime * Time.deltaTime;

            // make sure it doesn't drop below 0
            if (stamina < 0) 
            {
                stamina = 0;
            }

            // If the player runs out of stamina they cannot sprint anymore
            if (stamina == 0)
            {
                speed = walkSpeed; // forces the player to walk
            }
        }
        else
        {
            // Recover stamina over time when the player is not running
            stamina += 100f / staminaRecovery * Time.deltaTime;

            // make sure it doesn't go above 100
            if (stamina > 100f) 
            {
                stamina = 100f;
            }
        }
        


        // Jumping logic
        if (Input.GetButton("Jump") && canMove && characterController.isGrounded) // applies the jump power to height
        {
            moveDirection.y = jumpPower;
        }
        else // keeps the y the same
        {
            moveDirection.y = movementDirectionY;
        }

        // Gravity logic
        if (!characterController.isGrounded) // if the character is above ground (jumped)
        {
            moveDirection.y -= gravity * Time.deltaTime;
            // player can't move front/back or that it makes them move slower/less distance
            moveDirection.x *= 0.2f;
            moveDirection.z *= 0.2f;
        }

        // Crouching logic
        if (Input.GetKey(KeyCode.R) && canMove) // if the chcracter can move and presses r
        {
            characterController.height = crouchHeight; // crouches the player
            speed = crouchSpeed; // applies the speed when crouching
        }
        else 
        {
            characterController.height = defaultHeight; // keeps the normal height
        }

        // Movement
        characterController.Move(moveDirection * Time.deltaTime); // normal movement

        // Rotation and Camera movement
        if (canMove)
        {
            // Mouse horizontal movement: rotate the entire player body
            float mouseX = Input.GetAxis("Mouse X") * lookSpeed;
            transform.Rotate(0, mouseX, 0); // Rotate the player object based on horizontal mouse input

        }

    }
}

