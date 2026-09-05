using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(Collider2D))] // Added to guarantee access to the player's main collider
public class PlayerMove1 : MonoBehaviour
{
    [Header("Movement")] [SerializeField] private float moveSpeed = 6f;

    [Header("Jumping & Variable Height")] [SerializeField]
    private float jumpForce = 14f;

    [Range(0f, 1f)] [SerializeField] private float jumpCutMultiplier = 0.5f;

    [Header("Ground Pound Settings")] [SerializeField]
    private float groundPoundSpeed = 25f;

    [Header("Ground Check Settings")] 
    [SerializeField] private float castDistance = 0.1f; // How far below your collider to look
    [SerializeField] private LayerMask groundLayer;

    private Rigidbody2D rb;
    private Animator anim;
    private Collider2D playerCollider; // Used to cast outwards without hitting yourself

    private Vector2 movementInput;
    private bool isGrounded;
    private bool isGroundPounding;
    private bool isWalking;

    // Input state flags passed from Update to FixedUpdate
    private bool desiredJump;
    private bool isHoldingJump;
    private bool desiredGroundPound;

    private bool isFacingRight = true;
    private bool wasGroundPounding;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        playerCollider = GetComponent<Collider2D>();
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

        // Cast down using the player's actual collider shape to look specifically for enemies
        LayerMask enemyLayerMask = LayerMask.GetMask("Enemy");
        RaycastHit2D[] enemyHits = new RaycastHit2D[1];
        int enemyHitCount = playerCollider.Cast(Vector2.down, enemyHits, castDistance);
        
        bool isStandingOnEnemy = false;
        if (enemyHitCount > 0)
        {
            // Verify if the layer matches the Enemy mask
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

        // Combine Ground and Enemy layers
        LayerMask combinedGroundMask = groundLayer | LayerMask.GetMask("Enemy");
        
        // This cast uses your exact player collider shape and shoots it slightly downward.
        // It inherently IGNORES your own collider, preventing self-detection entirely.
        RaycastHit2D[] groundHits = new RaycastHit2D[1];
        int groundHitCount = playerCollider.Cast(Vector2.down, groundHits, castDistance);
        
        isGrounded = false;
        if (groundHitCount > 0)
        {
            // Verify if the object hit belongs to either the ground or enemy layer
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
        Enemy1Script enemy = collision.gameObject.GetComponent<Enemy1Script>();

        if (enemy != null)
        {
            if (isGroundPounding || wasGroundPounding || rb.linearVelocity.y < -1f)
            {
                isGroundPounding = false;
                wasGroundPounding = false;
                desiredGroundPound = false;
                anim.ResetTrigger("pressings"); 

                enemy.TakeDamage(1);
                enemy.KnockbackStun(1f);

                Rigidbody2D enemyRb = collision.gameObject.GetComponent<Rigidbody2D>();
                if (enemyRb != null && enemyRb.bodyType == RigidbodyType2D.Dynamic)
                {
                    enemyRb.mass = 10f; 
                    enemyRb.linearVelocity = Vector2.zero;

                    if (enemy.isGrounded)
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

                rb.linearVelocity = Vector2.zero; 
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce * 0.9f);
            }
        }
    }

    private System.Collections.IEnumerator RestoreEnemyMass(Rigidbody2D enemyRb)
    {
        yield return new WaitForSeconds(1f); 
        if (enemyRb != null)
        {
            enemyRb.mass = 10000f; 
        }
    }
}