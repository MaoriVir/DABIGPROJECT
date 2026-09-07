using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using UnityEngine.UI;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(Collider2D))] 
public class PlayerMove1 : MonoBehaviour
{
    private playerActions playerActions;
    
    private FlashEffect flashEffect;
    
    //NEW HP STUFF
    [SerializeField] private Slider hpSlider;
    
    [Header("Shield Projectile Settings")]
    [SerializeField] private GameObject shieldProjectilePrefab; 
    [SerializeField] private Transform projectileSpawnPoint; // Optional: Where the projectile spawns (e.g., player's position)
    
    [Header("Health Settings")]
    [SerializeField] private int maxHealth = 3;
    private int currentHealth;
    private bool isDead = false;
    
    [Header("Movement")] 
    [SerializeField] private float moveSpeed = 6f;

    [Header("Jumping & Variable Height")] 
    [SerializeField] private float jumpForce = 14f;
    [Range(0f, 1f)] [SerializeField] private float jumpCutMultiplier = 0.5f;

    [Header("Ground Pound Settings")] 
    [SerializeField] private float groundPoundSpeed = 25f;

    [Header("Ground Check Settings")] 
    [SerializeField] private float castDistance = 0.1f; 
    [SerializeField] private LayerMask groundLayer;

    private Rigidbody2D rb;
    private Animator anim;
    private Collider2D playerCollider; 

    private Vector2 movementInput;
    private bool isGrounded;
    private bool isGroundPounding;
    private bool isWalking;

    private bool desiredJump;
    private bool isHoldingJump;
    private bool desiredGroundPound;

    private bool isFacingRight = true;
    private bool wasGroundPounding;

    private void Awake()
    {
        flashEffect = GetComponent<FlashEffect>();
    }
    
    private void Start()
    {
        playerActions = GetComponent<playerActions>();
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        playerCollider = GetComponent<Collider2D>();
        currentHealth = maxHealth;
        hpSlider.maxValue = maxHealth;
        hpSlider.value = currentHealth;
    }

    public void OnMove(InputValue value)
    {
        movementInput = value.Get<Vector2>();
        isWalking = Mathf.Abs(movementInput.x) > 0.01f;
    }

    private void Update()
    {
        if (InputSystem.actions != null)
        {
            var jumpAction = InputSystem.actions.FindAction("Jump");
            if (jumpAction != null)
            {
                if (jumpAction.WasPressedThisFrame()) desiredJump = true;
                isHoldingJump = jumpAction.IsPressed();
            }
        }

        LayerMask enemyLayerMask = LayerMask.GetMask("Enemy");
        RaycastHit2D[] enemyHits = new RaycastHit2D[1];
        int enemyHitCount = playerCollider.Cast(Vector2.down, enemyHits, castDistance);
        
        bool isStandingOnEnemy = false;
        if (enemyHitCount > 0)
        {
            if (((1 << enemyHits[0].collider.gameObject.layer) & enemyLayerMask) != 0)
            {
                isStandingOnEnemy = true;
            }
        }

        if (movementInput.y < -0.5f && !isGrounded && !isStandingOnEnemy && !isGroundPounding)
        {
            desiredGroundPound = true;
        }

        anim.SetBool("isGrounded", isGrounded);
        anim.SetBool("isWalking", isWalking);
    }

    private void FixedUpdate()
    {
        wasGroundPounding = isGroundPounding;

        LayerMask combinedGroundMask = groundLayer | LayerMask.GetMask("Enemy");
        
        RaycastHit2D[] groundHits = new RaycastHit2D[1];
        int groundHitCount = playerCollider.Cast(Vector2.down, groundHits, castDistance);
        
        isGrounded = false;
        if (groundHitCount > 0)
        {
            if (((1 << groundHits[0].collider.gameObject.layer) & combinedGroundMask) != 0)
            {
                isGrounded = true;
            }
        }

        if (isGrounded && isGroundPounding)
        {
            isGroundPounding = false;
        }

        if (desiredGroundPound)
        {
            desiredGroundPound = false; 
            isGroundPounding = true;
            wasGroundPounding = true; 
            anim.SetTrigger("pressings");
        }

        if (isGroundPounding)
        {
            rb.linearVelocity = new Vector2(0f, -groundPoundSpeed);
            desiredJump = false; 
        }
        else
        {
            rb.linearVelocity = new Vector2(movementInput.x * moveSpeed, rb.linearVelocity.y);

            if (desiredJump)
            {
                desiredJump = false; 
                if (isGrounded)
                {
                    rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
                }
            }

            if (!isHoldingJump && rb.linearVelocity.y > 0f)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y * jumpCutMultiplier);
            }

