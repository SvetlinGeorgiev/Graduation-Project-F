using UnityEngine;

public class ItemPickup : MonoBehaviour
{
    public string itemName;
    public ItemType itemType;
    public int amount = 20;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Item newItem = new Item
            {
                itemName = this.itemName,
                itemType = this.itemType,
                amount = this.amount
            };

            InventoryManager inventory = other.GetComponent<InventoryManager>();
            if (inventory != null)
            {
                inventory.AddItem(newItem);
                Destroy(gameObject);
            }
        }
    }
}
