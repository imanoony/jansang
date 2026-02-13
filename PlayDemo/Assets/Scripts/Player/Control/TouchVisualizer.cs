using UnityEngine;
using UnityEngine.InputSystem.EnhancedTouch;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;

public class TouchVisualizer : MonoBehaviour
{
    public float circleSize = 60f;

    private Texture2D circleTexture;

    private void OnEnable()
    {
        EnhancedTouchSupport.Enable();
        CreateCircleTexture();
    }

    private void OnDisable()
    {
        EnhancedTouchSupport.Disable();
    }

    void OnGUI()
    {
        foreach (var touch in Touch.activeTouches)
        {
            Vector2 pos = touch.screenPosition;

            // GUI 좌표는 y가 반대라서 변환
            float x = pos.x - circleSize / 2f;
            float y = Screen.height - pos.y - circleSize / 2f;

            GUI.DrawTexture(
                new Rect(x, y, circleSize, circleSize),
                circleTexture
            );
        }
    }

    void CreateCircleTexture()
    {
        int size = 128;
        circleTexture = new Texture2D(size, size);

        Color clear = new Color(0, 0, 0, 0);
        Color color = new Color(1, 0, 0, 0.6f);

        Vector2 center = new Vector2(size / 2f, size / 2f);
        float radius = size / 2f;

        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                float dist = Vector2.Distance(new Vector2(x, y), center);
                circleTexture.SetPixel(x, y, dist <= radius ? color : clear);
            }
        }

        circleTexture.Apply();
    }
}