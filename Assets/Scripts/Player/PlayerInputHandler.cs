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

    private bool isGrounded = true; // To check if player is grounded
    private float verticalVelocity = 0f; // To track vertical movement
    private float gravity = -9.8f; // Gravity simulation
    private float jumpForce = 5f; // Jump strength

    public Transform cameraTransform; // Reference to the camera
    private Vector3 cameraOffset; // Fixed offset for camera
    private float cameraHeight = 2f; // Height above player
    private float cameraDistance = 3f; // Distance behind the player

    private float terrainY = 1f; // Y position of terrain

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
    }

    private void OnEnable(){controls.Player.Enable();}
    private void OnDisable(){controls.Player.Disable();}

    private void Update()
    {
        Movement();
        RotatePlayer();
        HandleJump();
        UpdateCamera();
    }

    private void Movement()
    {
        float speed = isRunning ? runSpeed : walkSpeed;
        Vector3 move = transform.right * moveInput.x + transform.forward * moveInput.y;
        transform.Translate(move * speed * Time.deltaTime, Space.World);
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
        if (!isGrounded)
        {
            verticalVelocity += gravity * Time.deltaTime;
        }

        transform.Translate(Vector3.up * verticalVelocity * Time.deltaTime, Space.World);

        if (transform.position.y <= terrainY)
        {
            transform.position = new Vector3(transform.position.x, terrainY, transform.position.z);
            verticalVelocity = 0f;
            isGrounded = true;
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

    private void UpdateCamera()
    {
        if (cameraTransform != null)
        {
            Vector3 cameraPosition = transform.position - transform.forward * cameraDistance + Vector3.up * cameraHeight;
            cameraTransform.position = Vector3.Lerp(cameraTransform.position, cameraPosition, Time.deltaTime * 10f);
            cameraTransform.LookAt(transform.position + Vector3.up * 1.5f);
        }
    }
}
