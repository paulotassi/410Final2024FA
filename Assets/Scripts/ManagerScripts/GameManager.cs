using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement; // Required for handling scene transitions.
using UnityEngine.UI;
using TMPro; // Required for using TextMeshPro (TMP) for UI text elements.

public class GameManager : MonoBehaviour
{
    // SerializeField allows private variables to be set from the Unity Inspector.
    [SerializeField] private TMP_Text Player1ScoreText; // Text element to display Player 1's score.
    [SerializeField] private TMP_Text Player2ScoreText; // Text element to display Player 2's score.
    [SerializeField] private TMP_Text RoundEndText; // Text element to display a message when the round ends.
    [SerializeField] private TMP_Text gameTimerText; // Text element to show the remaining game time.
    [SerializeField] public int playerStartingLifeCount = 3;

    [Header("Player 1 CD" +
        "")]

    [SerializeField] public Image Player1shootCDDisplay;
    [SerializeField] private float Player1ShootCDMaxValue;
    [SerializeField] private float Player1ShootCDTimer;
    [SerializeField] private bool Player1ShootCDEnabled = false;

    [SerializeField] public Image Player1shieldCDDisplay;
    [SerializeField] private float Player1ShieldCDMaxValue;
    [SerializeField] private float Player1ShieldCDTimer;
    [SerializeField] private bool Player1ShieldCDEnabled = false;

    [Header("Player 2 CD" +
    "")]

    [SerializeField] public Image Player2shootCDDisplay;
    [SerializeField] private float Player2ShootCDMaxValue;
    [SerializeField] private float Player2ShootCDTimer;
    [SerializeField] private bool Player2ShootCDEnabled = false;

    [SerializeField] public Image Player2shieldCDDisplay;
    [SerializeField] private float Player2ShieldCDMaxValue;
    [SerializeField] private float Player2ShieldCDTimer;
    [SerializeField] private bool Player2ShieldCDEnabled = false;

    [Header("Score, Gamestates and Timers" +
    "")]

    [SerializeField] public GameObject roundEndTextObject; // GameObject used for the round end message (probably enabling/disabling visibility).

    [SerializeField] public int player1IngredientCount = 0; // Tracks Player 1's collected ingredients.
    [SerializeField] public int player2IngredientCount = 0; // Tracks Player 2's collected ingredients.
    private int totalScore; // Stores the combined score of both players.

    [SerializeField] public float remainingTime = 30; // Tracks the time left in the game round.
    private bool winStateMet = false; // Boolean to check if win conditions are met.

    [Header("Generic GameObjects for Inspector Use" +
    "")]

    [SerializeField] public bool competetiveMode = false;
    [SerializeField] public GameObject player1GameObject;
    [SerializeField] public GameObject player2GameObject;
    [SerializeField] public CapsuleCollider2D playerCollider1;
    [SerializeField] public CapsuleCollider2D playerCollider2;
    [SerializeField] public int playerLayer1;
    [SerializeField] public int playerLayer2;
    [SerializeField] public BossHP bossDead;


    private Dictionary<IngredientType, int> player1Ingredients = new Dictionary<IngredientType, int>();
    private Dictionary<IngredientType, int> player2Ingredients = new Dictionary<IngredientType, int>();


    // Update is called once per frame (every frame)

