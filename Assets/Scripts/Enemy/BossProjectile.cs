using UnityEngine;

public class BossProjectile : MonoBehaviour
{
    public float lifetime = 5f; // Time before the projectile is destroyed

    void Start()
    {
        // Destroy the projectile after a certain time
        Destroy(gameObject, lifetime);
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        // Check for collision with player or other targets
        if (collision.CompareTag("Player") || collision.CompareTag("Player2"))
        {
            // Add damage logic here (e.g., collision.gameObject.GetComponent<PlayerHealth>().TakeDamage(damage);)

            // Destroy the projectile on hit
            Destroy(gameObject);
        }
        else if (collision.CompareTag("Floor")) // Example for hitting the ground
        {
            Destroy(gameObject);
        }
    }
}
