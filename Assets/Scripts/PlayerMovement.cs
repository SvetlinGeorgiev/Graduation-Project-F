using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class PlayerMovement : MonoBehaviour
{
    private PlayerControls controls;
    private Rigidbody rb;
    private Vector2 moveInput;

    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float jumpForce = 8f;
    private bool isGrounded;
    public bool canDoubleJump = false;
    private bool hasDoubleJumpAbility = false;

    [Header("Attack Hitbox Settings")]
    public Vector3 attackBoxSize = new Vector3(1f, 1f, 1f);
    public Vector3 attackBoxOffset = new Vector3(1f, 0f, 0f);
    public LayerMask enemyLayer;
    public float attackDamage = 25f;


    [Header("Flip Settings")]
    public float flipSpeed = 360f;

    [Header("Knockback Settings")]
    public float knockbackForce = 10f;
    public float knockbackDuration = 0.2f;
    private bool isKnockedBack = false;

    [Header("Attack Settings")]
    public float comboResetTime = 1.0f;
    private int comboStep = 0;
    private bool isAttacking = false;
    private Coroutine comboResetCoroutine;

    private Quaternion originalRotation;

    public bool isClimbing = false;
    public bool canAttack = true;

    private void Awake()
    {
        controls = new PlayerControls();
        rb = GetComponent<Rigidbody>();
        originalRotation = transform.rotation;

        controls.Player.Move.performed += ctx => moveInput = ctx.ReadValue<Vector2>();
        controls.Player.Move.canceled += ctx => moveInput = Vector2.zero;
        controls.Player.Jump.performed += ctx => Jump();
        controls.Player.Attack.performed += ctx => AttemptAttack();
    }

    private void OnEnable() => controls.Enable();
    private void OnDisable() => controls.Disable();

    private void FixedUpdate()
    {
        if (!isKnockedBack && !isAttacking && !isClimbing)
        {
            rb.linearVelocity = new Vector3(moveInput.x * moveSpeed, rb.linearVelocity.y, 0);
        }

    }

    private void Jump()
    {
        if (isClimbing)
            return; // Don't allow jumping while on a ladder

        if (isGrounded)
        {
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, jumpForce, rb.linearVelocity.z);
            isGrounded = false;
            canDoubleJump = hasDoubleJumpAbility;
        }
        else if (canDoubleJump)
        {
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, jumpForce, rb.linearVelocity.z);
            canDoubleJump = false;
            StartCoroutine(FlipAnimation());
        }
    }


    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = true;
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = false;
        }
    }

    public void UnlockDoubleJump()
    {
        hasDoubleJumpAbility = true;
        Debug.Log("Double Jump Unlocked!");
    }

    private IEnumerator FlipAnimation()
    {
        float rotation = 0;
        while (rotation < 360)
        {
            float step = flipSpeed * Time.deltaTime;
            transform.Rotate(Vector3.forward, step);
            rotation += step;
            yield return null;
        }
        transform.rotation = Quaternion.identity;
    }

    public void TakeDamage(Vector3 damageSource)
    {
        StartCoroutine(Knockback(damageSource));
    }

    private IEnumerator Knockback(Vector3 damageSource)
    {
        isKnockedBack = true;

        Vector3 knockbackDirection = (transform.position - damageSource).normalized;
        knockbackDirection.y = 0.5f;

        rb.linearVelocity = Vector3.zero;
        rb.AddForce(knockbackDirection * knockbackForce, ForceMode.Impulse);

        yield return new WaitForSeconds(knockbackDuration);

        isKnockedBack = false;
    }

    private void AttemptAttack()
    {
        if (isAttacking || !canAttack) return;

        comboStep++;
        if (comboStep > 3) comboStep = 1;

        if (comboResetCoroutine != null)
            StopCoroutine(comboResetCoroutine);

        comboResetCoroutine = StartCoroutine(ResetComboAfterDelay());

        StartCoroutine(PerformAttackTilt(comboStep));
    }

    private IEnumerator ResetComboAfterDelay()
    {
        yield return new WaitForSeconds(comboResetTime);
        comboStep = 0;
    }

    private void DetectAndDamageEnemies()
    {
        Vector3 attackOrigin = transform.position + (transform.right * attackBoxOffset.x);
        Collider[] hitEnemies = Physics.OverlapBox(attackOrigin, attackBoxSize / 2f, Quaternion.identity, enemyLayer);

        foreach (Collider enemy in hitEnemies)
        {
            EnemyHealth health = enemy.GetComponent<EnemyHealth>();
            if (health != null)
            {
                health.TakeDamage(attackDamage);
            }
        }
    }


    private IEnumerator PerformAttackTilt(int step)
    {
        isAttacking = true;
        float tiltDuration = 0.15f;

        Quaternion attackRotation = originalRotation;

        switch (step)
        {
            case 1: attackRotation = Quaternion.Euler(0, 0, 20f); break;
            case 2: attackRotation = Quaternion.Euler(0, 0, -20f); break;
            case 3: attackRotation = Quaternion.Euler(20f, 0, 0); break;
        }

        transform.rotation = attackRotation;

        // Call damage function
        DetectAndDamageEnemies();

        yield return new WaitForSeconds(tiltDuration);
        transform.rotation = originalRotation;
        isAttacking = false;
    }

}
