using UnityEngine;
using System.Collections.Generic;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance; // Singleton to access inventory

    private List<string> inventory = new List<string>(); // Simple inventory storage
    public int playerHealth = 100; // Player's health

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    public void AddItem(string item)
    {
        inventory.Add(item);
        Debug.Log($"Picked up: {item}");
    }

    public void UseHealthPotion()
    {
        if (inventory.Contains("Health Potion"))
        {
            inventory.Remove("Health Potion");
            playerHealth = Mathf.Min(playerHealth + 25, 100); // Heal but max at 100
            Debug.Log($"Used Health Potion! Current HP: {playerHealth}");
        }
        else
        {
            Debug.Log("No health potions left!");
        }
    }

    public void ShowInventory()
    {
        Debug.Log("Inventory: " + string.Join(", ", inventory));
    }
}
