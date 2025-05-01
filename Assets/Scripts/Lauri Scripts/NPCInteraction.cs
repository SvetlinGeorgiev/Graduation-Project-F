using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;


public class NPCInteraction : MonoBehaviour
{
    public string playerTag = "Player";
    public AudioClip interactionSound; // Sound to play 
    public GameObject dialogueUI; // UI element for talking
    public TextMeshProUGUI dialogueText;
    public string[] dialogueLines;
    public GameObject questUI; // Quest for upper corner
    public TextMeshProUGUI questText;
    public string questDescription;

    private AudioSource audioSource;
    private int currentDialogueIndex = 0;
    private bool isPlayerInRange = false;
    public bool questCompleted = false;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        dialogueUI.SetActive(false); // Hide dialogue UI initially
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(playerTag))
        {
            isPlayerInRange = true;
            PlayInteractionSound();

            // Only start dialogue if the quest is not completed
            if (!questCompleted)
            {
                StartDialogue();
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(playerTag))
        {
            isPlayerInRange = false;
            EndDialogue();
        }
    }

    void PlayInteractionSound()
    {
        if (interactionSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(interactionSound);
        }
    }

    void StartDialogue()
    {
        if (dialogueLines.Length > 0)
        {
            dialogueUI.SetActive(true);
            dialogueText.text = dialogueLines[currentDialogueIndex];
        }
    }

    void Update()
    {
        if (isPlayerInRange && Input.GetKeyDown(KeyCode.E)) // Press 'E' to continue dialogue
        {
            if (!questCompleted) // Only allow dialogue if the quest is not completed
            {
                currentDialogueIndex++;
                if (currentDialogueIndex < dialogueLines.Length)
                {
                    dialogueText.text = dialogueLines[currentDialogueIndex];
                }
                else
                {
                    EndDialogue();
                    AddQuestToUI();
                }
            }
        }
    }

    void EndDialogue()
    {
        dialogueUI.SetActive(false);
        currentDialogueIndex = 0;
    }

    void AddQuestToUI()
    {
        if (questUI != null && questText != null)
        {
            questUI.SetActive(true);
            questText.text = questDescription;
        }
    }

    public void CompleteQuest()
    {
        if (questUI != null && questText != null)
        {
            questText.text = "Quest Completed!"; // Update the quest UI
            questCompleted = true; // Mark the quest as completed
            StartCoroutine(FadeOutQuestText()); // Start fading out the text
        }
    }

    private IEnumerator FadeOutQuestText()
    {
        yield return new WaitForSeconds(5f); // Wait for 5 seconds

        Color originalColor = questText.color;
        float fadeDuration = 2f; // Duration of the fade-out
        float elapsedTime = 0f;

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, elapsedTime / fadeDuration);
            questText.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
            yield return null;
        }

        questText.color = new Color(originalColor.r, originalColor.g, originalColor.b, 0f); // Ensure it's fully transparent
        questUI.SetActive(false); // Optionally hide the quest UI
    }
}
