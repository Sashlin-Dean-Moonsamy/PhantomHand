using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCombat : MonoBehaviour
{
    [Header("Animator References")]
    public Animator characterAnimator;
    public Animator gunAnimator;

    [Header("Combo Settings")]
    [Tooltip("Time before combo resets if no next punch is pressed")]
    public float comboResetTime = 1.0f;

    [Tooltip("Minimum time between punches to prevent spam")]
    public float punchCooldown = 0.25f;

    private int comboStep = 0;
    private float lastPunchTime;
    private float lastPunchTriggerTime = -999f;

    private InputSystem_Actions inputActions;

    void Awake()
    {
        inputActions = new InputSystem_Actions();

        // Subscribe to punch input (replace .Player.Attack with your correct map/action)
        inputActions.Player.Attack.performed += OnPunch;
    }

    void OnEnable() => inputActions.Enable();
    void OnDisable() => inputActions.Disable();

    void Update()
    {
        // Reset combo if player waits too long
        if (Time.time - lastPunchTime > comboResetTime)
        {
            comboStep = 0;
        }
    }

    private void OnPunch(InputAction.CallbackContext context)
    {
        if (!context.performed) return;
        HandlePunch();
    }

    private void HandlePunch()
    {
        // Prevent rapid retrigger
        if (Time.time - lastPunchTriggerTime < punchCooldown)
            return;

        lastPunchTriggerTime = Time.time;
        lastPunchTime = Time.time;
        comboStep++;

        // Determine which punch to trigger
        if (comboStep == 1)
        {
            SetTriggerBoth("firstPunch");
        }
        else if (comboStep == 2)
        {
            SetTriggerBoth("secondPunch");
            comboStep = 0; // reset after 2nd punch
        }
    }

    private void SetTriggerBoth(string triggerName)
    {
        if (characterAnimator)
        {
            characterAnimator.ResetTrigger(triggerName); // clear any stale trigger
            characterAnimator.SetTrigger(triggerName);
        }

        if (gunAnimator)
        {
            gunAnimator.ResetTrigger(triggerName);
            gunAnimator.SetTrigger(triggerName);
        }
    }
}
