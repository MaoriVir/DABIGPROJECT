using UnityEngine;
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Animator))]

public class Enemy1Script : MonoBehaviour
{
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
    public float stoppingDistance = 1f;

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
            // Near player horizontally: Stop moving and try to attack
            animator.SetBool("isMoving", false);

            if (Time.time >= nextAttackTime)
            {
                AttackPlayer();
                nextAttackTime = Time.time + attackRate;
            }
        }
        else
        {
            // Far from player horizontally: Move towards player
            MoveTowardsPlayer();
        }
    }

    void MoveTowardsPlayer()
    {
        animator.SetBool("isMoving", true);
    
        // 3. Create a target position using the player's X but the ENEMY'S OWN Y position
        Vector2 targetPosition = new Vector2(player.position.x, transform.position.y);

        // Move horizontally toward that target position
        transform.position = Vector2.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);
    }

    void FlipSprite()
    {
        if (player.position.x > transform.position.x)
            {
                spriteRenderer.flipX = true; 
            }
            // If player is to the left, unflip the sprite so it faces left (false)
            else if (player.position.x < transform.position.x)
            {
                spriteRenderer.flipX = false;
            }
    }

    void AttackPlayer()
    {
        // Triggers the Attack animation we set up in the Animator
        animator.SetTrigger("atk");
        
        // TODO: Add actual player damage logic here (e.g., player.GetComponent<PlayerHealth>().TakeDamage(1);)
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

       
        
        // Optional: Destroy the enemy GameObject after the animation finishes (e.g., 2 seconds)
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
