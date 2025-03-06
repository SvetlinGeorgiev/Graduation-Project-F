using UnityEngine;

public class DoubleJumpItem : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerMovement player = other.GetComponent<PlayerMovement>();
            if (player != null)
            {
                Debug.Log("DoubleJump");
                player.UnlockDoubleJump();
            }
            Destroy(gameObject); 
        }
    }
}
