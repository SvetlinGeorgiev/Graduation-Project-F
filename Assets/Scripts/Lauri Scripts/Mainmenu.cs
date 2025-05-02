using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
   
    public void StartGame()
    {
        
        SceneManager.LoadScene("Lauri_Scene");
    }

    //  Settings button
    public void OpenSettings()
    {
       
        Debug.Log("Settings button clicked");
    }

    //  Quit button
    public void QuitGame()
    {
        // Quit the application
        Debug.Log("Quit button clicked");
        Application.Quit();
    }
}
