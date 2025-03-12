using UnityEngine;
using System.Collections;

public class EnemyAI : MonoBehaviour
{
    public float moveSpeed = 3f;
    public float jumpForce = 8f;
    public float detectionRange = 7f;
    public float patrolTime = 2f; // Time before switching state
    public Transform groundCheck; // Empty GameObject at enemy's feet
    public LayerMask groundLayer;

    private Rigidbody rb;
    private bool isGrounded;
    private bool isChasing = false;
    private bool isIdle = false;
    private Vector3 movementDirection;
    private GameObject player;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        player = GameObject.FindGameObjectWithTag("Player");
        StartCoroutine(RandomBehavior());
    }

    private void FixedUpdate()
    {
        // Check if the enemy is on the ground
        isGrounded = Physics.CheckSphere(groundCheck.position, 0.2f, groundLayer);

        if (isChasing)
        {
            ChasePlayer();
        }
        else if (!isIdle)
        {
            Patrol();
        }
    }

    private IEnumerator RandomBehavior()
    {
        while (true)
        {
            float waitTime = Random.Range(1f, patrolTime);
            isIdle = Random.value > 0.5f; // 50% chance to idle

            if (isIdle)
            {
                StartCoroutine(IdleAnimation());
            }
            else
            {
                movementDirection = (Random.value > 0.5f) ? Vector3.right : Vector3.left;
            }

            yield return new WaitForSeconds(waitTime);
        }
    }

    private void Patrol()
    {
        // Move left or right
        rb.linearVelocity = new Vector3(movementDirection.x * moveSpeed, rb.linearVelocity.y, 0);
    }

    private void ChasePlayer()
    {
        if (player == null) return;

        Vector3 directionToPlayer = (player.transform.position - transform.position).normalized;

        // Lock movement to left/right axis only
        movementDirection = new Vector3(Mathf.Sign(directionToPlayer.x), 0, 0);

        // Move towards the player
        rb.linearVelocity = new Vector3(movementDirection.x * moveSpeed, rb.linearVelocity.y, 0);

        // Check if jump is needed
        if (isGrounded && player.transform.position.y > transform.position.y + 1f)
        {
            Jump();
        }
    }

    private void Jump()
    {
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, jumpForce, 0);
    }

    private IEnumerator IdleAnimation()
    {
        float tiltAmount = 10f;
        float tiltSpeed = 2f;
        float timeElapsed = 0f;

        while (timeElapsed < patrolTime)
        {
            float angle = Mathf.Sin(Time.time * tiltSpeed) * tiltAmount;
            transform.rotation = Quaternion.Euler(0, 0, angle);
            timeElapsed += Time.deltaTime;
            yield return null;
        }

        transform.rotation = Quaternion.identity; // Reset tilt
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isChasing = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isChasing = false;
            StartCoroutine(RandomBehavior());
        }
    }
}
