using UnityEngine;
using UnityEngine.AI;
using System.Collections;

[RequireComponent(typeof(NavMeshAgent), typeof(Rigidbody))]
public class EnemyAI : MonoBehaviour
{
    public float detectionRange = 7f;
    public float attackRange = 1.8f;
    public float dashForce = 10f;
    public float attackCooldownMin = 1f;
    public float attackCooldownMax = 3f;
    public int attackDamage = 20;
    public float knockbackForce = 5f;
    public float chaseLostDuration = 5f;
    public float patrolTime = 2f;

    private GameObject player;
    private NavMeshAgent agent;
    private Rigidbody rb;

    private bool isChasing = false;
    private bool isSearching = false;
    private bool isIdle = false;
    private bool isAttacking = false;
    private bool isJumping = false;
    private Vector3 lastSeenPlayerPosition;
    private float searchTimer = 0f;
    private float nextComboTime = 0f;

    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        rb = GetComponent<Rigidbody>();
        player = GameObject.FindGameObjectWithTag("Player");

        agent.updateRotation = false;
        agent.updateUpAxis = false;
        agent.autoTraverseOffMeshLink = false;

        StartCoroutine(RandomBehavior());
    }

    private void Update()
{
    Vector3 position = transform.position;
    transform.position = new Vector3(position.x, position.y, -16.43f);

    if (player == null) return;

    if (agent.isOnOffMeshLink && !isAttacking && !isJumping)
    {
        StartCoroutine(TraverseNavMeshLink());
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
            agent.isStopped = true;

            if (!isAttacking && Time.time >= nextComboTime)
            {
                StartCoroutine(PerformAttack());
            }
        }
        else
        {
            agent.isStopped = false;
            agent.SetDestination(player.transform.position);
        }
    }
    else if (isChasing)
    {
        isChasing = false;
        isSearching = true;
        searchTimer = chaseLostDuration;
        agent.SetDestination(lastSeenPlayerPosition);
    }

    if (isSearching)
    {
        searchTimer -= Time.deltaTime;
        if (searchTimer <= 0f)
        {
            isSearching = false;
            agent.isStopped = true;
            StartCoroutine(RandomBehavior());
        }
    }
}


    private bool CanSeePlayer()
    {
        float distance = Vector3.Distance(transform.position, player.transform.position);
        return distance <= detectionRange;
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
                agent.isStopped = true;
            }
            else
            {
                Vector3 randomDirection = (Random.value > 0.5f ? Vector3.right : Vector3.left);
                Vector3 newDestination = transform.position + randomDirection * 3f;
                NavMeshHit hit;
                if (NavMesh.SamplePosition(newDestination, out hit, 3f, NavMesh.AllAreas))
                {
                    agent.SetDestination(hit.position);
                    agent.isStopped = false;
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
            int attackType = Random.Range(0, 3);

            if (attackType == 0)
                transform.rotation = Quaternion.Euler(0, 0, 10);
            else if (attackType == 1)
                transform.rotation = Quaternion.Euler(0, 0, -10);
            else
                transform.rotation = Quaternion.Euler(50, 0, -50);

            if (Vector3.Distance(transform.position, player.transform.position) < 2f)
            {
                PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();
                if (playerHealth != null)
                {
                    playerHealth.TakeDamage(attackDamage);
                }

                Rigidbody playerRb = player.GetComponent<Rigidbody>();
                if (playerRb != null)
                {
                    Vector3 knockbackDirection = (player.transform.position - transform.position).normalized;
                    playerRb.AddForce(knockbackDirection * knockbackForce, ForceMode.Impulse);
                }
            }

            yield return new WaitForSeconds(0.3f);
            transform.rotation = Quaternion.identity;
            yield return new WaitForSeconds(0.2f);
        }

        float randomCooldown = Random.Range(attackCooldownMin, attackCooldownMax);
        yield return new WaitForSeconds(randomCooldown);

        isAttacking = false;
        nextComboTime = Time.time + randomCooldown;
    }

    private IEnumerator TraverseNavMeshLink()
    {
        isJumping = true;
        agent.isStopped = true;

        OffMeshLinkData data = agent.currentOffMeshLinkData;
        Vector3 startPos = transform.position;
        Vector3 endPos = data.endPos + Vector3.up * agent.baseOffset;

        float duration = 0.4f;
        float time = 0f;

        while (time < duration)
        {
            float t = time / duration;
            transform.position = Vector3.Lerp(startPos, endPos, t) + Vector3.up * Mathf.Sin(t * Mathf.PI) * 1.5f;
            time += Time.deltaTime;
            yield return null;
        }

        transform.position = endPos;
        agent.CompleteOffMeshLink();
        agent.isStopped = false;
        isJumping = false;
    }


    private void DashTowardsPlayer()
    {
        if (player == null) return;

        Vector3 direction = (player.transform.position - transform.position).normalized;
        rb.AddForce(new Vector3(Mathf.Sign(direction.x) * dashForce, 0, 0), ForceMode.Impulse);
    }
}
