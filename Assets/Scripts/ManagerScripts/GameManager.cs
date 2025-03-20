//THIS SCRIPT WAS HEAVILY COMMENTED WITH CHATGPT AS WELL AS SOME EFFECIENCY USING TERTIARY OPERATORS

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement; // Required for handling scene transitions.
using UnityEngine.UI;
using TMPro; // Required for using TextMeshPro (TMP) for UI text elements.

public class GameManager : MonoBehaviour
{
    //======================================================
    // UI Elements and Game Settings
    //======================================================
    [SerializeField] private TMP_Text Player1ScoreText;      // Text element to display Player 1's score.
    [SerializeField] private TMP_Text Player2ScoreText;      // Text element to display Player 2's score.
    [SerializeField] private TMP_Text RoundEndText;          // Text element to display a message when the round ends.
    [SerializeField] private TMP_Text gameTimerText;         // Text element to show the remaining game time.
    [SerializeField] public int playerStartingLifeCount = 3; // Starting life count for players.
    [SerializeField] public bool isPaused = false;
    [SerializeField] public GameObject pauseMenuUI;


    //======================================================
    // Player 1 Cooldown Variables
    //======================================================
    [Header("Player 1 CD")]
    [SerializeField] public Image Player1shootCDDisplay;      // UI image for Player 1 shoot cooldown.
    [SerializeField] private float Player1ShootCDMaxValue;     // Maximum value for Player 1 shoot cooldown.
    [SerializeField] private float Player1ShootCDTimer;        // Timer tracking Player 1 shoot cooldown.
    [SerializeField] private bool Player1ShootCDEnabled = false;// Flag for whether Player 1 shoot cooldown is active.

    [SerializeField] public Image Player1StunCDDisplay;       // UI image for Player 1 stun cooldown.
    [SerializeField] private float Player1StunCDMaxValue;      // Maximum value for Player 1 stun cooldown.
    [SerializeField] private float Player1StunCDTimer;         // Timer tracking Player 1 stun cooldown.
    [SerializeField] private bool Player1StunCDEnabled = false; // Flag for whether Player 1 stun cooldown is active.

    [SerializeField] public Image Player1shieldCDDisplay;     // UI image for Player 1 shield cooldown.
    [SerializeField] private float Player1ShieldCDMaxValue;    // Maximum value for Player 1 shield cooldown.
    [SerializeField] private float Player1ShieldCDTimer;       // Timer tracking Player 1 shield cooldown.
    [SerializeField] private bool Player1ShieldCDEnabled = false;// Flag for whether Player 1 shield cooldown is active.



    //======================================================
    // Player 2 Cooldown Variables
    //======================================================
    [Header("Player 2 CD")]
    [SerializeField] public Image Player2shootCDDisplay;      // UI image for Player 2 shoot cooldown.
    [SerializeField] private float Player2ShootCDMaxValue;     // Maximum value for Player 2 shoot cooldown.
    [SerializeField] private float Player2ShootCDTimer;        // Timer tracking Player 2 shoot cooldown.
    [SerializeField] private bool Player2ShootCDEnabled = false;// Flag for whether Player 2 shoot cooldown is active.

    [SerializeField] public Image Player2StunCDDisplay;       // UI image for Player 2 stun cooldown.
    [SerializeField] private float Player2StunCDMaxValue;      // Maximum value for Player 2 stun cooldown.
    [SerializeField] private float Player2StunCDTimer;         // Timer tracking Player 2 stun cooldown.
    [SerializeField] private bool Player2StunCDEnabled = false; // Flag for whether Player 2 stun cooldown is active.

    [SerializeField] public Image Player2shieldCDDisplay;     // UI image for Player 2 shield cooldown.
    [SerializeField] private float Player2ShieldCDMaxValue;    // Maximum value for Player 2 shield cooldown.
    [SerializeField] private float Player2ShieldCDTimer;       // Timer tracking Player 2 shield cooldown.
    [SerializeField] private bool Player2ShieldCDEnabled = false;// Flag for whether Player 2 shield cooldown is active.

