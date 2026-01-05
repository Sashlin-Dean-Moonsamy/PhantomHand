using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D), typeof(Animator))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;

    [Header("Gravity Settings")]
    public float gravity = -35f;        // How fast player accelerates downward
    public float maxFallSpeed = -20f;   // Terminal velocity

    [Header("Ground Check")]
    public LayerMask groundLayer;

    private Rigidbody2D rb;
    private Collider2D col;
    private Animator animator;

    private Vector2 inputMove;
    private Vector2 velocity;
    private bool isGrounded;
    private bool facingRight = false;

    private float verticalVelocity = 0f;  // Manual gravity

    private InputSystem_Actions inputActions;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();
        animator = GetComponent<Animator>();

        rb.bodyType = RigidbodyType2D.Kinematic;

        inputActions = new InputSystem_Actions();
        inputActions.Player.Enable();
    }

    void Update()
    {
        // Read movement input (X axis)
        inputMove = inputActions.Player.Move.ReadValue<Vector2>();

        bool wasGrounded = isGrounded;
        isGrounded = IsGrounded();

        // Horizontal velocity
        velocity.x = inputMove.x * moveSpeed;

        // Apply manual gravity if NOT grounded
        if (!isGrounded)
        {
            verticalVelocity -= gravity * Time.deltaTime;
            verticalVelocity = Mathf.Max(verticalVelocity, maxFallSpeed);
        }
        else
        {
            verticalVelocity = 0f; // reset when standing on ground
        }

        // Flip sprite
        if (velocity.x > 0.1f && !facingRight)
            Flip();
        else if (velocity.x < -0.1f && facingRight)
            Flip();

        UpdateAnimator(wasGrounded);
    }

    void FixedUpdate()
    {
        Vector2 nextPos = rb.position;

        // Apply horizontal movement
        nextPos.x += velocity.x * Time.fixedDeltaTime;

        // Apply gravity movement
        nextPos.y += verticalVelocity * Time.fixedDeltaTime;

        // Ground snapping if falling into ground
        if (verticalVelocity <= 0f) // only when moving down
        {
            RaycastHit2D hit = Physics2D.BoxCast(
                col.bounds.center,
                col.bounds.size,
                0f,
                Vector2.down,
                Mathf.Abs(verticalVelocity * Time.fixedDeltaTime) + 0.1f,
                groundLayer
            );

            if (hit.collider != null)
            {
                // Snap perfectly on top of ground
                nextPos.y = hit.point.y + col.bounds.extents.y;

                verticalVelocity = 0f;
                isGrounded = true;
            }
        }

        rb.MovePosition(nextPos);
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
        scale.x *= -1f;
        transform.localScale = scale;
    }

    private void UpdateAnimator(bool wasGrounded)
    {
        if (!animator) return;

        animator.SetFloat("Speed", Mathf.Abs(velocity.x));
        animator.SetBool("isGrounded", isGrounded);
    }

    void OnDrawGizmos()
    {
        if (col == null) return;

        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(col.bounds.center + Vector3.down * 0.05f, col.bounds.size);
    }
}
