using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossPhase2Attack : MonoBehaviour
{
    public GameObject projectilePrefab; // Reference to the projectile prefab
    public float attackRange = 10f;     // Distance within which the boss can attack
    public float projectileSpeed = 10f;  // Speed of the projectile
    public float attackCooldown = 2f;    // Time between attacks
    private float lastAttackTime = 0f;   // Timer for attack cooldown
    


    private Transform player1;            // Reference to player 1's transform
    private Transform player2;            // Reference to player 2's transform
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
                    FireProjectile(targetPlayer);
                    lastAttackTime = Time.time; // Reset the cooldown timer
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

    void FireProjectile(Transform targetPlayer)
    {
        // Instantiate the projectile at the boss's position
        GameObject projectile = Instantiate(projectilePrefab, transform.position, Quaternion.identity);

        // Calculate the direction to the target player
        Vector2 direction = (targetPlayer.position - transform.position).normalized;

        // Get the Rigidbody2D component of the projectile
        Rigidbody2D rb = projectile.GetComponent<Rigidbody2D>();

        // Set the projectile's velocity toward the target player
        rb.velocity = direction * projectileSpeed;
    }

    bool IsPlayerAlive(Transform targetPlayer)
    {
        // Check if the target player is alive by comparing health to 0
        PlayerHealth targetPlayerHealth = targetPlayer.GetComponent<PlayerHealth>();
        return targetPlayerHealth != null && targetPlayerHealth.currentHealth > 0; // Assumes health is a public variable in PlayerHealth
    }
}
