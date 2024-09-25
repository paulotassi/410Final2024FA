using UnityEngine;

public class PlayerController: MonoBehaviour
{
    // Movement variables
    public float initialMoveSpeed = 1f;
    public float moveSpeed = 5f;
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

    void Start()
    {
        // Get the Rigidbody2D component
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        // Get horizontal input (A/D or Left/Right arrow keys)
        horizontalInput = Input.GetAxis("Horizontal");

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
        if (gameObject.GetComponent<Rigidbody2D>().velocity.x >= transitionThreshold && gameObject.GetComponent<Rigidbody2D>().gravityScale >= 0|| gameObject.GetComponent<Rigidbody2D>().velocity.x <= -transitionThreshold && gameObject.GetComponent<Rigidbody2D>().gravityScale >= 0)
        {
            gameObject.GetComponent<Rigidbody2D>().gravityScale -= gravityChangeRate * Time.deltaTime; 
        }
        else if (gameObject.GetComponent<Rigidbody2D>().velocity.x <= transitionThreshold && gameObject.GetComponent<Rigidbody2D>().gravityScale <= 1 || gameObject.GetComponent<Rigidbody2D>().velocity.x >= -transitionThreshold && gameObject.GetComponent<Rigidbody2D>().gravityScale <= 1)
        {
            gameObject.GetComponent<Rigidbody2D>().gravityScale += gravityChangeRate * Time.deltaTime;
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
    }

    void Jump()
    {
        // Add force to jump
        rb.velocity = new Vector2(rb.velocity.x, jumpForce);
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
