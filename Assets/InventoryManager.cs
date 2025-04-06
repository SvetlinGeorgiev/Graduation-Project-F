using System.Collections.Generic;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    public List<Item> inventory = new List<Item>();
    public InventoryUI inventoryUI; 

    public void AddItem(Item item)
    {
        inventory.Add(item);
        Debug.Log($"Picked up: {item.itemName}");

        if (inventoryUI != null)
            inventoryUI.UpdateInventoryUI(inventory);
    }

    public void UseItem(int index)
    {
        if (index < 0 || index >= inventory.Count) return;

        Item item = inventory[index];
        item.Use(gameObject);
        inventory.RemoveAt(index);

        if (inventoryUI != null)
            inventoryUI.UpdateInventoryUI(inventory);
    }
}
