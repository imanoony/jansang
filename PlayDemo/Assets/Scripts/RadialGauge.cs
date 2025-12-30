using UnityEngine;
using UnityEngine.UI;

public class RadialGauge : MonoBehaviour
{
    public Image fillImage;

    // value: 0~1
    public void SetValue(float value)
    {
        fillImage.fillAmount = Mathf.Clamp01(value);
    }
}