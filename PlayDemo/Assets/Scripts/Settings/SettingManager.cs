using System;
using UnityEngine;
using Cysharp.Threading.Tasks;
using UnityEngine.UI;

public class SettingManager : MonoBehaviour
{
    public Slider AllSlider;
    public Slider BGMSlider;
    public Slider EffectSlider;
    public GameObject Stop_Button;
    public GameObject Setting_Button;
    public GameObject stop_Panel;
    public GameObject panel;
    public GameObject sound_panel;
    public float AllSound = 1;
    public float BGMSound = 1;
    public float EffectSound = 1;
    public void Awake()
    {
        Stop_Button.SetActive(true);
        Setting_Button.SetActive(true);
        stop_Panel.SetActive(false);
        panel.SetActive(false);
        sound_panel.SetActive(false);
    }
    private void Start()
    {
        AllSlider.onValueChanged.RemoveAllListeners();
        BGMSlider.onValueChanged.RemoveAllListeners();
        EffectSlider.onValueChanged.RemoveAllListeners();
        
        AllSlider.onValueChanged.AddListener(OnAllSoundChanged);
        BGMSlider.onValueChanged.AddListener(OnBGMSoundChanged);
        EffectSlider.onValueChanged.AddListener(OnEffectSoundChanged);
    }

    public void StopGame()
    {
        if (Time.timeScale > 0)
        {
            Stop_Button.SetActive(true);
            Setting_Button.SetActive(false);
            panel.SetActive(false);
            sound_panel.SetActive(false);
            Time.timeScale = 0f;
            stop_Panel.SetActive(true);
        }
        else
        {
            Stop_Button.SetActive(true);
            Setting_Button.SetActive(true);
            stop_Panel.SetActive(false);
            panel.SetActive(false);
            sound_panel.SetActive(false);
            Time.timeScale = 1f;
        }
    }
    public void OpenSetting()
    {
        Stop_Button.SetActive(false);
        Setting_Button.SetActive(false);
        stop_Panel.SetActive(false);
        sound_panel.SetActive(false);
        panel.SetActive(true);
        // 시간 정지
        Time.timeScale = 0f;
    }

    public void CloseSetting()
    {
        Stop_Button.SetActive(true);
        Setting_Button.SetActive(true);
        stop_Panel.SetActive(false);
        panel.SetActive(false);
        sound_panel.SetActive(false);
        // 시간 다시 흐르게
        Time.timeScale = 1f;
    }

    public void ShowSound_Panel()
    {
        stop_Panel.SetActive(false);
        sound_panel.SetActive(true);
        panel.SetActive(false);

        // 현재 값 세팅
        AllSlider.SetValueWithoutNotify(AllSound);
        BGMSlider.SetValueWithoutNotify(BGMSound);
        EffectSlider.SetValueWithoutNotify(EffectSound);
    }
    private void OnAllSoundChanged(float value)
    {
        AllSound = value;
    }
    private void OnBGMSoundChanged(float value)
    {
        BGMSound = value;
    }
    private void OnEffectSoundChanged(float value)
    {
        EffectSound = value;
    }
    public void CloseSound_Panel()
    {
        stop_Panel.SetActive(false);
        sound_panel.SetActive(false);
        panel.SetActive(true);
    }
}