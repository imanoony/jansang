using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CheckPointController : MonoBehaviour
{
    [Header("UI Root")]
    [SerializeField] GameObject mainPanel;
    [SerializeField] GameObject upgradePanel;

    [Header("Resource Text")]
    [SerializeField] TMP_Text resourceText;

    [Header("Buttons")]
    [SerializeField] Button saveButton;
    [SerializeField] Button upgradeButton;
    [SerializeField] Button backButton;

    [Header("Upgrade Buttons")]
    [SerializeField] Button hpUpButton;
    [SerializeField] Button gaugeUpButton;
    [SerializeField] Button resetButton;
    [SerializeField] Button upgradeBackButton;
    
    [Header("Upgrade Amount")]
    [SerializeField] TMP_Text hpUpText;
    [SerializeField] TMP_Text gaugeUpText;
    [SerializeField] TMP_Text resetText;
    CharacterManager player;

    void OnEnable()
    {
        player = GameManager.Instance.Char;
        OpenMain();
        BindButtons();
    }
    void OpenMain()
    {
        mainPanel.SetActive(true);
        upgradePanel.SetActive(false);
        RefreshUI();
    }
    void RefreshUI()
    {
        resourceText.text = player.LightMaterial.ToString();

        // 비용 조건
        hpUpButton.interactable     = player.canUse(player.HPUpMaterial());
        hpUpText.text = player.HPUpMaterial().ToString();
        gaugeUpButton.interactable  = player.canUse(player.GaugeUpMaterial()); 
        gaugeUpText.text = player.GaugeUpMaterial().ToString();
        resetButton.interactable = (player.HPLevel > 0 || player.GaugeLevel > 0);
        resetText.text = player.UsedMaterial.ToString();
    }

    void BindButtons()
    {
        saveButton.onClick.RemoveAllListeners();
        upgradeButton.onClick.RemoveAllListeners();
        backButton.onClick.RemoveAllListeners();

        hpUpButton.onClick.RemoveAllListeners();
        gaugeUpButton.onClick.RemoveAllListeners();
        resetButton.onClick.RemoveAllListeners();

        saveButton.onClick.AddListener(SaveGame);
        upgradeButton.onClick.AddListener(OpenUpgrade);
        backButton.onClick.AddListener(CloseUI);

        hpUpButton.onClick.AddListener(UpgradeHp);
        gaugeUpButton.onClick.AddListener(UpgradeGauge);
        resetButton.onClick.AddListener(ResetStatus);
        upgradeBackButton.onClick.AddListener(OpenMain);
    }

    void SaveGame()
    {
        Debug.Log("게임 저장");
        // SaveManager.Instance.Save();
    }

    void OpenUpgrade()
    {
        mainPanel.SetActive(false);
        upgradePanel.SetActive(true);
        RefreshUI();
    }

    void CloseUI()
    {
        GameManager.Instance.UI.EndCheckPoint();
    }

    void UpgradeHp()
    {
        if (!player.canUse(player.HPUpMaterial())) return;
        player.UpgradeHP();
        RefreshUI();
    }

    void UpgradeGauge()
    {
        if (!player.canUse(player.GaugeUpMaterial())) return;
        player.UpgradeGauge();
        RefreshUI();
    }

    void ResetStatus()
    {
        if (player.HPLevel == 0 && player.GaugeLevel == 0) return;
        player.ResetUpgrade();
        RefreshUI();
    }
}
