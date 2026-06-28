using UnityEngine;

public class Player : Entity
{
    [Header("Player Details")]
    [SerializeField] private float moveSpeed = 10f;
    [SerializeField] private float jumpForce = 15f;
    [SerializeField] private bool canJump = true;

    private float xInput;

    [Header("Unique Ability System")]
    public float abilityCooldown = 0f;
    public float abilityMaxCooldown = 8f;
    public float abilityDuration = 0f;
    
    private bool isInvincible = false;
    private bool isDashing = false;
    private float dashDirection = 1f;
    private bool isRaging = false;
    private float originalGravityScale;

    protected override void Awake()
    {
        base.Awake();
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            originalGravityScale = rb.gravityScale;
        }
    }

    protected override void Update()
    {
        CheckInput();
        base.Update();
       
        if (Input.GetKeyDown(KeyCode.Space))
            TryToJump();
        
        if (Input.GetKeyDown(KeyCode.K) || Input.GetMouseButtonDown(0))
            HandleAttack();

        // Listen for Unique Ability Trigger
        if (Input.GetKeyDown(KeyCode.E))
            TryTriggerAbility();

        // Handle Active Ability Timers
        HandleAbilityCooldowns();
    }

    private void CheckInput()
    {
        xInput = Input.GetAxisRaw("Horizontal");
    }

    protected override void HandleMovement()
    {
        if (isDashing)
        {
            if (rb != null)
                rb.linearVelocity = new Vector2(dashDirection * 28f, 0f);
            return;
        }

        if (canMove)
            rb.linearVelocity = new Vector2(xInput * moveSpeed, rb.linearVelocity.y);
        else
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
    }

    private void TryToJump()
    {
        if (isGrounded && canJump && !isDashing)
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
    }

    public override void SetEntityStats(int maxHP, float speed, float jump, Color tintColor)
    {
        base.SetEntityStats(maxHP, speed, jump, tintColor);
        moveSpeed = speed;
        jumpForce = jump;
    }

    public override void TakeDamage()
    {
        // Ignore damage if Shield or Dash is active!
        if (isInvincible) return;

        int damage = 1;
        if (GameManager.instance != null)
        {
            if (GameManager.instance.currentDifficulty == GameManager.Difficulty.Medium)
                damage = 2;
            else if (GameManager.instance.currentDifficulty == GameManager.Difficulty.High)
                damage = 3;
        }

        base.TakeDamage(damage);
    }

    public override void DamageTargets()
    {
        if (attackPoint == null) return;
        
        float currentRadius = attackRadius;
        if (isRaging)
        {
            currentRadius = attackRadius * 1.8f; // Warrior Rage has a massive slash radius!
        }

        Collider2D[] enemyColliders = Physics2D.OverlapCircleAll(attackPoint.position, currentRadius, whatIsTarget);
        foreach (Collider2D enemy in enemyColliders)
        {
            Entity entityTarget = enemy.GetComponent<Entity>();
            if (entityTarget != null)
            {
                if (isRaging)
                    entityTarget.TakeDamage(2); // Warrior Rage deals 2 damage per hit!
                else
                    entityTarget.TakeDamage(1);
            }
        }
    }

    protected override void Die()
    {
        base.Die();
        if (AudioManager.instance != null)
            AudioManager.instance.PlayGameOverSound();
            
        UI.instance.EnableGameOverUI();
    }

    // Unique Ability Helpers
    private void TryTriggerAbility()
    {
        if (abilityCooldown > 0f) return;
        if (GameManager.instance == null) return;

        GameManager.CharacterType hero = GameManager.instance.currentCharacter;

        // Play Synthesized Ability SFX
        if (AudioManager.instance != null)
            AudioManager.instance.PlayAbilitySound();

        if (hero == GameManager.CharacterType.Knight)
        {
            isInvincible = true;
            abilityDuration = 3f;
            abilityCooldown = abilityMaxCooldown;
        }
        else if (hero == GameManager.CharacterType.Rogue)
        {
            isDashing = true;
            isInvincible = true;
            abilityDuration = 0.25f;
            abilityCooldown = abilityMaxCooldown;
            dashDirection = facingDir;
            if (rb != null)
            {
                rb.gravityScale = 0f;
                rb.linearVelocity = new Vector2(dashDirection * 28f, 0f);
            }
        }
        else if (hero == GameManager.CharacterType.Wizard)
        {
            abilityCooldown = abilityMaxCooldown;
            TriggerManaFlare();
        }
        else if (hero == GameManager.CharacterType.Warrior)
        {
            isRaging = true;
            abilityDuration = 4.0f;
            abilityCooldown = abilityMaxCooldown;
            originalGravityScale = rb != null ? rb.gravityScale : 2f;
            moveSpeed = moveSpeed * 1.6f;
        }
    }

    private void HandleAbilityCooldowns()
    {
        if (GameManager.instance != null && GameManager.instance.isGameActive)
        {
            if (abilityCooldown > 0f)
                abilityCooldown -= Time.deltaTime;

            if (abilityDuration > 0f)
            {
                abilityDuration -= Time.deltaTime;
                UpdateAbilityVisuals();

                if (abilityDuration <= 0f)
                {
                    EndAbility();
                }
            }
        }
    }

    private void UpdateAbilityVisuals()
    {
        if (sr == null) return;

        float flash = Mathf.PingPong(Time.time * 15f, 1f);

        if (isInvincible && GameManager.instance.currentCharacter == GameManager.CharacterType.Knight)
        {
            // Bulky Golden Aura
            sr.color = Color.Lerp(new Color(1f, 0.84f, 0.3f, 1f), new Color(1f, 1f, 1f, 1f), flash);
        }
        else if (isDashing)
        {
            // Bright green agile neon flash
            sr.color = Color.Lerp(new Color(0.3f, 0.9f, 0.4f, 1f), new Color(1f, 1f, 1f, 0.6f), flash);
        }
        else if (isRaging)
        {
            // Crimson Berserker Rage glow
            sr.color = Color.Lerp(new Color(0.9f, 0.2f, 0.2f, 1f), new Color(1f, 1f, 1f, 1f), flash);
        }
    }

    private void EndAbility()
    {
        isInvincible = false;
        isDashing = false;
        isRaging = false;

        if (rb != null)
            rb.gravityScale = originalGravityScale;

        if (GameManager.instance != null)
        {
            GameManager.CharacterData data = GameManager.instance.GetCharacterData(GameManager.instance.currentCharacter);
            moveSpeed = data.speed;
            if (sr != null)
                sr.color = data.color;
        }
    }

    private void TriggerManaFlare()
    {
        // Magical overlap blast damages nearby targets
        Collider2D[] cols = Physics2D.OverlapCircleAll(transform.position, 5f, whatIsTarget);
        foreach (Collider2D col in cols)
        {
            Entity enemy = col.GetComponent<Entity>();
            if (enemy != null)
            {
                enemy.TakeDamage();
            }
        }

        // Trigger wizard blast visual flash
        StartCoroutine(WizardBlastRoutine());
    }

    private System.Collections.IEnumerator WizardBlastRoutine()
    {
        if (sr != null)
        {
            sr.color = new Color(0.9f, 0.3f, 1f, 1f); // purple magic flare
            Vector3 originalScale = transform.localScale;
            transform.localScale = originalScale * 1.5f;
            yield return new WaitForSeconds(0.18f);
            transform.localScale = originalScale;
            if (GameManager.instance != null)
                sr.color = GameManager.instance.GetCharacterData(GameManager.instance.currentCharacter).color;
        }
    }

    public float GetAbilityCooldownNormalized()
    {
        if (abilityMaxCooldown <= 0f) return 0f;
        return Mathf.Clamp01(abilityCooldown / abilityMaxCooldown);
    }
}