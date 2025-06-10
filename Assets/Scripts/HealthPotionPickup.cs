using UnityEngine;

public class HealthPotionPickup : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
        if (playerHealth != null)
        {
            playerHealth.AddPotion();
            Destroy(gameObject);
        }
    }
}
