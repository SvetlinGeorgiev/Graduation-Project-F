using UnityEngine;
using System.Collections;

public class EnemyAIRanged : MonoBehaviour
{
    public float moveSpeed = 3f;
    public float jumpForce = 8f;
    public float detectionRange = 7f;
    public float attackRange = 1.8f;
    public float runAwayRange = 5f;
    public float throwProjectileRange = 4f;
    public float dashForce = 10f;
    public float attackCooldown = 2f;
    public float patrolTime = 2f;
    public float chaseLostDuration = 5f;
    public GameObject projectilePrefab;
    public float knockbackForce = 5f;

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
    private float attackCooldownTimer = 0f;
    private float nextComboTime = 0f;

    private Vector3 movementDirection;
    private Vector3 lastSeenPlayerPosition;
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
            else if (distanceToPlayer <= runAwayRange)
            {
                RunAwayFromPlayer();
            }
            else if (distanceToPlayer <= throwProjectileRange)
            {
                ThrowProjectile();
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

        float distanceToPlayer = Vector3.Distance(transform.position, player.transform.position);
        if (distanceToPlayer > runAwayRange && distanceToPlayer <= detectionRange)
        {
            Vector3 directionToPlayer = (player.transform.position - transform.position).normalized;
            movementDirection = new Vector3(Mathf.Sign(directionToPlayer.x), 0, 0);
            rb.linearVelocity = new Vector3(movementDirection.x * moveSpeed, rb.linearVelocity.y, 0);
        }
    }

    private void MoveToLastSeenDirection()
    {
        Vector3 direction = (lastSeenPlayerPosition - transform.position).normalized;
        movementDirection = new Vector3(Mathf.Sign(direction.x), 0, 0);
        rb.linearVelocity = new Vector3(movementDirection.x * moveSpeed, rb.linearVelocity.y, 0);
    }

    private void RunAwayFromPlayer()
    {
        if (player == null) return;

        float distanceToPlayer = Vector3.Distance(transform.position, player.transform.position);
        if (distanceToPlayer <= runAwayRange && distanceToPlayer > attackRange)
        {
            Vector3 directionAwayFromPlayer = (transform.position - player.transform.position).normalized;
            movementDirection = new Vector3(Mathf.Sign(directionAwayFromPlayer.x), 0, 0);
            rb.linearVelocity = new Vector3(movementDirection.x * moveSpeed, rb.linearVelocity.y, 0);
        }
    }

    private void ThrowProjectile()
    {
        Debug.Log("Throw projectile");
        if (projectilePrefab != null && Time.time >= lastAttackTime + attackCooldown)
        {
            lastAttackTime = Time.time;

            GameObject projectile = Instantiate(projectilePrefab, transform.position + Vector3.up, Quaternion.identity);
            Vector3 direction = (player.transform.position - transform.position).normalized;

            Rigidbody rbProjectile = projectile.GetComponent<Rigidbody>();
            if (rbProjectile != null)
            {
                rbProjectile.linearVelocity = direction * 10f; 
            }

            StartCoroutine(HandleProjectileDamage(projectile));
        }
    }


    private IEnumerator HandleProjectileDamage(GameObject projectile)
    {
        float lifetime = 3f;
        float timer = 0f;
        Debug.Log("Handle projectile");
        while (projectile != null && timer < lifetime)
        {
            if (player != null && Vector3.Distance(projectile.transform.position, player.transform.position) < 1.5f)
            {
                PlayerHealth health = player.GetComponent<PlayerHealth>();
                if (health != null)
                {
                    health.TakeDamage(attackDamage);
                    Debug.Log("Damage projectile");
                }

                Destroy(projectile);
                break;
            }

            timer += Time.deltaTime;
            yield return null;
        }

        if (projectile != null)
        {
            Destroy(projectile);
        }
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
        bool upperDetected = Physics.Raycast(edgeCheckPos.position, (upperCheckPos.position - edgeCheckPos.position).normalized, out upperHit, Vector3.Distance(edgeCheckPos.position, upperCheckPos.position), groundLayer);

        RaycastHit lowerHit;
        bool lowerDetected = Physics.Raycast(lowerCheckPos.position, movementDirection, out lowerHit, 1.5f, groundLayer);

        RaycastHit headHit;
        bool headBlocked = Physics.Raycast(transform.position + Vector3.up * 0.5f, Vector3.up, out headHit, upperCheckPos.position.y - (transform.position.y + 0.5f), groundLayer);

        if (upperDetected && isGrounded && !headBlocked)
        {
            float platformY = upperHit.point.y;
            float enemyY = transform.position.y;
            if (platformY > enemyY + 0.2f)
            {
                Jump();
            }
        }
        else if (edgeDetected && lowerDetected && isGrounded)
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
        int comboCount = Random.Range(1, 4);

        for (int i = 0; i < comboCount; i++)
        {
            int attackType = Random.Range(0, 3);

            if (attackType == 0) transform.rotation = Quaternion.Euler(0, 0, 10);
            else if (attackType == 1) transform.rotation = Quaternion.Euler(0, 0, -10);
            else if (attackType == 2) transform.rotation = Quaternion.Euler(50, 0, -50);

            if (Vector3.Distance(transform.position, player.transform.position) < 2f)
            {
                PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();
                if (playerHealth != null)
                {
                    playerHealth.TakeDamage(attackDamage);
                }

                Vector3 knockbackDir = (player.transform.position - transform.position).normalized;
                Rigidbody playerRb = player.GetComponent<Rigidbody>();
                if (playerRb != null)
                {
                    playerRb.AddForce(knockbackDir * knockbackForce, ForceMode.Impulse);
                }
            }

            yield return new WaitForSeconds(0.4f);
        }

        transform.rotation = Quaternion.identity;
        nextComboTime = Time.time + Random.Range(attackCooldownMin, attackCooldownMax);
        isAttacking = false;
    }
}
