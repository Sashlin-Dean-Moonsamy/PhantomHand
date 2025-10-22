using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D), typeof(Animator))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float jumpForce = 12f;
    public float gravity = 120f; // Stronger gravity for faster falling
    public LayerMask groundLayer;

    private Rigidbody2D rb;
    private Collider2D col;
    private Animator animator;

    private Vector2 inputMove;
    private Vector2 velocity;
    private bool isGrounded;
    private bool facingRight = false;
    private bool jumpQueued;

    private InputSystem_Actions inputActions;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();
        animator = GetComponent<Animator>();

        rb.bodyType = RigidbodyType2D.Kinematic;

        inputActions = new InputSystem_Actions();
        inputActions.Player.Enable();

        inputActions.Player.Jump.performed += OnJump;
    }

    void OnDestroy()
    {
        inputActions.Player.Jump.performed -= OnJump;
    }

    void Update()
    {
        // Read movement input
        inputMove = inputActions.Player.Move.ReadValue<Vector2>();

        // Check ground contact
        bool wasGrounded = isGrounded;
        isGrounded = IsGrounded();

        // Handle jump input when grounded
        if (jumpQueued && isGrounded)
        {
            velocity.y = jumpForce;
            jumpQueued = false;
            isGrounded = false; // Immediately set to false on jump
            animator.SetTrigger("Jump");
        }

        // Apply gravity when airborne
        if (!isGrounded)
        {
            velocity.y -= gravity * Time.deltaTime;
            if (velocity.y < -40f)
                velocity.y = -40f;
        }
        else if (velocity.y < 0)
        {
            velocity.y = 0f;
        }

        // Horizontal movement
        velocity.x = inputMove.x * moveSpeed;

        // Flip sprite based on movement direction
        if (velocity.x > 0.1f && !facingRight) Flip();
        else if (velocity.x < -0.1f && facingRight) Flip();

        // Update animator parameters
        UpdateAnimator(wasGrounded);
    }

    void FixedUpdate()
    {
        // Move player based on calculated velocity
        Vector2 nextPos = rb.position + velocity * Time.fixedDeltaTime;
        rb.MovePosition(nextPos);
    }

    private void OnJump(InputAction.CallbackContext ctx)
    {
        jumpQueued = true;
    }

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

    private void Flip()
    {
        facingRight = !facingRight;
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
    }

    private void UpdateAnimator(bool wasGrounded)
    {
        if (!animator) return;

        animator.SetFloat("Speed", Mathf.Abs(velocity.x));
        animator.SetFloat("yVelocity", velocity.y);

        // Instantly reflect airborne state in animator
        animator.SetBool("isGrounded", isGrounded);

        // Optional: Add landing trigger when falling -> grounded
        if (!wasGrounded && isGrounded)
        {
            animator.SetTrigger("Land");
        }
    }

    void OnDrawGizmos()
    {
        if (col == null) return;
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(col.bounds.center + Vector3.down * 0.05f, col.bounds.size);
    }
}
