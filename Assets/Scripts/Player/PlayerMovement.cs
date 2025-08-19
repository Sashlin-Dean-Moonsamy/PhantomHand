using UnityEngine;
using UnityEngine.InputSystem; // Needed for new Input System

// This script handles Carl's movement using the new Input System.
// It assumes you have a Rigidbody2D, Animator, and PlayerInput components on the player.
// Movement is kinematic (we control velocity manually), not physics-based (no AddForce).
[RequireComponent(typeof(Rigidbody2D), typeof(Animator))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f; // Horizontal movement speed
    public float jumpForce = 12f; // How high Carl jumps

    [Header("Crouch Settings")]
    public float crouchSpeedMultiplier = 0.5f; // Movement is slower when crouching

    [Header("Ground Check Settings")]
    public Transform groundCheck; // Empty GameObject placed at Carl's feet in the hierarchy
    public float groundCheckRadius = 0.2f; // Size of the circle that checks for ground
    public LayerMask groundLayer; // Assign ground tiles to a "Ground" layer in Unity and reference that layer here

    // Private references to components
    private Rigidbody2D rb;
    private Animator animator;

    // Variables to store input state
    private Vector2 inputMovement;
    private bool isJumpPressed = false;
    private bool isCrouching = false;
    private bool isGrounded = false;

    void Awake()
    {
        // Grab components attached to this GameObject
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        // Ground check: Check if Carl is touching the ground using a tiny circle below his feet
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

        // Set animation values if an Animator is attached
        if (animator != null)
        {
            animator.SetFloat("Speed", Mathf.Abs(inputMovement.x)); // Used for Idle/Run transitions
            animator.SetBool("IsJumping", !isGrounded); // Used for Jump state
            animator.SetBool("IsCrouching", isCrouching); // Used for crouch animations
        }

        // If the jump button was pressed and Carl is grounded, apply vertical jump velocity
        if (isJumpPressed && isGrounded)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        }
    }

    void FixedUpdate()
    {
        // Calculate horizontal movement
        float moveX = inputMovement.x * moveSpeed;

        // Slow down movement if crouching
        if (isCrouching)
        {
            moveX *= crouchSpeedMultiplier;
        }

        // Apply velocity to Rigidbody (kinematic movement)
        rb.linearVelocity = new Vector2(moveX, rb.linearVelocity.y);

        // Clear jump flag after applying jump
        isJumpPressed = false;
    }

    // Called automatically by Input System when the Move action is triggered
    public void OnMove(InputAction.CallbackContext context)
    {
        inputMovement = context.ReadValue<Vector2>(); // Gets horizontal movement input (-1 to 1)
    }

    // Called when the Jump action is triggered
    public void OnJump(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            isJumpPressed = true; // Set flag to apply jump in Update
        }
    }

    // Called when Crouch action is triggered (button press or axis input)
    public void OnCrouch(InputAction.CallbackContext context)
    {
        // If value > 0.5, assume crouch is active
        isCrouching = context.ReadValue<float>() > 0.5f;
    }
}
