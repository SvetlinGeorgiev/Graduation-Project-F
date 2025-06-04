using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneChange : MonoBehaviour
{
    //public Scene scene;
    public Object sceneToLoad;

    public void NextScene()
    {
       // SceneManager.LoadScene(scene);
        SceneManager.LoadScene(sceneToLoad.name);

    }
    public void Exit()
    {
        Application.Quit();
    }
}
