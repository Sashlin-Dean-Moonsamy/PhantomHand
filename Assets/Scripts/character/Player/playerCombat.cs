using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCombat : MonoBehaviour
{
    [Header("Animators")]
    public Animator characterAnimator;
    public Animator gunAnimator;

    [Header("Cooldowns")]
    public float lightAttackCooldown = 0.25f;
    public float heavyAttackCooldown = 0.6f;

    private float lastLightTime;
    private float lastHeavyTime;

    private InputSystem_Actions inputActions;

    void Awake()
    {
        inputActions = new InputSystem_Actions();

        inputActions.Player.LightAttack.performed += _ => LightAttack();
        inputActions.Player.HeavyAttack.performed += _ => HeavyAttack();
    }

    void OnEnable() => inputActions.Enable();
    void OnDisable() => inputActions.Disable();

    // ---------------- LIGHT ----------------

    void LightAttack()
    {
        if (Time.time - lastLightTime < lightAttackCooldown)
            return;

        lastLightTime = Time.time;
        TriggerBoth("LightAttack");
    }

    // ---------------- HEAVY ----------------

    void HeavyAttack()
    {
        if (Time.time - lastHeavyTime < heavyAttackCooldown)
            return;

        lastHeavyTime = Time.time;
        TriggerBoth("HeavyAttack");
    }

    // ---------------- SHARED ----------------

    void TriggerBoth(string trigger)
    {
        if (characterAnimator)
        {
            characterAnimator.ResetTrigger("LightAttack");
            characterAnimator.ResetTrigger("HeavyAttack");
            characterAnimator.SetTrigger(trigger);
        }

        if (gunAnimator)
        {
            gunAnimator.ResetTrigger("LightAttack");
            gunAnimator.ResetTrigger("HeavyAttack");
            gunAnimator.SetTrigger(trigger);
        }
    }
}
