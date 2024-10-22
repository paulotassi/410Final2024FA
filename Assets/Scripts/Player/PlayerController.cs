using Cinemachine;
using UnityEngine;
using System.Collections;
using UnityEngine.U2D.Animation; // For sprite animation, unused in this script but necessary for Player Animation
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PlayerController : MonoBehaviour
{
    // Player movement variables
    public Vector2 movementInput = Vector2.zero; //input vector
    public Vector2 aimInput = Vector2.zero; //input vector
    public bool canShoot = true; //Shooting bool
    public float shootCoolDown; //How long until players can shoot again
    public GameObject projectilePrefab; //The projectile prefab holding the motion for projectiles
    public GameObject projectileSpawnLocation; //spawnLocation of the rotating familiar
    public GameObject projectileSpawnRotation; //spawn rotation to follow the Familiar direction
    public float initialMoveSpeed = 1f; // Initial movement speed before any acceleration
    public float moveSpeed = 5f; // Current movement speed (changes with input)
    public float moveHorizontalFlightSpeed = 1f; // Speed when moving horizontally during flight
    public float flightSpeed = 1f; // Vertical flight speed
    public float topSpeed = 100f; // Maximum allowed speed
    public float jumpForce = 10f; // Force applied when jumping
    public float transitionThreshold = 50f; // Speed threshold for flight mode transition
    public float liftChangeRate = 0.1f; // Rate at which gravity changes when transitioning between flight and grounded states
    public float fallRate = 3f; // Rate at which gravity changes when transitioning from flight to grounded states
    public float liftForce = 0f;
    public float flightThreshold = 5f;
    public float inactivityTime = 0f; // Time player has been inactive
    public float inactivityThreshold = 2f; // Time threshold for considering inactivity (in seconds)

    //Player Dash
    public bool isDashing = false;
    public bool canDash = true;
    public float dashSpeed = 20f;
    public float dashCooldown = 3f;
    public float dashDuration = 1.0f;
    public TrailRenderer trailRenderer;

    // Player Actions
    public bool jumped = false;
    public bool fired = false;
    public bool dashed = false;


    // Ground check variables
    public Transform groundCheck; // Point used to detect if the player is grounded
    public LayerMask groundLayer; // Layer that represents the ground
    public float groundCheckRadius = 0.2f; // Radius of the overlap circle for ground detection
    public Vector3 groundCheckYOffset; // Offset for ground check position

    // Internal state variables
    private Rigidbody2D rb; // Reference to the player's Rigidbody2D component
    public bool isGrounded; // Whether the player is currently grounded
    public float horizontalInput; // Horizontal input from the player
    public float verticalInput; // Vertical input from the player (used in flight mode)
    public bool flightMode = false; // Tracks if the player is in flight mode

    // Animation variables
    public Animator animator; // Reference to the Animator for controlling animations

    //PlayerCam Switch
    public CinemachineVirtualCamera virtualCameraLeft;
    public CinemachineVirtualCamera virtualCameraRight;

    // Start is called before the first frame update
    void Start()
    {
        // Get components on start
        rb = GetComponent<Rigidbody2D>(); // Retrieve Rigidbody2D component for physics
        animator = GetComponent<Animator>(); // Retrieve Animator component for animations
    }

    // Update is called once per frame
    void Update()
    {

        // Capture player input for horizontal (A/D, Left/Right arrows) and vertical (W/S, Up/Down arrows) movement
        horizontalInput = movementInput.x;
        verticalInput = movementInput.y;

        // Ground check: checks if the player is touching the ground
        isGrounded = Physics2D.OverlapCircle(groundCheck.position - groundCheckYOffset, groundCheckRadius, groundLayer);

        // Set speed parameter in the animator for movement animations
        animator.SetFloat("Speed", moveSpeed);

        // Flip the sprite based on movement direction (left or right)
        if (horizontalInput < 0)
        {
            GetComponent<SpriteRenderer>().flipX = true; // Flip to face left
            virtualCameraLeft.Priority = 10; //Changing Camera Priority to go left of player
            virtualCameraRight.Priority = 9;

        }
        else if (horizontalInput > 0)
        {
            GetComponent<SpriteRenderer>().flipX = false; // Flip to face right
            virtualCameraLeft.Priority = 9; //Changing Camera Priority to go right of player
            virtualCameraRight.Priority = 10;
        }

        // Toggle flight animation when airborne
        if (!isGrounded)
        {
            animator.SetBool("FlightMode", true); // Set flight animation when not grounded
        }
        else
        {
            animator.SetBool("FlightMode", false); // Disable flight animation when grounded
        }

        // Handle jumping when spacebar is pressed, but only if grounded
        if (jumped && isGrounded)
        {
            Jump(); // Trigger jump
        }

        //Handles shooting if left Mouse is clicked or right Trigger
        if (fired & canShoot)
        {
            canShoot = false;
            StartCoroutine(Shoot());
        }

        if (dashed & canDash)
        {
            StartCoroutine(Dash());
        }

        // Reset move speed to initial value when no horizontal input
        if (horizontalInput == 0)
        {
            moveSpeed = initialMoveSpeed;
        }

        // Handle flight mode transition based on velocity and gravity scale
        // Reduce gravity when moving fast horizontally, otherwise increase gravity
        if (!isGrounded && Mathf.Abs(rb.velocity.y) < 0.1f && !flightMode || !flightMode && Mathf.Abs(rb.velocity.x) >= transitionThreshold)
        {
            flightMode = true; // Activate flight mode when at the peak of the jump
        }
        else if (isGrounded)
        {
            flightMode = false; // Disable flight mode when grounded
        }

        // Track player inactivity time
        if (horizontalInput == 0 && verticalInput == 0)
        {
            inactivityTime += Time.deltaTime; // Increment inactivity time if no input
        }
        else
        {
            inactivityTime = 0f; // Reset inactivity time when player provides input
        }

        // If inactivity exceeds threshold, reduce lift force to bring the player down
        if (inactivityTime > inactivityThreshold && isGrounded == false)
        {
            fallRate += liftChangeRate / 3; // Reduce lift force slowly
        }
        else 
        {
            fallRate = 0f; 
        }
        fallRate = Mathf.Clamp(fallRate, 0, 15f);
    }

    // FixedUpdate is called at fixed intervals, used for physics-based calculations
    void FixedUpdate()
    {
        if (isDashing)
        {
            return;
        }
        // Apply movement every physics frame
        Move();
        // Reset move speed to initial value when no horizontal input
        if (horizontalInput == 0)
        {
            moveSpeed = initialMoveSpeed;
        }
        // Accelerate the player until they reach top speed
        if (moveSpeed <= topSpeed)
        {
            moveSpeed += 0.5f; // Increase speed gradually
        }
        else if (moveSpeed >= topSpeed)
        {
            moveSpeed = topSpeed;//Caps moveSpeed
        }
    }

    // Move the player horizontally or in flight mode
    void Move()
    {

        if (flightMode)
        {
            // In flight mode, move based on both horizontal and vertical input
            rb.velocity = new Vector2(horizontalInput * moveHorizontalFlightSpeed, verticalInput * flightSpeed - fallRate);
        }
        else
        {
            // On the ground, move only horizontally
            rb.velocity = new Vector2(horizontalInput * moveSpeed, rb.velocity.y);
        }
    }

    // Handle the jump action by applying a vertical force
    void Jump()
    {
        rb.AddForce(new Vector2(rb.velocity.x, jumpForce), ForceMode2D.Impulse); // Apply jump force
        
    }


    private IEnumerator Shoot()
    {
        
        Instantiate(projectilePrefab, projectileSpawnLocation.transform.position , projectileSpawnRotation.transform.rotation);
        yield return new WaitForSeconds(shootCoolDown);
        canShoot = true;
    }

    private IEnumerator Dash()
    {
        canDash = false;
        isDashing = true;
        rb.velocity = new Vector2(horizontalInput * moveHorizontalFlightSpeed * dashSpeed, verticalInput * dashSpeed * flightSpeed - fallRate);
        trailRenderer.emitting = true;
        yield return new WaitForSeconds (dashDuration);
        trailRenderer.emitting = false;
        isDashing = false;
        yield return new WaitForSeconds(dashCooldown);
        canDash = true;

    }


    public void OnMove(InputAction.CallbackContext context)
    {
        movementInput = context.ReadValue<Vector2>();
    }
    public void OnJump(InputAction.CallbackContext context) 
    {
        jumped = context.action.triggered;
    }

    public void OnAim(InputAction.CallbackContext context)
    {
        aimInput = context.ReadValue<Vector2>();
    }
    public void OnShoot(InputAction.CallbackContext context)
    {
        fired = context.action.triggered;
    }
    public void OnDash(InputAction.CallbackContext context)
    {
        dashed = context.action.triggered;
    }
}
