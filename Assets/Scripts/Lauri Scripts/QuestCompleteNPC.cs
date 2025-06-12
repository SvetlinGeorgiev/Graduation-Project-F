using UnityEngine;
using System.Collections.Generic;



public class QuestCompletionNPC : MonoBehaviour
{
    public string playerTag = "Player";
    public NPCInteraction questGiver;
    public ScreenSpaceDialogue screenSpaceDialogue; 
    [TextArea(2, 5)]
    public List<string> dialogueLines; 

    private bool isPlayerInRange = false;
    private int currentDialogueIndex = 0;
    private bool dialogueActive = false;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(playerTag))
        {
            isPlayerInRange = true;
            if (!dialogueActive)
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

    void Update()
    {
        if (isPlayerInRange && Input.GetKeyDown(KeyCode.E))
        {
            if (dialogueActive)
            {
                AdvanceDialogue();
            }
            else
            {
                CompleteQuest();
            }
        }
    }

    void StartDialogue()
    {
        if (dialogueLines.Count > 0 && screenSpaceDialogue != null)
        {
            dialogueActive = true;
            currentDialogueIndex = 0;
            screenSpaceDialogue.ShowDialogue(dialogueLines[currentDialogueIndex]);
        }
    }

    void AdvanceDialogue()
    {
        currentDialogueIndex++;
        if (currentDialogueIndex < dialogueLines.Count)
        {
            screenSpaceDialogue.ShowDialogue(dialogueLines[currentDialogueIndex]);
        }
        else
        {
            EndDialogue();
            CompleteQuest();
        }
    }

    void EndDialogue()
    {
        if (screenSpaceDialogue != null)
            screenSpaceDialogue.HideDialogue();
        dialogueActive = false;
        currentDialogueIndex = 0;
    }

    void CompleteQuest()
    {
        if (questGiver != null && !questGiver.questCompleted)
        {
            questGiver.CompleteQuest();
            Debug.Log("Quest completed by interacting with this NPC.");
        }
    }
}
