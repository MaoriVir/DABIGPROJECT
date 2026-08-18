using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMove : MonoBehaviour
{
     [Header("Movement")]
    [SerializeField] private float moveSpeed = 6f;

    [Header("Jumping & Variable Height")]
    [SerializeField] private float jumpForce = 14f;      
    [Range(0f, 1f)] 
    [SerializeField] private float jumpCutMultiplier = 0.5f; 

    [Header("Ground Pound Settings")]
    [SerializeField] private float groundPoundSpeed = 25f; // Super fast downward velocity
    private bool isGroundPounding = false;

    [Header("Ground Check Settings")]
    [SerializeField] private Transform groundCheckPoint;
    [SerializeField] private float groundCheckRadius = 0.2f;
    [SerializeField] private LayerMask groundLayer;

    private Rigidbody2D rb;
    private Vector2 movementInput;
    private bool isGrounded;
    private bool jumpPressedThisFrame;
    private bool isHoldingJump;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    // Tracks WASD / D-pad
    public void OnMove(InputValue value)
    {
        movementInput = value.Get<Vector2>();
    }

    private void Update()
    {
        // 1. Regular Ground Check
        isGrounded = Physics2D.OverlapCircle(groundCheckPoint.position, groundCheckRadius, groundLayer);

        // 2. Clear ground pound state once you touch the floor
        if (isGrounded && isGroundPounding)
        {
            isGroundPounding = false;
        }

        // 3. Direct Input Polling for Jumping
        jumpPressedThisFrame = InputSystem.actions.FindAction("Jump").WasPressedThisFrame();
        isHoldingJump = InputSystem.actions.FindAction("Jump").IsPressed();

        if (jumpPressedThisFrame && isGrounded && !isGroundPounding)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        }

        // 4. Detect Ground Pound: If pressing S (Y value is negative) AND in mid-air
        if (movementInput.y < -0.5f && !isGrounded && !isGroundPounding)
        {
            TriggerGroundPound();
        }
    }

    private void TriggerGroundPound()
    {
        isGroundPounding = true;
        // Zero out horizontal velocity entirely and smash down hard
        rb.linearVelocity = new Vector2(0f, -groundPoundSpeed);
    }

    private void FixedUpdate()
    {
        if (isGroundPounding)
        {
            // Lock horizontal movement and maintain the ground pound smash speed
            rb.linearVelocity = new Vector2(0f, -groundPoundSpeed);
        }
        else
        {
            // Standard left/right movement
            rb.linearVelocity = new Vector2(movementInput.x * moveSpeed, rb.linearVelocity.y);

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
