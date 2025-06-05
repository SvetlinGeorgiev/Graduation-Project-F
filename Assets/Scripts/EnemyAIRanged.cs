using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class EnemyAIRanged : MonoBehaviour
{
    public float detectionRange = 7f;
    public float attackRange = 1.8f;
    public float runAwayRange = 5f;
    public float throwProjectileRange = 4f;
    public float attackCooldown = 2f;
    public float patrolTime = 2f;
    public float chaseLostDuration = 5f;
    public GameObject projectilePrefab;
    public float knockbackForce = 5f;
    public int attackDamage = 20;
    public float attackCooldownMin = 1f;
    public float attackCooldownMax = 3f;

    public Transform groundCheck;
    public LayerMask groundLayer;

    private NavMeshAgent agent;
    private GameObject player;
    private Vector3 lastSeenPlayerPosition;
    private bool isChasing = false;
    private bool isSearching = false;
    private bool isIdle = false;
    private bool isAttacking = false;
    private float searchTimer = 0f;
    private float lastAttackTime = 0f;
    private float nextComboTime = 0f;
    private bool isJumping = false;

    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        agent = GetComponent<NavMeshAgent>();
        agent.updateRotation = false;
        agent.updateUpAxis = false;
        StartCoroutine(RandomBehavior());
    }

    private void Update()
    {
        if (agent.isOnOffMeshLink && !isJumping)
        {
            StartCoroutine(HandleJump());
            return;
        }

        if (player == null) return;

        float distanceToPlayer = Vector3.Distance(transform.position, player.transform.position);

        if (CanSeePlayer())
        {
            isChasing = true;
            isSearching = false;
            searchTimer = 0f;
            lastSeenPlayerPosition = player.transform.position;

            if (distanceToPlayer <= attackRange)
            {
                if (!isAttacking && Time.time >= nextComboTime)
                {
                    StartCoroutine(PerformAttack());
                }
                agent.ResetPath();
            }
            else if (distanceToPlayer <= runAwayRange)
            {
                RunAwayFromPlayer();
            }
            else if (distanceToPlayer <= throwProjectileRange)
            {
                agent.ResetPath();
                ThrowProjectile();
            }
            else
            {
                agent.SetDestination(player.transform.position);
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
            searchTimer -= Time.deltaTime;
            if (searchTimer > 0)
            {
                agent.SetDestination(lastSeenPlayerPosition);
            }
            else
            {
                isSearching = false;
                StartCoroutine(RandomBehavior());
            }
        }
    }

    private bool CanSeePlayer()
    {
        return player != null && Vector3.Distance(transform.position, player.transform.position) <= detectionRange;
    }

    private void RunAwayFromPlayer()
    {
        if (player == null) return;

        Vector3 runDirection = (transform.position - player.transform.position).normalized;
        Vector3 runTo = transform.position + runDirection * 5f;

        NavMeshHit hit;
        if (NavMesh.SamplePosition(runTo, out hit, 2f, NavMesh.AllAreas))
        {
            agent.SetDestination(hit.position);
        }
    }

    private void ThrowProjectile()
    {
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

        while (projectile != null && timer < lifetime)
        {
            if (player != null && Vector3.Distance(projectile.transform.position, player.transform.position) < 1.5f)
            {
                PlayerHealth health = player.GetComponent<PlayerHealth>();
                if (health != null) health.TakeDamage(attackDamage);
                Destroy(projectile);
                break;
            }

            timer += Time.deltaTime;
            yield return null;
        }

        if (projectile != null) Destroy(projectile);
    }

    private IEnumerator PerformAttack()
    {
        isAttacking = true;
        nextComboTime = Time.time + Random.Range(attackCooldownMin, attackCooldownMax);

        yield return new WaitForSeconds(0.5f); // simulate wind-up

        if (player != null && Vector3.Distance(transform.position, player.transform.position) <= attackRange)
        {
            PlayerHealth health = player.GetComponent<PlayerHealth>();
            if (health != null) health.TakeDamage(attackDamage);

            Rigidbody playerRb = player.GetComponent<Rigidbody>();
            if (playerRb != null)
            {
                Vector3 knockbackDir = (player.transform.position - transform.position).normalized;
                playerRb.AddForce(knockbackDir * knockbackForce, ForceMode.Impulse);
            }
        }

        yield return new WaitForSeconds(0.3f); // simulate attack recovery
        isAttacking = false;
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
                agent.ResetPath();
            }
            else
            {
                Vector3 randomDirection = Random.insideUnitSphere * 4f;
                randomDirection += transform.position;
                NavMeshHit hit;
                if (NavMesh.SamplePosition(randomDirection, out hit, 2f, NavMesh.AllAreas))
                {
                    agent.SetDestination(hit.position);
                }
            }

            yield return new WaitForSeconds(waitTime);
        }
    }

    private IEnumerator IdleAnimation()
    {
        yield return new WaitForSeconds(Random.Range(1f, patrolTime));
    }

    private IEnumerator HandleJump()
    {
        isJumping = true;
        yield return new WaitForSeconds(0.3f);
        agent.CompleteOffMeshLink();
        isJumping = false;
    }
}
