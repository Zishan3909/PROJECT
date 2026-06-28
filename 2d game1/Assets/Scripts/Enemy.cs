using UnityEngine;

public class Enemy : Entity
{
    private Transform player;
    private Transform girl;
    private Transform currentTarget;

    private bool targetDetected;

    [Header("Movement details")]
    [SerializeField] protected float moveSpeed = 3.5f;

    protected override void Awake()
    {
        base.Awake();
        
        // Find targets
        Player p = FindFirstObjectByType<Player>();
        if (p != null) player = p.transform;

        ObjectToProtect g = FindFirstObjectByType<ObjectToProtect>();
        if (g != null) girl = g.transform;
    }

    protected override void Update()
    {
        SelectTarget();
        base.Update();
       
        HandleAttack();
    }

    private void SelectTarget()
    {
        // Prioritize the girl if she exists, otherwise target the player
        if (girl != null)
            currentTarget = girl;
        else if (player != null)
            currentTarget = player;
    }

    protected override void HandleFlip()
    {
        if (currentTarget == null) return;

        // Face the current target
        if (currentTarget.position.x > transform.position.x && !facingRight)
            Flip();
        else if (currentTarget.position.x < transform.position.x && facingRight)
            Flip();
    }

    protected override void HandleAttack()
    {
        // Only attack if target detected
        if (targetDetected)
            anim.SetTrigger("attack");
    }

    protected override void HandleMovement()
    {
        if (currentTarget == null) return;

        // Calculate direction to target
        float direction = currentTarget.position.x > transform.position.x ? 1 : -1;

        // Only move if we are not already close enough to attack
        float distanceToTarget = Vector2.Distance(transform.position, currentTarget.position);
        
        if (canMove && distanceToTarget > attackRadius * 0.8f)
            rb.linearVelocity = new Vector2(direction * moveSpeed, rb.linearVelocity.y);
        else
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
    }
    
    protected override void HandCollision()
    {
        base.HandCollision();

        if (attackPoint == null) return;

        // FIX: Exclude Enemy layer from whatIsTarget to prevent "attacking anywhere" (self-detection)
        LayerMask targetMask = whatIsTarget;
        targetMask &= ~(1 << gameObject.layer);

        targetDetected = Physics2D.OverlapCircle(attackPoint.position, attackRadius, targetMask);
    }

    protected override void Die()
    {
        base.Die();
        UI.instance.AddKillCount();
    }
}