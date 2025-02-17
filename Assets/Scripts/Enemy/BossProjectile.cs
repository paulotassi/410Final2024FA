using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;

public class BossProjectile : MonoBehaviour
{
    public float lifetime = 5f; // Time before the projectile is destroyed
    public int Damage = 20;
    public GameObject smallerProjectilePrefab;
    public float smallerProjectileLifetime = 3f;
    public int numberOfSmallerProjectiles = 6;
    public float splitRadius = 1f;

    void Start()
    {
        // Destroy the projectile after a certain time
        // Destroy(gameObject, lifetime);
        Invoke(nameof(SplitProjectile), lifetime);
    }

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

    void SplitProjectile()
    {
        // Split into smaller projectiles in a circular pattern
        for (int i = 0; i < numberOfSmallerProjectiles; i++)
        {
            // Calculate angle for each projectile
            float angle = i * (360f / numberOfSmallerProjectiles);
            Vector3 direction = Quaternion.Euler(0, 0, angle) * Vector3.right;

            // Instantiate smaller projectile at current position
            GameObject newProjectile = Instantiate(smallerProjectilePrefab, transform.position, Quaternion.identity);

            // Set velocity or movement for the smaller projectile
            Rigidbody2D rb = newProjectile.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.linearVelocity = direction * splitRadius; // Adjust speed if needed
            }

            // Set the smaller projectile to destroy itself after its lifetime
            Destroy(newProjectile, smallerProjectileLifetime);
        }

        // Destroy the original projectile
        Destroy(gameObject);
    }
}
