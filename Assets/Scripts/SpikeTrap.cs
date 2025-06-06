using UnityEngine;
using System.Collections;

public class SpikeTrap : MonoBehaviour
{
    public float upPosition = 1f; 
    public float downPosition = 0f; 
    public float riseSpeed = 2f; 
    public float delayBeforeRetract = 1f; 
    public float delayBeforeRise = 2f; 

    private Vector3 startPos;
    private Vector3 endPos;
    private bool isUp = false;
    private float initialY;

    private void Start()
    {
        initialY = transform.position.y;
        startPos = new Vector3(transform.position.x, initialY + downPosition, transform.position.z);
        endPos = new Vector3(transform.position.x, initialY + upPosition, transform.position.z);
        transform.position = startPos;
        StartCoroutine(SpikeCycle());
    }

    private IEnumerator SpikeCycle()
    {
        while (true)
        {
            yield return new WaitForSeconds(delayBeforeRise);
            StartCoroutine(MoveSpikes(endPos)); 
            isUp = true;

            yield return new WaitForSeconds(delayBeforeRetract);
            StartCoroutine(MoveSpikes(startPos)); 
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
                playerHealth.TakeDamage(25); 
            }
        }
    }

}
