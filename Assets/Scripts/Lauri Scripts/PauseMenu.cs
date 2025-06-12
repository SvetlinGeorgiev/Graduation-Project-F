using UnityEngine;

public class PauseMenu : MonoBehaviour
{
    public GameObject pauseMenuPanel; 
    private bool isPaused = false;

    void Update()
    {
        // Toggle the pause menu when tab pressed
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            if (isPaused)
            {
                ResumeGame();
            }
            else
            {
                PauseGame();
            }
        }
    }

    public void PauseGame()
    {
        isPaused = true;
        pauseMenuPanel.SetActive(true); 
        Time.timeScale = 0f; 
    }

    public void ResumeGame()
    {
        isPaused = false;
        pauseMenuPanel.SetActive(false); 
        Time.timeScale = 1f; 
    }

  
    public void OpenInventory()
    {
        Debug.Log("Inventory button clicked");
        
    }

    public void OpenSkillTree()
    {
        Debug.Log("Skill Tree button clicked");
        
    }

    public void OpenWingTsunPractice()
    {
        Debug.Log("Wing Tsun Practice button clicked");
        
    }
}
