using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputHandler : MonoBehaviour
{
    // Movement Settings
    [SerializeField] private float walkSpeed = 5f;
    [SerializeField] private float runSpeed = 10f;
    [SerializeField] private float gravity = -9.8f;
    [SerializeField] private float jumpForce = 2f;

    // Rotation Settings
    [SerializeField] private float yaSensitivity = 2f; // mouse sensitivity - horizontal
    [SerializeField] private float pitchSensitivity = 2f; // mouse sensitivity - vertical
    [SerializeField] private float minPitchAngle = -3f;
    [SerializeField] private float maxPitchAngle = 30f;

    // References
    [SerializeField] private Transform cameraTransform;
    
    private InputActions controls;
    private Vector2 moveInput;
    private Vector2 lookInput;
    private bool isRunning;
    private bool isGrounded = true;
    private float verticalVelocity = 0f; // up/down movement speed
    private float pitch = 0f;

    private float slopeAngle = 0f;
    private Vector3 hitNormal;

    private CharacterController characterController;
    private Animator animator;

    // Inventory UI reference
    public GameObject inventoryPanel;
    private bool isInventoryOpen = false; // Make inventory UI not visible at launch

    // PauseMenu UI reference
    public GameObject pausePanel;
    private bool isPaused = false; // Make the game not paused at launch

    private void Awake()
    {
        InitializeInputActions();
        characterController = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
    }

    private void Start()
    {
        LockCursor(); // Lock the cursor at the start of the game
    }

    private void OnEnable() => controls.Player.Enable();
    private void OnDisable() => controls.Player.Disable();

    private void Update()
    {
        if (!isPaused)
        {
            if (!isInventoryOpen)
            {
                CheckSlope();
                if (slopeAngle > characterController.slopeLimit && isGrounded)
                {
                    HandleSliding();
                }
                else
                {
                    HandleMovement();
                }
                RotatePlayer();
                ApplyJump();
            }
        }
    }

    private void InitializeInputActions()
    {
        controls = new InputActions();

        controls.Player.Move.performed += ctx => moveInput = ctx.ReadValue<Vector2>();
        controls.Player.Move.canceled += ctx => moveInput = Vector2.zero;

        controls.Player.Look.performed += ctx => lookInput = ctx.ReadValue<Vector2>();
        controls.Player.Look.canceled += ctx => lookInput = Vector2.zero;

        controls.Player.Run.performed += ctx => isRunning = ctx.ReadValueAsButton();
        controls.Player.Run.canceled += ctx => isRunning = false;

        controls.Player.Inventory.performed += ctx => ToggleInventory(); // Inventory
        controls.Player.Pause.performed += ctx => TogglePause(); // Pause Menu

        controls.Player.Jump.performed += ctx => TriggerJump();
    }

    private void HandleMovement()
    {
        float speed = isRunning ? runSpeed : walkSpeed;
        Vector3 moveDirection = CalculateMoveDirection();

        animator.SetBool("isWalking", moveInput.sqrMagnitude > 0);
        animator.SetBool("isRunning", isRunning && moveInput.sqrMagnitude > 0);

        if (!isGrounded)
        {
            verticalVelocity += gravity * Time.deltaTime;
        }

        moveDirection.y = verticalVelocity;
        characterController.Move(moveDirection * speed * Time.deltaTime);

        isGrounded = characterController.isGrounded;
        if (isGrounded && verticalVelocity < 0)
        {
            verticalVelocity = -0.1f;
        }
    }

    private Vector3 CalculateMoveDirection()
    {
        return transform.right * moveInput.x + transform.forward * moveInput.y;
    }

    private void RotatePlayer()
    {
        if (lookInput.x != 0)
        {
            float ya = lookInput.x * yaSensitivity;
            transform.Rotate(0, ya, 0);
        }

        if (lookInput.y != 0)
        {
            pitch -= lookInput.y * pitchSensitivity;
            pitch = Mathf.Clamp(pitch, minPitchAngle, maxPitchAngle);
            cameraTransform.localEulerAngles = new Vector3(pitch, cameraTransform.localEulerAngles.y, 0);
        }
    }

    private void ApplyJump()
    {
        if (isGrounded && controls.Player.Jump.triggered)
        {
            verticalVelocity = jumpForce;
            isGrounded = false;
        }
    }

    private void TriggerJump()
    {
        if (isGrounded)
        {
            verticalVelocity = jumpForce;
            isGrounded = false;
        }
    }

    private void CheckSlope()
    {
        if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, 1.5f))
        {
            hitNormal = hit.normal;
            slopeAngle = Vector3.Angle(hitNormal, Vector3.up);
        }
        else
        {
            slopeAngle = 0f;
        }
    }

    private void HandleSliding()
    {
        Vector3 slideDirection = new Vector3(hitNormal.x, -hitNormal.y, hitNormal.z);
        characterController.Move(slideDirection * walkSpeed * Time.deltaTime);

        verticalVelocity += gravity * Time.deltaTime;
        slideDirection.y = verticalVelocity;
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

    private void TogglePause()
    {
        isPaused = !isPaused;
        pausePanel.SetActive(isPaused);

        if (isPaused)
        {
            UnlockCursor();
        }
        else
        {
            LockCursor();
        }
    }
}
