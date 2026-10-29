using UnityEngine;

public class EnemyHealth : MonoBehaviour, IDamageable
{
    [Header("Health")]
    [SerializeField] private float maxHealth = 100f;

    [Header("Knockback")]
    [SerializeField] private float knockbackMultiplier = 1f;

    [Header("Death")]
    [SerializeField] private float deathDestroyDelay = 1.2f;

    private float currentHealth;

    private Rigidbody2D rb;
    private Animator animator;

    private bool dead;

    private void Awake()
    {
        currentHealth = maxHealth;

        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
    }

    // =========================================================
    // DAMAGE
    // =========================================================

    public void TakeDamage(
        float damage,
        Vector2 hitDirection,
        float knockback,
        PlayerAttackType attackType
    )
    {
        if (dead)
            return;

        currentHealth -= damage;

        Debug.Log(
            $"{gameObject.name} took {damage} damage. " +
            $"Attack: {attackType}. " +
            $"Remaining HP: {Mathf.Max(currentHealth, 0f)}"
        );

        ApplyKnockback(
            hitDirection,
            knockback
        );

        if (currentHealth <= 0f)
        {
            Die();
            return;
        }

        PlayHitAnimation(attackType);
    }

    // =========================================================
    // HIT REACTION
    // =========================================================

    private void PlayHitAnimation(
        PlayerAttackType attackType
    )
    {
        if (animator == null)
            return;

        switch (attackType)
        {
            case PlayerAttackType.Heavy:

                animator.ResetTrigger("Hit");
                animator.ResetTrigger("HeavyHit");

                animator.SetTrigger("HeavyHit");

                break;

            default:

                animator.ResetTrigger("HeavyHit");
                animator.ResetTrigger("Hit");

                animator.SetTrigger("Hit");

                break;
        }
    }

    // =========================================================
    // KNOCKBACK
    // =========================================================

    private void ApplyKnockback(
        Vector2 direction,
        float strength
    )
    {
        if (rb == null)
            return;

        if (rb.bodyType != RigidbodyType2D.Dynamic)
            return;

        // Push the enemy away from the player.
        direction = -direction;

        rb.AddForce(
            direction.normalized *
            strength *
            knockbackMultiplier,
            ForceMode2D.Impulse
        );
    }

    // =========================================================
    // DEATH
    // =========================================================

    private void Die()
    {
        if (dead)
            return;

        dead = true;

        Debug.Log(
            $"{gameObject.name} died."
        );

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
        }

        if (animator != null)
        {
            animator.ResetTrigger("Hit");
            animator.ResetTrigger("HeavyHit");
            animator.ResetTrigger("Death");

            animator.SetTrigger("Death");
        }

        Destroy(
            gameObject,
            deathDestroyDelay
        );
    }

    // =========================================================
    // GETTERS
    // =========================================================

    public float GetCurrentHealth()
    {
        return currentHealth;
    }

    public float GetMaxHealth()
    {
        return maxHealth;
    }

    public bool IsDead()
    {
        return dead;
    }
}