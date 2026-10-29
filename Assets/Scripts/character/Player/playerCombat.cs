using UnityEngine;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(PlayerMovement))]
public class PlayerCombat : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private PlayerHitbox playerHitbox;

    [Header("Weapon")]
    [SerializeField] private Animator weaponAnimator;

    [Header("Light Attack Combo")]
    [SerializeField] private string slapIndexParameter = "SlapIndex";
    [SerializeField] private string lightAttackTrigger = "LightAttack";
    [SerializeField, Min(1)] private int slapCount = 4;

    [Header("Combat Timing")]
    [SerializeField] private float actionCooldown = 0.1f;

    [Header("Attack Lock")]
    [SerializeField] private bool lockMovementDuringAttacks = true;

    private Animator playerAnimator;
    private InputSystem_Actions inputActions;

    private float lastActionTime;
    private int nextSlapIndex;

    private bool actionLocked;
    private bool dead;

    private int slapIndexHash;
    private int lightAttackHash;

    private void Awake()
    {
        playerAnimator = GetComponent<Animator>();

        if (playerMovement == null)
            playerMovement = GetComponent<PlayerMovement>();

        if (playerHitbox == null)
            playerHitbox = GetComponentInChildren<PlayerHitbox>();

        inputActions = new InputSystem_Actions();

        slapIndexHash = Animator.StringToHash(slapIndexParameter);
        lightAttackHash = Animator.StringToHash(lightAttackTrigger);

        if (weaponAnimator == null)
            Debug.LogWarning(
                "PlayerCombat: Weapon Animator is not assigned. " +
                "Player attacks will still work.");

        if (playerHitbox == null)
            Debug.LogWarning(
                "PlayerCombat: PlayerHitbox was not found.");

        if (!playerAnimator.HasParameterOfType(
                slapIndexHash, AnimatorControllerParameterType.Int))
        {
            Debug.LogWarning(
                "PlayerCombat: Add an Int parameter named '" +
                slapIndexParameter + "' to the player Animator.");
        }
    }

    private void OnEnable()
    {
        if (inputActions != null)
            inputActions.Enable();
    }

    private void OnDisable()
    {
        if (inputActions != null)
            inputActions.Disable();

        DisableHitbox();
        EndParry();
        RestoreAnimators();
        SetActionLocked(false);
    }

    private void Update()
    {
        if (dead || actionLocked)
            return;

        if (Time.time < lastActionTime + actionCooldown)
            return;

        HandleInput();
    }

    // =========================================================
    // INPUT
    // =========================================================

    private void HandleInput()
    {
        if (inputActions.Player.LightAttack.WasPressedThisFrame())
        {
            if (playerMovement.IsGrounded())
                LightAttack();
            else
                Slam();

            return;
        }

        if (inputActions.Player.HeavyAttack.WasPressedThisFrame())
        {
            HeavyAttack();
            return;
        }

        if (inputActions.Player.Kick.WasPressedThisFrame())
        {
            Kick();
            return;
        }

        if (inputActions.Player.Parry.WasPressedThisFrame())
        {
            Parry();
            return;
        }

        if (inputActions.Player.Dash.WasPressedThisFrame())
        {
            if (playerMovement.StartDash())
                lastActionTime = Time.time;

            return;
        }

        if (inputActions.Player.Grind.WasPressedThisFrame())
        {
            if (playerMovement.StartGrind())
                lastActionTime = Time.time;
        }
    }

    // =========================================================
    // LIGHT ATTACK COMBO
    // =========================================================

    private void LightAttack()
    {
        if (dead)
            return;

        if (slapCount < 1)
            return;

        DisableHitbox();
        EndParry();

        RestoreAnimators();

        // Select the current slap pose.
        playerAnimator.SetInteger(
            slapIndexHash, nextSlapIndex);

        // Request the matching Animator transition.
        playerAnimator.ResetTrigger(lightAttackHash);
        playerAnimator.SetTrigger(lightAttackHash);

        // Advance the combo and wrap back to the first slap.
        nextSlapIndex = (nextSlapIndex + 1) % slapCount;

        lastActionTime = Time.time;

        if (lockMovementDuringAttacks)
            SetActionLocked(true);
    }

    // =========================================================
    // OTHER ATTACKS
    // =========================================================

    private void HeavyAttack()
    {
        TriggerAttack("HeavyAttack", "HeavyAttack");
    }

    private void Kick()
    {
        TriggerAttack("Kick", "Kick");
    }

    private void Slam()
    {
        TriggerAttack("Slam", "Slam");
    }

    private void Parry()
    {
        TriggerAttack("Parry", "Parry");
    }

    private void TriggerAttack(
        string playerTrigger, string weaponTrigger)
    {
        if (dead)
            return;

        DisableHitbox();
        EndParry();

        RestoreAnimators();

        playerAnimator.ResetTrigger(playerTrigger);
        playerAnimator.SetTrigger(playerTrigger);

        if (weaponAnimator != null)
        {
            weaponAnimator.ResetTrigger(weaponTrigger);
            weaponAnimator.SetTrigger(weaponTrigger);
        }

        lastActionTime = Time.time;

        if (lockMovementDuringAttacks)
            SetActionLocked(true);
    }

    // =========================================================
    // ANIMATION EVENTS
    // =========================================================

    public void EnableLightHitbox()
    {
        if (playerHitbox != null)
            playerHitbox.EnableAttack(PlayerAttackType.Light);
    }

    public void EnableHeavyHitbox()
    {
        if (playerHitbox != null)
            playerHitbox.EnableAttack(PlayerAttackType.Heavy);
    }

    public void EnableSlamHitbox()
    {
        if (playerHitbox != null)
            playerHitbox.EnableAttack(PlayerAttackType.Slam);
    }

    public void EnableKickHitbox()
    {
        if (playerHitbox != null)
            playerHitbox.EnableAttack(PlayerAttackType.Kick);
    }

    public void DisableHitbox()
    {
        if (playerHitbox != null)
            playerHitbox.DisableAttack();
    }

    // =========================================================
    // PARRY EVENTS
    // =========================================================

    public void BeginParry()
    {
        if (playerHitbox != null)
            playerHitbox.BeginParry();
    }

    public void EndParry()
    {
        if (playerHitbox != null)
            playerHitbox.EndParry();
    }

    // =========================================================
    // END ACTION
    // =========================================================

    public void EndAction()
    {
        DisableHitbox();
        EndParry();

        RestoreAnimators();
        SetActionLocked(false);
    }

    public void SetActionLocked(bool locked)
    {
        actionLocked = locked;

        if (playerMovement != null)
            playerMovement.SetMovementLocked(locked);
    }

    private void RestoreAnimators()
    {
        if (playerAnimator != null)
            playerAnimator.enabled = true;

        if (weaponAnimator != null)
            weaponAnimator.enabled = true;
    }

    // =========================================================
    // DEATH
    // =========================================================

    public void Die()
    {
        if (dead)
            return;

        dead = true;

        DisableHitbox();
        EndParry();

        RestoreAnimators();
        SetActionLocked(true);

        playerAnimator.ResetTrigger("Death");
        playerAnimator.SetTrigger("Death");
    }

    // =========================================================
    // GETTERS
    // =========================================================

    public bool IsDead()
    {
        return dead;
    }

    public bool IsActionLocked()
    {
        return actionLocked;
    }
}

// =============================================================
// ANIMATOR PARAMETER VALIDATION
// =============================================================

public static class AnimatorParameterExtensions
{
    public static bool HasParameterOfType(
        this Animator animator,
        int parameterHash,
        AnimatorControllerParameterType type)
    {
        if (animator == null || animator.runtimeAnimatorController == null)
            return false;

        foreach (AnimatorControllerParameter parameter
                 in animator.parameters)
        {
            if (parameter.nameHash == parameterHash &&
                parameter.type == type)
            {
                return true;
            }
        }

        return false;
    }
}
