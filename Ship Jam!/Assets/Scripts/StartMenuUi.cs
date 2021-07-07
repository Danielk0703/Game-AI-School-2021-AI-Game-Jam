using UnityEngine;
using UnityEngine.SceneManagement;

public class StartMenuUi : MonoBehaviour
{
    public void LoadScene(int sceneId)
    {
        SceneManager.LoadScene(sceneId);
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}