using System.Collections;
using UnityEngine;

public class Entity : MonoBehaviour
{
    
    protected Animator anim;
    protected Rigidbody2D rb;
    protected Collider2D col;
    protected SpriteRenderer sr;

    [Header("Health")]
    [SerializeField] protected int maxHealth = 1;
    [SerializeField] protected int currentHealth;
    [SerializeField] private Material damageMaterial;
    [SerializeField] private float damageFeedbackDuration = .1f;
    private Coroutine damageFeedbackCoroutine;

    // ADDED: To store the default material
    private Material defaultMaterial;

    [Header("Attack details")]
    [SerializeField] protected float attackRadius;
    [SerializeField] protected Transform attackPoint;
    [SerializeField] protected LayerMask whatIsTarget;

    [Header("Collision details")]
    [SerializeField] private float groundCheckDistance;
    [SerializeField] private LayerMask whatIsGround;
    protected bool isGrounded;

    //facing direction details
    protected int facingDir = 1;
    protected bool facingRight = true;
    protected bool canMove = true;

    protected virtual void Awake()
    {
      
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();
        anim = GetComponentInChildren<Animator>();
        sr = GetComponentInChildren<SpriteRenderer>();
        
        if (sr != null)
            defaultMaterial = sr.material;

        if (rb != null && rb.gravityScale <= 0)
            rb.gravityScale = 1;

        currentHealth = maxHealth;
     
    }

    public virtual void SetEntityStats(int maxHP, float speed, float jump, Color tintColor)
    {
        maxHealth = maxHP;
        currentHealth = maxHP;
        if (sr != null)
            sr.color = tintColor;
    }

    protected virtual void Update()
    {
        HandCollision();
        HandleAnimations();
        HandleFlip();
    }

    protected virtual void FixedUpdate()
    {
        HandleMovement();

        // Prevent "buoyancy" or lifting by zeroing Y velocity if firmly grounded and not jumping
        if (isGrounded && rb.linearVelocity.y < 0.1f && rb.linearVelocity.y > -0.1f)
        {
            // Optional: slight downward force to keep them snapped to ground
            // rb.linearVelocity = new Vector2(rb.linearVelocity.x, -0.01f);
        }
    }

    public virtual void DamageTargets()
    {
        if (attackPoint == null) return;

        Collider2D[] enemyColliders = Physics2D.OverlapCircleAll(attackPoint.position, attackRadius, whatIsTarget);
        foreach (Collider2D enemy in enemyColliders)
        {
            Entity entityTarget = enemy.GetComponent<Entity>();
           
             entityTarget.TakeDamage();
        }
    }

    public virtual void TakeDamage()
    {
        TakeDamage(1);
    }

    public virtual void TakeDamage(int damage)
    {
        currentHealth = currentHealth - damage;
        PlayDamageFeedback();

        if (currentHealth <= 0)
            Die();
    }
    protected virtual void Die()
    {
        if (AudioManager.instance != null)
            AudioManager.instance.PlayKillSound();

        anim.enabled = false;
        col.enabled = false;
        rb.gravityScale = 12;
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, 15);
        Destroy(gameObject, 3);
    }


    private void PlayDamageFeedback()
    {
        if (damageFeedbackCoroutine != null)
            StopCoroutine(damageFeedbackCoroutine);

        StartCoroutine(DamageFeedbackCo());
    }
    private IEnumerator DamageFeedbackCo()
    {
        Material originalMat = sr.material;

        sr.material = damageMaterial;

        // Wait for the duration
        yield return new WaitForSeconds(damageFeedbackDuration);

        // FIX: Explicitly set the material back to the original
        sr.material = defaultMaterial;
    }


    // ... (rest of your movement and collision methods remain the same)

    public virtual void EnableMovement(bool enable)
    {
        canMove = enable;

    }

    protected void HandleAnimations()
    {
        if (anim != null && rb != null)
        {
            anim.SetFloat("xVelocity", Mathf.Abs(rb.linearVelocity.x));
            anim.SetFloat("yVelocity", rb.linearVelocity.y);
            anim.SetBool("isGrounded", isGrounded);
        }
    }


    protected virtual void HandleAttack()
    {
        if (anim != null) 
        {
            anim.SetTrigger("attack");
            if (AudioManager.instance != null)
                AudioManager.instance.PlayAttackSound();
        }
    }


    protected virtual void HandleMovement()
    {

    }

    protected virtual void HandCollision()
    {
        if (col != null)
        {
            // EXCLUDE the entity's own layer to prevent self-collision
            LayerMask mask = whatIsGround.value == 0 ? (LayerMask)(~0) : whatIsGround;
            mask &= ~(1 << gameObject.layer);

            // Use a sensible default if groundCheckDistance is too large or small
            float checkDistance = groundCheckDistance;
            if (checkDistance <= 0) checkDistance = 0.1f;
            if (checkDistance > 0.5f) checkDistance = 0.1f; 

            isGrounded = Physics2D.Raycast(col.bounds.center, Vector2.down, col.bounds.extents.y + checkDistance, mask);
        }

        // Safety check for attackPoint to avoid console errors
        if (attackPoint == null && this is Enemy)
        {
            // Only log once to avoid spam, or just return safely
            return;
        }
    }

    protected virtual void HandleFlip()
    {
        if (rb.linearVelocity.x > 0 && facingRight == false)
            Flip();
        else if (rb.linearVelocity.x < 0 && facingRight == true)
            Flip();
    }

    public void Flip()
    {
        transform.Rotate(0, 180, 0);
        facingRight = !facingRight;
        facingDir =  facingDir * -1;
    }

    private void OnDrawGizmos()
    {
        Collider2D collider = GetComponent<Collider2D>();
        if (collider != null)
        {
            Gizmos.DrawLine(collider.bounds.center, collider.bounds.center + new Vector3(0, -(collider.bounds.extents.y + groundCheckDistance)));
        }
        else
        {
            Gizmos.DrawLine(transform.position, transform.position + new Vector3(0, -groundCheckDistance));
        }

        if (attackPoint != null)
            Gizmos.DrawWireSphere(attackPoint.position, attackRadius);
    }
}
