using UnityEngine;
using DG.Tweening;

public class Collectible : MonoBehaviour, IPoolable
{
    [Header("Collectible Settings")]
    [SerializeField] private int value = 1;
    [SerializeField] private float rotationSpeed = 90f;
    [SerializeField] private float floatHeight = 0.2f;
    [SerializeField] private float floatSpeed = 1f;

    [Header("Visual Effects")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private ParticleSystem collectParticleSystem;

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
        SetupAnimation();
    }

    private void OnDisable()
    {
        StopAnimations();
    }

    public void OnObjectSpawn()
    {
        // Reset state when spawned from pool
        transform.localScale = Vector3.one;
        transform.rotation = Quaternion.identity;

        if (spriteRenderer != null)
        {
            spriteRenderer.color = Color.white;
        }

        startPosition = transform.position;
        SetupAnimation();
    }

    private void SetupAnimation()
    {
        // Create animation sequence
        animationSequence = DOTween.Sequence();

        // Rotate the collectible
        //animationSequence.Join(transform.DORotate(new Vector3(0, 0, 360), rotationSpeed / 60, RotateMode.FastBeyond360)
        //    .SetEase(Ease.Linear)
        //    .SetLoops(-1, LoopType.Restart));

        //// Float up and down
        //animationSequence.Join(transform.DOLocalMoveY(startPosition.y + floatHeight, floatSpeed)
        //    .SetEase(Ease.InOutSine)
        //    .SetLoops(-1, LoopType.Yoyo));

        // Pulse scale slightly
        //transform.DOScale(Vector3.one * 1.1f, floatSpeed * 1.5f).SetLink(gameObject).SetEase(Ease.InOutSine).SetLoops(-1, LoopType.Yoyo);
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
        // Check if the player collected this item
        if (other.CompareTag("Player"))
        {
            PlayerController player = other.GetComponent<PlayerController>();

            if (player != null)
            {
                // Call the collection method on the player
                player.CollectCoin();

                // Play collection effect
                PlayCollectEffect();

                // Return to pool
                ObjectPooler pooler = FindObjectOfType<ObjectPooler>();
                if (pooler != null)
                {
                    pooler.ReturnToPool(gameObject);
                }
                else
                {
                    gameObject.SetActive(false);
                }
            }
        }
    }

    private void PlayCollectEffect()
    {
        // Play particle effect if available
        if (collectParticleSystem != null)
        {
            // Detach particle system so it can finish playing after the collectible is disabled
            collectParticleSystem.transform.SetParent(null);
            collectParticleSystem.Play();

            // Clean up particles after they finish
            float duration = collectParticleSystem.main.duration + collectParticleSystem.main.startLifetime.constantMax;
            DOVirtual.DelayedCall(duration, () => {
                if (collectParticleSystem != null)
                {
                    Destroy(collectParticleSystem.gameObject);
                }
            });
        }
    }
}