    //======================================================
    // Game State Variables
    //======================================================
    [Header("Score, Gamestates and Timers")]
    [SerializeField] public GameObject roundEndTextObject;   // GameObject for round end message.
    [SerializeField] public int player1IngredientCount = 0;    // Tracks Player 1's collected ingredients.
    [SerializeField] public int player2IngredientCount = 0;    // Tracks Player 2's collected ingredients.
    private int totalScore;                                  // Stores the combined score of both players.
    [SerializeField] public float remainingTime = 30;         // Tracks the remaining time in the game round.
    private bool winStateMet = false;                        // Boolean to check if win conditions are met.

    //======================================================
    // Generic GameObjects for Inspector Use
    //======================================================
    [Header("Generic GameObjects for Inspector Use")]
    [SerializeField] public bool competetiveMode = false;      // Flag for competitive mode.
    [SerializeField] public GameObject player1GameObject;      // Reference to Player 1 GameObject.
    [SerializeField] public GameObject player2GameObject;      // Reference to Player 2 GameObject.
    [SerializeField] public CapsuleCollider2D playerCollider1; // Player 1 collider.
    [SerializeField] public CapsuleCollider2D playerCollider2; // Player 2 collider.
    [SerializeField] public int playerLayer1;                  // Layer for Player 1.
    [SerializeField] public int playerLayer2;                  // Layer for Player 2.
    [SerializeField] public BossHP bossDead;                   // Reference to the BossHP script.

    //======================================================
    // Ingredient Tracking Dictionaries
    //======================================================
    private Dictionary<IngredientType, int> player1Ingredients = new Dictionary<IngredientType, int>(); // Tracks Player 1 ingredients.
    private Dictionary<IngredientType, int> player2Ingredients = new Dictionary<IngredientType, int>(); // Tracks Player 2 ingredients.

    //======================================================
    // Cached Components (for efficiency)
    //======================================================
    private PlayerController player1Controller;
    private PlayerController player2Controller;
    private PlayerHealth player1Health;
    private PlayerHealth player2Health;

    //======================================================
    // Start: Initialization of players, components, and cooldown timers.
    //======================================================
    private void Start()
    {
        // Find player GameObjects by tag.
        player1GameObject = GameObject.FindWithTag("Player");
        player2GameObject = GameObject.FindWithTag("Player2");

        // Cache PlayerController and PlayerHealth components.
        player1Controller = player1GameObject.GetComponent<PlayerController>();
        player2Controller = player2GameObject.GetComponent<PlayerController>();
        player1Health = player1GameObject.GetComponent<PlayerHealth>();
        player2Health = player2GameObject.GetComponent<PlayerHealth>();

        // Get colliders and their layers.
        playerCollider1 = player1GameObject.GetComponent<CapsuleCollider2D>();
        playerLayer1 = playerCollider1.gameObject.layer;
        playerCollider2 = player2GameObject.GetComponent<CapsuleCollider2D>();
        playerLayer2 = playerCollider2.gameObject.layer;

        // Get reference to boss object.
        bossDead = FindFirstObjectByType<BossHP>();

        // Initialize cooldown timers from PlayerController settings.
        GetPlayerCDTimers();
    }

