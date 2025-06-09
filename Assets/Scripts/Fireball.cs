using UnityEngine;

public class Fireball : MonoBehaviour
{
    public float speed = 15f;
    private Vector3 targetPosition;
    public int damage;
    private GameObject explosionVFXPrefab;

    private bool initialized = false;

    public void Initialize(Vector3 targetPos, int damageAmount, GameObject explosionPrefab)
    {
        targetPosition = targetPos;
        damage = damageAmount;
        explosionVFXPrefab = explosionPrefab;
        initialized = true;
    }

    private void Update()
    {
        if (!initialized) return;

        // Move towards target in 2.5D (X,Y plane) ignoring Z for depth
        Vector3 direction = (targetPosition - transform.position).normalized;
        direction.z = 0;
        transform.position += direction * speed * Time.deltaTime;

        // Destroy if close enough
        if (Vector3.Distance(transform.position, targetPosition) < 0.5f)
        {
            Explode();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!initialized) return;

        if (other.CompareTag("Player"))
        {
            PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damage);
            }

            Explode();
        }
        else if (!other.CompareTag("Boss")) // Ignore boss itself
        {
            Explode();
        }
    }

    private void Explode()
    {
        if (explosionVFXPrefab != null)
        {
            Instantiate(explosionVFXPrefab, transform.position, Quaternion.identity);
        }
        Destroy(gameObject);
    }
}
