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
    public Transform respawnPoint; // (optional initial spawn)

    private Vector3 checkpointPosition;
    private Quaternion checkpointRotation;
    private bool hasCheckpoint = false;

    private PlayerControls controls;
    private Rigidbody rb;

    //public Animator anim;

    private void Awake()
    {
        controls = new PlayerControls();
        controls.Player.UseItem.performed += ctx => UsePotion();
        rb = GetComponent<Rigidbody>();
    }

    private void Start()
    {
        currentHealth = maxHealth;
        healthBar.maxValue = maxHealth;
        healthBar.value = currentHealth;
        UpdatePotionUI();

        if (respawnPoint != null)
        {
            checkpointPosition = respawnPoint.position;
            checkpointRotation = respawnPoint.rotation;
            hasCheckpoint = true;
        }
    }

    private void OnEnable() => controls.Enable();
    private void OnDisable() => controls.Disable();

    private void Update()
    {
        if (Keyboard.current.eKey.wasPressedThisFrame)
        {
            UsePotion();
        }
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        healthBar.value = currentHealth;
        //anim.SetTrigger("GettingHit");

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
        Debug.Log("Player Died!");

        currentHealth = maxHealth;
        healthBar.value = currentHealth;

        if (hasCheckpoint && rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.position = checkpointPosition;
            rb.rotation = checkpointRotation;
            rb.Sleep();
        }
        else if (hasCheckpoint)
        {
            transform.position = checkpointPosition;
            transform.rotation = checkpointRotation;
        }

        UpdatePotionUI();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Checkpoint"))
        {
            checkpointPosition = other.transform.position;
            checkpointRotation = other.transform.rotation;
            hasCheckpoint = true;

            Debug.Log("Checkpoint reached at: " + checkpointPosition);
            Destroy(other.gameObject);
        }
    }
}
