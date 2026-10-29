using System.Collections;
using UnityEngine;
using TMPro;

public class SharkBreachIntro : MonoBehaviour
{
    [Header("Player")]
    [SerializeField] private PlayerMovement player;

    [Header("Health UI")]
    [SerializeField] private PlayerHealthUI playerHealthUI;

    [Header("Camera")]
    [SerializeField] private Camera mainCamera;
    [SerializeField] private CameraFollow cameraFollow;
    [SerializeField] private Transform sharkCameraPoint;

    [Header("Camera Movement")]
    [SerializeField] private float cameraMoveDuration = 1.2f;
    [SerializeField] private float cameraReturnDuration = 1.2f;

    [Header("Sharks")]
    [SerializeField] private Animator shark1;
    [SerializeField] private Animator shark2;
    [SerializeField] private Animator shark3;

    [Header("Timing")]
    [SerializeField] private float delayBeforeFirstShark = 0.5f;
    [SerializeField] private float delayBetweenSharks = 0.6f;
    [SerializeField] private float delayAfterThirdShark = 0.5f;

    [Header("Camera Shake")]
    [SerializeField] private float shakeDuration = 0.25f;
    [SerializeField] private float shakeStrength = 0.15f;

    [Header("Encounter Title")]
    [SerializeField] private CanvasGroup encounterTitle;
    [SerializeField] private TMP_Text encounterText;

    [SerializeField] private string encounterName = "SHARKS";

    [SerializeField] private float titleFadeInTime = 0.4f;
    [SerializeField] private float titleDisplayTime = 1.5f;
    [SerializeField] private float titleFadeOutTime = 0.4f;

    private bool sequenceStarted;

    private Vector3 playerCameraPosition;

    private void Awake()
    {
        // Automatically find Main Camera if not assigned.
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }

        // Automatically find CameraFollow if not assigned.
        if (cameraFollow == null && mainCamera != null)
        {
            cameraFollow =
                mainCamera.GetComponent<CameraFollow>();
        }

        // Hide encounter title at startup.
        if (encounterTitle != null)
        {
            encounterTitle.alpha = 0f;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Prevent the sequence from playing multiple times.
        if (sequenceStarted)
            return;

        // Only the Player can activate the sequence.
        if (!other.CompareTag("Player"))
            return;

        sequenceStarted = true;

        StartCoroutine(PlaySequence());
    }

    private IEnumerator PlaySequence()
    {
        Debug.Log(
            "SHARK BREACH INTRO STARTED"
        );

        // =====================================================
        // VALIDATION
        // =====================================================

        if (mainCamera == null)
        {
            Debug.LogError(
                "SharkBreachIntro: Main Camera is not assigned."
            );

            UnlockPlayer();
            yield break;
        }

        if (sharkCameraPoint == null)
        {
            Debug.LogError(
                "SharkBreachIntro: Shark Camera Point is not assigned."
            );

            UnlockPlayer();
            yield break;
        }

        // =====================================================
        // LOCK PLAYER
        // =====================================================

        if (player != null)
        {
            player.SetMovementLocked(true);
        }

        // =====================================================
        // TAKE CAMERA CONTROL
        // =====================================================

        if (cameraFollow != null)
        {
            cameraFollow.SetCinematicMode(true);
        }

        // Save the camera's current position.
        playerCameraPosition =
            mainCamera.transform.position;

        // =====================================================
        // PAN TO SHARKS
        // =====================================================

        yield return StartCoroutine(
            MoveCameraTo(
                sharkCameraPoint.position,
                cameraMoveDuration
            )
        );

        yield return new WaitForSeconds(
            delayBeforeFirstShark
        );

        // =====================================================
        // SHARK 1
        // =====================================================

        if (shark1 != null)
        {
            Debug.Log(
                "SHARK 1 POPPING OUT"
            );

            shark1.SetTrigger("PopOut");

            yield return StartCoroutine(
                CameraShake()
            );
        }
        else
        {
            Debug.LogWarning(
                "SharkBreachIntro: " +
                "Shark 1 Animator is not assigned."
            );
        }

        yield return new WaitForSeconds(
            delayBetweenSharks
        );

        // =====================================================
        // SHARK 2
        // =====================================================

        if (shark2 != null)
        {
            Debug.Log(
                "SHARK 2 POPPING OUT"
            );

            shark2.SetTrigger("PopOut");

            yield return StartCoroutine(
                CameraShake()
            );
        }
        else
        {
            Debug.LogWarning(
                "SharkBreachIntro: " +
                "Shark 2 Animator is not assigned."
            );
        }

        yield return new WaitForSeconds(
            delayBetweenSharks
        );

        // =====================================================
        // SHARK 3
        // =====================================================

        if (shark3 != null)
        {
            Debug.Log(
                "SHARK 3 POPPING OUT"
            );

            shark3.SetTrigger("PopOut");

            yield return StartCoroutine(
                CameraShake()
            );
        }
        else
        {
            Debug.LogWarning(
                "SharkBreachIntro: " +
                "Shark 3 Animator is not assigned."
            );
        }

        yield return new WaitForSeconds(
            delayAfterThirdShark
        );

        // =====================================================
        // SHOW ENCOUNTER TITLE
        // =====================================================

        yield return StartCoroutine(
            ShowEncounterTitle()
        );

        // =====================================================
        // RETURN CAMERA TO PLAYER
        // =====================================================

        yield return StartCoroutine(
            MoveCameraTo(
                playerCameraPosition,
                cameraReturnDuration
            )
        );

        // =====================================================
        // RESTORE CAMERA FOLLOW
        // =====================================================

        if (cameraFollow != null)
        {
            cameraFollow.SetCinematicMode(false);
        }

        // =====================================================
        // SHOW HEALTH UI
        // =====================================================

        if (playerHealthUI != null)
        {
            playerHealthUI.ShowHealthUI();
        }
        else
        {
            Debug.LogWarning(
                "SharkBreachIntro: " +
                "Player Health UI is not assigned."
            );
        }

        // =====================================================
        // UNLOCK PLAYER
        // =====================================================

        UnlockPlayer();

        Debug.Log(
            "SHARK BREACH INTRO FINISHED"
        );
    }

