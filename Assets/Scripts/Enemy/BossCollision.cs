using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class BossCollision : MonoBehaviour
{
    //If player collides with the boss they take damage
    public int Damage = 20;

   

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Check for collision with player or other targets
        if (collision.gameObject.CompareTag("Player") || collision.gameObject.CompareTag("Player2"))
        {
            // Get the PlayerHealth component of the player that was hit
            PlayerHealth playerHealth = collision.gameObject.GetComponent<PlayerHealth>();

            if (playerHealth != null)
            {
                // Call the TakeDamage method on the player
                playerHealth.TakeDamage(Damage);
            }

        }
    }
    

}