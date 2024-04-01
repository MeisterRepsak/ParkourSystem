using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[RequireComponent(typeof(PlayerInputManager))]
[RequireComponent(typeof(Rigidbody))]
public class PlayerMovement : MonoBehaviour
{
    PlayerInputManager pm;

    [Header("Movement")]
    float moveSpeed;
    public float walkSpeed;
    public float sprintSpeed;
    public float crouchSpeed;

    public float groundDrag;
    public bool isSprinting = false;

    [Header("Jumping")]
    public float jumpForce;
    public float airMultiplier;

    [Header("Crouching")]
    public float crouchYScale;
    public bool isCrouched = false;
    float startYScale;

    [Header("Ground Check")]
    public float playerHeight;
    public LayerMask whatIsGround;
    bool grounded;

    [Header("Slope Handling")]
    public float maxSlopeAngle;
    RaycastHit slopeHit;
    bool exitingSlope;

    public Transform orientation;

    Vector3 moveDirection;
    Rigidbody rb;

    public MovementState state;
    public enum MovementState
    {
        Walking,
        Sprinting,
        Crouching,
        Air,
    }

    // Start is called before the first frame update
    void Start()
    {
        pm = GetComponent<PlayerInputManager>();
        rb = GetComponent<Rigidbody>();

        startYScale = transform.localScale.y;
    }

    private void Update()
    {
        
        grounded = Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f + 0.2f, whatIsGround);
        SpeedControl();
        StateHandler();

        HandleInputs();
        

        // handle drag
        if (grounded)
            rb.drag = groundDrag;
        else
            rb.drag = 0;
    }

    private void FixedUpdate()
    {
        MovePlayer();
    }

    void HandleInputs()
    {
        if (pm.doJump && grounded && !isCrouched)
            Jump();


        if (pm.doCrouch && !isCrouched && grounded)
        {
            transform.localScale = new Vector3(transform.localScale.x, crouchYScale, transform.localScale.z);
            rb.AddForce(Vector3.down * 5f, ForceMode.Impulse);
            isCrouched = true;
        }
        else if(pm.doCrouch && isCrouched || pm.doJump && isCrouched || isSprinting && isCrouched)
        {
            transform.localScale = new Vector3(transform.localScale.x, startYScale, transform.localScale.z);
            isCrouched = false;
        }
    }

    void MovePlayer()
    {
        // calculate movement direction
        moveDirection = orientation.forward * pm.moveInput.y + orientation.right * pm.moveInput.x;

        // on ground
        if (grounded)
        {       
            if(OnSlope() && !exitingSlope)
            {
                rb.AddForce(GetSlopeMoveDirection() * moveSpeed * 20f, ForceMode.Force);
                if (rb.velocity.y > 0)
                    rb.AddForce(Vector3.down * 10f, ForceMode.Force);
            }
            else
            {
                rb.AddForce(moveDirection.normalized * moveSpeed * 12f, ForceMode.Force);
            }

        }
            

        // in air
        else
        {
            rb.AddForce(moveDirection.normalized * moveSpeed * 12f * airMultiplier, ForceMode.Force);
        }

        // turn off while on slope
        rb.useGravity = !OnSlope();
    }

    void SpeedControl()
    {

        // limiting speed on slope
        if (OnSlope())
        {
            if(rb.velocity.magnitude > moveSpeed)
                rb.velocity = rb.velocity.normalized * moveSpeed;
        }
        // limiting on ground or air
        else 
        {
            Vector3 flatVel = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

            // limit velocity if needed 
            if (flatVel.magnitude > moveSpeed)
            {
                Vector3 limitedVel = flatVel.normalized * moveSpeed;
                rb.velocity = new Vector3(limitedVel.x, rb.velocity.y, limitedVel.z);
            }
        }


        
    }

    void Jump()
    {
        exitingSlope = true;
        // reset y velocity
        rb.velocity = new Vector3(rb.velocity.x, 0, rb.velocity.z);


        rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);

        Invoke(nameof(ResetSlopeRule), 0.25f);
    }

    void ResetSlopeRule()
    {
        exitingSlope = false;
    }

    bool OnSlope()
    {
        if(Physics.Raycast(transform.position, Vector3.down, out slopeHit, playerHeight * 0.5f + 0.3f))
        {
            float angle = Vector3.Angle(Vector3.up, slopeHit.normal);
            return angle <= maxSlopeAngle && angle != 0;
        }

        return false;
    }

    Vector3 GetSlopeMoveDirection()
    {
        return Vector3.ProjectOnPlane(moveDirection, slopeHit.normal).normalized;
    }

    void StateHandler()
    {

        

        // Mode - Sprinting
        if(grounded && pm.isSprinting)
        {
            float dot = Vector2.Dot(pm.moveInput, Vector2.up);

            // Safe guard for sprint
            if(dot >= 0.7f)
            {
                state = MovementState.Sprinting;
                moveSpeed = sprintSpeed;
                isSprinting = true;
            }
            else
            {
                state = MovementState.Walking;
                moveSpeed = walkSpeed;
                isSprinting = false;
            }

        }
        // Mode - Walking
        else if(grounded)
        {
            state = MovementState.Walking;
            moveSpeed = walkSpeed;
            isSprinting = false;
        }
        // Mode - Air
        else
        {
            float dot = Vector2.Dot(pm.moveInput, Vector2.up);

            // Safe guard for sprint
            if (dot >= 0.7f && pm.isSprinting)
            {
                state = MovementState.Air;
                moveSpeed = sprintSpeed;
                isSprinting = true;
            }
            else
            {
                state = MovementState.Air;
                moveSpeed = walkSpeed;
                isSprinting = false;
            }
        }

        if (isCrouched && grounded)
        {
            state = MovementState.Crouching;
            moveSpeed = crouchSpeed;
        }
    }
}
