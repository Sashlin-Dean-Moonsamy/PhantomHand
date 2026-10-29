
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

    [Header("Slap Frame Sampling")]
    [Tooltip("Clip containing the player and weapon sprite animation.")]
    [SerializeField] private AnimationClip slapClip;

    [Tooltip("GameObject whose hierarchy contains the player sprite.")]
    [SerializeField] private GameObject playerAnimationTarget;

    [Tooltip("GameObject whose hierarchy contains the weapon sprite.")]
    [SerializeField] private GameObject weaponAnimationTarget;

    [Header("Combat Timing")]
    [SerializeField] private float actionCooldown = 0.1f;

    [Header("Attack Lock")]
    [SerializeField] private bool lockMovementDuringAttacks = true;

    private Animator playerAnimator;
    private InputSystem_Actions inputActions;

    private float lastActionTime;

    private bool actionLocked;
    private bool dead;

    private int slapFrameIndex;
    private int slapTotalFrames;
    private float slapFrameRate;

    private void Awake()
    {
        playerAnimator = GetComponent<Animator>();

        if (playerMovement == null)
            playerMovement = GetComponent<PlayerMovement>();

        if (playerHitbox == null)
            playerHitbox = GetComponentInChildren<PlayerHitbox>();

        if (playerAnimationTarget == null)
            playerAnimationTarget = gameObject;

        if (weaponAnimationTarget == null && weaponAnimator != null)
            weaponAnimationTarget = weaponAnimator.gameObject;

        inputActions = new InputSystem_Actions();

        if (weaponAnimator == null)
            Debug.LogWarning("PlayerCombat: Weapon Animator is not assigned.");

        if (playerHitbox == null)
            Debug.LogWarning("PlayerCombat: PlayerHitbox was not found.");

        if (slapClip != null)
        {
            slapFrameRate = slapClip.frameRate;

            slapTotalFrames = Mathf.Max(
                1,
                Mathf.RoundToInt(slapClip.length * slapFrameRate)
            );
        }
        else
        {
            Debug.LogWarning("PlayerCombat: Slap clip is not assigned.");
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

        RestoreAnimators();
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
    // LIGHT ATTACK
    // =========================================================

    private void LightAttack()
    {
        if (dead || slapClip == null)
            return;

        DisableHitbox();

        // Stop normal Animator playback before sampling.
        playerAnimator.enabled = false;

        if (weaponAnimator != null)
            weaponAnimator.enabled = false;

        // Sample both hierarchies at exactly the same time.
        PlayNextSlapFrame();

        lastActionTime = Time.time;

        if (lockMovementDuringAttacks)
            SetActionLocked(true);
    }

    private void PlayNextSlapFrame()
    {
        if (slapFrameRate <= 0f || slapTotalFrames <= 0)
            return;

        float sampleTime = slapFrameIndex / slapFrameRate;

        if (playerAnimationTarget != null)
        {
            slapClip.SampleAnimation(
                playerAnimationTarget,
                sampleTime
            );
        }

        if (weaponAnimationTarget != null)
        {
            slapClip.SampleAnimation(
                weaponAnimationTarget,
                sampleTime
            );
        }

        slapFrameIndex = (slapFrameIndex + 1) % slapTotalFrames;
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

    private void TriggerAttack(string playerTrigger, string weaponTrigger)
    {
        if (dead)
            return;

        DisableHitbox();

        // Restore regular animation playback for normal attacks.
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

    private void RestoreAnimators()
    {
        if (playerAnimator != null)
            playerAnimator.enabled = true;

        if (weaponAnimator != null)
            weaponAnimator.enabled = true;
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

        if (playerMovement != null)
            playerMovement.SetMovementLocked(true);

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
