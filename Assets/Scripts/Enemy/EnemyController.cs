using UnityEngine;
using System.Collections;  // Add this line

public class EnemyController : MonoBehaviour
{
    public Transform pointA;
    public Transform pointB;
    public float patrolSpeed = 2f;
    public float chaseSpeed = 5f;
    public float detectionRange = 5f;
    public float attackRange = 1f;
    public float attackCooldown = 1f;
    public int attackDamage = 10;
    public float searchDuration = 2f; // Time spent in search state
    public float flipInterval = 0.5f; // Time between flips in search state
    public float stunDuration = 1f;
    public AudioSource source;
    public AudioClip attackSound;

    private enum State { Patrolling, Chasing, Searching, Stunned }
    private State currentState = State.Patrolling;

    private Transform player1;
    private Transform player2;
    private PlayerHealth player1Health;
    private PlayerHealth player2Health;
    private Transform targetPlayer; // The closest player
    private bool movingToPointB = true;
    private float lastAttackTime = 0f;
    private float searchTimer = 0f;
    private float flipTimer = 0f;
    private bool facingRight = true;

    private SpriteRenderer spriteRenderer;
   

    private void Start()
    {
        // Find both players by their tags
        player1 = GameObject.FindGameObjectWithTag("Player").transform;
        player2 = GameObject.FindGameObjectWithTag("Player2").transform;

        // Get PlayerHealth components for both players
        player1Health = player1.GetComponent<PlayerHealth>();
        player2Health = player2.GetComponent<PlayerHealth>();

        // Get the SpriteRenderer and store the original color
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        // Check if either player is dead and return to patrolling
        if (player1Health.currentHealth <= 0)
        {
            targetPlayer = null; // Reset target player
            currentState = State.Patrolling; // Switch back to patrolling if player 1 is dead
        }
        else if (player2Health.currentHealth <= 0)
        {
            targetPlayer = null; // Reset target player
            currentState = State.Patrolling; // Switch back to patrolling if player 2 is dead
        }

        // Determine which player is closer and alive
        if (player1Health.currentHealth > 0 && Vector2.Distance(transform.position, player1.position) <= detectionRange)
        {
            targetPlayer = player1;
            currentState = State.Chasing;
        }
        else if (player2Health.currentHealth > 0 && Vector2.Distance(transform.position, player2.position) <= detectionRange)
        {
            targetPlayer = player2;
            currentState = State.Chasing;
        }
        else if (currentState == State.Chasing && (targetPlayer == player1 && player1Health.currentHealth <= 0 ||
                                                    targetPlayer == player2 && player2Health.currentHealth <= 0))
        {
            targetPlayer = null; // Reset target if the current target dies
            currentState = State.Patrolling; // Switch back to patrolling if target is dead
        }
        else if (currentState == State.Chasing && (Vector2.Distance(transform.position, player1.position) > detectionRange &&
                                                     Vector2.Distance(transform.position, player2.position) > detectionRange))
        {
            currentState = State.Searching;
            searchTimer = searchDuration; // Start search timer
        }

        switch (currentState)
        {
            case State.Patrolling:
                Patrol();
                break;

            case State.Chasing:
                ChasePlayer();  
                break;

            case State.Searching:
                Search();
                if (searchTimer <= 0)
                {
                    currentState = State.Patrolling; // Return to patrolling after search
                }
                break;
            
            case State.Stunned:
                StartCoroutine(Stunned(stunDuration));                
                break;
        }
    }

    private void Patrol()
    {
        Transform targetPoint = movingToPointB ? pointB : pointA;
        float step = patrolSpeed * Time.deltaTime;

        // Flip the sprite to face the target point
        FlipSprite(targetPoint.position.x);

        transform.position = Vector2.MoveTowards(transform.position, targetPoint.position, step);

        if (Vector2.Distance(transform.position, targetPoint.position) < 0.1f)
        {
            movingToPointB = !movingToPointB; // Switch direction
        }
    }

    public IEnumerator Stunned(float stunDuration)
    {
        yield return new WaitForSeconds(stunDuration);
    }

    private void ChasePlayer()
    {
        if (targetPlayer == null) return;

        float distanceToTargetPlayer = Vector2.Distance(transform.position, targetPlayer.position);

        // Flip the sprite to face the target player
        FlipSprite(targetPlayer.position.x);

        if (distanceToTargetPlayer <= attackRange)
        {
            AttackPlayer(); // Call the attack function if within range
        }
        else
        {
            float step = chaseSpeed * Time.deltaTime;
            transform.position = Vector2.MoveTowards(transform.position, targetPlayer.position, step);
        }
    }

    private void Search()
    {
        searchTimer -= Time.deltaTime;
        flipTimer -= Time.deltaTime;

        // Flip the sprite repeatedly while searching
        if (flipTimer <= 0f)
        {
            FlipSprite(transform.position.x + (facingRight ? -1 : 1)); // Flip back and forth
            flipTimer = flipInterval; // Reset the flip timer
        }
    }

    private void AttackPlayer()
    {
        PlayerHealth targetPlayerHealth = (targetPlayer == player1) ? player1Health : player2Health;

        if (Time.time >= lastAttackTime + attackCooldown && targetPlayerHealth != null)
        {
            targetPlayerHealth.TakeDamage(attackDamage); // Call the TakeDamage method on the player
            source.PlayOneShot(attackSound);
            Debug.Log("Enemy attacks " + targetPlayer.tag + " for " + attackDamage + " damage!");
            lastAttackTime = Time.time;
        }
    }

    private void FlipSprite(float targetX)
    {
        // Flip the sprite to face the target's position based on the X axis
        if ((targetX > transform.position.x && !facingRight) || (targetX < transform.position.x && facingRight))
        {
            facingRight = !facingRight;
            Vector3 scale = transform.localScale;
            scale.x *= -1;
            transform.localScale = scale;
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange); // Draw detection range

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange); // Draw attack range

        Gizmos.color = Color.green;
        if (pointA != null) Gizmos.DrawLine(transform.position, pointA.position); // Draw line to point A
        if (pointB != null) Gizmos.DrawLine(transform.position, pointB.position); // Draw line to point B
    }
}
