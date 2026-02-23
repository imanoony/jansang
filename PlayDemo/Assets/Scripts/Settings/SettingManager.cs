using System;
using UnityEngine;
using Cysharp.Threading.Tasks;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

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
    private AudioManager audioManager;
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
        audioManager = GameManager.Instance != null ? GameManager.Instance.Audio : null;

        AllSlider.onValueChanged.RemoveAllListeners();
        BGMSlider.onValueChanged.RemoveAllListeners();
        EffectSlider.onValueChanged.RemoveAllListeners();
        
        AllSlider.onValueChanged.AddListener(OnAllSoundChanged);
        BGMSlider.onValueChanged.AddListener(OnBGMSoundChanged);
        EffectSlider.onValueChanged.AddListener(OnEffectSoundChanged);

        SyncSoundFromAudio();
    }

    public void StopGame()
    {
        if (timestopid == -1)
        {
            Stop_Button.SetActive(true);
            Setting_Button.SetActive(false);
            panel.SetActive(false);
            sound_panel.SetActive(false);
            timestopid = GameManager.Instance.TimeManager.EnterBulletTime(0);
            stop_Panel.SetActive(true);
        }
        else
        {
            Stop_Button.SetActive(true);
            Setting_Button.SetActive(true);
            stop_Panel.SetActive(false);
            panel.SetActive(false);
            sound_panel.SetActive(false);
            if (timestopid != -1) GameManager.Instance.TimeManager.ExitBulletTime(timestopid);
            timestopid = -1;
        }
    }

    private int timestopid = -1;
    public void OpenSetting()
    {
        Stop_Button.SetActive(false);
        Setting_Button.SetActive(false);
        stop_Panel.SetActive(false);
        sound_panel.SetActive(false);
        panel.SetActive(true);
        // 시간 정지
        timestopid = GameManager.Instance.TimeManager.EnterBulletTime(0);
    }

    public void CloseSetting()
    {
        Stop_Button.SetActive(true);
        Setting_Button.SetActive(true);
        stop_Panel.SetActive(false);
        panel.SetActive(false);
        sound_panel.SetActive(false);
        // 시간 다시 흐르게
        if (timestopid != -1) GameManager.Instance.TimeManager.ExitBulletTime(timestopid);
        timestopid = -1;
    }

    public void ShowSound_Panel()
    {
        stop_Panel.SetActive(false);
        sound_panel.SetActive(true);
        panel.SetActive(false);

        // 현재 값 세팅
        SyncSoundFromAudio();
    }
    private void OnAllSoundChanged(float value)
    {
        AllSound = value;
        if (audioManager != null) audioManager.SetMasterVolume(value);
    }
    private void OnBGMSoundChanged(float value)
    {
        BGMSound = value;
        if (audioManager != null) audioManager.SetBgmVolume(value);
    }
    private void OnEffectSoundChanged(float value)
    {
        EffectSound = value;
        if (audioManager != null) audioManager.SetSfxVolume(value);
    }
    public void CloseSound_Panel()
    {
        stop_Panel.SetActive(false);
        sound_panel.SetActive(false);
        panel.SetActive(true);
    }

    public void RestartScene()
    {
        if (timestopid != -1)
        {
            GameManager.Instance.TimeManager.ExitBulletTime(timestopid);
            timestopid = -1;
        }
        
        Stop_Button.SetActive(true);
        Setting_Button.SetActive(true);
        stop_Panel.SetActive(false);
        panel.SetActive(false);
        sound_panel.SetActive(false);

        var gameManager = GameManager.Instance;
        var sceneManager = gameManager != null ? gameManager.MySceneManager : null;
        var audio = gameManager != null ? gameManager.Audio : null;
        float fadeOut = sceneManager != null ? Mathf.Max(0f, sceneManager.fadeTime) : 1f;
        if (audio != null) audio.StopBgm(fadeOut);

        if (sceneManager != null)
        {
            sceneManager.ReloadCurrentScene();
        }
        else
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }

    private void SyncSoundFromAudio()
    {
        if (audioManager == null)
        {
            AllSlider.SetValueWithoutNotify(AllSound);
            BGMSlider.SetValueWithoutNotify(BGMSound);
            EffectSlider.SetValueWithoutNotify(EffectSound);
            return;
        }

        AllSound = audioManager.MasterVolume;
        BGMSound = audioManager.BgmVolume;
        EffectSound = audioManager.SfxVolume;

        AllSlider.SetValueWithoutNotify(AllSound);
        BGMSlider.SetValueWithoutNotify(BGMSound);
        EffectSlider.SetValueWithoutNotify(EffectSound);
    }
}
