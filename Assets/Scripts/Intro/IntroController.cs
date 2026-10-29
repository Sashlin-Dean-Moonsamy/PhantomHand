using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using System.Collections;

public class IntroController : MonoBehaviour
{
    [Header("UI")]
    public CanvasGroup titleUI;
    public CanvasGroup blackScreen;

    [Header("Comic")]
    public UnityEngine.UI.Image comicImage;
    public Sprite[] comicPages;
    public float minTimePerPage = 1.0f;
    public float autoAdvanceAfter = 6f;

    [Header("City")]
    public string citySceneName = "City";

    [Header("Camera")]
    public Camera mainCamera;
    public Transform restaurantTarget;

    [Header("Opening Fade")]
    public float titleFadeDelay = 0.3f;
    public float titleFadeInDuration = 1.5f;

    [Header("Intro Sequence")]
    public float titleFadeOutDuration = 0.5f;
    public float panDuration = 2.5f;
    public float pauseBeforeZoom = 0.75f;
    public float zoomDuration = 0.45f;
    public float zoomSize = 3f;
    public float blackFadeDuration = 0.4f;

    private bool started = false;
    private bool titleFinishedFadingIn = false;

  private void Start()
{
    if (titleUI != null)
    {
        titleUI.gameObject.SetActive(true);
        titleUI.alpha = 0f;
        titleUI.interactable = false;
        titleUI.blocksRaycasts = false;
    }

    if (blackScreen != null)
    {
        blackScreen.alpha = 0f;
        blackScreen.gameObject.SetActive(false);
    }

    if (comicImage != null)
    {
        // Keep the GameObject active, just hide the Image component.
        comicImage.gameObject.SetActive(true);
        comicImage.enabled = false;
        comicImage.color = Color.white;
    }

    StartCoroutine(FadeInTitle());
}

    private void Update()
    {
        // Prevent the player from skipping while the logo is still fading in.
        if (started || !titleFinishedFadingIn)
            return;

        bool keyboardPressed =
            Keyboard.current != null &&
            Keyboard.current.anyKey.wasPressedThisFrame;

        bool mousePressed =
            Mouse.current != null &&
            Mouse.current.leftButton.wasPressedThisFrame;

        bool gamepadPressed =
            Gamepad.current != null &&
            Gamepad.current.buttonSouth.wasPressedThisFrame;

        if (keyboardPressed || mousePressed || gamepadPressed)
        {
            started = true;
            StartCoroutine(PlayIntro());
        }
    }

    private IEnumerator FadeInTitle()
    {
        if (titleUI == null)
        {
            titleFinishedFadingIn = true;
            yield break;
        }

        yield return new WaitForSeconds(titleFadeDelay);

        float elapsed = 0f;

        while (elapsed < titleFadeInDuration)
        {
            elapsed += Time.deltaTime;

            float progress = Mathf.Clamp01(
                elapsed / titleFadeInDuration
            );

            // Smooth fade instead of a completely linear fade.
            progress = Mathf.SmoothStep(0f, 1f, progress);

            titleUI.alpha = progress;

            yield return null;
        }

        titleUI.alpha = 1f;
        titleUI.interactable = true;
        titleUI.blocksRaycasts = true;

        titleFinishedFadingIn = true;
    }

    private IEnumerator PlayIntro()
{
    yield return StartCoroutine(FadeOutTitle());

    yield return StartCoroutine(PanCameraToRestaurant());

    yield return new WaitForSeconds(pauseBeforeZoom);

    StartCoroutine(ZoomIntoRestaurant());

    yield return StartCoroutine(FadeToBlack());

    bool hasComicPages =
        comicImage != null &&
        comicPages != null &&
        comicPages.Length > 0;

    if (hasComicPages)
    {
        comicImage.enabled = true;
        comicImage.sprite = comicPages[0];
        comicImage.transform.SetAsLastSibling();
    }

    yield return StartCoroutine(FadeFromBlack());

    if (hasComicPages)
        yield return StartCoroutine(PlayComicPages());

    yield return StartCoroutine(FadeToBlack());

    if (hasComicPages)
    {
        comicImage.enabled = false;
    }

    SceneManager.LoadScene(citySceneName);
}

