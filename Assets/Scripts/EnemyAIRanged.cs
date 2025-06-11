using UnityEngine;
using System.Collections;
using UnityEngine.AI;

public class EnemyAIRanged : MonoBehaviour
{
    public float moveSpeed = 3f;
    public float jumpForce = 8f;
    public float detectionRange = 7f;
    public float attackRange = 1.8f;
    public float runAwayRange = 5f;
    public float throwProjectileRange = 4f;
    public float dashForce = 10f;
    public float patrolTime = 2f;
    public float chaseLostDuration = 5f;
    public GameObject projectilePrefab;
    public float knockbackForce = 5f;

    public Transform groundCheck;
    public LayerMask groundLayer;
    public int attackDamage = 20;
    public float attackCooldownMin = 1f;
    public float attackCooldownMax = 3f;

    private Rigidbody rb;
    private NavMeshAgent agent;
    private AgentLinkMover linkMover;
    private bool isGrounded;
    private bool isChasing = false;
    private bool isIdle = false;
    private bool isSearching = false;
    private bool isAttacking = false;

    private float searchTimer = 0f;
    private float nextComboTime = 0f;
    private float lastProjectileTime = 0f;
    private Vector3 lastSeenPlayerPosition;
    private GameObject player;

    private Vector3 patrolDestination;
    private bool hasPatrolDestination = false;

    public Animator anim;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        agent = GetComponent<NavMeshAgent>();
        linkMover = GetComponent<AgentLinkMover>();
        agent.updateRotation = false;
        agent.updateUpAxis = false;
        agent.speed = moveSpeed;

