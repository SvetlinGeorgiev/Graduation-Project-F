using UnityEngine;
using System.Collections;
using UnityEngine.AI;

public class EnemyAI : MonoBehaviour
{
    public float moveSpeed = 3f;
    public float jumpForce = 8f;
    public float detectionRange = 7f;
    public float attackRange = 1.8f;
    public float dashForce = 10f;
    public float attackCooldown = 2f;
    public float patrolTime = 1f;
    public float chaseLostDuration = 5f;

    private int comboStep = 0;
    //Declaring the anaimator
    [SerializeField]
    private Animator animEnemy;

    public Transform groundCheck;
    public LayerMask groundLayer;
    public int attackDamage = 20;
    public float attackCooldownMin = 1f;
    public float attackCooldownMax = 3f;
    public float knockbackForce = 5f;

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
    private Vector3 lastSeenPlayerPosition;
    private GameObject player;

    private Vector3 patrolDestination;
    private bool hasPatrolDestination = false;
    private bool wasMovingToPatrol = false;
    private float patrolDistanceMin = 2f;
    private float patrolDistanceMax = 15f;

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
        {
            return;
        }

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
                agent.ResetPath();
            }
            else
            {
                ChasePlayer();
            }

            FaceDirection(player.transform.position - transform.position);
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
                FaceDirection(lastSeenPlayerPosition - transform.position);
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

        // Face patrol direction if roaming
        if (!isChasing && !isSearching && !isIdle && agent.velocity.sqrMagnitude > 0.01f)
        {
            FaceDirection(agent.velocity);
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
        {
            return;
        }

        if (!hasPatrolDestination && !agent.pathPending && !agent.hasPath)
        {
            float patrolDistance = Random.Range(patrolDistanceMin, patrolDistanceMax);
            Vector3 horizontalDir = Random.value > 0.5f ? Vector3.right : Vector3.left;
            Vector3 verticalDir = Random.value > 0.8f ? Vector3.down : Vector3.zero;
            Vector3 direction = (horizontalDir + verticalDir).normalized;
            Vector3 targetPos = transform.position + direction * patrolDistance;

            if (Physics.Raycast(targetPos + Vector3.up * 5f, Vector3.down, out RaycastHit hitInfo, 10f, groundLayer))
            {
                targetPos.y = hitInfo.point.y + 0.1f;
            }

            if (NavMesh.SamplePosition(targetPos, out NavMeshHit navHit, 7f, NavMesh.AllAreas))
            {
                patrolDestination = navHit.position;
                if (agent.SetDestination(patrolDestination))
                {
                    hasPatrolDestination = true;
                    wasMovingToPatrol = true;
                    //ieie: Set off the walking animation
                    animEnemy.SetBool("Walking", true);
                    Debug.DrawLine(transform.position, patrolDestination, Color.green, 2f);
                }
            }
        }

        if (hasPatrolDestination && !agent.pathPending)
        {
            if (agent.remainingDistance <= agent.stoppingDistance + 0.1f)
            {
                //ieie: Stop the walking
                animEnemy.SetBool("Walking", false);
                if (agent.velocity.sqrMagnitude < 0.01f)
                {
                    hasPatrolDestination = false;
                    wasMovingToPatrol = false;
                }
            }
        }
    }

    private void ChasePlayer()
    {
        if (player == null) return;
        agent.SetDestination(player.transform.position);
    }

    private void MoveToLastSeenDirection()
    {
        agent.SetDestination(lastSeenPlayerPosition);
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
                    wasMovingToPatrol = false;
                }
            }
            else
            {
                if (!hasPatrolDestination)
                {
                    agent.ResetPath();
                }

                hasPatrolDestination = false;
                wasMovingToPatrol = false;
            }

            yield return new WaitForSeconds(waitTime);
        }
    }

    private IEnumerator IdleAnimation()
    {
        float timeElapsed = 0f;
        while (timeElapsed < patrolTime)
        {
            //ieie: Set off idle animation
            animEnemy.SetBool("Idle", true);
            timeElapsed += Time.deltaTime;
            yield return null;
        }

        transform.rotation = Quaternion.identity;
    }

    private IEnumerator PerformAttack()
    {
        isAttacking = true;
        int comboCount = Random.Range(1, 6);

        for (int i = 0; i < comboCount; i++)
        {
            int attackType = Random.Range(0, 3);

            //ieie: Shortened and removed other transforms apart from animations
            if (attackType == 0) animEnemy.SetTrigger("Hit2");
            else if (attackType == 1) animEnemy.SetTrigger("Uppercut");
            else animEnemy.SetTrigger("Low Kick");

            if (Vector3.Distance(transform.position, player.transform.position) < 2f)
            {
                PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();
                if (playerHealth != null) playerHealth.TakeDamage(attackDamage);

                Vector3 knockbackDirection = (player.transform.position - transform.position).normalized;
                Rigidbody playerRb = player.GetComponent<Rigidbody>();
                if (playerRb != null)
                {
                    playerRb.AddForce(knockbackDirection * knockbackForce, ForceMode.Impulse);
                }
            }

            FaceDirection(player.transform.position - transform.position);

            yield return new WaitForSeconds(0.3f);
            transform.rotation = Quaternion.identity;
            yield return new WaitForSeconds(0.2f);
        }

        float randomCooldown = Random.Range(attackCooldownMin, attackCooldownMax);
        yield return new WaitForSeconds(randomCooldown);
        isAttacking = false;
    }

    private void DashTowardsPlayer()
    {
        if (player == null) return;

        Vector3 direction = (player.transform.position - transform.position).normalized;
        rb.AddForce(new Vector3(Mathf.Sign(direction.x) * dashForce, 0, 0), ForceMode.Impulse);
    }

    private void FaceDirection(Vector3 direction)
    {
        if (direction.sqrMagnitude > 0.01f)
        {
            Vector3 flatDir = new Vector3(direction.x, 0f, 0f);
            if (flatDir != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(flatDir, Vector3.up);
                transform.rotation = targetRotation;
            }
        }
    }
}
