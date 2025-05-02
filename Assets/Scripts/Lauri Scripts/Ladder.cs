using UnityEngine;

public class Ladder : MonoBehaviour
{
    private bool isClimbing = false;
    private GameObject player;
    public float climbSpeed = 3f;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            player = other.gameObject;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isClimbing = false;
            player.GetComponent<Rigidbody>().useGravity = true; // Ensure gravity is re-enabled
            player = null;
        }
    }

    private void Update()
    {
        if (player != null && Input.GetKeyDown(KeyCode.E))
        {
            isClimbing = true;
            player.GetComponent<Rigidbody>().useGravity = false;
        }

        if (isClimbing)
        {
            float verticalInput = Input.GetAxis("Vertical");
            player.transform.Translate(Vector3.up * verticalInput * climbSpeed * Time.deltaTime);

            if (Input.GetKeyDown(KeyCode.Space))
            {
                isClimbing = false;
                player.GetComponent<Rigidbody>().useGravity = true;
            }
        }
    }
}
