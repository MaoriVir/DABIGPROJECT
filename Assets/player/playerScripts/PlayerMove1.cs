using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Animator))]
public class PlayerMove1 : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 6f;

    [Header("Jumping & Variable Height")]
    [SerializeField] private float jumpForce = 14f;
    [Range(0f, 1f)] [SerializeField] private float jumpCutMultiplier = 0.5f;

    [Header("Ground Pound Settings")]
    [SerializeField] private float groundPoundSpeed = 25f;

    [Header("Ground Check Settings")]
    [SerializeField] private Transform groundCheckPoint;
    [SerializeField] private float groundCheckRadius = 0.2f;
    [SerializeField] private LayerMask groundLayer;

    private Rigidbody2D rb;
    private Animator anim;
    
    private Vector2 movementInput;
    private bool isGrounded;
    private bool isGroundPounding;
    private bool isWalking;

    // Input state flags passed from Update to FixedUpdate
    private bool desiredJump;
    private bool isHoldingJump;
    private bool desiredGroundPound;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
    }

    // New Input System Message Callback for WASD / Stick
    public void OnMove(InputValue value)
    {
        movementInput = value.Get<Vector2>();
        isWalking = Mathf.Abs(movementInput.x) > 0.01f;
    }

    private void Update()
    {
        // 1. Gather inputs reliably every frame
        if (InputSystem.actions != null)
        {
            var jumpAction = InputSystem.actions.FindAction("Jump");
            if (jumpAction != null)
            {
                // Set flag to true if pressed; cleared only after FixedUpdate processes it
                if (jumpAction.WasPressedThisFrame()) desiredJump = true;
                isHoldingJump = jumpAction.IsPressed();
            }
        }

        // 2. Queue ground pound intent if pressing down in mid-air
        if (movementInput.y < -0.5f && !isGrounded && !isGroundPounding)
        {
            desiredGroundPound = true;
        }

        // 3. Update animator parameters
        anim.SetBool("isGrounded", isGrounded);
        anim.SetBool("isWalking", isWalking);
    }

    private void FixedUpdate()
    {
        // 1. Perform Ground Check right before moving physics
        bool WasGroundedBefore = isGrounded;
        isGrounded = Physics2D.OverlapCircle(groundCheckPoint.position, groundCheckRadius, groundLayer);

        // Snap out of ground pound state upon landing
        if (isGrounded && isGroundPounding)
        {
            isGroundPounding = false;
        }

        // 2. Process Ground Pound
        if (desiredGroundPound)
        {
            desiredGroundPound = false; // Reset flag
            isGroundPounding = true;
            anim.SetTrigger("pressings");
        }

        // 3. Execute Movement State Machine
        if (isGroundPounding)
        {
            // Force hard downward smash, lock horizontal speed
            rb.linearVelocity = new Vector2(0f, -groundPoundSpeed);
            desiredJump = false; // Cancel buffered jumps during smash
        }
        else
        {
            // Standard left/right movement
            rb.linearVelocity = new Vector2(movementInput.x * moveSpeed, rb.linearVelocity.y);

            // Process jump intent
            if (desiredJump)
            {
                desiredJump = false; // Consume the input flag
                if (isGrounded)
                {
                    rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
                }
            }

            // Variable jump height cut logic
            if (!isHoldingJump && rb.linearVelocity.y > 0f)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y * jumpCutMultiplier);
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (groundCheckPoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(groundCheckPoint.position, groundCheckRadius);
        }
    }
}