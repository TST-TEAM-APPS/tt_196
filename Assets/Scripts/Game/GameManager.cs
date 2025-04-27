using UnityEngine;
using System;
using System.Collections;
using DG.Tweening;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    [Header("Game Components")]
    [SerializeField] private PlayerController playerPrefab;
    [SerializeField] private ObstacleSpawner obstacleSpawner;
    [SerializeField] private UIManager uiManager;
    [SerializeField] private AudioManager audioManager;
    [SerializeField] private CameraController cameraController;
    [SerializeField] private BackgroundManager backgroundManager;
    [SerializeField] private InputManager inputManager;
    [Header("Shop Integration")]
    [SerializeField] private Button shopButton;
    private ShopManager shopManager;


    [Header("Player Settings")]
    [SerializeField] private Vector3 playerStartPosition = new Vector3(0, -4f, 0);

    [Header("Game Settings")]
    [SerializeField] private float difficultyIncreaseInterval = 15f;
    [SerializeField] private float difficultyMultiplier = 1.1f;
    [SerializeField] private float gameSpeed = 1f;
    [SerializeField] private float maxGameSpeed = 2f;
    [SerializeField] private float gameSpeedIncreaseRate = 0.05f;

    // Game state
    private PlayerController player;
    private int currentScore = 0;
    private int coinsCollected = 0;
    private int highScore = 0;
    private int totalCoins = 0;
    private bool isGameRunning = false;
    private float nextDifficultyIncrease = 0f;
    private float nextSpeedIncrease = 0f;

    public bool IsGameRunning => isGameRunning;
    public float GameSpeed => gameSpeed;

    // Events
    public event Action OnGameStart;
    public event Action OnGameOver;
    public event Action<int> OnScoreChanged;
    public event Action<int> OnCoinsChanged;
    public event Action<int> OnLivesChanged;
    public event Action<float> OnDifficultyIncreased;
    public event Action<int> OnCoinsRemoved;

    private void Awake()
    {
        LoadGameData();
    }

    private void Start()
    {
        // Validate all references
        if (!ValidateReferences())
        {
            Debug.LogError("GameManager: Missing references! Please assign all required components in the inspector.");
            return;
        }

        // Initialize UI
        uiManager.Initialize(this, audioManager);

        // Show main menu
        uiManager.ShowMainMenu();
        uiManager.UpdateHighScoreText(highScore);
        uiManager.UpdateTotalCoinsText(totalCoins);
        shopManager = GetComponent<ShopManager>();
        if (shopManager == null)
        {
            shopManager = FindObjectOfType<ShopManager>();
            if (shopManager == null)
            {
                Debug.LogWarning("ShopManager not found in scene!");
            }
        }

        // Set up shop button if available
        if (shopButton != null)
        {
            shopButton.onClick.AddListener(OpenShop);
        }
        // Start menu music
        audioManager.PlayMenuMusic();
    }

    public void OpenShop()
    {
        if (shopManager != null)
        {
            shopManager.OpenShop();

            // Play sound
            if (audioManager != null)
            {
                audioManager.PlaySound("MenuClick");
            }
        }
    }

    private bool ValidateReferences()
    {
        if (playerPrefab == null) Debug.LogError("GameManager: PlayerController prefab is not assigned!");
        if (obstacleSpawner == null) Debug.LogError("GameManager: ObstacleSpawner is not assigned!");
        if (uiManager == null) Debug.LogError("GameManager: UIManager is not assigned!");
        if (audioManager == null) Debug.LogError("GameManager: AudioManager is not assigned!");
        if (cameraController == null) Debug.LogError("GameManager: CameraController is not assigned!");
        if (backgroundManager == null) Debug.LogError("GameManager: BackgroundManager is not assigned!");
        if (inputManager == null) Debug.LogError("GameManager: InputManager is not assigned!");

        return playerPrefab != null && obstacleSpawner != null && uiManager != null &&
               audioManager != null && cameraController != null && backgroundManager != null &&
               inputManager != null;
    }

    public void StartGame()
    {
        if (isGameRunning)
            return;

        // Reset game state
        isGameRunning = true;
        currentScore = 0;
        coinsCollected = 0;
        gameSpeed = 1f;
        Time.timeScale = 1f;

        // Create player
        if (player != null)
            Destroy(player.gameObject);

        player = Instantiate(playerPrefab, playerStartPosition, Quaternion.identity);
        player.Initialize(this, audioManager);

        // Setup game components
        obstacleSpawner.Initialize(player.transform, this);
        backgroundManager.Initialize(player.transform);
        cameraController.Initialize(player.transform);
        inputManager.Initialize(player);

        // Setup timers
        nextDifficultyIncrease = Time.time + difficultyIncreaseInterval;
        nextSpeedIncrease = Time.time + 10f;

        // Show UI and play audio
        uiManager.ShowGameplayUI();
        audioManager.PlayGameMusic();

        // Start game
        backgroundManager.StartScrolling();
        obstacleSpawner.StartSpawning();
        player.StartRunning();
        cameraController.PlayStartGameEffect();

        // Notify listeners
        OnGameStart?.Invoke();

        // Update UI
        uiManager.UpdateScoreText(0);
        uiManager.UpdateCoinsText(0);
        uiManager.UpdateLivesUI(player.Lives);

        Debug.Log("Game started");
    }

    public void GameOver()
    {
        if (!isGameRunning)
            return;

        isGameRunning = false;
        Time.timeScale = 1f;

        // Update scores
        if (currentScore > highScore)
        {
            highScore = currentScore;
        }

        totalCoins += coinsCollected;

        // Save data
        SaveGameData();

        // Notify listeners
        OnGameOver?.Invoke();

        // Stop game components
        obstacleSpawner.StopSpawning();
        backgroundManager.StopScrolling();
        player.StopRunning();
        cameraController.PlayGameOverEffect();

        // Show game over UI
        StartCoroutine(ShowGameOverAfterDelay(1f));

        Debug.Log("Game over");
    }

    public void AddCoins(int amount)
    {
        coinsCollected += amount;
        OnCoinsChanged?.Invoke(coinsCollected);
        uiManager.UpdateCoinsText(coinsCollected);
    }

    public void UpdateLives(int lives)
    {
        OnLivesChanged?.Invoke(lives);
        uiManager.UpdateLivesUI(lives);
    }

    private void Update()
    {
        if (!isGameRunning || player == null)
            return;

        UpdateScore();
        CheckDifficultyIncrease();
        CheckSpeedIncrease();
    }

    private void UpdateScore()
    {
        int newScore = Mathf.FloorToInt(player.DistanceTraveled);

        if (newScore != currentScore)
        {
            currentScore = newScore;
            OnScoreChanged?.Invoke(currentScore);
            uiManager.UpdateScoreText(currentScore);
        }
    }

    private void CheckDifficultyIncrease()
    {
        if (Time.time > nextDifficultyIncrease)
        {
            IncreaseDifficulty();
            nextDifficultyIncrease = Time.time + difficultyIncreaseInterval;
        }
    }

    private void CheckSpeedIncrease()
    {
        if (Time.time > nextSpeedIncrease)
        {
            if (gameSpeed < maxGameSpeed)
            {
                gameSpeed += gameSpeedIncreaseRate;
                OnDifficultyIncreased?.Invoke(gameSpeed);
            }

            nextSpeedIncrease = Time.time + 10f;
        }
    }

    private void IncreaseDifficulty()
    {
        obstacleSpawner.IncreaseDifficulty(difficultyMultiplier);
    }

    private IEnumerator ShowGameOverAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        uiManager.ShowGameOverUI();
        uiManager.UpdateFinalScore(currentScore, highScore, coinsCollected, totalCoins);
        audioManager.PlaySound("GameOver");
    }

    public void RestartGame()
    {
        // Make sure time scale is normal
        Time.timeScale = 1f;

        // Stop the current game if running
        if (isGameRunning && player != null)
        {
            // Kill DOTween animations
            DOTween.Kill(player.transform);
            player.StopRunning();
        }

        // Clear all obstacles and stop spawning
        obstacleSpawner.ClearObstacles();
        obstacleSpawner.StopSpawning();

        // Reset background and stop scrolling
        backgroundManager.StopScrolling();
        backgroundManager.ResetBackground();
        backgroundManager.ClearAllBackgrounds(); // Добавляем полную очистку фонов

        // Reset camera
        cameraController.ResetCamera();

        // Destroy the current player
        if (player != null)
        {
            Destroy(player.gameObject);
            player = null;
        }

        // Clear any remaining objects in pools to ensure a clean state
        ObjectPooler pooler = GetComponent<ObjectPooler>();
        if (pooler != null)
        {
            pooler.ClearAllPools();
        }

        // Make sure UI is in the correct state
        uiManager.ShowGameplayUI(); // Ensure we're showing gameplay UI and not pause menu

        // Reset game state variables
        isGameRunning = false; // Will be set to true in StartGame()
        currentScore = 0;
        coinsCollected = 0;
        gameSpeed = 1f;

        // Start a new game - this will create a new player and initialize everything
        StartGame();

        Debug.Log("Game restarted");
    }

    public void ReturnToMainMenu()
    {
        // Stop all game elements
        if (player != null)
        {
            DOTween.Kill(player.transform);
            Destroy(player.gameObject);
            player = null;
        }

        // Clear all obstacles and stop spawning
        obstacleSpawner.ClearObstacles();
        obstacleSpawner.StopSpawning();

        // Reset background and stop scrolling
        backgroundManager.StopScrolling();
        backgroundManager.ResetBackground();
        backgroundManager.ClearAllBackgrounds(); // Add this call to fully clear backgrounds

        // Reset camera
        cameraController.ResetCamera();

        // Clear any remaining objects in pools
        ObjectPooler pooler = GetComponent<ObjectPooler>();
        if (pooler != null)
        {
            pooler.ClearAllPools();
        }

        // Reset game state
        isGameRunning = false;
        currentScore = 0;
        coinsCollected = 0;
        gameSpeed = 1f;

        // Show main menu UI
        uiManager.ShowMainMenu();
        uiManager.UpdateHighScoreText(highScore);
        uiManager.UpdateTotalCoinsText(totalCoins);

        // Play menu music
        audioManager.PlayMenuMusic();

        Debug.Log("Returned to main menu");
    }

    private void SaveGameData()
    {
        GameData data = new GameData
        {
            HighScore = highScore,
            TotalCoins = totalCoins
        };

        string json = JsonUtility.ToJson(data);
        PlayerPrefs.SetString("GameData", json);
        PlayerPrefs.Save();

        Debug.Log("Game data saved");
    }

    public void MoneyCheat()
    {
        totalCoins += 100;
        SaveGameData();
    }

    public void RemoveTotalCoins(int value)
    {
        totalCoins -= value;
        OnCoinsRemoved(totalCoins);
        SaveGameData();
    }

    private void LoadGameData()
    {
        if (PlayerPrefs.HasKey("GameData"))
        {
            string json = PlayerPrefs.GetString("GameData");
            GameData data = JsonUtility.FromJson<GameData>(json);

            highScore = data.HighScore;
            totalCoins = data.TotalCoins;

            Debug.Log("Game data loaded");
        }
        else
        {
            highScore = 0;
            totalCoins = 0;

            Debug.Log("No saved game data found");
        }
    }

    public void QuitGame()
    {
        SaveGameData();
        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    // Public getters for UI manager
    public int GetHighScore() => highScore;
    public int GetTotalCoins() => totalCoins;
}

[Serializable]
public class GameData
{
    public int HighScore;
    public int TotalCoins;
}