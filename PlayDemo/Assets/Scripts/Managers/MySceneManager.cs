using UnityEngine;

public class MySceneManager : MonoBehaviour
{
    public void LoadScene(string name)
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(name);
    }
}
