using UnityEngine;
using UnityEngine.InputSystem;


public class PlayerInputHandler : MonoBehaviour
{
    private InputActions controls;
    private Vector2 moveInput;
    private Vector2 lookInput;

    private void Awake()
    {
        controls = new InputActions();

        // Bind the actions
        controls.Player.Move.performed += ctx => moveInput = ctx.ReadValue<Vector2>();
        controls.Player.Look.performed += ctx => lookInput = ctx.ReadValue<Vector2>();
        controls.Player.Move.canceled += ctx => moveInput = Vector2.zero;
        controls.Player.Interact.performed += _ => Interact();
    }

    private void OnEnable()
    {
        controls.Player.Enable();
    }

    private void OnDisable()
    {
        controls.Player.Disable();
    }

    private void Update()
    {
        // Movement example
        Vector3 move = new Vector3(moveInput.x, 0, moveInput.y);
        transform.Translate(move * Time.deltaTime * 5);

        // Look example
        transform.Rotate(0, lookInput.x, 0);

    }

    private void Interact()
    {
        Debug.Log("Interaction triggered");
    }
}
