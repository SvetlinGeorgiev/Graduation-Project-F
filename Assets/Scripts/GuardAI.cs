using UnityEngine;
using System.Collections;

public class GuardAI : MonoBehaviour
{
    public float moveSpeed = 3f;
    public float jumpForce = 8f;
    public float detectionRange = 7f;
    public float attackRange = 1.8f;
    public float dashForce = 10f;
    public float attackCooldown = 2f;
    public float patrolTime = 2f;
    public float chaseLostDuration = 5f;

    public Transform edgeCheck, edgeCheckLeft;
    public Transform upperPlatformCheck, upperPlatformCheckLeft;
    public Transform lowerPlatformCheck, lowerPlatformCheckLeft;
    public Transform groundCheck;
    public LayerMask groundLayer;
    public int attackDamage = 20;
    public float attackCooldownMin = 1f; 
    public float attackCooldownMax = 3f; 


    private Rigidbody rb;
    private bool isGrounded;
    private bool isChasing = false;
    private bool isIdle = false;
    private bool isSearching = false;
    private bool isAttacking = false;
    private float searchTimer = 0f;
    private float lastAttackTime = 0f;
    public float knockbackForce = 5f;
    private float attackCooldownTimer = 0f;
    private float nextComboTime = 0f;



    private Vector3 movementDirection;
    private Vector3 lastSeenPlayerPosition;
    private GameObject player;
    public Transform staffObject;


    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        player = GameObject.FindGameObjectWithTag("Player");
        StartCoroutine(RandomBehavior());
    }

    private void FixedUpdate()
    {
        isGrounded = Physics.CheckSphere(groundCheck.position, 0.2f, groundLayer);

        if (CanSeePlayer())
        {
            isChasing = true;
            isSearching = false;
            searchTimer = 0f;
            lastSeenPlayerPosition = player.transform.position;

            float distanceToPlayer = Vector3.Distance(transform.position, player.transform.position);

            if (distanceToPlayer <= attackRange)
            {
                if (!isAttacking && Time.time >= nextComboTime)
                {
                    StartCoroutine(PerformAttack());
                }
            }
            else
            {
                ChasePlayer();
            }
        }
        else if (isChasing)
        {
            isChasing = false;
            isSearching = true;
            searchTimer = chaseLostDuration;
        }

        if (isSearching)
        {
            searchTimer -= Time.fixedDeltaTime;

            if (searchTimer > 0)
            {
                MoveToLastSeenDirection();
            }
            else
            {
                isSearching = false;
                StartCoroutine(RandomBehavior());
            }
        }

        if (!isChasing && !isSearching && !isIdle)
        {
            Patrol();
        }

        CheckForPlatformsAndEdges();
        // Lock Z position in case physics or other forces nudge it
        Vector3 pos = transform.position;
        pos.z = 1.8393f;
        transform.position = pos;

    }

    private bool CanSeePlayer()
    {
        if (player == null) return false;
        float distance = Vector3.Distance(transform.position, player.transform.position);
        return distance <= detectionRange;
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

    private void MoveToLastSeenDirection()
    {
        Vector3 direction = (lastSeenPlayerPosition - transform.position).normalized;
        movementDirection = new Vector3(Mathf.Sign(direction.x), 0, 0);
        rb.linearVelocity = new Vector3(movementDirection.x * moveSpeed, rb.linearVelocity.y, 0);
    }

    private IEnumerator RandomBehavior()
    {
        while (!isChasing && !isSearching)
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
        bool upperPlatformDetected = Physics.Raycast(edgeCheckPos.position, rayDirection, out upperHit, rayDistance, groundLayer);

        RaycastHit lowerHit;
        bool lowerPlatformDetected = Physics.Raycast(lowerCheckPos.position, movementDirection, out lowerHit, 1.5f, groundLayer);

        RaycastHit headHit;
        bool headBlocked = Physics.Raycast(transform.position + Vector3.up * 0.5f, Vector3.up, out headHit, upperCheckPos.position.y - (transform.position.y + 0.5f), groundLayer);

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

    private IEnumerator PerformAttack()
{
    isAttacking = true;
    int comboCount = Random.Range(1, 4); // 1 to 3 attack combo

    for (int i = 0; i < comboCount; i++)
    {
        int attackType = Random.Range(0, 4); // 0 = stab1, 1 = stab2, 2 = swing1, 3 = swing2

        // Reset staff rotation
        staffObject.localRotation = Quaternion.identity;

        switch (attackType)
        {
            case 0: // Stab 1
                yield return StartCoroutine(StaffStabAnimation(15));
                break;
            case 1: // Stab 2
                yield return StartCoroutine(StaffStabAnimation(-15));
                break;
            case 2: // Swing 1
                yield return StartCoroutine(StaffSwingAnimation(60));
                break;
            case 3: // Swing 2
                yield return StartCoroutine(StaffSwingAnimation(-60));
                break;
        }

        // Damage check and knockback
        if (Vector3.Distance(transform.position, player.transform.position) < 2f)
        {
            PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(attackDamage);
            }

            Vector3 knockbackDirection = (player.transform.position - transform.position).normalized;
            Rigidbody playerRb = player.GetComponent<Rigidbody>();
            if (playerRb != null)
            {
                playerRb.AddForce(knockbackDirection * knockbackForce, ForceMode.Impulse);
            }
        }

        yield return new WaitForSeconds(0.4f); // Attack delay
        staffObject.localRotation = Quaternion.identity;
        yield return new WaitForSeconds(0.2f);
    }

    float randomCooldown = Random.Range(attackCooldownMin, attackCooldownMax);
    yield return new WaitForSeconds(randomCooldown);
    isAttacking = false;
}

    private IEnumerator StaffStabAnimation(float forwardTilt)
{
    float duration = 0.2f;
    float elapsed = 0f;
    while (elapsed < duration)
    {
        float angle = Mathf.Lerp(0, forwardTilt, elapsed / duration);
        staffObject.localRotation = Quaternion.Euler(angle, 0, 0);
        elapsed += Time.deltaTime;
        yield return null;
    }
}

private IEnumerator StaffSwingAnimation(float swingAngle)
{
    float duration = 0.3f;
    float elapsed = 0f;
    while (elapsed < duration)
    {
        float angle = Mathf.Lerp(0, swingAngle, Mathf.Sin((elapsed / duration) * Mathf.PI));
        staffObject.localRotation = Quaternion.Euler(0, 0, angle);
        elapsed += Time.deltaTime;
        yield return null;
    }
}







    private void DashTowardsPlayer()
    {
        if (player == null) return;

        Vector3 direction = (player.transform.position - transform.position).normalized;
        rb.AddForce(new Vector3(Mathf.Sign(direction.x) * dashForce, 0, 0), ForceMode.Impulse);
    }
}
