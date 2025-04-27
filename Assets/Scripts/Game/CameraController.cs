using UnityEngine;
using DG.Tweening;

public class CameraController : MonoBehaviour
{
    [Header("Follow Settings")]
    [SerializeField] private float smoothSpeed = 0.125f;
    [SerializeField] private float lookAheadDistance = 2f;
    [SerializeField] private bool lockX = true;
    [SerializeField] private Vector3 offset = new Vector3(0, 0, -10);

    [Header("Camera Effects")]
    [SerializeField] private float hitShakeIntensity = 0.5f;
    [SerializeField] private float hitShakeDuration = 0.3f;
    [SerializeField] private float gameStartZoomOutDuration = 1.5f;
    [SerializeField] private float gameOverZoomInDuration = 1.0f;

    private Vector3 initialPosition;
    private float initialSize;
    private Camera mainCamera;
    private Transform targetTransform;
    private PlayerController playerController;

    private void Awake()
    {
        mainCamera = GetComponent<Camera>();
        initialSize = mainCamera != null ? mainCamera.orthographicSize : 5f;
        initialPosition = transform.position;
    }

    public void Initialize(Transform target)
    {
        targetTransform = target;

        if (target != null)
        {
            playerController = target.GetComponent<PlayerController>();

            if (playerController != null)
            {
                playerController.OnJump += PlayJumpEffect;
                playerController.OnHit += PlayHitShakeEffect;
            }
        }

        Debug.Log("CameraController initialized with target: " + (targetTransform != null));
    }

    private void OnDestroy()
    {
        if (playerController != null)
        {
            playerController.OnJump -= PlayJumpEffect;
            playerController.OnHit -= PlayHitShakeEffect;
        }

        DOTween.Kill(transform);
        if (mainCamera != null)
        {
            DOTween.Kill(mainCamera);
        }
    }

    private void LateUpdate()
    {
        // If no target, don't follow anything
        if (targetTransform == null)
            return;

        // Calculate the target position with look ahead distance
        Vector3 desiredPosition = new Vector3(
            lockX ? initialPosition.x : targetTransform.position.x + offset.x,
            targetTransform.position.y + lookAheadDistance + offset.y,
            offset.z
        );

        // Smoothly follow the target
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
        transform.position = smoothedPosition;
    }

    public void PlayStartGameEffect()
    {
        transform.DOKill();

        if (mainCamera != null)
        {
            // Start with a close zoom and zoom out for a dynamic effect
            float startSize = initialSize * 0.7f;
            mainCamera.orthographicSize = startSize;
            mainCamera.DOOrthoSize(initialSize, gameStartZoomOutDuration).SetEase(Ease.OutBack);
        }

        // Start slightly rotated and straighten up
        //transform.rotation = Quaternion.Euler(0, 0, 5);
        //transform.DORotate(Vector3.zero, gameStartZoomOutDuration).SetEase(Ease.OutBack);

        Debug.Log("Played camera start game effect");
    }

    public void PlayGameOverEffect()
    {
        transform.DOKill();

        // Shake the camera for a dramatic effect
        transform.DOShakePosition(0.5f, 0.3f, 10, 90, false, true);

        if (mainCamera != null)
        {
            // Zoom in on the player when game over
            mainCamera.DOOrthoSize(initialSize * 0.8f, gameOverZoomInDuration).SetEase(Ease.InOutSine);
        }

        // Tilt the camera slightly
        transform.DORotate(new Vector3(0, 0, -5), gameOverZoomInDuration).SetEase(Ease.InOutSine);

        Debug.Log("Played camera game over effect");
    }

    private void PlayHitShakeEffect()
    {
        transform.DOShakePosition(hitShakeDuration, hitShakeIntensity, 10, 90, false, true);

        if (mainCamera != null)
        {
            // Quick zoom in and out for impact
            float originalSize = mainCamera.orthographicSize;
            mainCamera.DOOrthoSize(originalSize * 0.9f, hitShakeDuration / 2).SetLoops(2, LoopType.Yoyo);
        }
    }

    private void PlayJumpEffect()
    {
        // Subtle camera movement up when the player jumps
        //transform.DOMoveY(transform.position.y + 0.3f, 0.2f).SetLoops(2, LoopType.Yoyo).SetEase(Ease.OutQuad);

        if (mainCamera != null)
        {
            // Slight zoom out for a more dynamic feel
            float originalSize = mainCamera.orthographicSize;
            mainCamera.DOOrthoSize(originalSize * 1.05f, 0.2f).SetLoops(2, LoopType.Yoyo).SetEase(Ease.OutQuad);
        }

    }

    public void ResetCamera()
    {
        transform.DOKill();

        if (mainCamera != null)
        {
            DOTween.Kill(mainCamera);
            mainCamera.orthographicSize = initialSize;
        }

        transform.position = initialPosition;
        transform.rotation = Quaternion.identity;

        Debug.Log("Camera reset");
    }
}