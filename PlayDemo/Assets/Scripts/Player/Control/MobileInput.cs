using UnityEngine;
using UnityEngine.InputSystem.EnhancedTouch;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;

public class MobileInput : MonoBehaviour
{
    [Header("Reference")]
    public PlayerMovement2D player;

    [Header("Tuning")]
    public float moveSensitivity = 100f;
    public float jumpThreshold = 150f;
    public float dashThreshold = 120f;
    public float tapThreshold = 30f;

    private int leftFingerId = -1;
    private int rightFingerId = -1;

    private Vector2 leftStartPos;
    private Vector2 rightStartPos;

    private void OnEnable()
    {
        EnhancedTouchSupport.Enable();
    }

    private void OnDisable()
    {
        EnhancedTouchSupport.Disable();
    }

    void Update()
    {
        foreach (var touch in Touch.activeTouches)
        {
            Vector2 pos = touch.screenPosition;

            bool isLeftSide = pos.x < Screen.width * 0.5f;

            if (touch.phase == UnityEngine.InputSystem.TouchPhase.Began)
            {
                if (isLeftSide && leftFingerId == -1)
                {
                    leftFingerId = touch.touchId;
                    leftStartPos = pos;
                }
                else if (!isLeftSide && rightFingerId == -1)
                {
                    rightFingerId = touch.touchId;
                    rightStartPos = pos;
                }
            }

            if (touch.touchId == leftFingerId)
            {
                HandleLeftTouch(touch);
            }

            if (touch.touchId == rightFingerId)
            {
                HandleRightTouch(touch);
            }
        }
    }

    void HandleLeftTouch(Touch touch)
    {
        switch (touch.phase)
        {
            case UnityEngine.InputSystem.TouchPhase.Moved:

                Vector2 delta = touch.screenPosition - leftStartPos;

                // 좌우 이동
                float moveValue = Mathf.Clamp(delta.x / moveSensitivity, -1f, 1f);
                player.SetMoveInput(moveValue);

                // 점프 (위로 충분히 올렸을 때 한 번만)
                if (delta.y > jumpThreshold)
                {
                    player.TriggerJump();
                    leftStartPos.y = touch.screenPosition.y; // 중복 점프 방지
                }

                break;

            case UnityEngine.InputSystem.TouchPhase.Ended:
            case UnityEngine.InputSystem.TouchPhase.Canceled:

                player.SetMoveInput(0);
                leftFingerId = -1;

                break;
        }
    }

    void HandleRightTouch(Touch touch)
    {
        switch (touch.phase)
        {
            case UnityEngine.InputSystem.TouchPhase.Ended:

                Vector2 delta = touch.screenPosition - rightStartPos;

                // 탭 → 공격
                if (delta.magnitude < tapThreshold)
                {
                    Debug.Log("Attack");
                    //player.TriggerAttack();
                }
                // 스와이프 → 대쉬
                else if (delta.magnitude > dashThreshold)
                {
                    Vector2 dashDir = delta.normalized;
                    player.TriggerDash(dashDir);
                }

                rightFingerId = -1;
                break;

            case UnityEngine.InputSystem.TouchPhase.Canceled:
                rightFingerId = -1;
                break;
        }
    }
}
