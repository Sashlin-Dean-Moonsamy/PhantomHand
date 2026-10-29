using UnityEngine;
using UnityEngine.Events;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health")]
    [SerializeField] private int maxHealth = 10;
    [SerializeField] private int currentHealth;

    [Header("Hit Reaction")]
    [SerializeField] private float hitAnimationCooldown = 0.1f;

    [Header("Death")]
    [SerializeField] private bool disableMovementOnDeath = true;

    private Animator animator;
    private PlayerMovement playerMovement;

    private bool dead;
    private float lastHitTime;

    public UnityEvent<int, int> OnHealthChanged;
    public UnityEvent OnDeath;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        playerMovement = GetComponent<PlayerMovement>();

        // Start player at full health.
        currentHealth = maxHealth;
    }

    private void Start()
    {
        // Tell the UI the starting health.
        OnHealthChanged?.Invoke(
            currentHealth,
            maxHealth
        );
    }

    // =========================================================
    // DAMAGE
    // =========================================================

    public void TakeDamage(int damage)
    {
        if (dead)
            return;

        if (damage <= 0)
            return;

        currentHealth -= damage;

        currentHealth = Mathf.Clamp(
            currentHealth,
            0,
            maxHealth
        );

        Debug.Log(
            $"Player took {damage} damage. " +
            $"Health: {currentHealth}/{maxHealth}"
        );

        // Tell the health UI about the new health.
        OnHealthChanged?.Invoke(
            currentHealth,
            maxHealth
        );

        // Play hit animation.
        PlayHitAnimation();

        // Check for death.
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    // =========================================================
    // HIT ANIMATION
    // =========================================================

    private void PlayHitAnimation()
    {
        if (animator == null)
            return;

        if (Time.time <
            lastHitTime + hitAnimationCooldown)
        {
            return;
        }

        lastHitTime = Time.time;

        animator.ResetTrigger("Hit");
        animator.SetTrigger("Hit");
    }

    // =========================================================
    // DEATH
    // =========================================================

    private void Die()
    {
        if (dead)
            return;

        dead = true;

        Debug.Log("Player died.");

        if (disableMovementOnDeath &&
            playerMovement != null)
        {
            playerMovement.SetMovementLocked(true);
        }

        if (animator != null)
        {
            animator.ResetTrigger("Hit");
            animator.ResetTrigger("Death");

            animator.SetTrigger("Death");
        }

        // Tell the UI that the player died.
        OnDeath?.Invoke();
    }

    // =========================================================
    // HEAL
    // =========================================================

    public void Heal(int amount)
    {
        if (dead)
            return;

        if (amount <= 0)
            return;

        currentHealth += amount;

        currentHealth = Mathf.Clamp(
            currentHealth,
            0,
            maxHealth
        );

        OnHealthChanged?.Invoke(
            currentHealth,
            maxHealth
        );
    }

    // =========================================================
    // GETTERS
    // =========================================================

    public int GetCurrentHealth()
    {
        return currentHealth;
    }

    public int GetMaxHealth()
    {
        return maxHealth;
    }

    public bool IsDead()
    {
        return dead;
    }
}
