using UnityEngine;
using UnityEngine.UI;
using TMPro; // Import the TextMesh Pro namespace to use TMP_Text
using System.Collections;

//COMMENTED BY CHATGPT
public class PlayerHealth : MonoBehaviour
{
    // Public Variables
    public int maxHealth = 100; // Maximum health value for the player
    public int currentHealth;   // Variable to track the player's current health

    public Image healthBar; // Reference to the UI Image that displays the health bar
    public TMP_Text P1RespawnTimer; // Text to display respawn timer for Player 1
    public TMP_Text P2RespawnTimer; // Text to display respawn timer for Player 2
    public GameObject playerSprite; // Reference to the player sprite GameObject
    private Vector2 lastPosition;   // Variable to store the last known position of the player
    private Rigidbody2D playerRB;   // Reference to the player's Rigidbody2D component
    private PlayerController playerController; // Reference to the player's control script
    public GameManager gameManager; // Reference to the GameManager script to handle game-related data
    public int playerLifeCountRemaining;

    // Additional Health and Shield Settings
    public bool isInvincible = false;             // Flag for temporary invincibility
    public float invincibilityDurationSeconds;    // Duration of invincibility after respawn
    public GameObject playerModel;                // Reference to the player's model GameObject
    public GameObject Shield;                     // Shield GameObject to visually indicate invincibility
    public int player1IngredientLossOnDeath;      // Ingredients lost by Player 1 on death
    public int player2IngredientLossOnDeath;      // Ingredients lost by Player 2 on death
    public GameObject ingredientPrefab;           // Prefab to spawn ingredients upon death

    public int playerIndex; // Player ID (0 = Player 1, 1 = Player 2)
    private SpriteRenderer spriteRenderer; // Reference to the player's SpriteRenderer component

    private void Awake()
    {
        // Initialization code runs before Start
        playerRB = playerSprite.GetComponent<Rigidbody2D>(); // Get Rigidbody2D component for movement control
        Shield.SetActive(false); // Start with the shield inactive
        gameManager = FindFirstObjectByType<GameManager>(); // Find the GameManager instance in the scene

        // Set ingredient loss variables from the gameManager for each player
        player1IngredientLossOnDeath = gameManager.player1IngredientCount;
        player2IngredientLossOnDeath = gameManager.player2IngredientCount;
        playerLifeCountRemaining = gameManager.playerStartingLifeCount;
    }

    private void Start()
    {
        // Set initial health and UI elements
        currentHealth = maxHealth; // Set current health to maximum at the start
        UpdateHealthBar(); // Update the health bar to reflect the initial health
        lastPosition = transform.position; // Store the starting position
        spriteRenderer = playerSprite.GetComponent<SpriteRenderer>(); // Access the SpriteRenderer
        playerController = playerSprite.GetComponent<PlayerController>(); // Get the PlayerController script
    }

    private void Update()
    {
        ReviveShield(); // Check and update the shield's visibility based on invincibility status
    }

    public void TakeDamage(int damage)
    {
        // Prevent taking damage if invincibility is active
        if (isInvincible) return;

        lastPosition = transform.position; // Update the last position before taking damage
        currentHealth -= damage; // Reduce health by the damage amount
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth); // Ensure health stays within 0 and maxHealth

        StartCoroutine(playerController.createScreenShake(7)); // Trigger screen shake effect on damage
        UpdateHealthBar(); // Refresh the health bar UI