    private void Start()
    {
        player1GameObject = GameObject.FindWithTag("Player");
        player2GameObject = GameObject.FindWithTag("Player2");

        playerCollider1 = player1GameObject.GetComponent<CapsuleCollider2D>();
        playerLayer1 = playerCollider1.gameObject.layer;
        playerCollider2 = player2GameObject.GetComponent<CapsuleCollider2D>();
        playerLayer2 = playerCollider2.gameObject.layer;
        bossDead = FindFirstObjectByType<BossHP>();
       

        Player2ShieldCDMaxValue = player2GameObject.GetComponent<PlayerController>().shieldCooldown + player2GameObject.GetComponent<PlayerController>().shieldDuration;
        Player2ShootCDMaxValue = player2GameObject.GetComponent<PlayerController>().shootCoolDown;
        Player2ShootCDTimer = Player2ShootCDMaxValue;
        Player2ShieldCDTimer = Player2ShieldCDMaxValue;

        Player1ShieldCDMaxValue = player1GameObject.GetComponent<PlayerController>().shieldCooldown + player1GameObject.GetComponent<PlayerController>().shieldDuration;
        Player1ShootCDMaxValue = player1GameObject.GetComponent<PlayerController>().shootCoolDown;
        Player1ShootCDTimer = Player1ShootCDMaxValue;
        Player1ShieldCDTimer = Player1ShieldCDMaxValue;

    }
    void Update()
    {

        if (competetiveMode == false)
        {
            Physics2D.IgnoreLayerCollision(playerLayer1, playerLayer2, true);
            Physics2D.IgnoreLayerCollision(playerLayer2, playerLayer1, true);
            Physics2D.IgnoreLayerCollision(playerLayer2, playerLayer2, true);
            Physics2D.IgnoreLayerCollision(playerLayer1, playerLayer1, true);        // Update the UI with the current number of ingredients for each player.
            Player1ScoreText.text = "Ingredients Collected: " + player1IngredientCount;
            Player2ScoreText.text = "Ingredients Collected: " + player2IngredientCount;
        } 
        else if (competetiveMode)
        {
            Physics2D.IgnoreLayerCollision(playerLayer1, playerLayer2, false);
            Physics2D.IgnoreLayerCollision(playerLayer2, playerLayer1, false);
            Physics2D.IgnoreLayerCollision(playerLayer2, playerLayer2, false);
            Physics2D.IgnoreLayerCollision(playerLayer1, playerLayer1, false);
            Player1ScoreText.text = "Lives Remaining: " + player1GameObject.GetComponent<PlayerHealth>().playerLifeCountRemaining;
            Player2ScoreText.text = "Lives Remaining: " + player2GameObject.GetComponent<PlayerHealth>().playerLifeCountRemaining;
        }

        


        //Display Shoot CD for Player 2
        if (player2GameObject.GetComponent<PlayerController>().fired && Player2ShootCDEnabled == false)
        {
            StartCoroutine(player2ShootCDTrigger());
        }
        Player2shootCDDisplay.fillAmount = Player2ShootCDTimer/Player2ShootCDMaxValue;
        Player2ShootCDTimer += Time.deltaTime;

        if (player2GameObject.GetComponent<PlayerController>().shielded && Player2ShieldCDEnabled == false)
        {
            StartCoroutine(player2ShieldCDTrigger());
        }
        Player2shieldCDDisplay.fillAmount = Player2ShieldCDTimer / Player2ShieldCDMaxValue;
        Player2ShieldCDTimer += Time.deltaTime;

        //Display Shoot CD for Player 1
        if (player1GameObject.GetComponent<PlayerController>().fired && Player1ShootCDEnabled == false)
        {
            StartCoroutine(player1ShootCDTrigger());
        }
        Player1shootCDDisplay.fillAmount = Player1ShootCDTimer / Player1ShootCDMaxValue;
        Player1ShootCDTimer += Time.deltaTime;

        if (player1GameObject.GetComponent<PlayerController>().shielded && Player1ShieldCDEnabled == false)
        {
            StartCoroutine(player1ShieldCDTrigger());
        }
        Player1shieldCDDisplay.fillAmount = Player1ShieldCDTimer / Player1ShieldCDMaxValue;
        Player1ShieldCDTimer += Time.deltaTime;

        // Calculate the total score (sum of both players' collected ingredients). THIS CAN BE REMOVED
        totalScore = player1IngredientCount + player2IngredientCount;

        // Decrease the remaining game time, using Time.deltaTime to account for real-time.
        remainingTime -= Time.deltaTime;

        // Convert remaining time to minutes and seconds.
        int minutes = Mathf.FloorToInt(remainingTime / 60); // Minutes component.
        int seconds = Mathf.FloorToInt(remainingTime % 60); // Seconds component.

        // Update the game timer text on the UI, formatted as MM:SS.
        gameTimerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);

