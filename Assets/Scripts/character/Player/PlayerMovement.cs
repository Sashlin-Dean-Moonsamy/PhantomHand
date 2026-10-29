using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Animator))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float acceleration = 45f;
    [SerializeField] private float deceleration = 60f;

    [Header("Jump")]
    [SerializeField] private float jumpForce = 12f;
    [SerializeField] private float gravity = 35f;
    [SerializeField] private float maxFallSpeed = 20f;

    [Header("Ground Check")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.15f;
    [SerializeField] private LayerMask groundLayer;

    [Header("Dash")]
    [SerializeField] private float dashSpeed = 16f;
    [SerializeField] private float dashDuration = 0.2f;
    [SerializeField] private float dashCooldown = 0.5f;

    [Header("Grind")]
    [SerializeField] private float grindSpeed = 8f;
    [SerializeField] private float grindDuration = 0.5f;

    [Header("Waiting Animation")]
    [SerializeField] private float waitingAnimationInterval = 5f;
    [SerializeField] private int waitingAnimationLoops = 3;

    [Header("Breach Alert")]
    [SerializeField] private BreachAlertUI breachAlertUI;

    private Rigidbody2D rb;
    private Animator animator;

    private InputSystem_Actions inputActions;

    private Vector2 moveInput;

    private float horizontalVelocity;
    private float verticalVelocity;

    private bool facingRight = false;
    private bool movementLocked;

    private bool isGrounded;
    private bool wasGrounded;

    // Dash
    private bool isDashing;
    private float dashTimer;
    private float dashCooldownTimer;
    private float dashDirection;

    // Grind
    private bool isGrinding;
    private float grindTimer;

    // Waiting
    private float waitingTimer;
    private int waitingAnimationCount;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();

        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.gravityScale = 0f;

        inputActions = new InputSystem_Actions();
    }

    private void OnEnable()
    {
        inputActions.Enable();
    }

    private void OnDisable()
    {
        inputActions.Disable();
    }

    private void Update()
    {
        if (movementLocked)
        {
            StopMovement();

            animator.SetFloat("Speed", 0f);
            animator.SetBool("isGrounded", true);
            animator.SetFloat("verticalVelocity", 0f);

            return;
        }

        UpdateTimers();

        UpdateGroundedState();

        ReadMovementInput();

        HandleJump();

        HandleFacing();

        UpdateWaitingAnimation();

        UpdateAnimator();
    }

    private void FixedUpdate()
    {
        if (movementLocked)
            return;

        if (isDashing)
        {
            PerformDash();
            return;
        }

        if (isGrinding)
        {
            PerformGrind();
            return;
        }

        ApplyGravity();

        PerformNormalMovement();
    }

    // =========================================================
    // NORMAL MOVEMENT
    // =========================================================

    private void ReadMovementInput()
    {
        moveInput = inputActions.Player.Move.ReadValue<Vector2>();

        float targetVelocity = moveInput.x * moveSpeed;

        float rate = Mathf.Abs(targetVelocity) > 0.01f
            ? acceleration
            : deceleration;

        horizontalVelocity = Mathf.MoveTowards(
            horizontalVelocity,
            targetVelocity,
            rate * Time.deltaTime
        );
    }

    private void PerformNormalMovement()
    {
        Vector2 movement = new Vector2(
            horizontalVelocity,
            verticalVelocity
        ) * Time.fixedDeltaTime;

        rb.MovePosition(rb.position + movement);
    }

    // =========================================================
    // JUMP
    // =========================================================

    private void HandleJump()
    {
        if (!inputActions.Player.Jump.WasPressedThisFrame())
            return;

        if (!isGrounded)
            return;

        if (isDashing || isGrinding)
            return;

        verticalVelocity = jumpForce;
    }

    private void ApplyGravity()
    {
        if (isGrounded && verticalVelocity <= 0f)
        {
            verticalVelocity = 0f;
            return;
        }

        verticalVelocity -= gravity * Time.fixedDeltaTime;

        verticalVelocity = Mathf.Max(
            verticalVelocity,
            -maxFallSpeed
        );
    }

    // =========================================================
    // DASH
    // =========================================================

    public bool StartDash()
    {
        if (movementLocked)
            return false;

        if (isDashing || isGrinding)
            return false;

        if (dashCooldownTimer > 0f)
            return false;

        if (Mathf.Abs(moveInput.x) > 0.1f)
        {
            dashDirection = Mathf.Sign(moveInput.x);
        }
        else
        {
            dashDirection = facingRight ? 1f : -1f;
        }

        isDashing = true;

        dashTimer = dashDuration;
        dashCooldownTimer = dashCooldown;

        horizontalVelocity = 0f;

        animator.ResetTrigger("Dash");
        animator.SetTrigger("Dash");

        return true;
    }

    private void PerformDash()
    {
        Vector2 movement = new Vector2(
            dashDirection * dashSpeed,
            0f
        ) * Time.fixedDeltaTime;

        rb.MovePosition(rb.position + movement);

        dashTimer -= Time.fixedDeltaTime;

        if (dashTimer <= 0f)
        {
            isDashing = false;
            dashTimer = 0f;
        }
    }

    // =========================================================
    // GRIND
    // =========================================================

    public bool StartGrind()
    {
        if (movementLocked)
            return false;

        if (isDashing || isGrinding)
            return false;

        isGrinding = true;
        grindTimer = grindDuration;

        horizontalVelocity = 0f;

        animator.ResetTrigger("Grind");
        animator.SetTrigger("Grind");

        return true;
    }

    private void PerformGrind()
    {
        float direction = Mathf.Abs(moveInput.x) > 0.1f
            ? Mathf.Sign(moveInput.x)
            : (facingRight ? 1f : -1f);

        Vector2 movement = new Vector2(
            direction * grindSpeed,
            0f
        ) * Time.fixedDeltaTime;

        rb.MovePosition(rb.position + movement);

        grindTimer -= Time.fixedDeltaTime;

        if (grindTimer <= 0f)
        {
            isGrinding = false;
            grindTimer = 0f;
        }
    }

    // =========================================================
    // GROUND
    // =========================================================

    private void UpdateGroundedState()
    {
        wasGrounded = isGrounded;

        if (groundCheck == null)
        {
            isGrounded = false;
        }
        else
        {
            isGrounded = Physics2D.OverlapCircle(
                groundCheck.position,
                groundCheckRadius,
                groundLayer
            );
        }

        if (isGrounded && !wasGrounded && verticalVelocity < 0f)
        {
            verticalVelocity = 0f;
        }

        animator.SetBool("isGrounded", isGrounded);
    }

    // =========================================================
    // ANIMATOR
    // =========================================================

    private void UpdateAnimator()
    {
        animator.SetFloat(
            "Speed",
            Mathf.Abs(horizontalVelocity)
        );

        animator.SetFloat(
            "verticalVelocity",
            verticalVelocity
        );
    }

    // =========================================================
    // WAITING
    // =========================================================

    private void UpdateWaitingAnimation()
    {
        if (!isGrounded ||
            isDashing ||
            isGrinding ||
            Mathf.Abs(moveInput.x) > 0.1f)
        {
            waitingTimer = 0f;
            waitingAnimationCount = 0;
            return;
        }

        if (waitingAnimationCount >= waitingAnimationLoops)
            return;

        waitingTimer += Time.deltaTime;

        if (waitingTimer >= waitingAnimationInterval)
        {
            animator.SetTrigger("waiting");

            waitingAnimationCount++;
            waitingTimer = 0f;
        }
    }

    // =========================================================
    // FACING
    // =========================================================

    private void HandleFacing()
    {
        if (isDashing || isGrinding)
            return;

        if (moveInput.x > 0.1f && !facingRight)
        {
            Flip();
        }
        else if (moveInput.x < -0.1f && facingRight)
        {
            Flip();
        }
    }

    private void Flip()
    {
        facingRight = !facingRight;

        Vector3 scale = transform.localScale;
        scale.x *= -1f;
        transform.localScale = scale;
    }

    // =========================================================
    // LOCK
    // =========================================================

    public void SetMovementLocked(bool locked)
    {
        movementLocked = locked;

        if (locked)
        {
            StopMovement();

            isDashing = false;
            isGrinding = false;

            dashTimer = 0f;
            grindTimer = 0f;

            animator.SetFloat("Speed", 0f);
        }
    }

    private void StopMovement()
    {
        moveInput = Vector2.zero;
        horizontalVelocity = 0f;
        verticalVelocity = 0f;
    }

    // =========================================================
    // GETTERS
    // =========================================================

    public bool IsGrounded()
    {
        return isGrounded;
    }

    public bool IsDashing()
    {
        return isDashing;
    }

    public bool IsGrinding()
    {
        return isGrinding;
    }

    public bool IsMovementLocked()
    {
        return movementLocked;
    }

    public float GetFacingDirection()
    {
        return facingRight ? 1f : -1f;
    }

    // =========================================================
    // TIMERS
    // =========================================================

    private void UpdateTimers()
    {
        if (dashCooldownTimer > 0f)
        {
            dashCooldownTimer -= Time.deltaTime;

            if (dashCooldownTimer < 0f)
                dashCooldownTimer = 0f;
        }
    }

    // =========================================================
    // BREACH ALERT
    // =========================================================

    public void TriggerBreachAlert()
    {
        if (breachAlertUI == null)
        {
            Debug.LogWarning(
                "Breach Alert UI is not assigned."
            );

            return;
        }

        breachAlertUI.TriggerBreachAlert();
    }

    // =========================================================
    // DEBUG
    // =========================================================

    private void OnDrawGizmosSelected()
    {
        if (groundCheck == null)
            return;

        Gizmos.color = Color.green;

        Gizmos.DrawWireSphere(
            groundCheck.position,
            groundCheckRadius
        );
    }
}