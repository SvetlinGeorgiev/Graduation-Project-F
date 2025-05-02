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
        controls.Player.UseItem.performed += ctx => UseItem();
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

    public void RestoreHealth(int amount)
    {
        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
        healthBar.value = currentHealth;
    }

    private void UseItem()
    {
        InventoryManager inventory = GetComponent<InventoryManager>();
        if (inventory != null)
        {
            inventory.UseItem(0); 
        }
    }


    private void Die()
    {
        Debug.Log("Player Died!");
    }
}