    private IEnumerator PlayComicPages()
    {
        int currentPage = 0;

        while (currentPage < comicPages.Length)
        {
            comicImage.sprite = comicPages[currentPage];

            float elapsed = 0f;

            while (true)
            {
                elapsed += Time.deltaTime;

                bool inputReceived =
                    elapsed >= minTimePerPage &&
                    ((Keyboard.current != null && Keyboard.current.anyKey.wasPressedThisFrame) ||
                     (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame) ||
                     (Gamepad.current != null && Gamepad.current.buttonSouth.wasPressedThisFrame));

                bool autoAdvance =
                    autoAdvanceAfter > 0f && elapsed >= autoAdvanceAfter;

                if (inputReceived || autoAdvance)
                    break;

                yield return null;
            }

            currentPage++;
        }
    }

    private IEnumerator FadeOutTitle()
    {
        if (titleUI == null)
            yield break;

        titleUI.interactable = false;
        titleUI.blocksRaycasts = false;

        float elapsed = 0f;
        float startAlpha = titleUI.alpha;

        while (elapsed < titleFadeOutDuration)
        {
            elapsed += Time.deltaTime;

            float progress = Mathf.Clamp01(
                elapsed / titleFadeOutDuration
            );

            titleUI.alpha = Mathf.Lerp(startAlpha, 0f, progress);

            yield return null;
        }

        titleUI.alpha = 0f;
        titleUI.gameObject.SetActive(false);
    }

    private IEnumerator PanCameraToRestaurant()
    {
        if (mainCamera == null || restaurantTarget == null)
            yield break;

        Vector3 startPos = mainCamera.transform.position;

        Vector3 endPos = new Vector3(
            restaurantTarget.position.x,
            restaurantTarget.position.y,
            startPos.z
        );

        float elapsed = 0f;

        while (elapsed < panDuration)
        {
            elapsed += Time.deltaTime;

            float progress = Mathf.Clamp01(
                elapsed / panDuration
            );

            progress = Mathf.SmoothStep(0f, 1f, progress);

            mainCamera.transform.position =
                Vector3.Lerp(startPos, endPos, progress);

            yield return null;
        }

        mainCamera.transform.position = endPos;
    }

    private IEnumerator ZoomIntoRestaurant()
    {
        if (mainCamera == null)
            yield break;

        float startSize = mainCamera.orthographicSize;
        float elapsed = 0f;

        while (elapsed < zoomDuration)
        {
            elapsed += Time.deltaTime;

            float progress = Mathf.Clamp01(
                elapsed / zoomDuration
            );

            mainCamera.orthographicSize =
                Mathf.Lerp(startSize, zoomSize, progress);

            yield return null;
        }

        mainCamera.orthographicSize = zoomSize;
    }

    private IEnumerator FadeFromBlack()
    {
        if (blackScreen == null)
            yield break;

        float elapsed = 0f;

        while (elapsed < blackFadeDuration)
        {
            elapsed += Time.deltaTime;

            float progress = Mathf.Clamp01(
                elapsed / blackFadeDuration
            );

            blackScreen.alpha = Mathf.Lerp(1f, 0f, progress);

            yield return null;
        }

        blackScreen.alpha = 0f;
        blackScreen.blocksRaycasts = false;
    }

    private IEnumerator FadeToBlack()
    {
        if (blackScreen == null)
        {
            yield return new WaitForSeconds(0.2f);
            yield break;
        }

        blackScreen.gameObject.SetActive(true);
        blackScreen.alpha = 0f;
        blackScreen.blocksRaycasts = true;

        float elapsed = 0f;

        while (elapsed < blackFadeDuration)
        {
            elapsed += Time.deltaTime;

            float progress = Mathf.Clamp01(
                elapsed / blackFadeDuration
            );

            blackScreen.alpha = progress;

            yield return null;
        }

        blackScreen.alpha = 1f;
    }
}