        // Check if the player's health is depleted
        if (currentHealth <= 0)
        {
            Die(); // Trigger death logic
        }
    }

    private void UpdateHealthBar()
    {
        // Update the health bar's fill amount to match the current health percentage
        healthBar.fillAmount = (float)currentHealth / maxHealth;
    }

    private void Die()
    {
        Debug.Log("Player " + playerIndex + " died!"); // Log the player's death

        // Player-specific ingredient loss and ingredient spawning logic
            if (this.gameObject.name == "Player")
            {
                gameManager.player1DecreaseIngredient(Mathf.FloorToInt(gameManager.player1IngredientCount / 2)); // Reduce Player 1's ingredient count
                playerLifeCountRemaining--;

                for (int i = 0; i < gameManager.player1IngredientCount; i++)
                {
                    Vector3[] spawnlocation = new Vector3[i]; // Create an array to hold spawn positions for ingredients

                    for (int j = 0; j < spawnlocation.Length; j++)
                    {
                        // Generate a random position near the player
                        spawnlocation[j] = new Vector3(
                            this.gameObject.transform.position.x + Random.Range(-3, 3),
                            this.gameObject.transform.position.y + Random.Range(-3, 3),
                            this.gameObject.transform.position.z
                        );
                    }

                    GameObject[] totalCoinDrops = new GameObject[i]; // Array to hold ingredient GameObjects

                    for (int j = 0; j < totalCoinDrops.Length; j++)
                    {
                        // Instantiate each ingredient at its calculated spawn location
                        totalCoinDrops[j] = Instantiate(ingredientPrefab, spawnlocation[j], Quaternion.identity);
                    }
                }
            }
            else if (this.gameObject.name == "Player 2")
            {
                gameManager.player2DecreaseIngredient(Mathf.FloorToInt(gameManager.player2IngredientCount / 2)); // Reduce Player 2's ingredient count
                playerLifeCountRemaining--;

                for (int i = 0; i < gameManager.player2IngredientCount; i++)
                {
                    Vector3[] spawnlocation = new Vector3[i]; // Create array for ingredient spawn positions

                    for (int j = 0; j < spawnlocation.Length; j++)
                    {
                        spawnlocation[j] = new Vector3(
                            this.gameObject.transform.position.x + Random.Range(-3, 3),
                            this.gameObject.transform.position.y + Random.Range(-3, 3),
                            this.gameObject.transform.position.z
                        );
                    }

                    GameObject[] totalCoinDrops = new GameObject[i]; // Array for instantiated ingredients

                    for (int j = 0; j < totalCoinDrops.Length; j++)
                    {
                        totalCoinDrops[j] = Instantiate(ingredientPrefab, spawnlocation[j], Quaternion.identity);
                    }
                }
            }
        StartCoroutine(Respawn(5f)); // Start the respawn process with a delay
    }

    private void ScaleModelTo(Vector3 scale)
    {
        // Scale the player's model to the specified scale
        playerModel.transform.localScale = scale;
    }

    public void ReviveShield()
    {
        // Enable or disable the shield based on invincibility status
        Shield.SetActive(isInvincible);
    }

    public IEnumerator Respawn(float duration)
    {
        playerController.enabled = false; // Disable player controls during respawn
        playerRB.simulated = false;       // Disable Rigidbody to prevent movement
        transform.localScale = Vector3.zero; // Set scale to zero to "hide" player

        // Display the respawn timer
        TMP_Text countdownText = (playerIndex == 0) ? P1RespawnTimer : P2RespawnTimer;
        countdownText.gameObject.SetActive(true); // Enable the countdown timer

        // Countdown loop for respawn
        for (float t = duration; t > 0; t -= 1f)
        {
            countdownText.text = t.ToString("F0"); // Update timer display each second
            yield return new WaitForSeconds(1f);   // Wait for one second
        }

        // Reset player settings after countdown
        transform.position = lastPosition;
        transform.localScale = new Vector3(1.6187f, 1.6187f, 1.6187f); // Reset scale
        playerController.enabled = true;   // Re-enable player controls
        playerRB.simulated = true;         // Reactivate Rigidbody
        currentHealth = maxHealth;         // Reset health
        UpdateHealthBar();                 // Refresh the health bar

        StartCoroutine(BecomeTempInvincible()); // Activate temporary invincibility

        countdownText.text = "";           // Clear the timer text
        countdownText.gameObject.SetActive(false); // Hide the countdown timer
    }

    IEnumerator BecomeTempInvincible()
    {
        // Temporarily set player to invincible
        isInvincible = true;

        yield return new WaitForSeconds(invincibilityDurationSeconds); // Wait specified duration

        isInvincible = false; // Disable invincibility after the duration
    }
}
