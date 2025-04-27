using UnityEngine;
using DG.Tweening;

public class Obstacle : MonoBehaviour, IPoolable
{
    public enum ObstacleType
    {
        Jumpable,   // Can be jumped over
        Slideable,  // Can slide under
        Avoidable   // Must move to different lane
    }

    [Header("Obstacle Settings")]
    [SerializeField] private ObstacleType obstacleType = ObstacleType.Jumpable;
    [SerializeField] private float movementDistance = 0f;
    [SerializeField] private float movementSpeed = 0f;
    [SerializeField] private float rotationSpeed = 0f;
    [SerializeField] private bool animateOnStart = true;

    [Header("Visual Effects")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private ParticleSystem collisionParticleSystem;

    private Vector3 startPosition;
    private Sequence animationSequence;

    private void Awake()
    {
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }
    }

    private void OnEnable()
    {
        startPosition = transform.position;

        if (animateOnStart)
        {
            SetupAnimation();
        }
    }

    private void OnDisable()
    {
        StopAnimations();
    }

    public void OnObjectSpawn()
    {
        // Reset object state when spawned from pool
        transform.localScale = Vector3.one;
        transform.rotation = Quaternion.identity;

        if (spriteRenderer != null)
        {
            spriteRenderer.color = Color.white;
        }

        startPosition = transform.position;

        if (animateOnStart)
        {
            SetupAnimation();
        }

        // Debug.Log($"Obstacle spawned: {gameObject.name}, Type: {obstacleType}");
    }

    private void SetupAnimation()
    {
        // Choose animation based on obstacle type
        switch (obstacleType)
        {
            case ObstacleType.Jumpable:
                SetupJumpableAnimation();
                break;

            case ObstacleType.Slideable:
                SetupSlideableAnimation();
                break;

            case ObstacleType.Avoidable:
                SetupAvoidableAnimation();
                break;
        }
    }

    private void SetupJumpableAnimation()
    {
        // Jumpable obstacles can scale up/down slightly
        if (rotationSpeed > 0f)
        {
            // Rotate the obstacle
            transform.DORotate(new Vector3(0, 0, 360), rotationSpeed, RotateMode.FastBeyond360)
                .SetEase(Ease.Linear)
                .SetLoops(-1, LoopType.Restart);
        }
        //transform.DOScale(new Vector3(1f, 1.1f, 1f), 0.5f)
        //    .SetLoops(-1, LoopType.Yoyo)
        //    .SetEase(Ease.InOutSine);
    }

    private void SetupSlideableAnimation()
    {
        // Slideable obstacles can move horizontally slightly
    }

    private void SetupAvoidableAnimation()
    {
        // Avoidable obstacles can rotate or scale

    }

    private void SetupMovingAnimation()
    {
        if (movementDistance <= 0f || movementSpeed <= 0f)
            return;

        // Create a movement sequence
        animationSequence = DOTween.Sequence();

        // Choose a random direction if one isn't specified
        int randomDirection = Random.Range(0, 2) == 0 ? -1 : 1;
        Vector3 endPosition = startPosition + new Vector3(movementDistance * randomDirection, 0f, 0f);

        // Move back and forth
        animationSequence.Append(transform.DOMove(endPosition, movementSpeed).SetEase(Ease.InOutSine));
        animationSequence.Append(transform.DOMove(startPosition, movementSpeed).SetEase(Ease.InOutSine));

        // Loop forever
        animationSequence.SetLoops(-1, LoopType.Restart);
    }

    private void StopAnimations()
    {
        // Stop all animations
        transform.DOKill();

        if (animationSequence != null)
        {
            animationSequence.Kill();
            animationSequence = null;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Check if we collided with the player
        if (other.CompareTag("Player"))
        {
            PlayerController player = other.GetComponent<PlayerController>();

            if (player != null)
            {
                // Different behavior based on obstacle type and player state
                bool isPassable = false;

                // Check if player can pass based on their state
                if ((obstacleType == ObstacleType.Jumpable && player.IsJumping) ||
                    (obstacleType == ObstacleType.Slideable && player.IsSliding))
                {
                    isPassable = true;
                }

                if (!isPassable)
                {
                    // Player hit the obstacle
                    player.TakeDamage();
                    PlayCollisionEffect();

                    Debug.Log($"Player hit obstacle of type {obstacleType}");
                }
                else
                {
                    // Player successfully avoided the obstacle
                    Debug.Log($"Player avoided obstacle of type {obstacleType}");
                }
            }
        }
    }

    private void PlayCollisionEffect()
    {
        // Shake and flash the obstacle
        transform.DOShakeScale(0.3f, 0.5f, 10, 90, false);

        if (spriteRenderer != null)
        {
            spriteRenderer.DOColor(Color.red, 0.1f).SetLoops(2, LoopType.Yoyo);
        }

        // Play particle effect if available
        if (collisionParticleSystem != null)
        {
            collisionParticleSystem.Play();
        }
    }

    // Expose the obstacle type for other scripts
    public ObstacleType Type => obstacleType;
}