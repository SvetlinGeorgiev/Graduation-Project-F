using UnityEngine;
using System.Collections;

public class ArrowTrapButton : MonoBehaviour
{
    public GameObject arrowPrefab; 
    public Transform[] spawnPoints; 
    public float arrowSpeed = 10f;
    public float fireRate = 0.5f; 
    public int arrowCount = 3;
    private bool isActivated = false;

    private void OnTriggerEnter(Collider other)
    {
        if (!isActivated && other.CompareTag("Player"))
        {
            isActivated = true;
            StartCoroutine(FireArrows());
        }
    }

    private IEnumerator FireArrows()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");

        for (int i = 0; i < arrowCount; i++)
        {
            if (spawnPoints.Length == 0) yield break; 

            Transform chosenSpawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];

            GameObject arrow = Instantiate(arrowPrefab, chosenSpawnPoint.position, Quaternion.identity);
            
            if (player != null)
            {
                Vector3 direction = (player.transform.position - chosenSpawnPoint.position).normalized;

                arrow.transform.rotation *= Quaternion.Euler(0, 90, 0); 

                Rigidbody rb = arrow.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.linearVelocity = direction * arrowSpeed;
                }
            }

            yield return new WaitForSeconds(fireRate);
        }

        isActivated = false; 
    }
}
