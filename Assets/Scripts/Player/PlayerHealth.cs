using UnityEngine;
using UnityEngine.UI;
using TMPro; // Import TextMesh Pro namespace
using System.Collections;

public class PlayerHealth : MonoBehaviour
{
    public int maxHealth = 100;   // Max health value
    public int currentHealth;      // Current health value

    public Image healthBar;        // Reference to the health bar UI (Image component)
    public TMP_Text P1RespawnTimer;
    public TMP_Text P2RespawnTimer;
    public GameObject playerSprite; // Reference to the player sprite GameObject
    private Vector2 lastPosition; // Variable to store the last position of the player
    private Rigidbody2D playerRB;
    private PlayerController playerController;

    public bool isInvincible = false;
    public float invincibilityDurationSeconds;
    public GameObject playerModel;
    public GameObject Shield;
   

    public int playerIndex; // Identifier for the player (0 for Player 1, 1 for Player 2)
    private SpriteRenderer spriteRenderer; // Reference to the SpriteRenderer component

    private void Awake()
    {
        playerRB = playerSprite.GetComponent<Rigidbody2D>();
        Shield.SetActive(false);
    }

    private void Start()
    {
        currentHealth = maxHealth;    // Initialize health to max
        UpdateHealthBar();            // Initialize the health bar
        lastPosition = transform.position; // Initialize lastPosition to the starting position
        spriteRenderer = playerSprite.GetComponent<SpriteRenderer>(); // Get the SpriteRenderer
        playerController = playerSprite.GetComponent<PlayerController>();

    }

    private void Update()
    {
        ReviveShield();
    }

    public void TakeDamage(int damage)
    {
        if (isInvincible) return;


        lastPosition = transform.position; // Update last position before taking damage
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
        Debug.Log("Player " + playerIndex + " died!");

        //Respawn(lastPosition);

        StartCoroutine(Respawn(5f));


    }

    private void ScaleModelTo(Vector3 scale)
    {
        playerModel.transform.localScale = scale;
    }

    public void ReviveShield()
    {
        if (isInvincible)
        {
            Shield.SetActive(true);
        }
        else
        {
            Shield.SetActive(false);

        }
    }
    public IEnumerator Respawn(float duration)
    {
        playerController.enabled = false;
        playerRB.simulated = false;
        transform.localScale = new Vector3(0, 0, 0);

        // Determine which timer to use and enable it
        TMP_Text countdownText = (playerIndex == 0) ? P1RespawnTimer : P2RespawnTimer;
        countdownText.gameObject.SetActive(true); // Enable the countdown timer

        

        // Countdown loop
        for (float t = duration; t > 0; t -= 1f)
        {
            countdownText.text = t.ToString("F0"); // Update the countdown text
            yield return new WaitForSeconds(1f); // Wait for one second
        }

        // After countdown completes, respawn the player
        transform.position = lastPosition;
        transform.localScale = new Vector3(1.6187f, 1.6187f, 1.6187f);
        playerController.enabled = true;
        playerRB.simulated = true;
        currentHealth = maxHealth;
        UpdateHealthBar();
        

        StartCoroutine(BecomeTempInvincible());

        // Clear and disable the countdown text after respawn
        countdownText.text = "";
        countdownText.gameObject.SetActive(false); // Disable the countdown timer
    }

    IEnumerator BecomeTempInvincible()
    {
        Debug.Log(playerIndex + "is invincible");
        isInvincible = true;

        yield return new WaitForSeconds(invincibilityDurationSeconds);

        Debug.Log("Player is no longer invincible!");
        isInvincible = false;

    }


}