using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D), typeof(Animator))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;

    [Header("Jump Settings")]
    public float jumpForce = 12f;

    [Header("Gravity Settings")]
    public float gravity = -35f;
    public float maxFallSpeed = -20f;

    [Header("Dash Settings")]
    public float dashDistance = 3f;
    public float dashDuration = 0.12f;
    public float dashCooldown = 0.5f;

    [Header("Ground Check")]
    public LayerMask groundLayer;

    private Rigidbody2D rb;
    private Collider2D col;
    private Animator animator;

    private Vector2 inputMove;
    private Vector2 velocity;

    private bool isGrounded;
    private bool facingRight = false;
    private bool isDashing;

    private float verticalVelocity = 0f;
    private float lastDashTime;

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
        // ---------------- INPUT ----------------
        inputMove = inputActions.Player.Move.ReadValue<Vector2>();
        bool jumpPressed = inputActions.Player.Jump.WasPressedThisFrame();
        bool dashPressed = inputActions.Player.Dash.WasPressedThisFrame();

        bool wasGrounded = isGrounded;
        isGrounded = IsGrounded();

        // ---------------- JUMP ----------------
        if (jumpPressed && isGrounded)
        {
            verticalVelocity = jumpForce;
            isGrounded = false;
            animator.SetTrigger("Jump");
        }

        // ---------------- DASH ----------------
        if (dashPressed && !isDashing && Time.time - lastDashTime > dashCooldown)
        {
            float dir = facingRight ? 1f : -1f;
            StartCoroutine(DashRoutine(dir));
        }

        // ---------------- HORIZONTAL ----------------
        if (!isDashing)
            velocity.x = inputMove.x * moveSpeed;

        // ---------------- GRAVITY ----------------
        if (!isGrounded && !isDashing)
        {
            verticalVelocity -= gravity * Time.deltaTime;
            verticalVelocity = Mathf.Max(verticalVelocity, maxFallSpeed);
        }
        else if (isGrounded && verticalVelocity < 0f)
        {
            verticalVelocity = 0f;
        }

        // ---------------- FLIP ----------------
        if (velocity.x > 0.1f && !facingRight)
            Flip();
        else if (velocity.x < -0.1f && facingRight)
            Flip();

        // ---------------- ANIMATOR ----------------
        animator.SetFloat("Speed", Mathf.Abs(velocity.x));
        animator.SetBool("isGrounded", isGrounded);
    }

    void FixedUpdate()
    {
        if (isDashing) return;

        Vector2 nextPos = rb.position;
        nextPos.x += velocity.x * Time.fixedDeltaTime;
        nextPos.y += verticalVelocity * Time.fixedDeltaTime;

        // --------- Ground Snap ---------
        if (verticalVelocity <= 0f)
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
                nextPos.y = hit.point.y + col.bounds.extents.y;
                verticalVelocity = 0f;
                isGrounded = true;
            }
        }

        rb.MovePosition(nextPos);
    }

    // ---------------- DASH ROUTINE ----------------

    IEnumerator DashRoutine(float dir)
    {
        isDashing = true;
        lastDashTime = Time.time;
        animator.SetTrigger("Dash");

        float speed = dashDistance / dashDuration;
        float timer = 0f;

        while (timer < dashDuration)
        {
            rb.MovePosition(rb.position + Vector2.right * dir * speed * Time.fixedDeltaTime);
            timer += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }

        isDashing = false;
    }

    // ---------------- GROUND CHECK ----------------

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

    // ---------------- FLIP ----------------

    private void Flip()
    {
        facingRight = !facingRight;
        Vector3 scale = transform.localScale;
        scale.x *= -1f;
        transform.localScale = scale;
    }

    // ---------------- DEBUG ----------------

    void OnDrawGizmos()
    {
        if (col == null) return;

        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(
            col.bounds.center + Vector3.down * 0.05f,
            col.bounds.size
        );
    }
}
