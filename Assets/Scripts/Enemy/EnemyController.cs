using UnityEngine;
using System.Collections;

public class EnemyController : MonoBehaviour
{
    // Patrol and chase points
    public Transform pointA;
    public Transform pointB;

    // Movement speeds
    public float patrolSpeed = 2f;
    public float chaseSpeed = 5f;

    // Detection and attack parameters
    public float detectionRange = 5f;
    public float attackRange = 1f;
    public float attackCooldown = 1f;
    public int attackDamage = 10;

    // Search state parameters
    public float searchDuration = 2f;
    public float flipInterval = 0.5f; // Time between flipping directions in search mode

    // Stun parameters
    public float stunDuration = 1f;
    public bool isStunned = false;
    public bool stunnable = true;
    public float stunDiminishingReturn = 1.5f;

    // Audio variables
    public AudioSource source;
    public AudioClip attackSound;

    // Enemy states
    private enum State { Patrolling, Chasing, Searching }
    private State currentState = State.Patrolling;

    // Player references
    private Transform player1;
    private Transform player2;
    private PlayerHealth player1Health;
    private PlayerHealth player2Health;
    private Transform targetPlayer; // Closest player target

    // Movement tracking
    private bool movingToPointB = true;
    private float lastAttackTime = 0f;
    private float searchTimer = 0f;
    private float flipTimer = 0f;
    private bool facingRight = true;

    // Sprite rendering
    public SpriteRenderer spriteRenderer;

    private void Start()
    {
        // Find players in the scene
        player1 = GameObject.FindGameObjectWithTag("Player").transform;
        player2 = GameObject.FindGameObjectWithTag("Player2").transform;

        // Get health components of both players
        player1Health = player1.GetComponent<PlayerHealth>();
        player2Health = player2.GetComponent<PlayerHealth>();
    }

    private void Update()
    {
        // Check if either player is dead and reset to patrolling
        if (player1Health.currentHealth <= 0 || player2Health.currentHealth <= 0)
        {
            targetPlayer = null;
            currentState = State.Patrolling;
        }

        // Determine closest player within detection range
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
        else if (currentState == State.Chasing && targetPlayer == null)
        {
            // If the target dies or escapes, start searching
            currentState = State.Searching;
            searchTimer = searchDuration;
        }

        // Execute behavior based on current state
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
                    currentState = State.Patrolling;
                }
                break;
        }
    }

    // Patrol between two points
    public virtual void Patrol()
    {
        if (isStunned) return;

        Transform targetPoint = movingToPointB ? pointB : pointA;
        float step = patrolSpeed * Time.deltaTime;
        FlipSprite(targetPoint.position.x);
        transform.position = Vector2.MoveTowards(transform.position, targetPoint.position, step);

        if (Vector2.Distance(transform.position, targetPoint.position) < 0.1f && !isStunned)
        {
            movingToPointB = !movingToPointB;
        }
    }

    // Handles enemy getting stunned
    public virtual IEnumerator Stunned(float stunDuration)
    {
        isStunned = true;
        stunnable = false;
        Debug.Log(gameObject.name + " is Stunned for " + stunDuration);

        if (spriteRenderer == null)
        {
            Debug.LogError("No SpriteRenderer found!");
            yield break;
        }

        // Flash red effect during stun
        Color originalColor = spriteRenderer.color;
        Color flashColor = Color.red;
        float flashInterval = 0.1f;
        bool isFlashing = false;

        float elapsed = 0f;
        while (elapsed < stunDuration)
        {
            spriteRenderer.color = isFlashing ? originalColor : flashColor;
            isFlashing = !isFlashing;
            yield return new WaitForSeconds(flashInterval);
            elapsed += flashInterval;
        }
        spriteRenderer.color = originalColor;

        isStunned = false;
        Debug.Log(gameObject.name + " no longer Stunned.");
        yield return new WaitForSeconds(stunDuration * stunDiminishingReturn);
        stunnable = true;
    }

    // Chase the closest player
    public virtual void ChasePlayer()
    {
        if (targetPlayer == null || isStunned) return;

        float distanceToTarget = Vector2.Distance(transform.position, targetPlayer.position);
        FlipSprite(targetPlayer.position.x);

        if (distanceToTarget <= attackRange)
        {
            AttackPlayer();
        }
        else
        {
            float step = chaseSpeed * Time.deltaTime;
            transform.position = Vector2.MoveTowards(transform.position, targetPlayer.position, step);
        }
    }

    // Search for a player after losing sight
    public virtual void Search()
    {
        searchTimer -= Time.deltaTime;
        flipTimer -= Time.deltaTime;

        if (flipTimer <= 0f)
        {
            FlipSprite(transform.position.x + (facingRight ? -1 : 1));
            flipTimer = flipInterval;
        }
    }

    // Attack the targeted player
    public virtual void AttackPlayer()
    {
        if (isStunned || Time.time < lastAttackTime + attackCooldown) return;

        PlayerHealth targetHealth = (targetPlayer == player1) ? player1Health : player2Health;
        if (targetHealth != null)
        {
            targetHealth.TakeDamage(attackDamage);
            source.PlayOneShot(attackSound);
            Debug.Log("Enemy attacks " + targetPlayer.tag + " for " + attackDamage + " damage!");
            lastAttackTime = Time.time;
        }
    }

    // Flips the sprite to face the target direction
    private void FlipSprite(float targetX)
    {
        if ((targetX > transform.position.x && !facingRight) || (targetX < transform.position.x && facingRight))
        {
            facingRight = !facingRight;
            transform.localScale = new Vector3(-transform.localScale.x, transform.localScale.y, transform.localScale.z);
        }
    }
}
