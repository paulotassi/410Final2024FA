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
        // Try to find both players
        GameObject p1Obj = GameObject.FindGameObjectWithTag("Player");
        GameObject p2Obj = GameObject.FindGameObjectWithTag("Player2");

        if (p1Obj != null)
        {
            player1 = p1Obj.transform;
        }
        else
        {
            Debug.LogWarning("Player with tag 'Player' not found.");
        }

        if (p2Obj != null)
        {
            player2 = p2Obj.transform;
        }
        else
        {
            Debug.LogWarning("Player with tag 'Player2' not found.");
        }

        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer == null)
            {
                Debug.LogWarning("SpriteRenderer is not assigned and not found on this GameObject.");
            }
        }
    }

    void Update()
    {
        if ((player1 != null || player2 != null) && spriteRenderer != null)
        {
            Transform nearestPlayer = GetNearestPlayer();

            if (nearestPlayer != null)
            {
                FlipTowardsPlayer(nearestPlayer);
            }
        }
    }

    // Determine the nearest available player
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

    // Flip the boss to face the nearest player
    void FlipTowardsPlayer(Transform targetPlayer)
    {
        if (targetPlayer.position.x < transform.position.x && facingRight)
        {
            Flip();
        }
        else if (targetPlayer.position.x > transform.position.x && !facingRight)
        {
            Flip();
        }
    }

    // Flip the sprite
    void Flip()
    {
        facingRight = !facingRight;
        spriteRenderer.flipX = !spriteRenderer.flipX;
    }
}
