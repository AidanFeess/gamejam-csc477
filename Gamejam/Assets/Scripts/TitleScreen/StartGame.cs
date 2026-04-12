using UnityEngine;
using UnityEngine.SceneManagement;
public class StartButton : MonoBehaviour
{
    public string LevelName;

    void Start()
    {
        Time.timeScale = 1;
    }

    public void LoadLevel()
    {
        SceneManager.LoadScene(LevelName);
    }

    public void ExitGame()
    {
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #endif
        Application.Quit();
    }
}
