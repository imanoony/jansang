using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    public static bool isBattle = true;
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
        talk = gameObject.GetComponent<talkController>();
        MySceneManager = gameObject.GetComponent<MySceneManager>();
        Char.Init();
        UI.Init();

        UI.SetActiveGauge(true);
        UI.SetActiveHP(true);
        Char.InitHP();
        Char.InitGauge();
    }
    
    public CharacterManager Char { get; private set; }
    public UIManager UI { get; private set; }
    public TimeManager TimeManager { get; private set; }
    public talkController talk { get; private set; }
    public MySceneManager MySceneManager { get; private set; }
}
