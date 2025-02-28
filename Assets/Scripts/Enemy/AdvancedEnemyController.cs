using UnityEngine;
using System.Collections;

public class AdvancedEnemyController : EnemyController
{
    public Transform pointC; // Third patrol point
    public Transform pointD; // Fourth patrol point
    private Vector2 patrolCenter;
    private Vector2 patrolMinBounds;
    private Vector2 patrolMaxBounds;
    private Transform currentPatrolTarget;
    public int spiderlingAmount;
    public int maxSpiderlings;

    private void Awake()
    {
        maxSpiderlings = 8;
        // Calculate the center of the patrol area
        patrolCenter = (pointA.position + pointB.position + pointC.position + pointD.position) / 4f;

        // Calculate patrol boundaries
        patrolMinBounds = new Vector2(
            Mathf.Min(pointA.position.x, pointB.position.x, pointC.position.x, pointD.position.x),
            Mathf.Min(pointA.position.y, pointB.position.y, pointC.position.y, pointD.position.y)
        );
        patrolMaxBounds = new Vector2(
            Mathf.Max(pointA.position.x, pointB.position.x, pointC.position.x, pointD.position.x),
            Mathf.Max(pointA.position.y, pointB.position.y, pointC.position.y, pointD.position.y)
        );
    }

    protected override void Start()
    {
        base.Start(); // Call the base class Start method
        PickNewPatrolPoint();
        spiderlingAmount = Random.Range( 3, maxSpiderlings);
    }

    public override void Patrol()
    {
        if (isStunned) return;
        float step = patrolSpeed * Time.deltaTime;

        // Move toward the randomly selected patrol point
        transform.position = Vector2.MoveTowards(transform.position, currentPatrolTarget.position, step);

        // Flip the sprite to face the movement direction
        FlipSprite(currentPatrolTarget.position.x);

        // If the enemy reaches the patrol point, pick a new one
        if (Vector2.Distance(transform.position, currentPatrolTarget.position) < 0.1f)
        {
            Destroy(currentPatrolTarget.gameObject);
            PickNewPatrolPoint();
        }
    }

    public override void ChasePlayer()
    {
        if (targetPlayer == null || isStunned) return;

        float step = chaseSpeed * Time.deltaTime;
        Vector2 newPosition = Vector2.MoveTowards(transform.position, targetPlayer.position, step);

        // Check if new position would leave the patrol bounds
        if (newPosition.x < patrolMinBounds.x || newPosition.x > patrolMaxBounds.x ||
            newPosition.y < patrolMinBounds.y || newPosition.y > patrolMaxBounds.y)
        {
            // Return to the center and resume patrolling
            newPosition = Vector2.MoveTowards(transform.position, patrolCenter, step);
            currentState = State.Patrolling;
            return;
        }

        // Move normally towards the player
        transform.position = newPosition;
        FlipSprite(targetPlayer.position.x);

        float distanceToTarget = Vector2.Distance(transform.position, targetPlayer.position);

        if (distanceToTarget <= attackRange)
        {
            AttackPlayer();
        }
    }

    private void PickNewPatrolPoint()
    {
        // Randomly pick a new target patrol position within the patrol box
        Vector2 randomPoint = new Vector2(
            Random.Range(patrolMinBounds.x, patrolMaxBounds.x),
            Random.Range(patrolMinBounds.y, patrolMaxBounds.y)
        );

        // Create a temporary GameObject to hold the random position
        GameObject tempPoint = new GameObject("PatrolPoint");
        tempPoint.transform.position = randomPoint;
        currentPatrolTarget = tempPoint.transform;
        
    }
}
