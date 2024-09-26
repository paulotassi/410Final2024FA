using UnityEngine;

public class PlayerController: MonoBehaviour
{
    // Movement variables
    public float initialMoveSpeed = 1f;
    public float moveSpeed = 5f;
    public float moveHorizontalFlightSpeed = 1f;
    public float flightSpeed = 1f;
    public float topSpeed = 100f;
    public float jumpForce = 10f;
    public float transitionThreshold = 50f;
    public float gravityChangeRate = 0.1f;

    // Ground check variables
    public Transform groundCheck;
    public LayerMask groundLayer;
    public float groundCheckRadius = 0.2f;
    public Vector3 groundCheckYOffset;

    // Internal variables
    private Rigidbody2D rb;
    public bool isGrounded;
    private float horizontalInput;
    private float verticalInput;
    private bool flightMode = false;

    void Start()
    {
        // Get the Rigidbody2D component
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        // Get horizontal input (A/D or Left/Right arrow keys)
        horizontalInput = Input.GetAxis("Horizontal");
        verticalInput = Input.GetAxis("Vertical");

        // Check if the player is on the ground
        isGrounded = Physics2D.OverlapCircle(groundCheck.position - groundCheckYOffset, groundCheckRadius, groundLayer);

        // Jump when space is pressed and the player is grounded
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            Jump();
        }
        if (horizontalInput == 0)
        {
            moveSpeed = initialMoveSpeed;
        }
        // Lines 48 to 55 are the transitionary state for Flight Mode.
        if (rb.velocity.x >= transitionThreshold && rb.gravityScale >= 0|| rb.velocity.x <= -transitionThreshold && rb.gravityScale >= 0)
        {
            rb.gravityScale -= gravityChangeRate * Time.deltaTime; 
            
        }
        else if (rb.velocity.x <= transitionThreshold && rb.gravityScale <= 1 || rb.velocity.x >= -transitionThreshold && rb.gravityScale <= 1)
        {
            rb.gravityScale += (gravityChangeRate) * Time.deltaTime;
        }
        if (rb.gravityScale <= 0.5f)
        {
            flightMode = true;
        }
        else
        {
            flightMode = false;
        }

    }

    void FixedUpdate()
    {
        // Apply movement
        Move();
    }

    void Move()
    {
        // Move the player horizontally based on input
        if (moveSpeed <= topSpeed)
        {
            moveSpeed += 1f;
        }
        
        rb.velocity = new Vector2(horizontalInput * moveSpeed, rb.velocity.y);

        if (flightMode == true)
        {
            rb.velocity = new Vector2(horizontalInput * moveHorizontalFlightSpeed, verticalInput * flightSpeed);
        }
    }

    void Jump()
    {
        // Add force to jump
        rb.AddForce(new Vector2(rb.velocity.x, jumpForce));
    }

    void OnDrawGizmosSelected()
    {
        // Draw the ground check sphere in the editor for visual reference
        if (groundCheck != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(groundCheck.position - groundCheckYOffset, groundCheckRadius);
        }
    }
}
