using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHealthUI : MonoBehaviour
{
    [Header("Player")]
    [SerializeField] private PlayerHealth playerHealth;

    [Header("Health Bar")]
    [Tooltip("10 sprites ordered from 1 health to 10 health.")]
    [SerializeField] private Sprite[] healthSprites;

    [SerializeField] private Image healthImage;

    [Header("Final Blow")]
    [SerializeField] private Image finalBlowImage;

    [Header("Slide Animation")]
    [SerializeField] private float slideDuration = 0.5f;
    [SerializeField] private float slideDistance = 500f;

    [SerializeField] private AnimationCurve slideCurve =
        AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    [Header("Camera Shake")]
    [SerializeField] private CameraFollow cameraFollow;

    [SerializeField] private float shakeDuration = 0.18f;
    [SerializeField] private float shakeStrength = 0.08f;

    private bool hudVisible;

    private RectTransform healthRect;
    private Vector2 healthFinalPosition;

    private Coroutine slideCoroutine;

    private void Awake()
    {
        if (healthImage != null)
        {
            healthRect = healthImage.GetComponent<RectTransform>();
            healthFinalPosition = healthRect.anchoredPosition;
        }

        HideHealthUI();
    }

    private void Start()
    {
        if (playerHealth == null)
        {
            Debug.LogError(
                "PlayerHealthUI: Player Health is not assigned."
            );

            return;
        }

        playerHealth.OnHealthChanged.AddListener(UpdateHealthBar);
        playerHealth.OnDeath.AddListener(ShowFinalBlow);

        HideHealthUI();
    }

    private void OnDestroy()
    {
        if (playerHealth == null)
            return;

        playerHealth.OnHealthChanged.RemoveListener(UpdateHealthBar);
        playerHealth.OnDeath.RemoveListener(ShowFinalBlow);
    }

    public void ShowHealthUI()
    {
        if (hudVisible)
            return;

        hudVisible = true;

        if (finalBlowImage != null)
            finalBlowImage.gameObject.SetActive(false);

        if (healthImage != null)
        {
            healthImage.gameObject.SetActive(true);

            if (healthRect != null)
            {
                healthRect.anchoredPosition =
                    healthFinalPosition +
                    Vector2.left * slideDistance;
            }
        }

        if (playerHealth != null)
        {
            UpdateHealthBar(
                playerHealth.GetCurrentHealth(),
                playerHealth.GetMaxHealth()
            );
        }

        if (slideCoroutine != null)
            StopCoroutine(slideCoroutine);

        slideCoroutine = StartCoroutine(SlideHealthBar());

        Debug.Log("Player Health UI revealed.");
    }

    public void HideHealthUI()
    {
        hudVisible = false;

        if (slideCoroutine != null)
        {
            StopCoroutine(slideCoroutine);
            slideCoroutine = null;
        }

        if (healthImage != null)
            healthImage.gameObject.SetActive(false);

        if (finalBlowImage != null)
            finalBlowImage.gameObject.SetActive(false);
    }

    private IEnumerator SlideHealthBar()
    {
        if (healthRect == null)
            yield break;

        Vector2 startPosition =
            healthFinalPosition +
            Vector2.left * slideDistance;

        Vector2 endPosition =
            healthFinalPosition;

        float elapsed = 0f;

        while (elapsed < slideDuration)
        {
            elapsed += Time.deltaTime;

            float progress =
                Mathf.Clamp01(elapsed / slideDuration);

            float curvedProgress =
                slideCurve.Evaluate(progress);

            healthRect.anchoredPosition =
                Vector2.Lerp(
                    startPosition,
                    endPosition,
                    curvedProgress
                );

            yield return null;
        }

        healthRect.anchoredPosition = endPosition;

        // The health bar has landed on screen.
        // Now hit the camera with a small impact shake.
        if (cameraFollow != null)
        {
            cameraFollow.Shake(
                shakeDuration,
                shakeStrength
            );
        }

        slideCoroutine = null;
    }

    private void UpdateHealthBar(
        int currentHealth,
        int maxHealth
    )
    {
        if (!hudVisible)
            return;

        if (healthImage == null)
            return;

        if (healthSprites == null ||
            healthSprites.Length == 0)
        {
            Debug.LogWarning(
                "PlayerHealthUI: No health sprites assigned."
            );

            return;
        }

        if (currentHealth <= 0)
        {
            healthImage.gameObject.SetActive(false);
            return;
        }

        healthImage.gameObject.SetActive(true);

        int index = Mathf.Clamp(
            currentHealth - 1,
            0,
            healthSprites.Length - 1
        );

        healthImage.sprite = healthSprites[index];
    }

    private void ShowFinalBlow()
    {
        if (healthImage != null)
            healthImage.gameObject.SetActive(false);

        if (finalBlowImage != null)
            finalBlowImage.gameObject.SetActive(true);
    }
}