        // If the remaining time is less than or equal to 4 seconds...
        if (remainingTime <= 0 || bossDead!= null && bossDead.bossDead == true || player1GameObject.GetComponent<PlayerHealth>().playerLifeCountRemaining == 0 || player2GameObject.GetComponent<PlayerHealth>().playerLifeCountRemaining == 0)
        {
            // Display the game state (win/lose) text.
            gameStateText();
            gameTimerText.text = "00:00";

            // Call the gameEnd method to handle the end of the game.
            StartCoroutine(gameEnd());
         
        }
    }

    private IEnumerator player1ShootCDTrigger()
    {
        Player1ShootCDEnabled = true;
        Player1ShootCDTimer = 0;
        yield return new WaitForSeconds(0);

        yield return new WaitForSeconds(Player1ShootCDMaxValue);
        Player1ShootCDEnabled = false;

    }



    private IEnumerator player1ShieldCDTrigger()
    {
        Player1ShieldCDEnabled = true;
        Player1ShieldCDTimer = 0;
        yield return new WaitForSeconds(0);

        yield return new WaitForSeconds(Player1ShieldCDMaxValue);
        Player1ShieldCDEnabled = false;

    }

    private IEnumerator player2ShootCDTrigger()
    {
        Player2ShootCDEnabled = true;
        Player2ShootCDTimer = 0;
        yield return new WaitForSeconds(0);
        
        yield return new WaitForSeconds(Player2ShootCDMaxValue);
        Player2ShootCDEnabled = false;

    }

    private IEnumerator player2ShieldCDTrigger()
    {
        Player2ShieldCDEnabled = true;
        Player2ShieldCDTimer = 0;
        yield return new WaitForSeconds(0);

        yield return new WaitForSeconds(Player2ShieldCDMaxValue);
        Player2ShieldCDEnabled = false;

    }

    // Coroutine to handle the game ending and transition to another scene after a delay.
    private IEnumerator gameEnd()
    {
        // Wait for  seconds before executing the next line.

        winStateMet = true;
        // Wait for the valid duration
        yield return new WaitForSeconds(4);

        // Load the scene named "MVPScene" when the round ends.
        SceneManager.LoadScene("TitleScreen");
    }

    public void gameModeSwitch()
    {
        competetiveMode = !competetiveMode;
    }
    // Method to handle what happens when players enter the end zone.
    // This checks if the players have collected enough ingredients to win. THIS CAN BE REMOVED
    public void EndZoneEntry(int roundRequiredScore)
    {
        // If the total score is less than the required score for the round...
        if (player1IngredientCount < roundRequiredScore && player2IngredientCount < roundRequiredScore)
        {
            winStateMet = false;
            // Display "Not Enough Ingredients" and exit the function.
            RoundEndText.text = "Not Enough Ingredients";
            return;
        }

        // If enough ingredients were collected, set winStateMet to true.
        
        remainingTime = 5; 

        // Display the appropriate game state text.
        gameStateText();
    }


    // Method to update the round end text based on whether the win condition was met.
    public void gameStateText()
    {
        // If the win condition is met, display "Witches Win".
        if (winStateMet == true && player1IngredientCount > player2IngredientCount || player1GameObject.GetComponent<PlayerHealth>().playerLifeCountRemaining > player2GameObject.GetComponent<PlayerHealth>().playerLifeCountRemaining)
        {
            RoundEndText.text = "Player 1 Win";
        }
        else if (winStateMet == true && player2IngredientCount > player1IngredientCount || player1GameObject.GetComponent<PlayerHealth>().playerLifeCountRemaining < player2GameObject.GetComponent<PlayerHealth>().playerLifeCountRemaining)
        {
            RoundEndText.text = "Player 2 Win";
        }
        else if (bossDead != null && bossDead.bossDead == true)
        {
            RoundEndText.text = "You've Killed the Boss!";
        }
        // If the win condition is not met, display "Witches Lose".
        else if (winStateMet == false)
        {
            RoundEndText.text = "Witches Lose";
        }
    }

    // Method to increase Player 1's ingredient count by 1.
     public void player1IncreaseIngredient(IngredientType type)
    {
        if (!player1Ingredients.ContainsKey(type))
            player1Ingredients[type] = 0;

        player1Ingredients[type]++;

        if (player1Ingredients[type] >= 5)
        {

        }
            
    }
    public void player1DecreaseIngredient()
    {
        if (player1Ingredients.Count > 0)
            RemoveRandomIngredient(player1Ingredients);
    }

    // Method to increase Player 2's ingredient count by 1.
    
    public void player2IncreaseIngredient(IngredientType type)
    {
        if (!player2Ingredients.ContainsKey(type))
            player2Ingredients[type] = 0;

        player2Ingredients[type]++;

        if (player2Ingredients[type] >= 5)
        { 

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

        playerIngredients[randomType]-=ingredientKeys.Count/2;

        if (playerIngredients[randomType] <= 0)
            playerIngredients.Remove(randomType); // Remove if count reaches zero

    }

    private void ApplyBuff(GameObject player, IngredientType type)
    {
        Debug.Log($"{player.name} received a buff for collecting 5 {type}!");

        PlayerController playerController = player.GetComponent<PlayerController>();

        if (playerController != null)
        {
            Debug.Log("buff logic here?");
        }
    }

    public void increaseGameTime(float increaseAmount)
    {
        remainingTime = remainingTime + increaseAmount;
    }
}
