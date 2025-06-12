using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using TMPro;

public class BossAI : MonoBehaviour
{
    [Header("Basic Movement and Combat")]
    public float moveSpeed = 3f;
    public float detectionRange = 7f;
    public float attackRange = 1.8f;
    public float dashForce = 10f;
    public float attackCooldownMin = 1f;
    public float attackCooldownMax = 3f;
    public int attackDamage = 30;
    public float knockbackForce = 6f;
    public float chaseLostDuration = 5f;

    [Header("Retreat Parameters")]
    public float retreatDurationMin = 3f;
    public float retreatDurationMax = 6f;
    public float retreatShuffleDistance = 2f;
    public float retreatShuffleSpeed = 3f;
    public float retreatBackDistance = 4f;
    public float retreatBreakRange = 1.5f;

    [Header("Fireball Parameters")]
    public GameObject fireballPrefab;
    public GameObject explosionVFXPrefab;
    public TextMeshProUGUI fireballCountdownTextPrefab;
    public float fireballDashSpeed = 25f;
    public float fireballCountdownDuration = 3f;
    public int fireballDamage = 40;

    public Transform groundCheck;
    public LayerMask groundLayer;

    private Rigidbody rb;
    private NavMeshAgent agent;
    private AgentLinkMover linkMover;
    private GameObject player;
    private bool isGrounded;
    private bool isChasing = false;
    private bool isSearching = false;
    private bool isAttacking = false;
    private float nextComboTime = 0f;
    private float searchTimer = 0f;
    private Vector3 lastSeenPlayerPosition;
    private bool bossActivated = false;

    private bool isRetreating = false;
    private float retreatEndTime;
    private Vector3 retreatCenterPoint;
    private float retreatShuffleDirection = 1f;

    private enum RetreatState { None, BackingOut, Shuffling }
    private RetreatState retreatState = RetreatState.None;
    private Vector3 retreatBackTarget;

    private bool isFireballActive = false;

    private TextMeshProUGUI fireballCountdownInstance;

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

        BossHealthBar.Instance.Hide();

        StartCoroutine(FireballCheckRoutine());
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

        if (isFireballActive)
        {
            return;
        }