            if (movementInput.x > 0.01f && !isFacingRight) Flip();
            else if (movementInput.x < -0.01f && isFacingRight) Flip();
        }
    }

    private void Flip()
    {
        isFacingRight = !isFacingRight;
        Vector3 localScale = transform.localScale;
        localScale.x *= -1f;
        transform.localScale = localScale;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // 1. First, check if the thing we crashed into is on the Ground Layer
        bool hitGround = ((1 << collision.gameObject.layer) & groundLayer) != 0;

        if (hitGround)
        {
            // 2. Check if the shield script exists and is currently active
            if (playerActions != null && playerActions.isShieldActive)
            {
                SpawnShieldProjectile();
            }
        }

        Enemy1Script enemy1 = collision.gameObject.GetComponent<Enemy1Script>();
        Enemy2Script enemy2 = collision.gameObject.GetComponent<Enemy2Script>();
        ShipSpawner spawner = collision.gameObject.GetComponent<ShipSpawner>(); 

        bool hitEnemy = (enemy1 != null || enemy2 != null);
        bool hitSpawner = (spawner != null); 

        if (hitEnemy || hitSpawner)
        {
           
            
            if (isGroundPounding || wasGroundPounding || rb.linearVelocity.y < -1f)
            {
                isGroundPounding = false;
                wasGroundPounding = false;
                desiredGroundPound = false;
                anim.ResetTrigger("pressings"); 
                
               
                
                // --- CASE A: WE HIT ENEMY 1 (GROUND ENEMY) ---
                if (enemy1 != null)
                {
                    enemy1.TakeDamage(5);
                    enemy1.KnockbackStun(1f);

                    Rigidbody2D enemyRb = collision.gameObject.GetComponent<Rigidbody2D>();
                    if (enemyRb != null && enemyRb.bodyType == RigidbodyType2D.Dynamic)
                    {
                        enemyRb.mass = 10f; 
                        enemyRb.linearVelocity = Vector2.zero;

                        if (enemy1.isGrounded)
                        {
                            Vector2 launchVector = new Vector2(0f, jumpForce * 0.5f);
                            enemyRb.linearVelocity = launchVector;
                        }
                        else
                        {
                            enemyRb.AddForce(Vector2.down * groundPoundSpeed * 0.6f, ForceMode2D.Impulse);
                        }
                        StartCoroutine(RestoreEnemyMass(enemyRb));
                    }
                }
                // --- CASE B: WE HIT ENEMY 2 (FLOATING ENEMY) ---
                else if (enemy2 != null)
                {
                    enemy2.TakeDamage(5);
                    
                    if (!enemy2.isGrounded)
                    {
                        enemy2.TriggerSlamDown(groundPoundSpeed * 1.2f);
                    }
                    else
                    {
                        enemy2.KnockbackStun(1f);
                    }
                }
                // --- CASE C: WE HIT THE SPAWNER ---
                else if (spawner != null)
                {
                    // Adjust damage number (e.g., 1 or higher) based on your ShipSpawner health setup
                    spawner.TakeDamage(1f); 
                }

                // Bounce the player up into the air safely after a successful slam
                rb.linearVelocity = Vector2.zero; 
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce * 0.75f);
            }
            
            
        }
    }

    private IEnumerator RestoreEnemyMass(Rigidbody2D enemyRb)
    {
        yield return new WaitForSeconds(0.5f);
        if (enemyRb != null) enemyRb.mass = 1f; 
    }
    
    
    public void TakeDamage(int damageAmount)
    {
        if (isDead) return;

        currentHealth -= damageAmount;
        hpSlider.value = currentHealth;
        if (flashEffect != null)
        {
            flashEffect.Flash();
        }
        Debug.Log("Player hit! Remaining Health: " + currentHealth);

        // Optional: Trigger a hurt animation if you have one
        // anim.SetTrigger("hurt");

        if (currentHealth <= 0)
        {
            PlayerDefeated();
        }
    }

    private void PlayerDefeated()
    {
        isDead = true;
        movementInput = Vector2.zero;
        isWalking = false;
        
        anim.SetBool("isWalking", false);
        // anim.SetTrigger("die"); // Trigger death animation if you have one

        Debug.Log("Player has been defeated!");
        Destroy(this.gameObject);
    }


    private void SpawnShieldProjectile()
    {
        if (shieldProjectilePrefab != null)
        {
            // Use projectileSpawnPoint position if assigned, otherwise use player's current position
            Vector3 spawnPos = projectileSpawnPoint != null ? projectileSpawnPoint.position : transform.position;

            // Spawns the projectile prefab into the scene
            Instantiate(shieldProjectilePrefab, spawnPos, Quaternion.identity);
        }
    }
}
