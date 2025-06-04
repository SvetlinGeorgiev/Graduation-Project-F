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
            else transform.rotation = Quaternion.Euler(50, 0, -50);

            if (Vector3.Distance(transform.position, player.transform.position) < 2f)
            {
                PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();
                if (playerHealth != null) playerHealth.TakeDamage(attackDamage);

                Rigidbody playerRb = player.GetComponent<Rigidbody>();
                if (playerRb != null)
                {
                    Vector3 knockbackDir = (player.transform.position - transform.position).normalized;
                    playerRb.AddForce(knockbackDir * knockbackForce, ForceMode.Impulse);
                }
            }

            yield return new WaitForSeconds(0.4f);
        }

        transform.rotation = Quaternion.identity;
        nextComboTime = Time.time + Random.Range(attackCooldownMin, attackCooldownMax);
        isAttacking = false;
    }

    private IEnumerator HandleJump()
    {
        isJumping = true;
        agent.isStopped = true;

        OffMeshLinkData linkData = agent.currentOffMeshLinkData;
        Vector3 startPos = agent.transform.position;
        Vector3 endPos = linkData.endPos + Vector3.up * agent.baseOffset;

        float jumpDuration = 0.5f;
        float elapsedTime = 0f;

        while (elapsedTime < jumpDuration)
        {
            float t = elapsedTime / jumpDuration;
            float height = 2f * Mathf.Sin(Mathf.PI * t); // jump arc

            agent.transform.position = Vector3.Lerp(startPos, endPos, t) + Vector3.up * height;
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        agent.CompleteOffMeshLink();
        agent.isStopped = false;
        isJumping = false;
    }
}
