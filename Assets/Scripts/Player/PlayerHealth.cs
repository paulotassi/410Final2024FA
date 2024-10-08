using UnityEngine;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour
{
    public int maxHealth = 100;   // Max health value
    public int currentHealth;    // Current health value

    public Image healthBar;       // Reference to the health bar UI (Image component)

    private void Start()
    {
        currentHealth = maxHealth;    // Initialize health to max
        UpdateHealthBar();            // Initialize the health bar
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth); // Ensure health doesn't drop below 0 or exceed maxHealth

        UpdateHealthBar();    // Update the health bar based on new health value

        if (currentHealth <= 0)
        {
            Die();    // Call a method to handle player death
        }
    }

    private void UpdateHealthBar()
    {
        // Update the fill amount of the health bar based on the current health percentage
        healthBar.fillAmount = (float)currentHealth / maxHealth;
    }

    private void Die()
    {
        Debug.Log("Player died!");
        // Add player death logic here (e.g., respawn, game over, etc.)
    }
}
