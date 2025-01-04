using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputHandler : MonoBehaviour
{
    private InputActions controls;
    private Vector2 moveInput;
    private Vector2 lookInput;
    private bool isRunning;

    private float walkSpeed = 5f;
    private float runSpeed = 10f;

    private bool isGrounded = true;
    private float verticalVelocity = 0f;
    private float gravity = -9.8f;
    private float jumpForce = 2f;

    public Transform cameraTransform;
    private CharacterController characterController;

    // Inventory UI reference
    public GameObject inventoryPanel;
    private bool isInventoryOpen = false; // Make inventory UI not visible at launch

    Animator animator;

    private void Awake()
    {
        controls = new InputActions();

        // Bind the actions
        controls.Player.Move.performed += ctx => moveInput = ctx.ReadValue<Vector2>();
        controls.Player.Move.canceled += ctx => moveInput = Vector2.zero;
        controls.Player.Look.performed += ctx => lookInput = ctx.ReadValue<Vector2>();
        controls.Player.Look.canceled += ctx => lookInput = Vector2.zero;
        controls.Player.Run.performed += ctx => isRunning = ctx.ReadValueAsButton();
        controls.Player.Run.canceled += ctx => isRunning = false;
        controls.Player.Jump.performed += ctx => Jump();

        controls.Player.Inventory.performed += ctx => ToggleInventory();

        characterController = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
    }

    private void Start()
    {
        LockCursor(); // Lock the cursor at the start of the game
    }

    private void OnEnable() { controls.Player.Enable(); }
    private void OnDisable() { controls.Player.Disable(); }

    private void Update()
    {
        if (!isInventoryOpen) // Only allow movement when inventory is closed
        {
            Movement();
            RotatePlayer();
            HandleJump();
        }
    }

    private void Movement()
    {
        float speed = isRunning ? runSpeed : walkSpeed;
        Vector3 move = transform.right * moveInput.x + transform.forward * moveInput.y;

        // checks if player is moving
        bool isWalking = moveInput.x != 0 || moveInput.y != 0;
        animator.SetBool("isWalking", isWalking);
        animator.SetBool("isRunning", isWalking && isRunning);

        if (!isGrounded)
        {
            verticalVelocity += gravity * Time.deltaTime;
        }

        move.y = verticalVelocity;
        characterController.Move(move * speed * Time.deltaTime);

        isGrounded = characterController.isGrounded;
        if (isGrounded && verticalVelocity < 0)
        {
            verticalVelocity = -0.1f;
        }
    }

    private void RotatePlayer()
    {
        if (lookInput.x != 0)
        {
            float yRotation = lookInput.x * 2f;
            transform.Rotate(0, yRotation, 0);
        }
    }

    private void HandleJump()
    {
        if (!isGrounded) return;

        if (controls.Player.Jump.triggered)
        {
            verticalVelocity = jumpForce;
        }
    }

    private void Jump()
    {
        if (isGrounded)
        {
            verticalVelocity = jumpForce;
            isGrounded = false;
        }
    }

    private void ToggleInventory()
    {
        isInventoryOpen = !isInventoryOpen;
        inventoryPanel.SetActive(isInventoryOpen);

        if (isInventoryOpen)
        {
            UnlockCursor();
        }
        else
        {
            LockCursor();
        }
    }

    private void LockCursor()
    {
        Cursor.lockState = CursorLockMode.Locked; // Locks the cursor
        Cursor.visible = false;                  // Hides the cursor
    }

    private void UnlockCursor()
    {
        Cursor.lockState = CursorLockMode.None;  // Unlocks the cursor
        Cursor.visible = true;                   // Shows the cursor
    }
}