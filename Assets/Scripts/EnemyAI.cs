using UnityEngine;
using System.Collections;

public class EnemyAI : MonoBehaviour
{
    public float moveSpeed = 3f;
    public float jumpForce = 8f;
    public float detectionRange = 7f;
    public float patrolTime = 2f;

    public Transform edgeCheck, edgeCheckLeft; 
    public Transform upperPlatformCheck, upperPlatformCheckLeft;
    public Transform lowerPlatformCheck, lowerPlatformCheckLeft;
    public Transform groundCheck;
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
        isGrounded = Physics.CheckSphere(groundCheck.position, 0.2f, groundLayer);

        if (isChasing)
        {
            ChasePlayer();
        }
        else if (!isIdle)
        {
            Patrol();
        }
        
        CheckForPlatformsAndEdges();
    }

    private IEnumerator RandomBehavior()
    {
        while (true)
        {
            float waitTime = Random.Range(1f, patrolTime);
            isIdle = Random.value > 0.5f; 

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
        rb.linearVelocity = new Vector3(movementDirection.x * moveSpeed, rb.linearVelocity.y, 0);
    }

    private void ChasePlayer()
    {
        if (player == null) return;

        Vector3 directionToPlayer = (player.transform.position - transform.position).normalized;

        movementDirection = new Vector3(Mathf.Sign(directionToPlayer.x), 0, 0);

        rb.linearVelocity = new Vector3(movementDirection.x * moveSpeed, rb.linearVelocity.y, 0);
    }

    private void CheckForPlatformsAndEdges()
    {
        bool movingRight = movementDirection.x > 0;

        Transform edgeCheckPos = movingRight ? edgeCheck : edgeCheckLeft;
        Transform upperCheckPos = movingRight ? upperPlatformCheck : upperPlatformCheckLeft;
        Transform lowerCheckPos = movingRight ? lowerPlatformCheck : lowerPlatformCheckLeft;

        bool edgeDetected = !Physics.Raycast(edgeCheckPos.position, Vector3.down, 1f, groundLayer);

        RaycastHit upperHit;
        Vector3 rayDirection = (upperCheckPos.position - edgeCheckPos.position).normalized; 
        float rayDistance = Vector3.Distance(edgeCheckPos.position, upperCheckPos.position); 
        bool upperPlatformDetected = Physics.Raycast(
            edgeCheckPos.position, 
            rayDirection, 
            out upperHit,
            rayDistance, 
            groundLayer
        );

        RaycastHit lowerHit;
        bool lowerPlatformDetected = Physics.Raycast(
            lowerCheckPos.position, 
            movementDirection, 
            out lowerHit, 
            1.5f, 
            groundLayer
        );

        RaycastHit headHit;
        bool headBlocked = Physics.Raycast(
            transform.position + Vector3.up * 0.5f, 
            Vector3.up, 
            out headHit,
            upperCheckPos.position.y - (transform.position.y + 0.5f), 
            groundLayer
        );

        if (upperPlatformDetected && isGrounded && !headBlocked)
        {
            float platformY = upperHit.point.y;
            float enemyY = transform.position.y;

            if (platformY > enemyY + 0.2f) 
            {
                Jump();
            }
        }
        else if (edgeDetected && lowerPlatformDetected && isGrounded)
        {
            StartCoroutine(DelayedJump());
        }
    }





    private IEnumerator DelayedJump()
    {
        yield return new WaitForSeconds(0.2f); 
        if (isGrounded) 
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

        transform.rotation = Quaternion.identity; 
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
