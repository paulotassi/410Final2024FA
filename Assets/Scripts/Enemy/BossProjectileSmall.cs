using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossProjectileSmall : MonoBehaviour
{
    public int Damage = 20;

    void OnTriggerEnter2D(Collider2D collision)
    {
        // Check for collision with player or other targets
        if (collision.CompareTag("Player") || collision.CompareTag("Player2"))
        {
            // Get the PlayerHealth component of the player that was hit
            PlayerHealth playerHealth = collision.GetComponent<PlayerHealth>();

            if (playerHealth != null)
            {
                // Call the TakeDamage method on the player
                playerHealth.TakeDamage(Damage);
            }

            // Destroy the projectile on hit
            Destroy(gameObject);
        }
        else if (collision.CompareTag("Floor")) // Example for hitting the ground
        {
            Destroy(gameObject);
        }
    }
}
