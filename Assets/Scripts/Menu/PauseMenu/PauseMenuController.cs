using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PauseMenuController : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject pauseMenu;
    [SerializeField] private RectTransform menuPanel;
    [SerializeField] private CanvasGroup pauseCanvasGroup;

    [Header("Settings UI")]
    [SerializeField] private GameObject pauseButtons;
    [SerializeField] private GameObject settingsPanel;

    [Header("Animation")]
    [SerializeField] private float slideDuration = 0.25f;
    [SerializeField] private float hiddenOffset = 650f;

    [Header("Scene Settings")]
    [SerializeField] private string mainMenuSceneName = "MainMenu";

    private InputSystem_Actions inputActions;

    private bool isPaused;
    private bool isAnimating;

    private Vector2 visiblePosition;
    private Vector2 hiddenPosition;

    private void Awake()
    {
        inputActions = new InputSystem_Actions();

        if (menuPanel == null)
        {
            Debug.LogError("Menu Panel has not been assigned.");
            enabled = false;
            return;
        }

        visiblePosition = menuPanel.anchoredPosition;
        hiddenPosition = visiblePosition + new Vector2(hiddenOffset, 0f);

        menuPanel.anchoredPosition = hiddenPosition;

        if (pauseCanvasGroup != null)
        {
            pauseCanvasGroup.alpha = 0f;
            pauseCanvasGroup.interactable = false;
            pauseCanvasGroup.blocksRaycasts = false;
        }

        if (settingsPanel != null)
        {
            settingsPanel.SetActive(false);
        }

        if (pauseButtons != null)
        {
            pauseButtons.SetActive(true);
        }

        if (pauseMenu != null)
        {
            pauseMenu.SetActive(false);
        }

        Time.timeScale = 1f;
    }

    private void OnEnable()
    {
        if (inputActions == null)
        {
            inputActions = new InputSystem_Actions();
        }

        inputActions.UI.Enable();
        inputActions.UI.Menu.performed += OnMenuPressed;
    }

    private void OnDisable()
    {
        if (inputActions == null)
            return;

        inputActions.UI.Menu.performed -= OnMenuPressed;
        inputActions.UI.Disable();

        Time.timeScale = 1f;
    }

    private void OnMenuPressed(InputAction.CallbackContext context)
    {
        if (isAnimating)
            return;

        if (settingsPanel != null && settingsPanel.activeSelf)
        {
            CloseSettings();
            return;
        }

        TogglePause();
    }

    public void TogglePause()
    {
        if (isPaused)
        {
            ResumeGame();
        }
        else
        {
            PauseGame();
        }
    }

    public void PauseGame()
    {
        if (isPaused || isAnimating)
            return;

        isPaused = true;

        if (settingsPanel != null)
        {
            settingsPanel.SetActive(false);
        }

        if (pauseButtons != null)
        {
            pauseButtons.SetActive(true);
        }

        pauseMenu.SetActive(true);

        pauseCanvasGroup.alpha = 1f;
        pauseCanvasGroup.interactable = false;
        pauseCanvasGroup.blocksRaycasts = true;

        Time.timeScale = 0f;

        StartCoroutine(
            SlideMenu(
                hiddenPosition,
                visiblePosition,
                true
            )
        );
    }

    public void ResumeGame()
    {
        if (!isPaused || isAnimating)
            return;

        pauseCanvasGroup.interactable = false;

        StartCoroutine(
            SlideMenu(
                visiblePosition,
                hiddenPosition,
                false
            )
        );
    }

    private IEnumerator SlideMenu(
        Vector2 startPosition,
        Vector2 endPosition,
        bool opening
    )
    {
        isAnimating = true;

        float elapsed = 0f;

        while (elapsed < slideDuration)
        {
            elapsed += Time.unscaledDeltaTime;

            float progress = Mathf.Clamp01(
                elapsed / slideDuration
            );

            float smoothProgress =
                progress * progress * (3f - 2f * progress);

            menuPanel.anchoredPosition = Vector2.Lerp(
                startPosition,
                endPosition,
                smoothProgress
            );

            yield return null;
        }

        menuPanel.anchoredPosition = endPosition;

        if (opening)
        {
            pauseCanvasGroup.interactable = true;
        }
        else
        {
            isPaused = false;

            pauseCanvasGroup.alpha = 0f;
            pauseCanvasGroup.interactable = false;
            pauseCanvasGroup.blocksRaycasts = false;

            pauseMenu.SetActive(false);

            Time.timeScale = 1f;
        }

        isAnimating = false;
    }

    public void OpenSettings()
    {
        if (!isPaused || isAnimating)
            return;

        if (pauseButtons != null)
        {
            pauseButtons.SetActive(false);
        }

        if (settingsPanel != null)
        {
            settingsPanel.SetActive(true);
        }
    }

    public void CloseSettings()
    {
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(false);
        }

        if (pauseButtons != null)
        {
            pauseButtons.SetActive(true);
        }
    }

    public void RestartLevel()
    {
        Time.timeScale = 1f;

        Scene currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currentScene.name);
    }

    public void ReturnToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(mainMenuSceneName);
    }

    public void QuitGame()
    {
        Time.timeScale = 1f;

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}