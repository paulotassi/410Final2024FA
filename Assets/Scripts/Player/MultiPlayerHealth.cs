using System.Collections;
using UnityEngine;
using Unity.Netcode;

public class MultiPlayerHealth : NetworkBehaviour
{
    // Public Variables
    public int maxHealth = 100;                     // Maximum health value for the player
    public int currentHealth;                       // Variable to track the player's current health
    private Vector2 lastPosition;                   // Variable to store the last known position of the player
    public PlayerController playerController;      // Reference to the player's control script
    public bool isInvincible = false;               // Flag for temporary invincibility
    public float invincibilityDurationSeconds;      // Duration of invincibility after respawn
    public GameObject playerModel;                  // Reference to the player's model GameObject
    public GameObject Shield;                       // Shield GameObject to visually indicate invincibility
    private Rigidbody2D playerRB;                   // Reference to the player's Rigidbody2D component



    void Awake()
    {
        currentHealth = maxHealth; // Set current health to maximum at the start
        playerRB = GetComponent<Rigidbody2D>();
    }

    public void TakeDamage(int damage)
    {
        // Prevent taking damage if invincibility is active
        if (isInvincible) return;
        Debug.LogWarning(this.gameObject.name + " Current Health:  " + currentHealth);
        lastPosition = transform.position; // Update the last position before taking damage
        currentHealth -= damage; // Reduce health by the damage amount
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth); // Ensure health stays within 0 and maxHealth


        //StartCoroutine(playerController.createScreenShake(7f)); // Trigger screen shake effect on damage
       

        // Check if the player's health is depleted
        if (currentHealth <= 0)
        {
            Die(); // Trigger death logic
        }
    }
    private void Die()
    {
        StartCoroutine(Respawn(5f)); // Start the respawn process with a delay
    }

    public IEnumerator Respawn(float duration)
    {
        playerController.enabled = false; // Disable player controls during respawn
        playerRB.simulated = false;       // Disable Rigidbody to prevent movement
        transform.localScale = Vector3.zero; // Set scale to zero to "hide" p1layer


            yield return new WaitForSeconds(1f);   // Wait for one second
 

        // Reset player settings after countdown
        transform.position = lastPosition;
        transform.localScale = new Vector3(1.6187f, 1.6187f, 1.6187f); // Reset scale
        playerController.enabled = true;   // Re-enable player controls
        playerRB.simulated = true;         // Reactivate Rigidbody
        currentHealth = maxHealth;         // Reset health

    }
}