    //======================================================
    // GetPlayerCDTimers: Retrieve and set the cooldown timers from PlayerController.
    //======================================================
    public void GetPlayerCDTimers()
    {
        // For Player 2:
        Player2ShootCDMaxValue = player2Controller.shootCoolDown;
        Player2ShootCDTimer = Player2ShootCDMaxValue;
        Player2ShieldCDMaxValue = player2Controller.shieldCooldown + player2Controller.shieldDuration;
        Player2ShieldCDTimer = Player2ShieldCDMaxValue;
        Player2StunCDMaxValue = player2Controller.altShootCoolDown;
        Player2StunCDTimer = Player2StunCDMaxValue;

        // For Player 1:
        Player1ShootCDMaxValue = player1Controller.shootCoolDown;
        Player1ShootCDTimer = Player1ShootCDMaxValue;
        Player1ShieldCDMaxValue = player1Controller.shieldCooldown + player1Controller.shieldDuration;
        Player1ShieldCDTimer = Player1ShieldCDMaxValue;
        Player1StunCDMaxValue = player1Controller.altShootCoolDown;
        Player1StunCDTimer = Player1StunCDMaxValue;
    }

    //======================================================
    // Update: Called once per frame to update game state, UI, cooldowns, and timers.
    //======================================================
    void Update()
    {
        // Handle collision layers based on competitive mode.
        if (!competetiveMode)
        {
            Physics2D.IgnoreLayerCollision(playerLayer1, playerLayer2, true);
            Physics2D.IgnoreLayerCollision(playerLayer2, playerLayer1, true);
            Physics2D.IgnoreLayerCollision(playerLayer1, playerLayer1, true);
            Physics2D.IgnoreLayerCollision(playerLayer2, playerLayer2, true);

            // Update UI with ingredient counts.
            Player1ScoreText.text = "Ingredients Collected: " + player1IngredientCount;
            Player2ScoreText.text = "Ingredients Collected: " + player2IngredientCount;
        }
        else
        {
            Physics2D.IgnoreLayerCollision(playerLayer1, playerLayer2, false);
            Physics2D.IgnoreLayerCollision(playerLayer2, playerLayer1, false);
            Physics2D.IgnoreLayerCollision(playerLayer1, playerLayer1, false);
            Physics2D.IgnoreLayerCollision(playerLayer2, playerLayer2, false);

            // Update UI with remaining lives.
            Player1ScoreText.text = "Lives Remaining: " + player1Health.playerLifeCountRemaining;
            Player2ScoreText.text = "Lives Remaining: " + player2Health.playerLifeCountRemaining;
        }

        // Update cooldowns for Player 2.
        UpdateCooldown(ref Player2ShootCDTimer, Player2ShootCDMaxValue, Player2shootCDDisplay, player2Controller.fired, ref Player2ShootCDEnabled, player2ShootCDTrigger);
        UpdateCooldown(ref Player2ShieldCDTimer, Player2ShieldCDMaxValue, Player2shieldCDDisplay, player2Controller.shielded, ref Player2ShieldCDEnabled, player2ShieldCDTrigger);
        UpdateCooldown(ref Player2StunCDTimer, Player2StunCDMaxValue, Player2StunCDDisplay, player2Controller.altFired, ref Player2StunCDEnabled, player2StunCDTrigger);

        // Update cooldowns for Player 1.
        UpdateCooldown(ref Player1ShootCDTimer, Player1ShootCDMaxValue, Player1shootCDDisplay, player1Controller.fired, ref Player1ShootCDEnabled, player1ShootCDTrigger);
        UpdateCooldown(ref Player1ShieldCDTimer, Player1ShieldCDMaxValue, Player1shieldCDDisplay, player1Controller.shielded, ref Player1ShieldCDEnabled, player1ShieldCDTrigger);
        UpdateCooldown(ref Player1StunCDTimer, Player1StunCDMaxValue, Player1StunCDDisplay, player1Controller.altFired, ref Player1StunCDEnabled, player1StunCDTrigger);

        // Update total score (if needed).
        totalScore = player1IngredientCount + player2IngredientCount;

        // Update game timer.
        remainingTime -= Time.deltaTime;
        int minutes = Mathf.FloorToInt(remainingTime / 60);
        int seconds = Mathf.FloorToInt(remainingTime % 60);
        gameTimerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);

