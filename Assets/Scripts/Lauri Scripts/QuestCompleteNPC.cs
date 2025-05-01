using UnityEngine;

public class QuestCompletionNPC : MonoBehaviour
{
    public string playerTag = "Player"; // Tag for the player
    public NPCInteraction questGiver; // Reference to the first NPC's script

    private bool isPlayerInRange = false;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(playerTag))
        {
            isPlayerInRange = true;
            CheckQuestCompletion();
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(playerTag))
        {
            isPlayerInRange = false;
        }
    }

    void Update()
    {
        if (isPlayerInRange && Input.GetKeyDown(KeyCode.E)) // Press 'E' to interact
        {
            CompleteQuest();
        }
    }

    void CheckQuestCompletion()
    {
        if (questGiver != null && questGiver.questCompleted)
        {
            Debug.Log("Quest already completed.");
        }
    }

    void CompleteQuest()
    {
        if (questGiver != null && !questGiver.questCompleted)
        {
            questGiver.CompleteQuest(); // Mark the quest as completed
            Debug.Log("Quest completed by interacting with this NPC.");
        }
    }
}
