using UnityEngine;

public class Portal : MonoBehaviour
{
    public string playerTag = "Player"; // Tag for the player
    public Transform targetLocation; // The location to teleport the player to

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(playerTag))
        {
            TeleportPlayer(other.gameObject);
        }
    }

    private void TeleportPlayer(GameObject player)
    {
        if (targetLocation != null)
        {
            player.transform.position = targetLocation.position; // Move the player to the target location
            Debug.Log("Player teleported to: " + targetLocation.position);
        }
        else
        {
            Debug.LogWarning("Target location is not set for the portal.");
        }
    }
}
