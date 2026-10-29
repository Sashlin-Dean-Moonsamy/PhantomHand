using System.Collections;
using UnityEngine;

public class BreachAlertUI : MonoBehaviour
{
    [Header("Flash Settings")]
    [SerializeField] private float flashDuration = 0.15f;
    [SerializeField] private float flashPause = 0.1f;
    [SerializeField] private int flashCount = 3;

    [Header("Display Settings")]
    [SerializeField] private float displayDuration = 3f;
    [SerializeField] private float fadeDuration = 1f;

    private CanvasGroup canvasGroup;
    private Coroutine alertCoroutine;

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();

        if (canvasGroup == null)
        {
            Debug.LogError(
                "BreachAlertUI requires a CanvasGroup component."
            );

            return;
        }

        // Keep the GameObject ACTIVE.
        // We hide the UI using CanvasGroup instead.
        canvasGroup.alpha = 0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
    }

    public void TriggerBreachAlert()
    {
        if (canvasGroup == null)
            return;

        // If another alert is already playing,
        // stop it and restart the sequence.
        if (alertCoroutine != null)
        {
            StopCoroutine(alertCoroutine);
        }

        alertCoroutine = StartCoroutine(ShowAlert());
    }

    private IEnumerator ShowAlert()
    {
        // Make sure the alert starts invisible.
        canvasGroup.alpha = 0f;

        // ==========================================
        // FLASH
        // ==========================================

        for (int i = 0; i < flashCount; i++)
        {
            // Flash ON
            canvasGroup.alpha = 1f;

            yield return new WaitForSeconds(flashDuration);

            // Flash OFF
            canvasGroup.alpha = 0f;

            yield return new WaitForSeconds(flashPause);
        }

        // ==========================================
        // STAY ON SCREEN
        // ==========================================

        canvasGroup.alpha = 1f;

        yield return new WaitForSeconds(displayDuration);

        // ==========================================
        // FADE OUT
        // ==========================================

        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;

            float progress = elapsed / fadeDuration;

            canvasGroup.alpha = Mathf.Lerp(
                1f,
                0f,
                progress
            );

            yield return null;
        }

        // Make absolutely sure it is invisible.
        canvasGroup.alpha = 0f;

        alertCoroutine = null;
    }
}