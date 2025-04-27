using UnityEngine;
using System.Collections;
using System;

public class InputManager : MonoBehaviour
{
    [Header("Touch Settings")]
    [SerializeField] private float swipeThreshold = 50f;
    [SerializeField] private float tapThreshold = 0.2f;
    [SerializeField] private bool detectKeyboardInput = true;

    [Header("References")]
    [SerializeField] private PlayerController playerController;

    // Events for other systems to subscribe to
    public event Action<SwipeDirection> OnSwipe;

    private Vector2 startTouchPosition;
    private float startTouchTime;

    public enum SwipeDirection
    {
        None,
        Up,
        Down,
        Left,
        Right
    }

    public void Initialize(PlayerController player)
    {
        playerController = player;
        Debug.Log("InputManager initialized with player");
    }

    private void Update()
    {
        if (playerController == null)
            return;

        // Process touch input for mobile
        DetectTouchInput();

        // Optional keyboard input for testing
        if (detectKeyboardInput)
        {
            DetectKeyboardInput();
        }
    }

    private void DetectTouchInput()
    {
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);

            switch (touch.phase)
            {
                case TouchPhase.Began:
                    startTouchPosition = touch.position;
                    startTouchTime = Time.time;
                    break;

                case TouchPhase.Ended:
                case TouchPhase.Canceled:
                    Vector2 endTouchPosition = touch.position;
                    float touchDuration = Time.time - startTouchTime;

                    // If it's a quick touch with minimal movement, it's a tap
                    if (touchDuration < tapThreshold && Vector2.Distance(startTouchPosition, endTouchPosition) < swipeThreshold)
                    {
                        // Tap logic could go here if needed
                    }
                    else
                    {
                        // Otherwise it's a swipe
                        SwipeDirection direction = DetectSwipeDirection(startTouchPosition, endTouchPosition);

                        if (direction != SwipeDirection.None)
                        {
                            HandleSwipe(direction);
                        }
                    }
                    break;
            }
        }
    }

    private SwipeDirection DetectSwipeDirection(Vector2 startPos, Vector2 endPos)
    {
        Vector2 swipeDelta = endPos - startPos;

        // Check if the swipe is long enough
        if (swipeDelta.magnitude < swipeThreshold)
        {
            return SwipeDirection.None;
        }

        // Determine swipe direction based on the dominant axis
        float absX = Mathf.Abs(swipeDelta.x);
        float absY = Mathf.Abs(swipeDelta.y);

        // In portrait mode, a vertical swipe is more natural for y-axis movement
        if (absX > absY)
        {
            return swipeDelta.x > 0 ? SwipeDirection.Right : SwipeDirection.Left;
        }
        else
        {
            return swipeDelta.y > 0 ? SwipeDirection.Up : SwipeDirection.Down;
        }
    }

    private void DetectKeyboardInput()
    {
        if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.Space))
        {
            HandleSwipe(SwipeDirection.Up);
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S))
        {
            HandleSwipe(SwipeDirection.Down);
        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A))
        {
            HandleSwipe(SwipeDirection.Left);
        }
        else if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D))
        {
            HandleSwipe(SwipeDirection.Right);
        }
    }

    private void HandleSwipe(SwipeDirection direction)
    {
        // Notify any subscribers
        OnSwipe?.Invoke(direction);

        // Directly control player if available
        if (playerController != null)
        {
            switch (direction)
            {
                case SwipeDirection.Left:
                    playerController.MoveLane(-1);
                    break;

                case SwipeDirection.Right:
                    playerController.MoveLane(1);
                    break;

                case SwipeDirection.Up:
                    playerController.Jump();
                    break;

                case SwipeDirection.Down:
                    playerController.Slide();
                    break;
            }
        }
    }

    // Public method for UI buttons to trigger swipes
    public void TriggerSwipe(string direction)
    {
        SwipeDirection swipeDir = SwipeDirection.None;

        switch (direction.ToLower())
        {
            case "up":
                swipeDir = SwipeDirection.Up;
                break;

            case "down":
                swipeDir = SwipeDirection.Down;
                break;

            case "left":
                swipeDir = SwipeDirection.Left;
                break;

            case "right":
                swipeDir = SwipeDirection.Right;
                break;
        }

        if (swipeDir != SwipeDirection.None)
        {
            HandleSwipe(swipeDir);
        }
    }
}