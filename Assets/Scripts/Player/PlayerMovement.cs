using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class KinematicPlayerInput : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float jumpForce = 10f;
    public float gravity = 20f;
    public LayerMask groundLayer;

    private Rigidbody2D rb;
    private Collider2D col;

    private Vector2 inputMove;
    private Vector2 velocity;
    private bool isGrounded;

    private InputSystem_Actions inputActions;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();

        // Instantiate and enable the input actions
        inputActions = new InputSystem_Actions();
        inputActions.Player.Enable();

        // Subscribe to jump input
        inputActions.Player.Jump.performed += OnJump;
    }

    void OnDestroy()
    {
        // Always unsubscribe to avoid memory leaks
        inputActions.Player.Jump.performed -= OnJump;
    }

    void Update()
    {
        // Read movement input
        inputMove = inputActions.Player.Move.ReadValue<Vector2>();
        velocity.x = inputMove.x * moveSpeed;

        // Ground check
        isGrounded = IsGrounded();

        // Apply gravity manually (Kinematic body doesn't have built-in gravity)
        velocity.y -= gravity * Time.deltaTime;
    }

    void FixedUpdate()
    {
        rb.MovePosition(rb.position + velocity * Time.fixedDeltaTime);
    }

    private void OnJump(InputAction.CallbackContext context)
    {
        if (isGrounded)
        {
            velocity.y = jumpForce;
        }
    }

    // Ground detection using box cast
    private bool IsGrounded()
    {
        float extraHeight = 0.05f;
        RaycastHit2D hit = Physics2D.BoxCast(
            col.bounds.center,
            col.bounds.size,
            0f,
            Vector2.down,
            extraHeight,
            groundLayer
        );

        return hit.collider != null;
    }
}
