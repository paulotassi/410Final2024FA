using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    public int health = 100;
    public int damageAmount = 10;

    // This function gets called when another collider enters the trigger collider attached to this GameObject
    private void OnTriggerEnter2D(Collider2D other)
    {
        // Check if the object we collided with is tagged as "Playerspell"
        if (other.CompareTag("PlayerSpell"))
        {
            damageAmount = other.GetComponent<projectile>().projectileDamage;
            // Apply damage
            TakeDamage(damageAmount);
            if (other.gameObject.GetComponent<projectile>().projectileStun == true)
            { 
                this.gameObject.GetComponentInParent<EnemyController>().Stunned(other.gameObject.GetComponent<projectile>().projectileStunDuration);
            }

            // Optionally, destroy the Playerspell GameObject after collision
            // Destroy(other.gameObject);
        }
    }

    // Function to handle taking damage
    void TakeDamage(int damage)
    {
        health -= damage;
        Debug.Log("Health: " + health);

        // Check if health is 0 or below
        if (health <= 0)
        {
            // Handle death, like destroying the object
            Destroy(transform.parent.gameObject);
            Debug.Log("Object destroyed!");
        }
    }
}
