using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    public static bool isBattle = true;

    [Header("Hit Effect")]
    [SerializeField] private GameObject hitEffectPrefab;
    [SerializeField] private float hitEffectLifetime = 1.2f;

    [Header("Scene")]
    [SerializeField] private string titleSceneName = "TitleScene";

    [Header("Difficulty")]
    [SerializeField] private int fallbackDifficulty = 0;
    [SerializeField] private DifficultyTuning[] difficultyTunings;
    [System.Serializable]
    public struct DifficultyTuning
    {
        public int playerMaxHp;
        public float playerMaxGauge;
        public float enemyDetectionMultiplier;
        public float enemyHpMultiplier;
    }
    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        Char = gameObject.GetComponent<CharacterManager>();
        UI = gameObject.GetComponent<UIManager>();
        TimeManager = gameObject.GetComponent<TimeManager>();
        Audio = gameObject.GetComponent<AudioManager>();
        talk = gameObject.GetComponent<talkController>();
        MySceneManager = gameObject.GetComponent<MySceneManager>();
        ApplyDifficulty();
        Char.Init();
        UI.Init();
        if (Audio != null) Audio.Init();

        UI.SetActiveGauge(true);
        UI.SetActiveHP(true);
        Char.InitHP();
        Char.InitGauge();
        
        DontDestroyOnLoad(gameObject);
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }
    
    public CharacterManager Char { get; private set; }
    public UIManager UI { get; private set; }
    public TimeManager TimeManager { get; private set; }
    public AudioManager Audio { get; private set; }
    public talkController talk { get; private set; }
    public MySceneManager MySceneManager { get; private set; }
    public GameObject HitEffect => hitEffectPrefab;
    public float HitEffectLifetime => hitEffectLifetime;
    public float EnemyHpMultiplier { get; private set; } = 1f;

    public void SpawnHitEffect(Vector3 position)
    {
        if (hitEffectPrefab == null) return;
        var vfx = Instantiate(hitEffectPrefab, position, Quaternion.identity);
        if (hitEffectLifetime > 0f) Destroy(vfx, hitEffectLifetime);
    }

    public void ReturnToTitle()
    {
        Time.timeScale = 1f;
        if (TimeManager != null) TimeManager.ExitAllBulletTime();

        if (MySceneManager != null)
        {
            MySceneManager.LoadScene(titleSceneName);
        }
        else
        {
            SceneManager.LoadScene(titleSceneName, LoadSceneMode.Single);
        }
    }

    public void ApplyDifficulty()
    {
        int difficulty = fallbackDifficulty;
        if (DifficultyContainer.instance != null)
        {
            difficulty = DifficultyContainer.instance.difficulty;
        }

        if (difficultyTunings == null || difficultyTunings.Length == 0)
        {
            EnemyBase.SetDetectionRadiusMultiplier(1f);
            return;
        }

        int index = Mathf.Clamp(difficulty, 0, difficultyTunings.Length - 1);
        DifficultyTuning tuning = difficultyTunings[index];

        if (Char != null)
        {
            Char.ApplyDifficultyStats(tuning.playerMaxHp, tuning.playerMaxGauge);
        }

        float detectMultiplier = tuning.enemyDetectionMultiplier > 0f ? tuning.enemyDetectionMultiplier : 1f;
        EnemyBase.SetDetectionRadiusMultiplier(detectMultiplier);

        EnemyHpMultiplier = tuning.enemyHpMultiplier > 0f ? tuning.enemyHpMultiplier : 1f;
        EnemyBase.SetHealthMultiplier(EnemyHpMultiplier);
    }
}
