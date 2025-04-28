using UnityEngine;

public class PauseMenu : MonoBehaviour
{
    public GameObject pauseMenuPanel; // Reference to the pause menu panel
    private bool isPaused = false;

    void Update()
    {
        // Toggle the pause menu when the "TAB" key is pressed
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
        pauseMenuPanel.SetActive(true); // Show the pause menu
        Time.timeScale = 0f; // Freeze the game
    }

    public void ResumeGame()
    {
        isPaused = false;
        pauseMenuPanel.SetActive(false); // Hide the pause menu
        Time.timeScale = 1f; // Resume the game
    }

    // Methods for the buttons
    public void OpenInventory()
    {
        Debug.Log("Inventory button clicked");
        // Add inventory logic here
    }

    public void OpenSkillTree()
    {
        Debug.Log("Skill Tree button clicked");
        // Add skill tree logic here
    }

    public void OpenWingTsunPractice()
    {
        Debug.Log("Wing Tsun Practice button clicked");
        // Add Wing Tsun practice logic here
    }
}
