using UnityEngine;
using UnityEngine.SceneManagement;

public class StartMenuUi : MonoBehaviour
{
    private void Start()
    {
        Time.timeScale = 1; // animation on textures wasnt happening if this doesnt get reset when returning from gamescene
    }

    public void LoadScene(int sceneId)
    {
        SceneManager.LoadScene(sceneId);
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}