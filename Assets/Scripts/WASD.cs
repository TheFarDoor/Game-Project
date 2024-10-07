using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    // Initializes Public Variables
    public Camera playerCamera;

    public float walkSpeed = 6f; // walking speed
    public float runSpeed = 12f; // running speed
    public float crouchSpeed = 3f; // crouching speed

    public float jumpPower = 7f; // jumping power
    public float gravity = 10f; // gravity

    public float lookSpeed = 2f; // speed to look around with the mouse
    public float lookXLimit = 45f; // limit to how far the player can look up and down
    

    public float defaultHeight = 2f; // height of player
    public float crouchHeight = 1f; // height when crouching

    // Initializes Private Variables
    private Vector3 moveDirection = Vector3.zero; // stores/keeps the direction which the player moves
    private float rotationX = 0f; // tracks the direction which the player os looking up or down
    private float rotationY = 0f; // tracks the direction which the player os looking left or right

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
            walkSpeed = 6f; // keeps the walk speed
            runSpeed = 12f; // keeps the run speed
        }

        // Movement
        characterController.Move(moveDirection * Time.deltaTime); // normal movement

        // Rotation and Camera movement
        if (canMove)
        {
            rotationX += -Input.GetAxis("Mouse Y") * lookSpeed; // moves the character view based on the mouses movement for the y axis
            rotationX = Mathf.Clamp(rotationX, -lookXLimit, lookXLimit); // keeps the limit of looking up/down as the lookxlimit

            rotationY += Input.GetAxis("Mouse X") * lookSpeed; // moves the character view based on the mouses movement for the x axis
            playerCamera.transform.localRotation = Quaternion.Euler(rotationX, rotationY, 0); // the camera rotation updates


            float mouseX = Input.GetAxis("Mouse X") * lookSpeed; // 
            transform.Rotate(0, mouseX, 0);
        }

    }
}
