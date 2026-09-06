using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Animator))]
public class Enemy2Script : MonoBehaviour
{
    [Header("Projectile Settings")] public GameObject projectilePrefab;
    public Transform firePoint;

    private bool isStunned = false;
    private bool isBeingSlammed = false;

    [Header("Ground Check")] public bool isGrounded;
    public Transform groundCheckPoint;
    public float groundCheckRadius = 0.2f;
    public LayerMask groundLayer;

    [Header("References")] public Transform player;
    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private Rigidbody2D rb2d;

    [Header("Movement Settings")] public float moveSpeed = 3f;
    public float stoppingDistance = 5f;

    [Header("Attack Settings")] public float attackRate = 2f;
    private float nextAttackTime = 0f;

    [Header("Health Settings")] public int maxHealth = 2;
    private int currentHealth;
    private bool isDead = false;

    void Start()
    {
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        rb2d = GetComponent<Rigidbody2D>();
        currentHealth = maxHealth;

        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null) player = playerObj.transform;
        }
    }
    
    void Update()
    {
        if (isDead || player == null) return;
        
        if (isStunned && !isBeingSlammed)
        {
            if (rb2d != null) rb2d.linearVelocity = Vector2.zero;
            return;
        }
        
        if (isBeingSlammed) return; 

        float distance = Vector2.Distance(transform.position, player.position);
        FlipSprite();

        if (distance <= stoppingDistance)
        {
            animator.SetBool("isMoving", false);
            rb2d.linearVelocity = Vector2.zero; 

            if (Time.time >= nextAttackTime)
            {
                AttackPlayer();
                nextAttackTime = Time.time + attackRate;
            }
        }
        else
        {
            MoveTowardsPlayer();
        }
    }

    void FixedUpdate()
    {
        if (groundCheckPoint != null)
        {
            bool wasGroundedBefore = isGrounded;
            isGrounded = Physics2D.OverlapCircle(groundCheckPoint.position, groundCheckRadius, groundLayer);

            if (isGrounded && !wasGroundedBefore && isBeingSlammed)
            {
                rb2d.linearVelocity = Vector2.zero;
                isBeingSlammed = false;
            }
        }
    }

    void MoveTowardsPlayer()
    {
        animator.SetBool("isMoving", true);
        Vector2 direction = (player.position - transform.position).normalized;
        rb2d.linearVelocity = direction * moveSpeed;
    }

    void AttackPlayer()
    {
        animator.SetTrigger("atk");
    }

    public void SpawnProjectileEvent()
    {
        if (isDead || isStunned) return;

        if (projectilePrefab != null && firePoint != null)
        {
            GameObject newProjectile = Instantiate(projectilePrefab, firePoint.position, Quaternion.identity);
            EnemyProjectile bulletScript = newProjectile.GetComponent<EnemyProjectile>();
        
            if (bulletScript != null)
            {
                float shootDirection = spriteRenderer.flipX ? 1f : -1f;
                bulletScript.SetupDirection(shootDirection);
            }
        }
    }


    void FlipSprite()
    {
        // Record the current flip state before changing it
        bool wasFlippedRight = spriteRenderer.flipX;

        if (player.position.x > transform.position.x)
        {
            spriteRenderer.flipX = true;
        }
        else if (player.position.x < transform.position.x)
        {
            spriteRenderer.flipX = false;
        }
        
        if (wasFlippedRight != spriteRenderer.flipX && firePoint != null)
        {
            Vector3 localPos = firePoint.localPosition;
            localPos.x = -localPos.x;
            firePoint.localPosition = localPos;
        }
    }

    public void TakeDamage(int damageAmount)
{
    if (isDead) return;
    currentHealth -= damageAmount;
    if (currentHealth <= 0) Defeat();
}

void Defeat()
{
    isDead = true;
    rb2d.linearVelocity = Vector2.zero;
    animator.SetTrigger("defeat");
    Destroy(gameObject, 2f);
}

public void KnockbackStun(float duration)
{
    if (isDead) return;
    isStunned = true;
    animator.SetBool("isMoving", false); 
    rb2d.linearVelocity = Vector2.zero;
    Invoke(nameof(EndStun), duration);
}

public void TriggerSlamDown(float downwardForce)
{
    if (isDead) return;
    isStunned = true;
    isBeingSlammed = true;
    animator.SetBool("isMoving", false);
    rb2d.linearVelocity = new Vector2(0f, -downwardForce); 
}

private void EndStun()
{
    isStunned = false;
    isBeingSlammed = false; 
}

}