        player = GameObject.FindGameObjectWithTag("Player");
        StartCoroutine(RandomBehavior());
    }

    private void FixedUpdate()
    {
        isGrounded = Physics.CheckSphere(groundCheck.position, 0.2f, groundLayer);

        if (linkMover != null && linkMover.IsTraversing)
        {
            agent.ResetPath();
            return;
        }

        if (agent.isOnOffMeshLink)
            return;

        if (CanSeePlayer())
        {
            isChasing = true;
            isSearching = false;
            searchTimer = 0f;
            lastSeenPlayerPosition = player.transform.position;

            float distanceToPlayer = Vector3.Distance(transform.position, player.transform.position);
            FaceTarget(player.transform.position);

            if (distanceToPlayer <= attackRange)
            {
                if (!isAttacking && Time.time >= nextComboTime)
                {
                    StartCoroutine(PerformAttack());
                }
                agent.ResetPath();
            }
            else if (distanceToPlayer <= runAwayRange && distanceToPlayer > attackRange)
            {
                RunAwayFromPlayer();
            }
            else if (distanceToPlayer <= throwProjectileRange)
            {
                TryThrowProjectile();
                if (agent.hasPath)
                {
                    agent.ResetPath();
                    hasPatrolDestination = false;
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
                FaceTarget(lastSeenPlayerPosition);
            }
            else
            {
                isSearching = false;
                StartCoroutine(RandomBehavior());
            }
        }

        if (!isChasing && !isSearching && !isIdle && !agent.isOnOffMeshLink && (linkMover == null || !linkMover.IsTraversing))
        {
            Patrol();
        }

        Vector3 fixedPosition = transform.position;
        fixedPosition.z = -16.4f;
        transform.position = fixedPosition;
    }

    private bool CanSeePlayer()
    {
        if (player == null) return false;
        float distance = Vector3.Distance(transform.position, player.transform.position);
        return distance <= detectionRange;
    }

    private void Patrol()
    {
        if ((linkMover != null && linkMover.IsTraversing) || agent.isOnOffMeshLink)
            return;

        if (!hasPatrolDestination && !agent.pathPending && !agent.hasPath)
        {
            float patrolDistance = Random.Range(2f, 15f);

            Vector3 horizontalDir = Random.value > 0.5f ? Vector3.right : Vector3.left;
            Vector3 targetPos = transform.position + horizontalDir * patrolDistance;

            RaycastHit hitInfo;
            if (Physics.Raycast(targetPos + Vector3.up * 5f, Vector3.down, out hitInfo, 10f, groundLayer))
            {
                targetPos.y = hitInfo.point.y + 0.1f;
            }

            NavMeshHit navHit;
            if (NavMesh.SamplePosition(targetPos, out navHit, 7f, NavMesh.AllAreas))
            {
                patrolDestination = navHit.position;
                if (agent.SetDestination(patrolDestination))
                {
                    hasPatrolDestination = true;
                    FaceTarget(patrolDestination);
                    Debug.DrawLine(transform.position, patrolDestination, Color.cyan, 2f);
                }
            }
        }

        if (hasPatrolDestination && !agent.pathPending)
        {
            if (agent.remainingDistance <= agent.stoppingDistance + 0.1f && agent.velocity.sqrMagnitude < 0.01f)
            {
                hasPatrolDestination = false;
            }
        }
    }

    private void ChasePlayer()
    {
        if (player == null) return;
        agent.SetDestination(player.transform.position);
        FaceTarget(player.transform.position);
    }

    private void MoveToLastSeenDirection()
    {
        agent.SetDestination(lastSeenPlayerPosition);
    }

    private void RunAwayFromPlayer()
    {
        if (player == null) return;
        Vector3 directionAway = (transform.position - player.transform.position).normalized;
        Vector3 runTo = transform.position + directionAway * 5f;

        NavMeshHit navHit;
        if (NavMesh.SamplePosition(runTo, out navHit, 5f, NavMesh.AllAreas))
        {
            agent.SetDestination(navHit.position);
            FaceTarget(runTo);
        }
    }

    private void TryThrowProjectile()
    {
        if (projectilePrefab == null || player == null) return;

        float distanceToPlayer = Vector3.Distance(transform.position, player.transform.position);

        if (Time.time >= lastProjectileTime + Random.Range(attackCooldownMin, attackCooldownMax))
        {
            Vector3 origin = transform.position + Vector3.up * 1.5f;
            Vector3 target = player.transform.position + Vector3.up * 1f;
            Vector3 direction = (target - origin).normalized;

            if (Physics.Raycast(origin, direction, out RaycastHit hit, distanceToPlayer))
            {
                if (hit.collider.CompareTag("Player"))
                {
                    lastProjectileTime = Time.time;

                    GameObject projectile = Instantiate(projectilePrefab, origin, Quaternion.identity);
                    Rigidbody rbProjectile = projectile.GetComponent<Rigidbody>();

                    if (rbProjectile != null)
                    {
                        rbProjectile.linearVelocity = direction * 10f;
                    }

                    StartCoroutine(HandleProjectileDamage(projectile));
                    FaceTarget(player.transform.position);

                    if (agent.hasPath) agent.ResetPath();
                    hasPatrolDestination = false;
                    return;
                }
            }

            if (distanceToPlayer > runAwayRange)
            {
                Vector3 approachDirection = (player.transform.position - transform.position).normalized;
                float safeDistance = runAwayRange - 0.5f;
                Vector3 approachPosition = player.transform.position - approachDirection * safeDistance;

                NavMeshHit navHit;
                if (NavMesh.SamplePosition(approachPosition, out navHit, 5f, NavMesh.AllAreas))
                {
                    agent.SetDestination(navHit.position);
                    FaceTarget(navHit.position);
                    hasPatrolDestination = false;
                }
            }
            else
            {
                if (agent.hasPath) agent.ResetPath();
            }
        }
    }

    private IEnumerator HandleProjectileDamage(GameObject projectile)
    {
        float lifetime = 3f;
        float timer = 0f;
        while (projectile != null && timer < lifetime)
        {
            if (player != null && Vector3.Distance(projectile.transform.position, player.transform.position) < 1.5f)
            {
                PlayerHealth health = player.GetComponent<PlayerHealth>();
                if (health != null)
                {
                    health.TakeDamage(attackDamage);
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
                if (agent.hasPath)
                {
                    agent.ResetPath();
                    hasPatrolDestination = false;
                }
            }
            else
            {
                if (!hasPatrolDestination)
                {
                    agent.ResetPath();
                    hasPatrolDestination = false;
                }
            }

            yield return new WaitForSeconds(waitTime);
        }
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
            if (player != null)
            {
                FaceTarget(player.transform.position);
            }

            int attackType = Random.Range(0, 3);

            if (attackType == 0)
            {
                anim.SetTrigger("Hit2");
                //StartCoroutine(PerformAttackTilt(comboStep));
            }
            else if (attackType == 1)
            {
                anim.SetTrigger("Uppercut");
                //transform.rotation = Quaternion.Euler(0, 0, -10);
            }
            else
            {
                anim.SetTrigger("Low Kick");
                //transform.rotation = Quaternion.Euler(50, 0, -50);
            }

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

            yield return new WaitForSeconds(Random.Range(attackCooldownMin, attackCooldownMax));
        }

        transform.rotation = Quaternion.identity;
        nextComboTime = Time.time + Random.Range(attackCooldownMin, attackCooldownMax);
        isAttacking = false;
    }

    private void FaceTarget(Vector3 targetPos)
    {
        Vector3 direction = (targetPos - transform.position).normalized;
        if (direction.x != 0)
        {
            Vector3 localScale = transform.localScale;
            localScale.x = Mathf.Sign(direction.x) * Mathf.Abs(localScale.x);
            transform.localScale = localScale;
        }
    }
}