        // Check end-of-round conditions.
        if (remainingTime <= 0 || (bossDead != null && bossDead.bossDead) ||
            player1Health.playerLifeCountRemaining == 0 || player2Health.playerLifeCountRemaining == 0)
        {
            gameStateText();
            gameTimerText.text = "00:00";
            StartCoroutine(gameEnd());
        }
    }

    /// <summary>
    /// UpdateCooldown: Updates a cooldown timer and UI display.
    /// If the ability is triggered and not already on cooldown, starts the cooldown coroutine.
    /// </summary>
    private void UpdateCooldown(ref float timer, float maxValue, Image display, bool triggered, ref bool cooldownEnabled, System.Func<IEnumerator> coroutineFunction)
    {
        if (triggered && !cooldownEnabled)
        {
            StartCoroutine(coroutineFunction());
        }
        display.fillAmount = timer / maxValue;
        timer += Time.deltaTime;
    }

    //======================================================
    // Cooldown Coroutines for Player Abilities
    //======================================================
    private IEnumerator player1ShootCDTrigger()
    {
        Player1ShootCDEnabled = true;
        Player1ShootCDTimer = 0;
        yield return new WaitForSeconds(Player1ShootCDMaxValue);
        Player1ShootCDEnabled = false;
    }

    private IEnumerator player1ShieldCDTrigger()
    {
        Player1ShieldCDEnabled = true;
        Player1ShieldCDTimer = 0;
        yield return new WaitForSeconds(Player1ShieldCDMaxValue);
        Player1ShieldCDEnabled = false;
    }

    private IEnumerator player1StunCDTrigger()
    {
        Player1StunCDEnabled = true;
        Player1StunCDTimer = 0;
        yield return new WaitForSeconds(Player1StunCDMaxValue);
        Player1StunCDEnabled = false;
    }

    private IEnumerator player2ShootCDTrigger()
    {
        Player2ShootCDEnabled = true;
        Player2ShootCDTimer = 0;
        yield return new WaitForSeconds(Player2ShootCDMaxValue);
        Player2ShootCDEnabled = false;
    }

    private IEnumerator player2ShieldCDTrigger()
    {
        Player2ShieldCDEnabled = true;
        Player2ShieldCDTimer = 0;
        yield return new WaitForSeconds(Player2ShieldCDMaxValue);
        Player2ShieldCDEnabled = false;
    }

    private IEnumerator player2StunCDTrigger()
    {
        Player2StunCDEnabled = true;
        Player2StunCDTimer = 0;
        yield return new WaitForSeconds(Player2StunCDMaxValue);
        Player2StunCDEnabled = false;
    }


    //======================================================
    // gameEnd: Handles the end-of-game sequence and scene transition.
    //======================================================
    private IEnumerator gameEnd()
    {
        winStateMet = true;
        yield return new WaitForSeconds(4);
        SceneManager.LoadScene("TitleScreen");
    }

    //======================================================
    // gameModeSwitch: Toggles between competitive and non-competitive modes.
    //======================================================
    public void gameModeSwitch()
    {
        competetiveMode = !competetiveMode;
    }

    //======================================================
    // EndZoneEntry: Called when players enter the end zone; checks if ingredient threshold is met.
    //======================================================
    public void EndZoneEntry(int roundRequiredScore)
    {
        if (player1IngredientCount < roundRequiredScore && player2IngredientCount < roundRequiredScore)
        {
            winStateMet = false;
            RoundEndText.text = "Not Enough Ingredients";
            return;
        }
        remainingTime = 5;
        gameStateText();
    }

    //======================================================
    // gameStateText: Updates the round end text based on win conditions.
    //======================================================
    public void gameStateText()
    {
        if ((winStateMet && player1IngredientCount > player2IngredientCount) ||
            (player1Health.playerLifeCountRemaining > player2Health.playerLifeCountRemaining))
        {
            RoundEndText.text = "Player 1 Win";
        }
        else if ((winStateMet && player2IngredientCount > player1IngredientCount) ||
                 (player1Health.playerLifeCountRemaining < player2Health.playerLifeCountRemaining))
        {
            RoundEndText.text = "Player 2 Win";
        }
        else if (bossDead != null && bossDead.bossDead)
        {
            RoundEndText.text = "You've Killed the Boss!";
        }
        else if (!winStateMet)
        {
            RoundEndText.text = "Witches Lose";
        }
    }

    //======================================================
    // Ingredient Handling Methods
    //======================================================
    public void player1IncreaseIngredient(IngredientType type, GameObject player)
    {
        if (!player1Ingredients.ContainsKey(type))
            player1Ingredients[type] = 0;
        player1Ingredients[type]++;
        player1IngredientCount++;

        if (player1Ingredients[type] >= 5)
        {
            ApplyBuff(player, type);
            player1Ingredients[type] = 0;
        }
    }

    public void player1DecreaseIngredient()
    {
        if (player1Ingredients.Count > 0)
            RemoveRandomIngredient(player1Ingredients);
    }

    public void player2IncreaseIngredient(IngredientType type, GameObject player)
    {
        if (!player2Ingredients.ContainsKey(type))
            player2Ingredients[type] = 0;
        player2Ingredients[type]++;
        player2IngredientCount++;

        if (player2Ingredients[type] >= 5)
        {
            ApplyBuff(player, type);
            player1Ingredients[type] = 0; // Note: Resets Player1's count as per original logic.
        }
    }

    public void player2DecreaseIngredient()
    {
        if (player2Ingredients.Count > 0)
            RemoveRandomIngredient(player2Ingredients);
    }

    private void RemoveRandomIngredient(Dictionary<IngredientType, int> playerIngredients)
    {
        List<IngredientType> ingredientKeys = new List<IngredientType>(playerIngredients.Keys);
        IngredientType randomType = ingredientKeys[Random.Range(0, ingredientKeys.Count)];
        playerIngredients[randomType] -= ingredientKeys.Count / 2;
        if (playerIngredients[randomType] <= 0)
            playerIngredients.Remove(randomType);
    }

    private void ApplyBuff(GameObject player, IngredientType type)
    {
        PlayerController playerController = player.GetComponent<PlayerController>();
        if (playerController == null) return;

        BuffType buffToApply;
        switch (type)
        {
            case IngredientType.Herb:
                
                if (playerController.SpeedBuff) return;
                Debug.LogWarning("Speedbuff applied to:" + playerController.name);
                buffToApply = BuffType.SpeedBoost;
                playerController.SpeedBuff = true;
                break;
            case IngredientType.Finger:
                
                if (playerController.ShootBuff) return;
                Debug.LogWarning("Shootbuff applied to:" + playerController.name);
                buffToApply = BuffType.FireRateIncrease;
                playerController.ShootBuff = true;
                break;

            case IngredientType.FrogLeg:

                if (playerController.StunBuff) return;
                Debug.LogWarning("Shootbuff applied to:" + playerController.name);
                buffToApply = BuffType.StunMultiplier;
                playerController.StunBuff = true;
                break;
            case IngredientType.Spider:
                
                if (playerController.ShieldBuff) return;
                Debug.LogWarning("Shieldbuff applied to:" + playerController.name);
                buffToApply = BuffType.ShieldExtension;
                playerController.ShieldBuff = true;
                break;
            default:
                return;
        }
        playerController.ApplyBuff(buffToApply);
        GetPlayerCDTimers();
    }

    public void ResumeGame()
    {
        isPaused = false;
        Time.timeScale = 1f;
        if (pauseMenuUI != null)
        {
            pauseMenuUI.SetActive(false);
        }
    }

    public void ExitGame()
    {
        SceneManager.LoadScene("TitleScreen");
    }

    public void increaseGameTime(float increaseAmount)
    {
        remainingTime += increaseAmount;
    }
}
