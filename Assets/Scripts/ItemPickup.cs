using UnityEngine;

public class ItemPickup : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            InventoryManager.Instance.AddItem("Health Potion");
            Destroy(gameObject); // Remove potion from the scene
        }
    }
}