        if (isRetreating)
        {
            HandleRetreat();
            anim.SetBool("Walking", true);
        }
        else
        {
            if (CanSeePlayer())
            {
                if (!bossActivated)
                {
                    anim.SetBool("Walking", true);
                    bossActivated = true;
                    BossHealthBar.Instance.Show();
                }

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
                    agent.ResetPath();
                }
            }
        }

        if (!(isRetreating && retreatState == RetreatState.BackingOut))
        {
            FacePlayer(); // 🔥 Boss faces player unless backing out
        }

        Vector3 fixedPos = transform.position;
        fixedPos.z = -16.4f;
        transform.position = fixedPos;
    }

    private IEnumerator FireballCheckRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(7f);

            if (isFireballActive) continue;

            int rand = Random.Range(1, 5);

            if (rand == 1 && (isChasing || isRetreating) && CanSeePlayer())
            {
                StartCoroutine(FireballSequence());
            }
        }
    }

    private IEnumerator FireballSequence()
    {
        isAttacking = false;
        isRetreating = false;
        agent.isStopped = true;
        agent.ResetPath();
        agent.enabled = false;

        Vector3 dashDirection = (transform.position - player.transform.position).normalized;
        dashDirection.y = 0;
        dashDirection.z = 0;

        float dashDuration = 0.3f;
        float dashElapsed = 0f;

        rb.isKinematic = false;

        while (dashElapsed < dashDuration)
        {
            rb.linearVelocity = dashDirection * fireballDashSpeed;
            dashElapsed += Time.deltaTime;
            yield return null;
        }

        rb.linearVelocity = Vector3.zero;
        rb.isKinematic = true;

        isFireballActive = true;

        if (fireballCountdownTextPrefab != null)
        {
            Vector3 textPos = transform.position + new Vector3(0, 3f, 0);
            fireballCountdownInstance = Instantiate(fireballCountdownTextPrefab, textPos, Quaternion.identity, transform);
            fireballCountdownInstance.text = "Fireball: 3";
        }

        float countdown = fireballCountdownDuration;
        while (countdown > 0)
        {
            countdown -= Time.deltaTime;
            if (fireballCountdownInstance != null)
            {
                anim.SetTrigger("Hit");
                fireballCountdownInstance.text = $"Fireball: {Mathf.CeilToInt(countdown)}";
            }
            yield return null;
        }

        if (fireballCountdownInstance != null)
        {
            Destroy(fireballCountdownInstance.gameObject);
        }

        Vector3 fireballSpawnPos = transform.position + Vector3.up * 1.5f;

        Vector3 targetPos = player.transform.position;
        targetPos.z = fireballSpawnPos.z;

        GameObject fireballInstance = Instantiate(fireballPrefab, fireballSpawnPos, Quaternion.identity);
        Fireball fireballScript = fireballInstance.GetComponent<Fireball>();
        if (fireballScript != null)
        {
            fireballScript.Initialize(targetPos, fireballDamage, explosionVFXPrefab);
        }

        agent.enabled = true;
        agent.isStopped = false;

        isFireballActive = false;
    }

    private void HandleRetreat()
    {
        agent.isStopped = false;

        float distanceToPlayer = Vector3.Distance(transform.position, player.transform.position);

        if (distanceToPlayer < retreatBreakRange)
        {
            EndRetreat();
            return;
        }

        if (Time.time > retreatEndTime)
        {
            EndRetreat();
            return;
        }

        if (retreatState == RetreatState.BackingOut)
        {
            if (agent.destination != retreatBackTarget)
                agent.SetDestination(retreatBackTarget);

            if (!agent.pathPending && agent.remainingDistance <= 0.3f)
            {
                retreatCenterPoint = transform.position;
                retreatState = RetreatState.Shuffling;
                retreatShuffleDirection = 1f;
            }

            FaceAwayFromPlayer();
        }
        else if (retreatState == RetreatState.Shuffling)
        {
            float targetX = retreatCenterPoint.x + retreatShuffleDirection * retreatShuffleDistance;
            Vector3 targetPosition = new Vector3(targetX, transform.position.y, transform.position.z);

            agent.SetDestination(targetPosition);

            float distance = Mathf.Abs(transform.position.x - targetX);
            if (!agent.pathPending && distance < 0.2f)
            {
                retreatShuffleDirection *= -1f;
            }

            FacePlayer();
        }
    }

    private void FaceAwayFromPlayer()
    {
        if (player == null) return;

        Vector3 direction = player.transform.position - transform.position;
        if (direction.x > 0)
            transform.rotation = Quaternion.Euler(0, 180, 0);
        else
            transform.rotation = Quaternion.Euler(0, 0, 0);
    }

    private void EndRetreat()
    {
        isRetreating = false;
        retreatState = RetreatState.None;
        agent.speed = moveSpeed;
        agent.ResetPath();
    }

    private bool CanSeePlayer()
    {
        if (player == null) return false;
        return Vector3.Distance(transform.position, player.transform.position) <= detectionRange;
    }

    private void ChasePlayer()
    {
        if (isRetreating) return;
        if (player != null)
        {
            agent.SetDestination(player.transform.position);
        }
    }

    private void MoveToLastSeenDirection()
    {
        if (isRetreating) return;
        agent.SetDestination(lastSeenPlayerPosition);
    }

    private IEnumerator PerformAttack()
    {
        isAttacking = true;
        int comboCount = Random.Range(1, 4);

        for (int i = 0; i < comboCount; i++)
        {
            int attackType = Random.Range(0, 3);

            if (attackType == 0)
            {
                anim.SetTrigger("Hit2");
            }
            else if (attackType == 1)
            {
                anim.SetTrigger("Uppercut");
            }
            else
            {
                anim.SetTrigger("High Kick");
            }

            yield return new WaitForSeconds(0.4f);

            if (Vector3.Distance(transform.position, player.transform.position) <= attackRange + 0.5f)
            {
                PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();
                if (playerHealth != null)
                {
                    playerHealth.TakeDamage(attackDamage);
                }

                Rigidbody playerRb = player.GetComponent<Rigidbody>();
                if (playerRb != null)
                {
                    Vector3 knockbackDir = (player.transform.position - transform.position).normalized + Vector3.up * 0.3f;
                    playerRb.AddForce(knockbackDir * knockbackForce, ForceMode.Impulse);
                }
            }

            yield return new WaitForSeconds(Random.Range(attackCooldownMin, attackCooldownMax));
        }

        transform.rotation = Quaternion.identity;

        isAttacking = false;
        nextComboTime = Time.time + Random.Range(attackCooldownMin, attackCooldownMax);
    }

    private void FacePlayer()
    {
        if (player == null) return;

        Vector3 direction = player.transform.position - transform.position;
        if (direction.x > 0)
        {
            transform.rotation = Quaternion.Euler(0, 0, 0);
        }
        else
        {
            transform.rotation = Quaternion.Euler(0, 180, 0);
        }
    }

    public void TriggerRetreat()
    {
        if (!isRetreating)
        {
            EnterRetreatState();
        }
    }

    public void EnterRetreatState()
    {
        isRetreating = true;
        retreatEndTime = Time.time + Random.Range(retreatDurationMin, retreatDurationMax);

        Vector3 retreatDirection = (transform.position - player.transform.position).normalized;
        retreatBackTarget = transform.position + retreatDirection * retreatBackDistance;

        retreatBackTarget.y = transform.position.y;
        retreatBackTarget.z = transform.position.z;

        retreatState = RetreatState.BackingOut;

        agent.speed = retreatShuffleSpeed;
        agent.SetDestination(retreatBackTarget);
    }
}
