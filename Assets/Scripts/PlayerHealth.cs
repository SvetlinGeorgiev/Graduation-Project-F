using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using TMPro;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health Settings")]
    public int maxHealth = 100;
    [HideInInspector] public int currentHealth;
    public Slider healthBar;

    [Header("Potion Settings")]
    public int potionCount = 0;
    public int potionHealAmount = 20;
    public TextMeshProUGUI potionUIText;

    [Header("Respawn Settings")]
    public Transform respawnPoint;    // Assign your start point here

    private PlayerControls controls;
    private Rigidbody rb;

    private void Awake()
    {
        controls = new PlayerControls();
        controls.Player.UseItem.performed += ctx => UsePotion();
        rb = GetComponent<Rigidbody>();
    }

    private void Start()
    {
        // Initialize health and UI
        currentHealth = maxHealth;
        healthBar.maxValue = maxHealth;
        healthBar.value = currentHealth;
        UpdatePotionUI();
    }

    private void OnEnable() => controls.Enable();
    private void OnDisable() => controls.Disable();

    private void Update()
    {
        // Fallback keypress for E
        if (Keyboard.current.eKey.wasPressedThisFrame)
        {
            UsePotion();
        }
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        healthBar.value = currentHealth;

        if (currentHealth <= 0)
            Die();
    }

    public void RestoreHealth(int amount)
    {
        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
        healthBar.value = currentHealth;
    }

    public void AddPotion()
    {
        potionCount++;
        UpdatePotionUI();
    }

    private void UsePotion()
    {
        if (potionCount > 0 && currentHealth < maxHealth)
        {
            RestoreHealth(potionHealAmount);
            potionCount--;
            UpdatePotionUI();
        }
    }

    private void UpdatePotionUI()
    {
        if (potionUIText != null)
            potionUIText.text = $"Potions: {potionCount}";
    }

    private void Die()
    {
        // Reset health
        currentHealth = maxHealth;
        healthBar.value = currentHealth;

        // Teleport to respawn point
        if (respawnPoint != null)
        {
            transform.position = respawnPoint.position;
        }

        Debug.Log("Player Respawned");
    }
}
