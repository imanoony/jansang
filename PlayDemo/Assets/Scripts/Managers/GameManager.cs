using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    void Awake()
    {
        if (Instance != null && Instance != this)
            Destroy(gameObject);
        Instance = this;
        DontDestroyOnLoad(this.gameObject);

        CharacterManager = gameObject.GetComponent<CharacterManager>();
        UIManager = gameObject.GetComponent<UIManager>();
    }
    
    public CharacterManager CharacterManager { get; private set; }
    public UIManager UIManager { get; private set; }
}