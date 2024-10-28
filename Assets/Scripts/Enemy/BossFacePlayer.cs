using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossFacePlayer : MonoBehaviour
{
    private Transform player1;            // Reference to player 1's transform
    private Transform player2;            // Reference to player 2's transform
    public SpriteRenderer spriteRenderer; // Reference to the SpriteRenderer for flipping
    private bool facingRight = true;      // Tracks if the boss is facing right

    void Start()
    {
        // Find both players in the scene
        player1 = GameObject.FindGameObjectWithTag("Player").transform; // Assume player1 has tag "Player"
        player2 = GameObject.FindGameObjectWithTag("Player2").transform; // Assume player2 has tag "Player2"
    }

    void Update()
    {
        if (player1 != null && player2 != null)
        {
            // Get the nearest player
            Transform nearestPlayer = GetNearestPlayer();

            // Flip the boss to face the nearest player if necessary
            FlipTowardsPlayer(nearestPlayer);
        }
    }

    // Determine the nearest player
    Transform GetNearestPlayer()
    {
        float distanceToPlayer1 = Vector2.Distance(transform.position, player1.position);
        float distanceToPlayer2 = Vector2.Distance(transform.position, player2.position);

        // Return the closest player
        return distanceToPlayer1 < distanceToPlayer2 ? player1 : player2;
    }

    // Flip the boss to face the nearest player
    void FlipTowardsPlayer(Transform targetPlayer)
    {
        if (targetPlayer != null)
        {
            // Check if the player is on the opposite side (crossing the center of the boss)
            if (targetPlayer.position.x < transform.position.x && facingRight)
            {
                // Player is on the left, and boss is facing right; flip to face left
                Flip();
            }
            else if (targetPlayer.position.x > transform.position.x && !facingRight)
            {
                // Player is on the right, and boss is facing left; flip to face right
                Flip();
            }
        }
    }

    // Flip the sprite
    void Flip()
    {
        facingRight = !facingRight; // Toggle the facing direction
        spriteRenderer.flipX = !spriteRenderer.flipX; // Flip the sprite's X-axis
    }
}
