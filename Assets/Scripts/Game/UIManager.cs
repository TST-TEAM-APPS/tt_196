using UnityEngine;
using TMPro;
using UnityEngine.UI;
using DG.Tweening;
using System.Collections;
using System.Collections.Generic;

public class UIManager : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject gameplayPanel;
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private GameObject pausePanel;

    [Header("Main Menu UI")]
    [SerializeField] private TextMeshProUGUI highScoreText;
    [SerializeField] private TextMeshProUGUI totalCoinsText;
    [SerializeField] private Button playButton;
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button quitButton;

    [Header("Gameplay UI")]
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI coinsText;
    [SerializeField] private List<GameObject> lifeIcons;
    [SerializeField] private Button pauseButton;

    [Header("Pause Menu UI")]
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button restartButton;
    [SerializeField] private Button pauseSettingsButton;
    [SerializeField] private Button pauseMainMenuButton;

    [Header("Game Over UI")]
    [SerializeField] private TextMeshProUGUI finalScoreText;
    [SerializeField] private TextMeshProUGUI bestScoreText;
    [SerializeField] private TextMeshProUGUI coinsCollectedText;
    [SerializeField] private TextMeshProUGUI finalCoinsText;
    [SerializeField] private Button retryButton;
    [SerializeField] private Button mainMenuButton;

    [Header("Settings UI")]
    [SerializeField] private Slider musicVolumeSlider;
    [SerializeField] private Slider sfxVolumeSlider;
    [SerializeField] private Button backButton;

    [Header("Animation Settings")]
    [SerializeField] private float fadeTime = 0.3f;
    [SerializeField] private float scaleTime = 0.5f;

    [Header("Game Input Buttons")]
    [SerializeField] private Button leftButton;
    [SerializeField] private Button rightButton;
    [SerializeField] private Button jumpButton;
    [SerializeField] private Button slideButton;

    private GameObject lastActivePanel;
    private GameManager gameManager;
    private AudioManager audioManager;
    private bool isPaused = false;

    public void Initialize(GameManager manager, AudioManager audio)
    {
        gameManager = manager;
        audioManager = audio;
        gameManager.OnCoinsRemoved += UpdateTotalCoinsText;
        SetupButtons();

        // Инициализируем настройки звука при старте
        InitializeAudioSliders();
        Debug.Log("UIManager initialized");
    }

    private void SetupButtons()
    {
        // Main Menu Buttons
        if (playButton != null) playButton.onClick.AddListener(OnPlayButtonClicked);
        if (settingsButton != null) settingsButton.onClick.AddListener(() => ShowPanel(settingsPanel));
        if (quitButton != null) quitButton.onClick.AddListener(OnQuitButtonClicked);

        // Gameplay Buttons
        if (pauseButton != null) pauseButton.onClick.AddListener(PauseGame);

        // Pause Menu Buttons
        if (resumeButton != null) resumeButton.onClick.AddListener(ResumeGame);
        if (restartButton != null) restartButton.onClick.AddListener(OnRestartButtonClicked);
        if (pauseSettingsButton != null) pauseSettingsButton.onClick.AddListener(() => ShowPanel(settingsPanel));
        if (pauseMainMenuButton != null) pauseMainMenuButton.onClick.AddListener(OnMainMenuButtonClicked);

        // Game Input Buttons
        if (leftButton != null) leftButton.onClick.AddListener(() => MovePlayer(-1));
        if (rightButton != null) rightButton.onClick.AddListener(() => MovePlayer(1));
        if (jumpButton != null) jumpButton.onClick.AddListener(PlayerJump);
        if (slideButton != null) slideButton.onClick.AddListener(PlayerSlide);

        // Game Over Buttons
        if (retryButton != null) retryButton.onClick.AddListener(OnRestartButtonClicked);
        if (mainMenuButton != null) mainMenuButton.onClick.AddListener(OnMainMenuButtonClicked);

        // Settings Button
        if (backButton != null) backButton.onClick.AddListener(OnBackButtonClicked);

        // Audio Sliders
        if (musicVolumeSlider != null) musicVolumeSlider.onValueChanged.AddListener(SetMusicVolume);
        if (sfxVolumeSlider != null) sfxVolumeSlider.onValueChanged.AddListener(SetSFXVolume);
    }

    private void InitializeAudioSliders()
    {
        if (audioManager == null)
            return;

        // Получаем текущие значения громкости из PlayerPrefs
        float currentMusicVolume = PlayerPrefs.GetFloat("MusicVolume", 0.5f);
        float currentSFXVolume = PlayerPrefs.GetFloat("SFXVolume", 1.0f);

        // Устанавливаем значения ползунков
        if (musicVolumeSlider != null)
        {
            musicVolumeSlider.value = currentMusicVolume;
        }

        if (sfxVolumeSlider != null)
        {
            sfxVolumeSlider.value = currentSFXVolume;
        }

        Debug.Log($"Audio sliders initialized: Music={currentMusicVolume}, SFX={currentSFXVolume}");
    }

    public void ShowMainMenu()
    {
        isPaused = false;
        Time.timeScale = 1f;

        ShowPanel(mainMenuPanel);
        lastActivePanel = mainMenuPanel;

        if (highScoreText != null)
        {
            highScoreText.text = PlayerPrefs.GetInt("HighScore", 0).ToString();
        }

        if (totalCoinsText != null)
        {
            totalCoinsText.text = PlayerPrefs.GetInt("TotalCoins", 0).ToString();
        }
    }

    public void ShowGameplayUI()
    {
        // Ensure game is not paused when showing gameplay UI
        isPaused = false;
        Time.timeScale = 1f;

        // Hide all panels first to ensure clean state
        HideAllPanels();

        // Show and animate the gameplay panel
        ShowPanel(gameplayPanel);
        lastActivePanel = gameplayPanel;

        Debug.Log("Showing gameplay UI, resumed game flow");
    }

    public void ShowGameOverUI()
    {
        isPaused = false;
        Time.timeScale = 1f;

        ShowPanel(gameOverPanel);
        lastActivePanel = gameOverPanel;
    }

    public void PauseGame()
    {
        if (isPaused) return;

        isPaused = true;
        Time.timeScale = 0f; // Freeze the game

        ShowPanel(pausePanel);
        lastActivePanel = pausePanel;

        if (audioManager != null)
        {
            audioManager.PlaySound("MenuClick");
        }

        Debug.Log("Game paused");
    }

    private void OnRestartButtonClicked()
    {
        // Ensure we're not in a paused state
        isPaused = false;
        Time.timeScale = 1f;

        // Hide the pause panel if it's active
        if (pausePanel != null && pausePanel.activeSelf)
        {
            pausePanel.SetActive(false);
        }

        // Hide the game over panel if it's active
        if (gameOverPanel != null && gameOverPanel.activeSelf)
        {
            gameOverPanel.SetActive(false);
        }

        if (gameManager != null)
        {
            gameManager.RestartGame();
        }

        if (audioManager != null)
        {
            audioManager.PlaySound("ButtonClick");
        }

        Debug.Log("Restart button clicked, game restarting");
    }

    public void ResumeGame()
    {
        if (!isPaused) return;

        isPaused = false;
        Time.timeScale = 1f; // Resume normal time flow

        ShowPanel(gameplayPanel);
        lastActivePanel = gameplayPanel;

        if (audioManager != null)
        {
            audioManager.PlaySound("MenuClick");
        }

        Debug.Log("Game resumed");
    }

    private void ShowPanel(GameObject panel)
    {
        if (panel == null)
            return;

        // Hide all panels
        HideAllPanels();

        // Show the requested panel
        panel.SetActive(true);

        // Если это панель настроек, обновим ползунки звука
        if (panel == settingsPanel)
        {
            InitializeAudioSliders();
        }

        // Animate panel appearance
        panel.transform.localScale = Vector3.zero;
        panel.transform.DOScale(Vector3.one, scaleTime).SetEase(Ease.OutBack).SetUpdate(true);

        CanvasGroup canvasGroup = panel.GetComponent<CanvasGroup>();
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0;
            canvasGroup.DOFade(1, fadeTime).SetUpdate(true);
        }

        // Animate buttons
        AnimateButtons(panel);

        Debug.Log($"Showing panel: {panel.name}");
    }

    private void HideAllPanels()
    {
        GameObject[] panels = { mainMenuPanel, gameplayPanel, gameOverPanel, settingsPanel, pausePanel };

        foreach (GameObject panel in panels)
        {
            if (panel != null)
            {
                panel.SetActive(false);
            }
        }
    }

    private void AnimateButtons(GameObject panel)
    {
        Button[] buttons = panel.GetComponentsInChildren<Button>();

        for (int i = 0; i < buttons.Length; i++)
        {
            if (buttons[i] != null)
            {
                buttons[i].transform.localScale = Vector3.zero;
                buttons[i].transform.DOScale(1f, 0.3f)
                    .SetEase(Ease.OutBack)
                    .SetDelay(0.1f + (i * 0.1f))
                    .SetUpdate(true);
            }
        }
    }

    private void OnPlayButtonClicked()
    {
        isPaused = false;
        Time.timeScale = 1f;

        if (gameManager != null)
        {
            gameManager.StartGame();
        }

        if (audioManager != null)
        {
            audioManager.PlaySound("ButtonClick");
        }
    }

    private void OnMainMenuButtonClicked()
    {
        isPaused = false;
        Time.timeScale = 1f;

        if (gameManager != null)
        {
            gameManager.ReturnToMainMenu();
        }

        if (audioManager != null)
        {
            audioManager.PlaySound("ButtonClick");
        }
    }

    private void OnBackButtonClicked()
    {
        // If we came from pause menu, go back there
        if (lastActivePanel == pausePanel)
        {
            ShowPanel(pausePanel);
        }
        // If we came from main menu, go back there
        else if (lastActivePanel == mainMenuPanel)
        {
            ShowPanel(mainMenuPanel);
        }
        // Otherwise default to last active panel or main menu
        else if (lastActivePanel != null)
        {
            ShowPanel(lastActivePanel);
        }
        else
        {
            ShowPanel(mainMenuPanel);
        }

        if (audioManager != null)
        {
            audioManager.PlaySound("ButtonClick");
        }
    }

    private void OnQuitButtonClicked()
    {
        if (gameManager != null)
        {
            gameManager.QuitGame();
        }
    }

    private void SetMusicVolume(float volume)
    {
        if (audioManager != null)
        {
            audioManager.SetMusicVolume(volume);
        }

        PlayerPrefs.SetFloat("MusicVolume", volume);
        PlayerPrefs.Save();
    }

    private void SetSFXVolume(float volume)
    {
        if (audioManager != null)
        {
            audioManager.SetSFXVolume(volume);
        }

        PlayerPrefs.SetFloat("SFXVolume", volume);
        PlayerPrefs.Save();
    }

    public void UpdateScoreText(int score)
    {
        if (scoreText != null)
        {
            scoreText.text = score.ToString();

            scoreText.transform.DOScale(Vector3.one, 0f);
            scoreText.transform.DOPunchScale(Vector3.one * 0.2f, 0.2f, 1, 0.5f);
        }
    }

    public void UpdateCoinsText(int coins)
    {
        if (coinsText != null)
        {
            coinsText.text = coins.ToString();

            coinsText.transform.DOScale(1f, 0f);
            // Add animation
            coinsText.transform.DOScale(1.2f, 0.2f).SetLoops(2, LoopType.Yoyo);
        }
    }

    public void UpdateHighScoreText(int highScore)
    {
        if (highScoreText != null)
        {
            highScoreText.text = highScore.ToString();
        }
    }

    public void UpdateTotalCoinsText(int totalCoins)
    {
        if (totalCoinsText != null)
        {
            totalCoinsText.text = totalCoins.ToString();
        }
    }

    public void UpdateFinalScore(int score, int highScore, int coins, int totalCoins)
    {
        if (finalScoreText != null)
        {
            finalScoreText.text = score.ToString();
        }

        if (bestScoreText != null)
        {
            bestScoreText.text = highScore.ToString();
        }

        if (coinsCollectedText != null)
        {
            coinsCollectedText.text = coins.ToString();
        }

        if (finalCoinsText != null)
        {
            finalCoinsText.text = totalCoins.ToString();
        }

        // Highlight if new high score
        if (score >= highScore && highScore > 0)
        {
            PlayNewHighScoreAnimation();
        }
    }

    private void PlayNewHighScoreAnimation()
    {
        if (bestScoreText != null)
        {
            bestScoreText.transform.DOScale(1.5f, 0.5f).SetLoops(2, LoopType.Yoyo);
            bestScoreText.DOColor(Color.yellow, 0.5f).SetLoops(2, LoopType.Yoyo);
        }
    }

    public void UpdateLivesUI(int lives)
    {
        if (lifeIcons == null)
            return;

        // Update life icons
        for (int i = 0; i < lifeIcons.Count; i++)
        {
            if (lifeIcons[i] != null)
            {
                bool shouldBeActive = i < lives;

                if (shouldBeActive != lifeIcons[i].activeSelf)
                {
                    if (shouldBeActive)
                    {
                        // Animate life gained
                        lifeIcons[i].SetActive(true);
                        lifeIcons[i].transform.localScale = Vector3.zero;
                        lifeIcons[i].transform.DOScale(1f, 0.3f).SetEase(Ease.OutBack);
                    }
                    else
                    {
                        // Animate life lost
                        int index = i; // Create a local copy for the lambda
                        lifeIcons[i].transform.DOScale(0f, 0.3f).SetEase(Ease.InBack)
                            .OnComplete(() => {
                                if (index < lifeIcons.Count && lifeIcons[index] != null)
                                    lifeIcons[index].SetActive(false);
                            });
                    }
                }
            }
        }
    }

    private void MovePlayer(int direction)
    {
        InputManager inputManager = FindObjectOfType<InputManager>();
        if (inputManager != null)
        {
            inputManager.TriggerSwipe(direction < 0 ? "left" : "right");
        }
    }

    private void PlayerJump()
    {
        InputManager inputManager = FindObjectOfType<InputManager>();
        if (inputManager != null)
        {
            inputManager.TriggerSwipe("up");
        }
    }

    private void PlayerSlide()
    {
        InputManager inputManager = FindObjectOfType<InputManager>();
        if (inputManager != null)
        {
            inputManager.TriggerSwipe("down");
        }
    }

    private void OnDestroy()
    {
        DOTween.Kill(transform);
    }

    private void Update()
    {
        // Handle back button/escape key
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            // If we're playing, pause the game
            if (gameplayPanel != null && gameplayPanel.activeSelf && !isPaused)
            {
                PauseGame();
            }
            // If we're paused, resume the game
            else if (pausePanel != null && pausePanel.activeSelf)
            {
                ResumeGame();
            }
            // If we're in settings, go back
            else if (settingsPanel != null && settingsPanel.activeSelf)
            {
                OnBackButtonClicked();
            }
            // If we're in game over, go to main menu
            else if (gameOverPanel != null && gameOverPanel.activeSelf)
            {
                OnMainMenuButtonClicked();
            }
            // If we're in main menu, quit the game
            else if (mainMenuPanel != null && mainMenuPanel.activeSelf)
            {
                OnQuitButtonClicked();
            }
        }
    }
}