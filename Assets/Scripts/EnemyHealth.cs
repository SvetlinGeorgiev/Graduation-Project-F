using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    public float maxHealth = 100f;
    private float currentHealth;
    [SerializeField]
    //private Animator anim;
    private void Start()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(float amount)
    {
        currentHealth -= amount;
        //anim.SetTrigger("GetsHit");
        Debug.Log(gameObject.name + " took " + amount + " damage. Remaining health: " + currentHealth);

        if (currentHealth <= 0)
        {
            //anim.SetTrigger("Dead");
            
            Die();
        }
    }

    private void Die()
    {
        Debug.Log(gameObject.name + " has died!");
        QuestKillEnemies quest = FindObjectOfType<QuestKillEnemies>();
        if (quest != null)
        {
            quest.RegisterEnemyKill(gameObject);
        }
        Destroy(gameObject); 
    }
}
