using System.Collections.Generic;
using UnityEngine;

public class PlayerHitbox : MonoBehaviour
{
    [Header("Damage")]
    [SerializeField] private float lightDamage = 10f;
    [SerializeField] private float heavyDamage = 25f;
    [SerializeField] private float slamDamage = 30f;
    [SerializeField] private float kickDamage = 15f;

    [Header("Knockback")]
    [SerializeField] private float lightKnockback = 3f;
    [SerializeField] private float heavyKnockback = 7f;
    [SerializeField] private float slamKnockback = 10f;
    [SerializeField] private float kickKnockback = 5f;

    [Header("Camera Impact")]
    [SerializeField] private CameraFollow cameraFollow;

    [Header("Light Hit Shake")]
    [SerializeField] private float lightShakeDuration = 0.06f;
    [SerializeField] private float lightShakeStrength = 0.025f;

    [Header("Heavy Hit Shake")]
    [SerializeField] private float heavyShakeDuration = 0.12f;
    [SerializeField] private float heavyShakeStrength = 0.06f;

    [Header("Slam Hit Shake")]
    [SerializeField] private float slamShakeDuration = 0.15f;
    [SerializeField] private float slamShakeStrength = 0.08f;

    [Header("Kick Hit Shake")]
    [SerializeField] private float kickShakeDuration = 0.08f;
    [SerializeField] private float kickShakeStrength = 0.04f;

    private Collider2D hitbox;

    private PlayerAttackType currentAttack;

    private readonly HashSet<IDamageable> hitTargets =
        new HashSet<IDamageable>();

    private bool attackActive;
    private bool parryActive;

    private void Awake()
    {
        hitbox = GetComponent<Collider2D>();

        if (hitbox == null)
        {
            Debug.LogError(
                "PlayerHitbox requires a Collider2D."
            );

            return;
        }

        hitbox.isTrigger = true;
        hitbox.enabled = false;
    }

    // =========================================================
    // ATTACK HITBOX
    // =========================================================

    public void EnableAttack(
        PlayerAttackType attackType
    )
    {
        currentAttack = attackType;

        hitTargets.Clear();

        attackActive = true;

        if (hitbox != null)
        {
            hitbox.enabled = true;
        }
    }

    public void DisableAttack()
    {
        attackActive = false;

        hitTargets.Clear();

        if (hitbox != null)
        {
            hitbox.enabled = false;
        }
    }

    // =========================================================
    // PARRY
    // =========================================================

    public void BeginParry()
    {
        parryActive = true;
    }

    public void EndParry()
    {
        parryActive = false;
    }

    public bool IsParrying()
    {
        return parryActive;
    }

    // =========================================================
    // HIT DETECTION
    // =========================================================

    private void OnTriggerEnter2D(
        Collider2D other
    )
    {
        if (!attackActive)
            return;

        IDamageable damageable =
            other.GetComponentInParent<IDamageable>();

        if (damageable == null)
            return;

        // Prevent the same enemy from being hit
        // multiple times during one attack.
        if (hitTargets.Contains(damageable))
            return;

        hitTargets.Add(damageable);

        float damage = GetDamage();
        float knockback = GetKnockback();

        // Direction from player toward enemy.
        Vector2 direction =
            (other.transform.position -
             transform.root.position).normalized;

        if (direction.sqrMagnitude < 0.01f)
        {
            direction = Vector2.right;
        }

        // Apply damage.
        damageable.TakeDamage(
            damage,
            direction,
            knockback,
            currentAttack
        );

        // Camera impact happens only after
        // a valid target has been hit.
        TriggerHitShake();

        Debug.Log(
            $"{currentAttack} attack hit " +
            $"{other.gameObject.name}"
        );
    }

    // =========================================================
    // CAMERA SHAKE
    // =========================================================

    private void TriggerHitShake()
    {
        if (cameraFollow == null)
            return;

        float duration = 0f;
        float strength = 0f;

        switch (currentAttack)
        {
            case PlayerAttackType.Light:

                duration = lightShakeDuration;
                strength = lightShakeStrength;

                break;

            case PlayerAttackType.Heavy:

                duration = heavyShakeDuration;
                strength = heavyShakeStrength;

                break;

            case PlayerAttackType.Slam:

                duration = slamShakeDuration;
                strength = slamShakeStrength;

                break;

            case PlayerAttackType.Kick:

                duration = kickShakeDuration;
                strength = kickShakeStrength;

                break;

            default:
                return;
        }

        cameraFollow.Shake(
            duration,
            strength
        );
    }

    // =========================================================
    // DAMAGE
    // =========================================================

    private float GetDamage()
    {
        switch (currentAttack)
        {
            case PlayerAttackType.Light:
                return lightDamage;

            case PlayerAttackType.Heavy:
                return heavyDamage;

            case PlayerAttackType.Slam:
                return slamDamage;

            case PlayerAttackType.Kick:
                return kickDamage;

            default:
                return 0f;
        }
    }

    // =========================================================
    // KNOCKBACK
    // =========================================================

    private float GetKnockback()
    {
        switch (currentAttack)
        {
            case PlayerAttackType.Light:
                return lightKnockback;

            case PlayerAttackType.Heavy:
                return heavyKnockback;

            case PlayerAttackType.Slam:
                return slamKnockback;

            case PlayerAttackType.Kick:
                return kickKnockback;

            default:
                return 0f;
        }
    }
}