    // =========================================================
    // PLAYER
    // =========================================================

    private void UnlockPlayer()
    {
        if (player != null)
        {
            player.SetMovementLocked(false);
        }
    }

    // =========================================================
    // CAMERA
    // =========================================================

    private void MoveCameraImmediately(
        Vector3 destination
    )
    {
        destination.z =
            mainCamera.transform.position.z;

        mainCamera.transform.position =
            destination;
    }

    private IEnumerator MoveCameraTo(
        Vector3 destination,
        float duration
    )
    {
        Vector3 startPosition =
            mainCamera.transform.position;

        // Keep the camera's existing Z.
        destination.z =
            startPosition.z;

        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;

            float progress =
                Mathf.Clamp01(
                    elapsed / duration
                );

            // Smooth cinematic movement.
            progress =
                Mathf.SmoothStep(
                    0f,
                    1f,
                    progress
                );

            mainCamera.transform.position =
                Vector3.Lerp(
                    startPosition,
                    destination,
                    progress
                );

            yield return null;
        }

        // Make sure the camera ends exactly
        // at the destination.
        mainCamera.transform.position =
            destination;
    }

    // =========================================================
    // CAMERA SHAKE
    // =========================================================

    private IEnumerator CameraShake()
    {
        if (mainCamera == null)
            yield break;

        Vector3 basePosition =
            mainCamera.transform.position;

        float elapsed = 0f;

        while (elapsed < shakeDuration)
        {
            elapsed += Time.deltaTime;

            Vector2 randomOffset =
                Random.insideUnitCircle *
                shakeStrength;

            mainCamera.transform.position =
                new Vector3(
                    basePosition.x +
                    randomOffset.x,

                    basePosition.y +
                    randomOffset.y,

                    basePosition.z
                );

            yield return null;
        }

        // Restore exact camera position.
        mainCamera.transform.position =
            basePosition;
    }

    // =========================================================
    // ENCOUNTER TITLE
    // =========================================================

    private IEnumerator ShowEncounterTitle()
    {
        if (encounterTitle == null)
        {
            Debug.LogWarning(
                "SharkBreachIntro: " +
                "Encounter Title CanvasGroup is not assigned."
            );

            yield break;
        }

        // Set the title text.
        if (encounterText != null)
        {
            encounterText.text =
                encounterName;
        }

        // =====================================================
        // FADE IN
        // =====================================================

        float elapsed = 0f;

        while (elapsed < titleFadeInTime)
        {
            elapsed += Time.deltaTime;

            float progress =
                Mathf.Clamp01(
                    elapsed /
                    titleFadeInTime
                );

            encounterTitle.alpha =
                Mathf.Lerp(
                    0f,
                    1f,
                    progress
                );

            yield return null;
        }

        encounterTitle.alpha = 1f;

        // =====================================================
        // DISPLAY
        // =====================================================

        yield return new WaitForSeconds(
            titleDisplayTime
        );

        // =====================================================
        // FADE OUT
        // =====================================================

        elapsed = 0f;

        while (elapsed < titleFadeOutTime)
        {
            elapsed += Time.deltaTime;

            float progress =
                Mathf.Clamp01(
                    elapsed /
                    titleFadeOutTime
                );

            encounterTitle.alpha =
                Mathf.Lerp(
                    1f,
                    0f,
                    progress
                );

            yield return null;
        }

        encounterTitle.alpha = 0f;
    }
}
