using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossPhase1Attack : MonoBehaviour
{
    public GameObject projectilePrefab; // Reference to the projectile prefab
    public float attackRange = 10f;     // Distance within which the boss can attack
    public float projectileSpeed = 10f;  // Speed of the projectile
    public float attackCooldown = 2f;    // Time between attacks
    private float lastAttackTime = 0f;   // Timer for attack cooldown
    public int attackDamage = 20;
    

    private Transform player1;            // Reference to player 1's transform
    private Transform player2;            // Reference to player 2's transform
    private PlayerHealth player1Health;
    private PlayerHealth player2Health;
    

  

    void Start()
    {
        GameObject p1Obj = GameObject.FindGameObjectWithTag("Player");
        GameObject p2Obj = GameObject.FindGameObjectWithTag("Player2");

        if (p1Obj != null)
        {
            player1 = p1Obj.transform;
            player1Health = player1.GetComponent<PlayerHealth>();
        }
        else
        {
            Debug.LogWarning("Player with tag 'Player' not found.");
        }

        if (p2Obj != null)
        {
            player2 = p2Obj.transform;
            player2Health = player2.GetComponent<PlayerHealth>();
        }
        else
        {
            Debug.LogWarning("Player with tag 'Player2' not found.");
        }
           

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
        else if (player1 != null && player2 == null)
        {
            // Check which player is closer
            Transform targetPlayer = player1;

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
        else if (player2 != null && player1 == null)
        {
                // Check which player is closer
                Transform targetPlayer = player2;

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
        else
        {
            Debug.Log("All Players are Null");
        }
    }

    Transform GetNearestPlayer()
    {
        bool hasP1 = player1 != null;
        bool hasP2 = player2 != null;

        if (hasP1 && hasP2)
        {
            float dist1 = Vector2.Distance(transform.position, player1.position);
            float dist2 = Vector2.Distance(transform.position, player2.position);
            return dist1 < dist2 ? player1 : player2;
        }
        else if (hasP1)
        {
            return player1;
        }
        else if (hasP2)
        {
            return player2;
        }

        return null;
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
        rb.linearVelocity = direction * projectileSpeed;
    }

    bool IsPlayerAlive(Transform targetPlayer)
    {
        // Check if the target player is alive by comparing health to 0
        PlayerHealth targetPlayerHealth = targetPlayer.GetComponent<PlayerHealth>();
        return targetPlayerHealth != null && targetPlayerHealth.currentHealth > 0; // Assumes health is a public variable in PlayerHealth
    }
}
