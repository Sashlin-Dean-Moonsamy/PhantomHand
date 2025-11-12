using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D), typeof(Animator))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public LayerMask groundLayer;

    private Rigidbody2D rb;
    private Collider2D col;
    private Animator animator;

    private Vector2 inputMove;
    private Vector2 velocity;
    private bool isGrounded;
    private bool facingRight = false;

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
        // Read horizontal input
        inputMove = inputActions.Player.Move.ReadValue<Vector2>();

        // Ground check (for animation purposes)
        bool wasGrounded = isGrounded;
        isGrounded = IsGrounded();

        // Horizontal movement
        velocity.x = inputMove.x * moveSpeed;

        // Flip sprite direction
        if (velocity.x > 0.1f && !facingRight)
            Flip();
        else if (velocity.x < -0.1f && facingRight)
            Flip();

        // Update animations
        UpdateAnimator(wasGrounded);
    }

    void FixedUpdate()
    {
        // Move the player kinematically
        Vector2 nextPos = rb.position + velocity * Time.fixedDeltaTime;
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
        scale.x *= -1;
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
