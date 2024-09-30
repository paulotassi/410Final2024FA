using UnityEngine;
using UnityEngine.U2D.Animation; // For sprite animation, unused in this script but necessary for Player Animation

public class PlayerController : MonoBehaviour
{
    // Player movement variables
    public float initialMoveSpeed = 1f; // Initial movement speed before any acceleration
    public float moveSpeed = 5f; // Current movement speed (changes with input)
    public float moveHorizontalFlightSpeed = 1f; // Speed when moving horizontally during flight
    public float flightSpeed = 1f; // Vertical flight speed
    public float topSpeed = 100f; // Maximum allowed speed
    public float jumpForce = 10f; // Force applied when jumping
    public float transitionThreshold = 50f; // Speed threshold for flight mode transition
    public float gravityChangeRate = 0.1f; // Rate at which gravity changes when transitioning between flight and grounded states

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
        horizontalInput = Input.GetAxis("Horizontal");
        verticalInput = Input.GetAxis("Vertical");

        // Ground check: checks if the player is touching the ground
        isGrounded = Physics2D.OverlapCircle(groundCheck.position - groundCheckYOffset, groundCheckRadius, groundLayer);

        // Set speed parameter in the animator for movement animations
        animator.SetFloat("Speed", moveSpeed);

        // Flip the sprite based on movement direction (left or right)
        if (horizontalInput < 0)
        {
            GetComponent<SpriteRenderer>().flipX = true; // Flip to face left
        }
        else if (horizontalInput > 0)
        {
            GetComponent<SpriteRenderer>().flipX = false; // Flip to face right
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
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            Jump(); // Trigger jump
        }

        // Reset move speed to initial value when no horizontal input
        if (horizontalInput == 0)
        {
            moveSpeed = initialMoveSpeed;
        }

        // Handle flight mode transition based on velocity and gravity scale
        // Reduce gravity when moving fast horizontally, otherwise increase gravity
        if ((Mathf.Abs(rb.velocity.x) >= transitionThreshold) && rb.gravityScale >= 0)
        {
            rb.gravityScale -= gravityChangeRate; // Reduce gravity for smoother flight
        }
        else if ((Mathf.Abs(rb.velocity.x) < transitionThreshold) && rb.gravityScale <= 1)
        {
            rb.gravityScale += gravityChangeRate * Time.deltaTime; // Increase gravity when slowing down
        }

        // Toggle flight mode when gravity scale drops below 0.5
        if (rb.gravityScale <= 0.5f && !isGrounded)
        {
            flightMode = true;
            
        }
        else 
        { 
            flightMode = false; 
        }
    }

    // FixedUpdate is called at fixed intervals, used for physics-based calculations
    void FixedUpdate()
    {
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
            rb.velocity = new Vector2(horizontalInput * moveHorizontalFlightSpeed, verticalInput * flightSpeed);
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

    // Visualize the ground check area in the editor (helpful for debugging)
    void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.red; // Set color of the Gizmo to red
            Gizmos.DrawWireSphere(groundCheck.position - groundCheckYOffset, groundCheckRadius); // Draw the ground check sphere
        }
    }
}
