using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class PlayerHealth : MonoBehaviour
{
    public int maxHealth = 100;
    private int currentHealth;
    public Slider healthBar; 

    private PlayerControls controls;

    private void Awake()
    {
        controls = new PlayerControls();
        //controls.Player.UseItem.performed += ctx => UseHealthPotion();
    }

    private void Start()
    {
        currentHealth = maxHealth;
        healthBar.maxValue = maxHealth;
        healthBar.value = currentHealth;
    }

    private void OnEnable() => controls.Enable();
    private void OnDisable() => controls.Disable();

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        healthBar.value = currentHealth;

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    /*public void UseHealthPotion()
{
    int healAmount = InventoryManager.Instance.UseHealthPotion(); // Get the healing amount

    if (healAmount > 0) // If healing is received
    {
        currentHealth = Mathf.Min(currentHealth + healAmount, maxHealth); // Prevent overhealing
        healthBar.value = currentHealth;
    }
}*/


    private void Die()
    {
        Debug.Log("Player Died!");
    }
}
