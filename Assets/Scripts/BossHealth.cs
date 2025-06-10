using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class BossHealth : MonoBehaviour
{
    public float maxHealth = 500f;
    private float currentHealth;

    [Header("Boss UI")]
    public Slider bossHealthSlider;

    private bool isBlocking = false;

    private float damageWindowDuration = 7f;
    private Queue<DamageEvent> recentDamage = new Queue<DamageEvent>();

    private struct DamageEvent
    {
        public float timestamp;
        public float amount;

        public DamageEvent(float time, float damage)
        {
            timestamp = time;
            amount = damage;
        }
    }

    private void Start()
    {
        currentHealth = maxHealth;

        if (bossHealthSlider != null)
        {
            bossHealthSlider.maxValue = maxHealth;
            bossHealthSlider.value = currentHealth;
            bossHealthSlider.gameObject.SetActive(false);
        }
    }

    public void ActivateBoss()
    {
        if (bossHealthSlider != null)
        {
            bossHealthSlider.gameObject.SetActive(true);
        }
    }

    public void TakeDamage(float amount)
    {
        currentHealth -= amount;

        if (bossHealthSlider != null)
        {
            bossHealthSlider.value = currentHealth;
        }

        float currentTime = Time.time;
        recentDamage.Enqueue(new DamageEvent(currentTime, amount));

        // Remove damage events outside the 7-second window
        while (recentDamage.Count > 0 && currentTime - recentDamage.Peek().timestamp > damageWindowDuration)
        {
            recentDamage.Dequeue();
        }

        // Calculate total damage in the window
        float totalDamage = 0f;
        foreach (var dmg in recentDamage)
        {
            totalDamage += dmg.amount;
        }

        // Trigger retreat if total damage in window is 75 or more
        if (totalDamage >= 75f)
        {
            recentDamage.Clear(); // Prevent repeated retreat triggers

            BossAI bossAI = GetComponent<BossAI>();
            if (bossAI != null)
            {
                bossAI.EnterRetreatState();
            }
        }

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private IEnumerator BlockAnimation()
    {
        if (isBlocking) yield break;
        isBlocking = true;

        Quaternion originalRotation = transform.rotation;
        Quaternion blockRotation = Quaternion.Euler(-20f, transform.rotation.eulerAngles.y, transform.rotation.eulerAngles.z);

        float elapsed = 0f;
        float duration = 0.3f;

        while (elapsed < duration)
        {
            transform.rotation = Quaternion.Slerp(originalRotation, blockRotation, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.rotation = blockRotation;
        yield return new WaitForSeconds(0.4f);

        elapsed = 0f;
        while (elapsed < duration)
        {
            transform.rotation = Quaternion.Slerp(blockRotation, originalRotation, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.rotation = originalRotation;
        isBlocking = false;
    }

    private void Die()
    {
        if (bossHealthSlider != null)
        {
            bossHealthSlider.gameObject.SetActive(false);
        }

        Destroy(gameObject);
    }
}