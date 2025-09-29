using UnityEngine;

public class Ladder : MonoBehaviour
{
    private bool isClimbing = false;
    private GameObject player;
    public float climbSpeed = 9f;

    public Canvas climbPromptCanvas; // Reference to the world space canvas

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            player = other.gameObject;
            if (climbPromptCanvas != null)
                climbPromptCanvas.gameObject.SetActive(true); // Show prompt
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isClimbing = false;

            var movement = player.GetComponent<PlayerMovement>();
            movement.isClimbing = false;
            movement.canAttack = true;

            Rigidbody rb = player.GetComponent<Rigidbody>();
            rb.useGravity = true;

            if (climbPromptCanvas != null)
                climbPromptCanvas.gameObject.SetActive(false); // Hide prompt

            player = null;
        }
    }

    private void Update()
    {
        if (player != null && Input.GetKeyDown(KeyCode.E))
        {
            isClimbing = true;

            Rigidbody rb = player.GetComponent<Rigidbody>();
            rb.useGravity = false;
            rb.linearVelocity = Vector3.zero;

            var movement = player.GetComponent<PlayerMovement>();
            movement.isClimbing = true;
            movement.canAttack = false;

            if (climbPromptCanvas != null)
                climbPromptCanvas.gameObject.SetActive(false); // Hide prompt when climbing starts
        }

        if (isClimbing)
        {
            float verticalInput = Input.GetAxis("Vertical");
            player.transform.Translate(Vector3.up * verticalInput * climbSpeed * Time.deltaTime);

            if (Input.GetKeyDown(KeyCode.Space))
            {
                isClimbing = false;

                var movement = player.GetComponent<PlayerMovement>();
                movement.isClimbing = false;
                movement.canAttack = true;
                movement.canDoubleJump = false;

                Rigidbody rb = player.GetComponent<Rigidbody>();
                rb.useGravity = true;
            }
        }
    }
}
