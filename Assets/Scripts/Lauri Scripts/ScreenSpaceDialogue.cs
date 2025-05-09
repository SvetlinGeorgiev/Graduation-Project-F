using UnityEngine;
using TMPro;

public class ScreenSpaceDialogue : MonoBehaviour
{
    public GameObject framedDialogueBox; // Reference to the framed dialogue box
    public TextMeshProUGUI framedDialogueText; // Reference to the text inside the dialogue box

 
    /// <param name="dialogue">The dialogue text to display.</param>
    public void ShowDialogue(string dialogue)
    {
        if (framedDialogueBox != null && framedDialogueText != null)
        {
            framedDialogueBox.SetActive(true); // Show the dialogue box
            framedDialogueText.text = dialogue; // Set the dialogue text
        }
    }


    public void HideDialogue()
    {
        if (framedDialogueBox != null)
        {
            framedDialogueBox.SetActive(false); // Hide the dialogue box
        }
    }

    /// <param name="dialogue">The dialogue text to display.</param>
    /// <param name="duration">The duration to display the dialogue for.</param>
    public void ShowDialogueForDuration(string dialogue, float duration)
    {
        ShowDialogue(dialogue);
        Invoke(nameof(HideDialogue), duration); // Hide the dialogue after the specified duration
    }
}
