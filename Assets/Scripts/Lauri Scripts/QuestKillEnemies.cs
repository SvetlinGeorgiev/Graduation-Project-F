using UnityEngine;
using TMPro;
using System.Collections;

public class QuestKillEnemies : MonoBehaviour
{
    public int killTarget = 5;
    private int currentKills = 0;
    public bool questCompleted = false;
    public bool questActive = false;

    [Header("Enemy Tag to Track")]
    public string enemyTag = "Enemy";

    [Header("Quest Banner Animation")]
    public QuestPanelAnimator questPanelAnimator; 

    
    public void StartQuest()
    {
        questActive = true;
        questCompleted = false;
        currentKills = 0;
       
        if (questPanelAnimator != null)
        {
            questPanelAnimator.ResetPanel();
            questPanelAnimator.PlayOpenAnimation($"Defeat {killTarget} enemies: {currentKills}/{killTarget}");
        }
    }

    
    public void RegisterEnemyKill(GameObject killedEnemy)
    {
        if (!questActive || questCompleted) return;
        if (killedEnemy != null && killedEnemy.CompareTag(enemyTag))
        {
            currentKills++;
        }
        else if (string.IsNullOrEmpty(enemyTag))
        {
            currentKills++;
        }
        else
        {
            return;
        }

        
        if (questPanelAnimator != null)
        {
            questPanelAnimator.questText.text = $"Defeat {killTarget} enemies: {currentKills}/{killTarget}";
        }

        if (currentKills >= killTarget)
        {
            questCompleted = true;
            questActive = false;
            if (questPanelAnimator != null)
            {
                questPanelAnimator.questText.text = "Quest Completed!";
               
                questPanelAnimator.StartCoroutine(FadeAndShrinkBanner());
            }
            Debug.Log("Quest completed! Killed " + currentKills + " enemies.");
        }
    }

    private IEnumerator FadeAndShrinkBanner()
    {
        yield return new WaitForSeconds(2f); 
        if (questPanelAnimator != null)
            yield return questPanelAnimator.PlayShrinkAnimation();
    }

    public int GetCurrentKills() => currentKills;
    public int GetKillTarget() => killTarget;
    public bool IsQuestCompleted() => questCompleted;
}
