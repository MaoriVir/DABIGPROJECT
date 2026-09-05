using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Animator))]
public class Enemy1Script : MonoBehaviour
{
    [Header("Projectile Settings")]
    public GameObject projectilePrefab; // Drag your bullet prefab here
    public Transform firePoint;          // Create an empty child object at gun/hand position

    // note to self: so that it wont move while slammed
    private bool isStunned = false;
    
    [Header("Ground Check")]
    public bool isGrounded;
    public Transform groundCheckPoint; // Create an empty child object at enemy feet
    public float groundCheckRadius = 0.2f;
    public LayerMask groundLayer;
    
    [Header("References")]
    public Transform player; // Drag the Player GameObject here in the Inspector
    private Animator animator;
    private SpriteRenderer spriteRenderer;

    [Header("Movement Settings")]
    public float moveSpeed = 3f;
    public float stoppingDistance = 5f;

    [Header("Attack Settings")]
    public float attackRate = 2f; // Time in seconds between attacks
    private float nextAttackTime = 0f;

    [Header("Health Settings")]
    public int maxHealth = 2;
    private int currentHealth;
    private bool isDead = false;

    void Start()
    {
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        currentHealth = maxHealth;

        // --- AUTOMATIC PLAYER FINDER ---
        // If the player slot was left empty, find the object in the scene with the "Player" tag
        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                player = playerObj.transform;
            }
            else
            {
                Debug.LogError("Enemy1Script: Could not find a GameObject with the tag 'Player' in the scene!");
            }
        }
    }

    void Update()
    {
        if (isDead || player == null || isStunned) return;

        // Calculate distance to the player
        float distanceX = Mathf.Abs(transform.position.x - player.position.x);

        // Handle flipping the sprite based on player direction
        FlipSprite();

        if (distanceX <= stoppingDistance)
        {
            // The enemy has reached their shooting perimeter!
            animator.SetBool("isMoving", false);

            if (Time.time >= nextAttackTime)
            {
                AttackPlayer(); // Triggers your animation, which fires via the Animation Event
                nextAttackTime = Time.time + attackRate;
            }
        }
        else
        {
            // The player is too far away, move closer until we hit the stoppingDistance perimeter
            MoveTowardsPlayer();
        }
      
    }

    void MoveTowardsPlayer()
    {
        animator.SetBool("isMoving", true);
    
        // Target position using the player's X but the ENEMY'S OWN Y position
        Vector2 targetPosition = new Vector2(player.position.x, transform.position.y);

        // Move horizontally toward that target position
        transform.position = Vector2.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);
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

        // --- AUTOMATIC FIREPOINT FLIPPING ---
        // If the flip state changed this frame, mirror the fire point's X position
        if (wasFlippedRight != spriteRenderer.flipX && firePoint != null)
        {
            Vector3 localPos = firePoint.localPosition;
            localPos.x = -localPos.x; // Invert the X coordinate relative to the enemy center
            firePoint.localPosition = localPos;
        }
    }
    
    void AttackPlayer()
    {
        // 1. Only trigger the animation here
        animator.SetTrigger("atk");
    }

// 2. Add this NEW public method. The animation will call this directly!
    public void SpawnProjectileEvent()
    {
        if (isDead || isStunned) return; // Don't shoot if interrupted

        if (projectilePrefab != null && firePoint != null)
        {
            GameObject newProjectile = Instantiate(projectilePrefab, firePoint.position, Quaternion.identity);
        
            // Match this name exactly to your projectile script (EnemyProjectile or EnemyProjectile2D)
            EnemyProjectile bulletScript = newProjectile.GetComponent<EnemyProjectile>();
        
            if (bulletScript != null)
            {
                float shootDirection = spriteRenderer.flipX ? 1f : -1f;
                bulletScript.SetupDirection(shootDirection);
            }
        }
    }

    // Public method called when the player hits this enemy
    public void TakeDamage(int damageAmount)
    {
        if (isDead) return;

        currentHealth -= damageAmount;

        if (currentHealth <= 0)
        {
            Defeat();
        }
    }

    void Defeat()
    {
        isDead = true;
        
        // Triggers the Defeat animation from Any State
        animator.SetTrigger("defeat");
        
        // Destroy the enemy GameObject after the animation finishes
        Destroy(gameObject, 2f);
    }

    void FixedUpdate()
    {
        if (groundCheckPoint != null)
        {
            bool wasGroundedBefore = isGrounded;
            isGrounded = Physics2D.OverlapCircle(groundCheckPoint.position, groundCheckRadius, groundLayer);

            // INSTANT BRAKE: If the enemy was airborne and just landed on the platform
            if (isGrounded && !wasGroundedBefore)
            {
                Rigidbody2D rb2d = GetComponent<Rigidbody2D>();
                if (rb2d != null)
                {
                    // Force its velocity back to zero so it stops sliding instantly
                    rb2d.linearVelocity = Vector2.zero;
                }
            }
        }
    }
    
    public void KnockbackStun(float duration)
    {
        if (isDead) return;
    
        isStunned = true;
        animator.SetBool("isMoving", false); // Force walk animation to stop
    
        // Clear stun after the duration ends
        Invoke(nameof(EndStun), duration);
    }

    private void EndStun()
    {
        isStunned = false;
    }
}