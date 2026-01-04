using UnityEngine;
using UnityEngine.UI;

public class RadialGauge : MonoBehaviour
{
    public Image fillImage;

    public float lerpSpeed = 10f;
    // value: 0~1
    public void SetValue(float value)
    {
        fillImage.fillAmount = Mathf.Clamp01(Mathf.Lerp(fillImage.fillAmount,value,lerpSpeed * Time.deltaTime));
    }
}