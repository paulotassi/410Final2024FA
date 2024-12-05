using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossPhase2Attack : MonoBehaviour
{
    public GameObject projectilePrefab;   // Reference to the projectile prefab
    public float attackRange = 10f;       // Distance within which the boss can attack
    public float projectileSpeed = 10f;   // Speed of the projectile
    public float attackCooldown = 2f;     // Time between attack sets
    public float longCooldown = 5f;       // Longer cooldown after 5 volleys of 4 projectiles
    public float projectileSpacing = 1f;  // Spacing between projectiles in the line

    private float lastAttackTime = 0f;    // Timer for attack cooldown
    private int projectilesPerSet = 4;   // Number of projectiles in a set
    private int volleyCount = 0;         // Tracks how many volleys of 4 have been fired
    private int singleShotCount = 0;     // Tracks how many single shots have been fired
    private bool isShootingSingleShots = false; // Flag to check if in single-shot phase

    private Transform player1;           // Reference to player 1's transform
    private Transform player2;           // Reference to player 2's transform
    private PlayerHealth player1Health;
    private PlayerHealth player2Health;

    void Start()
    {
        // Find both players in the scene
        player1 = GameObject.FindGameObjectWithTag("Player").transform; // Assume player1 has tag "Player"
        player2 = GameObject.FindGameObjectWithTag("Player2").transform; // Assume player2 has tag "Player2"

        // Get PlayerHealth components for both players
        player1Health = player1.GetComponent<PlayerHealth>();
        player2Health = player2.GetComponent<PlayerHealth>();
    }

    void Update()
    {
        if (player1 != null && player2 != null)
        {
            // Check which player is closer
            Transform targetPlayer = GetNearestPlayer();

            // Attack if the player is within range, the cooldown has passed, and the player is alive
            if (targetPlayer != null &&
                Vector2.Distance(transform.position, targetPlayer.position) <= attackRange &&
                IsPlayerAlive(targetPlayer))
            {
                if (Time.time >= lastAttackTime + attackCooldown)
                {
                    if (!isShootingSingleShots)
                    {
                        if (volleyCount < 5)
                        {
                            // Fire a volley of 4 projectiles
                            StartCoroutine(FireProjectileSet(targetPlayer));
                            volleyCount++;
                        }
                        else
                        {
                            // Switch to single-shot phase
                            volleyCount = 0;
                            isShootingSingleShots = true;
                            singleShotCount = 0;
                            lastAttackTime = Time.time; // Wait before starting single shots
                        }
                    }
                    else
                    {
                        if (singleShotCount < 5)
                        {
                            // Fire single projectiles 5 times
                            StartCoroutine(FireSingleProjectile(targetPlayer));
                            singleShotCount++;
                        }
                        else
                        {
                            // After 5 single shots, go back to volley phase
                            isShootingSingleShots = false;
                            lastAttackTime = Time.time + longCooldown; // Apply long cooldown
                        }
                    }

                    // Update attack timer
                    lastAttackTime = Time.time;
                }
            }
        }
    }

    Transform GetNearestPlayer()
    {
        // Calculate distances to both players
        float distanceToPlayer1 = Vector2.Distance(transform.position, player1.position);
        float distanceToPlayer2 = Vector2.Distance(transform.position, player2.position);

        // Determine which player is closer
        return distanceToPlayer1 < distanceToPlayer2 ? player1 : player2;
    }

    IEnumerator FireProjectileSet(Transform targetPlayer)
    {
        Vector2 direction = (targetPlayer.position - transform.position).normalized;

        for (int i = 0; i < projectilesPerSet; i++)
        {
            // Instantiate the projectile at the boss's position with an offset
            Vector2 spawnPosition = (Vector2)transform.position + direction * i * projectileSpacing;
            GameObject projectile = Instantiate(projectilePrefab, spawnPosition, Quaternion.identity);

            // Get the Rigidbody2D component of the projectile
            Rigidbody2D rb = projectile.GetComponent<Rigidbody2D>();

            // Set the projectile's velocity toward the target player
            rb.velocity = direction * projectileSpeed;

            yield return new WaitForSeconds(0.1f); // Small delay between projectiles
        }
    }

    IEnumerator FireSingleProjectile(Transform targetPlayer)
    {
        // Fire a single projectile directly at the player
        GameObject projectile = Instantiate(projectilePrefab, transform.position, Quaternion.identity);

        Vector2 direction = (targetPlayer.position - transform.position).normalized;
        Rigidbody2D rb = projectile.GetComponent<Rigidbody2D>();

        rb.velocity = direction * projectileSpeed;

        yield return null; // No additional delay needed
    }

    bool IsPlayerAlive(Transform targetPlayer)
    {
        // Check if the target player is alive by comparing health to 0
        PlayerHealth targetPlayerHealth = targetPlayer.GetComponent<PlayerHealth>();
        return targetPlayerHealth != null && targetPlayerHealth.currentHealth > 0; // Assumes health is a public variable in PlayerHealth
    }
}
