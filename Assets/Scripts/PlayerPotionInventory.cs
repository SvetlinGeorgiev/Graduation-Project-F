using UnityEngine;
using TMPro;

public class PlayerPotionInventory : MonoBehaviour
{
    [Header("Potion Settings")]
    public int currentPotions = 0;
    public int healAmount = 20;
    public KeyCode useKey = KeyCode.E;
    public TextMeshProUGUI potionCountText;

    private PlayerHealth playerHealth;

    private void Start()
    {
        playerHealth = GetComponent<PlayerHealth>();
        UpdatePotionUI();
    }

    private void Update()
    {
        if (Input.GetKeyDown(useKey) && currentPotions > 0)
        {
            UsePotion();
        }
    }

    public void AddPotion()
    {
        currentPotions++;
        UpdatePotionUI();
    }

    private void UsePotion()
    {
        if (playerHealth != null)
        {
            playerHealth.RestoreHealth(healAmount);
            currentPotions--;
            UpdatePotionUI();
        }
    }

    private void UpdatePotionUI()
    {
        if (potionCountText != null)
        {
            potionCountText.text = $"Potions: {currentPotions}";
        }
    }
}
