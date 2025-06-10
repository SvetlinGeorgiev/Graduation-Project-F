using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class PlayerMovement : MonoBehaviour
{
    private PlayerControls controls;
    private Rigidbody rb;
    private Vector2 moveInput;
    [SerializeField]
    private Animator anim;

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

    [Header("Special Attack Settings")]
    public float chainPunchCooldown = 5f;
    public int chainPunchCount = 5;
    public float chainPunchTiltDuration = 0.5f;
    public float chainPunchResetDuration = 0.3f;
    private bool isChainPunching = false;
    private bool canUseChainPunch = true;

    private Quaternion originalRotation;

    public bool isClimbing = false;
    public bool canAttack = true;

    [SerializeField]
    private bool facingRight = true;

    private void Awake()
    {
        controls = new PlayerControls();
        rb = GetComponent<Rigidbody>();
        originalRotation = transform.rotation;

        controls.Player.Move.performed += ctx => moveInput = ctx.ReadValue<Vector2>();
        controls.Player.Move.canceled += ctx => moveInput = Vector2.zero;
        controls.Player.Jump.performed += ctx => Jump();
        controls.Player.Attack.performed += ctx => AttemptAttack();
        controls.Player.Kick.performed += ctx => AttemptKick();
        controls.Player.Roll.performed += ctx => Roll();
        controls.Player.SpecialAttack.performed += ctx => TryChainPunch();
    }

    private void OnEnable() => controls.Enable();
    private void OnDisable() => controls.Disable();

    private void FixedUpdate()
    {
     
        if (!isKnockedBack && !isClimbing && !isChainPunching)
        {
            rb.linearVelocity = new Vector3(moveInput.x * moveSpeed, rb.linearVelocity.y, 0);
            if (moveInput.x > 0)
            {
                anim.SetBool("Walk", true);
                facingRight = true;
                anim.SetBool("FacingRight", true);
                transform.rotation = new Quaternion(0, -1, 0, 0);
            }
            else if (moveInput.x < 0)
            {
                anim.SetBool("Walk", true);
                facingRight = false;
                anim.SetBool("FacingRight", false);
                transform.rotation = new Quaternion(0, 0, 0, 0);
            }
            else if (moveInput.x == 0)
            {
                anim.SetBool("Walk", false);
            }
          

        }
    }
   
    private void Jump()
    {
        anim.SetBool("Walk", false);
        //.Log("Jump");
        if (isClimbing)
        {
            //Debug.Log("Jumping Climb");
            return;
        }
        if (isGrounded)
        {
            //Debug.Log("Jumping grounded");
           
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, jumpForce, rb.linearVelocity.z);
            isGrounded = false;
            anim.SetTrigger("Jump");
            anim.SetBool("Walk", false);
            canDoubleJump = hasDoubleJumpAbility;

           
        }
        else if (canDoubleJump)
        {
            //Debug.Log("Jumping Double");
            
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, jumpForce, rb.linearVelocity.z);
            canDoubleJump = false;
            StartCoroutine(FlipAnimation());
        }
    }
    private void Roll()
    {
        //Debug.Log("Roll"); 
        if (isGrounded == true)
        {
            //Debug.Log("Roll Animation");
            anim.SetTrigger("Roll");
            if (facingRight)
            {
                //Debug.Log("Roll Right");
                
            }
            else
            {
                //Debug.Log("Roll Left");
                
            }

        }
        return;
    }


    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = true;
            anim.SetBool("Grounded",true);
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            anim.SetBool("Grounded", false);

            isGrounded = false;
        }
    }

    public void UnlockDoubleJump()
    {
        hasDoubleJumpAbility = true;
        //Debug.Log("Double Jump Unlocked!");
    }

    private IEnumerator FlipAnimation()
    {
        anim.SetTrigger("DoubleJump");
        anim.SetBool("Walk", false);
        
        yield return null;
        
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
        if (isAttacking || !canAttack || isChainPunching) return;

       
        comboStep = Random.Range(1, 5);

        if (comboResetCoroutine != null)
            StopCoroutine(comboResetCoroutine);
        

        comboResetCoroutine = StartCoroutine(ResetComboAfterDelay());

        StartCoroutine(PerformAttackTilt(comboStep));
    }
    private void AttemptKick()
    {
        if (isAttacking || !canAttack || isChainPunching) return;

      
        comboStep = Random.Range(1, 3);

        if (comboResetCoroutine != null)
            StopCoroutine(comboResetCoroutine);


        comboResetCoroutine = StartCoroutine(ResetComboAfterDelay());

        StartCoroutine(PerformKick(comboStep));
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

            BossHealth bossHealth = enemy.GetComponent<BossHealth>();
            if (bossHealth != null)
            {
                bossHealth.TakeDamage(attackDamage);
            }
        }
    }


    private IEnumerator PerformAttackTilt(int hit)
    {
        isAttacking = true;
        

        switch (hit)
        {
            case 1: anim.SetTrigger("Attack1"); break;
            case 2: anim.SetTrigger("Attack2"); break;
            case 3: anim.SetTrigger("Attack3"); break;
            case 4: anim.SetTrigger("Attack4"); break;

        }

      

        DetectAndDamageEnemies();

        yield return new WaitForSeconds(1f);
       
        isAttacking = false;
    }
    private IEnumerator PerformKick(int kick)
    {
        isAttacking = true;
        

        switch (kick)
        {
            case 1: anim.SetTrigger("Kick1"); break;
            case 2: anim.SetTrigger("Kick2"); break;
        }

       

        DetectAndDamageEnemies();

        yield return new WaitForSeconds(1f);
        isAttacking = false;
    }

    private void TryChainPunch()
    {
        if (canUseChainPunch && !isChainPunching)
        {
            StartCoroutine(ChainPunchRoutine());
        }
    }
   
    private IEnumerator ChainPunchRoutine()
    {
        isChainPunching = true;
        canUseChainPunch = false;

        Quaternion punchRotation = Quaternion.Euler(0, 0, -20f);

        for (int i = 0; i < chainPunchCount; i++)
        {
            transform.rotation = punchRotation;
            DetectAndDamageEnemies();
            yield return new WaitForSeconds(chainPunchTiltDuration);

            transform.rotation = originalRotation;
            yield return new WaitForSeconds(chainPunchResetDuration);
        }

        isChainPunching = false;

        yield return new WaitForSeconds(chainPunchCooldown);
        canUseChainPunch = true;
    }
}
