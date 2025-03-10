using UnityEngine;
using System.Collections;

public class SpikeTrap : MonoBehaviour
{
    public float upPosition = 1f; // The height when the spikes are up
    public float downPosition = 0f; // The height when the spikes are hidden
    public float riseSpeed = 2f; // How fast the spikes rise
    public float delayBeforeRetract = 1f; // How long they stay up
    public float delayBeforeRise = 2f; // Time before they rise again

    private Vector3 startPos;
    private Vector3 endPos;
    private bool isUp = false;

    private void Start()
    {
        startPos = new Vector3(transform.position.x, downPosition, transform.position.z);
        endPos = new Vector3(transform.position.x, upPosition, transform.position.z);
        transform.position = startPos; // Start hidden
        StartCoroutine(SpikeCycle());
    }

    private IEnumerator SpikeCycle()
    {
        while (true)
        {
            yield return new WaitForSeconds(delayBeforeRise);
            StartCoroutine(MoveSpikes(endPos)); // Move up
            isUp = true;

            yield return new WaitForSeconds(delayBeforeRetract);
            StartCoroutine(MoveSpikes(startPos)); // Move down
            isUp = false;
        }
    }

    private IEnumerator MoveSpikes(Vector3 targetPosition)
    {
        float elapsedTime = 0f;
        Vector3 startingPos = transform.position;

        while (elapsedTime < 1f)
        {
            transform.position = Vector3.Lerp(startingPos, targetPosition, elapsedTime);
            elapsedTime += Time.deltaTime * riseSpeed;
            yield return null;
        }

        transform.position = targetPosition;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isUp && other.CompareTag("Player"))
        {
            PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
            other.GetComponent<PlayerMovement>()?.TakeDamage(transform.position);
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(25); // Spikes deal 25 damage
            }
        }
    }

}
