using UnityEngine;

public class BossMovement : MonoBehaviour
{
    public float speed = 5f;
    public Transform[] movePoints;

    private int currentPointIndex = 0;
    private Rigidbody2D rb;

    // Set a small threshold to ensure it accurately detects when the boss reaches the point
    public float pointReachThreshold = 0.1f;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void FixedUpdate()
    {
        MoveBetweenPoints();
        
    }

    void MoveBetweenPoints()
    {
        // Get the target position of the current move point
        Vector2 targetPosition = movePoints[currentPointIndex].position;

        // Calculate direction and move the boss using Rigidbody2D
        Vector2 newPosition = Vector2.MoveTowards(rb.position, targetPosition, speed * Time.fixedDeltaTime);
        rb.MovePosition(newPosition);

        // Check if the boss is close enough to the target position
        if (Vector2.Distance(rb.position, targetPosition) <= pointReachThreshold)
        {
            // Update to the next point in the array
            currentPointIndex = (currentPointIndex + 1) % movePoints.Length;
        }
    }

    // Function to teleport the boss to a specific point
    public void TeleportToPoint(Transform point)
    {
        rb.position = point.position;
    }
}
