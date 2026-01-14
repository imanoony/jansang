using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
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

        Char.Init();
        UI.Init();

        UI.SetActiveGauge(true);
        UI.SetActiveHP(true);
        Char.InitHP();
        Char.InitGauge();
    }
    
    public CharacterManager Char { get; private set; }
    public UIManager UI { get; private set; }
}