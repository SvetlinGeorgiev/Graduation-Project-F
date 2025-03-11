using UnityEngine;

public class Arrow : MonoBehaviour
{
    public float lifetime = 5f;
    public int damage = 1;
    public float knockbackForce = 5f;

    private void Start()
    {
        Destroy(gameObject, lifetime); 
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            other.GetComponent<PlayerHealth>().TakeDamage(damage);

            Rigidbody playerRb = other.GetComponent<Rigidbody>();
            if (playerRb != null)
            {
                Vector3 knockbackDir = (other.transform.position - transform.position).normalized;
                playerRb.AddForce(knockbackDir * knockbackForce, ForceMode.Impulse);
            }

            Destroy(gameObject); 
        }
    }
}
