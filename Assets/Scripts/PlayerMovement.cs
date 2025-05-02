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

    [Header("Flip Settings")]
    public float flipSpeed = 360f;

    [Header("Knockback Settings")]
    public float knockbackForce = 10f;
    public float knockbackDuration = 0.2f;
    private bool isKnockedBack = false;

    private void Awake()
    {
        controls = new PlayerControls();
        controls.Player.Move.performed += ctx => moveInput = ctx.ReadValue<Vector2>();
        controls.Player.Move.canceled += ctx => moveInput = Vector2.zero;
        controls.Player.Jump.performed += ctx => Jump();

        rb = GetComponent<Rigidbody>();
    }

    private void OnEnable() => controls.Enable();
    private void OnDisable() => controls.Disable();

    private void FixedUpdate()
    {
        if (!isKnockedBack)
        {
            rb.linearVelocity = new Vector3(moveInput.x * moveSpeed, rb.linearVelocity.y, 0);
        }
    }

    private void Jump()
    {
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
}
