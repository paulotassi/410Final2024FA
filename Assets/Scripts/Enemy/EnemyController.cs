using UnityEngine;

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

    private Transform player;
    private PlayerHealth playerHealth; // Reference to the PlayerHealth script
    private bool movingToPointB = true;
    private float lastAttackTime = 0f;

    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        playerHealth = player.GetComponent<PlayerHealth>(); // Get the PlayerHealth component from the player
    }

    private void Update()
    {
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        if (distanceToPlayer <= detectionRange)
        {
            ChasePlayer(distanceToPlayer);
        }
        else
        {
            Patrol();
        }
    }

    private void Patrol()
    {
        Transform targetPoint = movingToPointB ? pointB : pointA;
        float step = patrolSpeed * Time.deltaTime;

        transform.position = Vector2.MoveTowards(transform.position, targetPoint.position, step);

        if (Vector2.Distance(transform.position, targetPoint.position) < 0.1f)
        {
            movingToPointB = !movingToPointB; // Switch direction
        }
    }

    private void ChasePlayer(float distanceToPlayer)
    {
        if (distanceToPlayer <= attackRange)
        {
            AttackPlayer(); // Call the attack function if within range
        }
        else
        {
            float step = chaseSpeed * Time.deltaTime;
            transform.position = Vector2.MoveTowards(transform.position, player.position, step);
        }
    }

    private void AttackPlayer()
    {
        if (Time.time >= lastAttackTime + attackCooldown)
        {
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(attackDamage); // Call the TakeDamage method on the player
                Debug.Log("Enemy attacks player for " + attackDamage + " damage!");
            }

            lastAttackTime = Time.time;
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
