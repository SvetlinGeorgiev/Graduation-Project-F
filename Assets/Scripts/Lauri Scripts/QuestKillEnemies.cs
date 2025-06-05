using UnityEngine;
using TMPro;

public class QuestKillEnemies : MonoBehaviour
{
    public int killTarget = 5;
    private int currentKills = 0;
    public bool questCompleted = false;

    [Header("Enemy to Track")]
    public GameObject enemyPrefab; // Assign the enemy prefab/type to track in Inspector

    // Call this method from your enemy script when an enemy dies
    public void RegisterEnemyKill(GameObject killedEnemy)
    {
        // Only count kills if the killed enemy matches the tracked prefab (by comparing prefab or tag)
        if (questCompleted) return;
        if (enemyPrefab != null && killedEnemy != null)
        {
            // Compare by prefab (if using prefab variants, consider using tags or a scriptable object ID)
            if (killedEnemy.name.Contains(enemyPrefab.name))
            {
                currentKills++;
            }
            else
            {
                return;
            }
        }
        else
        {
            // If no prefab assigned, count all kills
            currentKills++;
        }

        if (currentKills >= killTarget)
        {
            questCompleted = true;
            // Optionally, trigger quest completion logic here (e.g., notify NPCInteraction)
        }
    }

    public int GetCurrentKills() => currentKills;
    public int GetKillTarget() => killTarget;
    public bool IsQuestCompleted() => questCompleted;
}
