using UnityEngine;
using UnityEngine.InputSystem.EnhancedTouch;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;

public class MobileInput : MonoBehaviour
{
    [Header("Reference")]
    public PlayerMovement2D player;

    [Header("Tuning")]
    public float moveSensitivity = 100f;
    public float dashThreshold = 120f;
    public float tapThreshold = 30f;

    [Header("Double Tap")]
    public float doubleTapTime = 0.25f;

    private int leftFingerId = -1;
    private int rightFingerId = -1;

    private Vector2 leftStartPos;
    private Vector2 rightStartPos;

    private float lastRightTapTime = -1f;

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
                HandleLeftTouch(touch);

            if (touch.touchId == rightFingerId)
                HandleRightTouch(touch);
        }
    }

    // ✅ 좌측은 이동만
    void HandleLeftTouch(Touch touch)
    {
        switch (touch.phase)
        {
            case UnityEngine.InputSystem.TouchPhase.Moved:

                Vector2 delta = touch.screenPosition - leftStartPos;
                float moveValue = Mathf.Clamp(delta.x / moveSensitivity, -1f, 1f);
                player.SetMoveInput(moveValue);

                break;

            case UnityEngine.InputSystem.TouchPhase.Ended:
            case UnityEngine.InputSystem.TouchPhase.Canceled:

                player.SetMoveInput(0);
                leftFingerId = -1;

                break;
        }
    }

    // ✅ 우측: 공격 / 점프 / 대쉬
    void HandleRightTouch(Touch touch)
    {
        switch (touch.phase)
        {
            case UnityEngine.InputSystem.TouchPhase.Ended:

                Vector2 delta = touch.screenPosition - rightStartPos;

                // 📌 드래그 → 대쉬
                if (delta.magnitude > dashThreshold)
                {
                    Vector2 dashDir = delta.normalized;
                    player.TriggerDash(dashDir);
                }
                else if (delta.magnitude < tapThreshold)
                {
                    // 📌 탭 처리
                    float currentTime = Time.time;

                    // 더블탭 체크
                    if (currentTime - lastRightTapTime < doubleTapTime)
                    {
                        // ✅ 점프
                        player.TriggerJump();
                        lastRightTapTime = -1f; // 리셋
                    }
                    else
                    {
                        // 단일 탭 (공격 후보)
                        lastRightTapTime = currentTime;
                        StartCoroutine(HandleSingleTap());
                    }
                }

                rightFingerId = -1;
                break;

            case UnityEngine.InputSystem.TouchPhase.Canceled:
                rightFingerId = -1;
                break;
        }
    }

    System.Collections.IEnumerator HandleSingleTap()
    {
        yield return new WaitForSeconds(doubleTapTime);

        // 더블탭이 아니면 공격 실행
        if (lastRightTapTime > 0f)
        {
            //player.TriggerAttack();
            lastRightTapTime = -1f;
        }
    }
}
