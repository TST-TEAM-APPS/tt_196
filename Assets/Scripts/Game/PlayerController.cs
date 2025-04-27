using UnityEngine;
using DG.Tweening;
using System;
using System.Collections;

public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float forwardSpeed = 10f;
    [SerializeField] private float maxForwardSpeed = 25f;
    [SerializeField] private float speedIncreaseRate = 0.1f;
    [SerializeField] private float laneChangeSpeed = 0.2f;
    [SerializeField] private float jumpDuration = 0.5f;
    [SerializeField] private float jumpBoostMultiplier = 1f;
    [SerializeField] private float slideDuration = 0.5f;
    [SerializeField] private float slideBoostMultiplier = 1.5f;

    [Header("Lane Settings")]
    [SerializeField] private float laneDistance = 1.5f;
    [SerializeField] private int totalLanes = 3;

    [Header("Player State")]
    [SerializeField] private int lives = 3;
    [SerializeField] private bool isOneHitMode = false;

    [Header("Visual Components")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Sprite defaultSprite;
    [SerializeField] private Sprite jumpSprite;
    [SerializeField] private Sprite slideSprite;
    [SerializeField] private Sprite hitSprite;

    private int currentLane = 1; // 0 = left, 1 = center, 2 = right
    private bool isJumping = false;
    private bool isSliding = false;
    private bool isInvulnerable = false;

    private Vector3 initialScale;
    private BoxCollider2D playerCollider;
    private Vector2 defaultColliderSize;
    private Vector2 defaultColliderOffset;
    private float distanceTraveled = 0f;
    private bool isGameRunning = false;
    private float currentForwardSpeedMultiplier = 1f;

    private GameManager gameManager;
    private AudioManager audioManager;

    // Events
    public event Action OnJump;
    public event Action OnSlide;
    public event Action OnHit;

    // Properties
    public int Lives => lives;
    public float DistanceTraveled => distanceTraveled;
    public float ForwardSpeed => forwardSpeed * currentForwardSpeedMultiplier;
    public bool IsJumping => isJumping;
    public bool IsSliding => isSliding;

    private void Awake()
    {
        // Cache components and initial values
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();

        playerCollider = GetComponent<BoxCollider2D>();

        if (playerCollider != null)
        {
            defaultColliderSize = playerCollider.size;
            defaultColliderOffset = playerCollider.offset;
        }

        initialScale = transform.localScale;
    }

    public void Initialize(GameManager manager, AudioManager audio)
    {
        gameManager = manager;
        audioManager = audio;

        // Set initial state
        ResetPlayer();

        Debug.Log("Player initialized");
    }

    public void StartRunning()
    {
        isGameRunning = true;

        // Start running animation
        PlayRunAnimation();

        // Start with temporary invulnerability
        StartCoroutine(StartInvulnerability());

        Debug.Log("Player started running");
    }

    public void StopRunning()
    {
        isGameRunning = false;
        DOTween.Kill(transform);
    }

    private IEnumerator StartInvulnerability()
    {
        isInvulnerable = true;

        // Visual feedback for invulnerability
        if (spriteRenderer != null)
        {
            spriteRenderer.DOFade(0.5f, 0.2f).SetLoops(10, LoopType.Yoyo).SetLink(gameObject);
        }

        yield return new WaitForSeconds(2f);

        isInvulnerable = false;

        if (spriteRenderer != null)
        {
            spriteRenderer.DOFade(1f, 0.2f).SetLink(gameObject);
        }
    }

    private void Update()
    {
        if (!isGameRunning) return;

        // Handle movement
        MoveForward();

        // Update distance traveled
        distanceTraveled += ForwardSpeed * Time.deltaTime;

        // Increase speed over time
        if (forwardSpeed < maxForwardSpeed)
        {
            forwardSpeed += speedIncreaseRate * Time.deltaTime;
        }
    }

    private void MoveForward()
    {
        // Move player forward (upward) with current speed multiplier
        transform.Translate(Vector3.up * ForwardSpeed * Time.deltaTime);
    }

    public void MoveLane(int direction)
    {
        if (!isGameRunning || isJumping || isSliding) return;

        int targetLane = Mathf.Clamp(currentLane + direction, 0, totalLanes - 1);

        if (targetLane == currentLane) return;

        currentLane = targetLane;
        float targetX = (currentLane - ((totalLanes - 1) / 2f)) * laneDistance;

        // Kill any running animations
        transform.DOKill(false);

        // Create lane change animation
        Sequence laneChangeSequence = DOTween.Sequence();
        laneChangeSequence.Append(transform.DOMoveX(targetX, laneChangeSpeed).SetEase(Ease.OutSine).SetLink(gameObject));

        // Add tilt effect
        float tiltAngle = direction * 15f;
        laneChangeSequence.Join(transform.DORotate(new Vector3(0, 0, tiltAngle), laneChangeSpeed / 2).SetEase(Ease.OutSine).SetLink(gameObject));
        laneChangeSequence.Append(transform.DORotate(Vector3.zero, laneChangeSpeed / 2).SetEase(Ease.InSine).SetLink(gameObject));

        // Play sound
        if (audioManager != null)
        {
            audioManager.PlaySound("LaneChange");
        }
    }

    public void Jump()
    {
        if (!isGameRunning || isJumping || isSliding) return;

        isJumping = true;
        OnJump?.Invoke();

        // Apply speed boost during jump
        currentForwardSpeedMultiplier = jumpBoostMultiplier;

        // Set jump sprite
        if (spriteRenderer != null && jumpSprite != null)
        {
            spriteRenderer.sprite = jumpSprite;
        }

        if (playerCollider != null)
            {
                // Move collider up to match jump animation
                playerCollider.offset = new Vector2(defaultColliderOffset.x, defaultColliderOffset.y + 0.5f);
            }

        // Kill any running animations
        transform.DOKill(false);

        // Create a simple scale animation for jump
        Sequence jumpSequence = DOTween.Sequence();
        Vector3 jumpScale = new Vector3(initialScale.x * 1.2f, initialScale.y * 1.2f, initialScale.z);
        // Scale up quickly
        jumpSequence.Append(transform.DOScale(jumpScale, jumpDuration * 0.5f).SetEase(Ease.OutQuad).SetLink(gameObject));
        jumpSequence.Append(transform.DOScale(initialScale, jumpDuration * 0.5f).SetEase(Ease.InOutQuad).SetLink(gameObject));


        // Return to default state after jump
        jumpSequence.OnComplete(() => {
            isJumping = false;

            // Reset speed multiplier
            currentForwardSpeedMultiplier = 1f;

            if (spriteRenderer != null && defaultSprite != null && !isSliding)
            {
                spriteRenderer.sprite = defaultSprite;
            }

            // Reset collider
            if (playerCollider != null)
            {
                playerCollider.offset = defaultColliderOffset;
            }
        });

        // Play sound
        if (audioManager != null)
        {
            audioManager.PlaySound("Jump");
        }
    }

    public void Slide()
    {
        if (!isGameRunning || isJumping || isSliding) return;

        isSliding = true;
        OnSlide?.Invoke();

        // Apply speed boost during slide
        currentForwardSpeedMultiplier = slideBoostMultiplier;

        // Set slide sprite
        if (spriteRenderer != null && slideSprite != null)
        {
            spriteRenderer.sprite = slideSprite;
        }

        // Adjust collider for slide
        if (playerCollider != null)
        {
            playerCollider.size = new Vector2(defaultColliderSize.x, defaultColliderSize.y * 0.5f);
            playerCollider.offset = new Vector2(defaultColliderOffset.x, defaultColliderOffset.y - 0.5f);
        }

        // Create slide animation
        transform.DOKill(false);
        Sequence slideSequence = DOTween.Sequence();

        // Scale for slide
        Vector3 slideScale = new Vector3(initialScale.x * 1.2f, initialScale.y * 0.5f, initialScale.z);
        slideSequence.Append(transform.DOScale(slideScale, slideDuration * 0.5f).SetEase(Ease.OutQuad).SetLink(gameObject));
        slideSequence.Append(transform.DOScale(initialScale, slideDuration * 0.5f).SetEase(Ease.InOutQuad).SetLink(gameObject));

        // Return to default state after slide
        slideSequence.OnComplete(() => {
            isSliding = false;

            // Reset speed multiplier
            currentForwardSpeedMultiplier = 1f;

            if (spriteRenderer != null && defaultSprite != null && !isJumping)
            {
                spriteRenderer.sprite = defaultSprite;
            }

            // Reset collider
            if (playerCollider != null)
            {
                playerCollider.size = defaultColliderSize;
                playerCollider.offset = defaultColliderOffset;
            }
        });

        // Play sound
        if (audioManager != null)
        {
            audioManager.PlaySound("Slide");
        }
    }

    public void TakeDamage()
    {
        if (isInvulnerable) return;

        OnHit?.Invoke();

        if (isOneHitMode || lives <= 1)
        {
            lives = 0;
            GameOver();
        }
        else
        {
            lives--;

            if (gameManager != null)
            {
                gameManager.UpdateLives(lives);
            }

            // Play hit animation
            PlayHitAnimation();

            // Temporary invulnerability
            StartCoroutine(InvulnerabilityCoroutine());

            // Play sound
            if (audioManager != null)
            {
                audioManager.PlaySound("Hit");
            }
        }
    }

    private void PlayHitAnimation()
    {
        // Set hit sprite temporarily
        if (spriteRenderer != null && hitSprite != null)
        {
            spriteRenderer.sprite = hitSprite;

            DOVirtual.DelayedCall(0.3f, () => {
                if (spriteRenderer != null && defaultSprite != null && !isJumping && !isSliding)
                {
                    spriteRenderer.sprite = defaultSprite;
                }
            });
        }

        // Shake player
        transform.DOShakePosition(0.3f, 0.3f, 10, 90, false);

        // Flash red
        if (spriteRenderer != null)
        {
            spriteRenderer.DOColor(Color.red, 0.1f).SetLoops(2, LoopType.Yoyo).SetLink(gameObject);
        }
    }

    private IEnumerator InvulnerabilityCoroutine()
    {
        isInvulnerable = true;
        float invulnerabilityDuration = 2f;

        // Blinking effect
        if (spriteRenderer != null)
        {
            float blinkInterval = 0.2f;
            int blinkCount = Mathf.FloorToInt(invulnerabilityDuration / blinkInterval);

            for (int i = 0; i < blinkCount; i++)
            {
                spriteRenderer.color = new Color(1, 1, 1, 0.5f);
                yield return new WaitForSeconds(blinkInterval / 2);
                spriteRenderer.color = Color.white;
                yield return new WaitForSeconds(blinkInterval / 2);
            }
        }
        else
        {
            yield return new WaitForSeconds(invulnerabilityDuration);
        }

        isInvulnerable = false;
    }

    private void PlayRunAnimation()
    {
        // Simple breathing animation for running
        Sequence runSequence = DOTween.Sequence();
        runSequence.Append(transform.DOScaleY(initialScale.y * 1.05f, 0.3f).SetEase(Ease.InOutSine).SetLink(gameObject));
        runSequence.Append(transform.DOScaleY(initialScale.y * 0.95f, 0.3f).SetEase(Ease.InOutSine).SetLink(gameObject));
        runSequence.SetLoops(-1, LoopType.Yoyo).SetLink(gameObject);
    }

    private void GameOver()
    {
        // Dramatic game over animation
        Sequence deathSequence = DOTween.Sequence();

        // Rotate and fade out
        deathSequence.Append(transform.DORotate(new Vector3(0, 0, 180), 0.5f, RotateMode.FastBeyond360).SetEase(Ease.OutQuad).SetLink(gameObject));

        if (spriteRenderer != null)
        {
            deathSequence.Join(spriteRenderer.DOFade(0, 0.5f).SetDelay(0.3f).SetLink(gameObject));
        }

        deathSequence.OnComplete(() => {
            if (gameManager != null)
            {
                gameManager.GameOver();
            }
        });

        // Play sound
        if (audioManager != null)
        {
            audioManager.PlaySound("GameOver");
        }
    }

    public void ResetPlayer()
    {
        DOTween.Kill(transform);

        transform.position = Vector3.zero;
        transform.rotation = Quaternion.identity;
        transform.localScale = initialScale;

        currentLane = (totalLanes - 1) / 2; // Center lane
        lives = isOneHitMode ? 1 : 3;
        forwardSpeed = 3f;
        currentForwardSpeedMultiplier = 1f;
        distanceTraveled = 0f;
        isJumping = false;
        isSliding = false;
        isInvulnerable = false;
        isGameRunning = false;

        // Reset visual
        if (spriteRenderer != null)
        {
            spriteRenderer.color = Color.white;

            if (defaultSprite != null)
            {
                spriteRenderer.sprite = defaultSprite;
            }
        }

        // Reset collider
        if (playerCollider != null)
        {
            playerCollider.size = defaultColliderSize;
            playerCollider.offset = defaultColliderOffset;
        }
    }

    public void CollectCoin()
    {
        // Visual feedback
        //transform.DOPunchScale(new Vector3(0.2f, 0.2f, 0.2f), 0.2f, 1, 0.5f);

        // Update score
        if (gameManager != null)
        {
            gameManager.AddCoins(1);
        }

        // Play sound
        if (audioManager != null)
        {
            audioManager.PlaySound("Coin");
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Log collision for debugging
        Debug.Log("Player collided with: " + other.gameObject.name);
    }
}