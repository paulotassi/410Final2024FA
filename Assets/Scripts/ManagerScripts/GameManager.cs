using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement; // Required for handling scene transitions.
using TMPro; // Required for using TextMeshPro (TMP) for UI text elements.

public class GameManager : MonoBehaviour
{
    // SerializeField allows private variables to be set from the Unity Inspector.
    [SerializeField] private TMP_Text Player1ScoreText; // Text element to display Player 1's score.
    [SerializeField] private TMP_Text Player2ScoreText; // Text element to display Player 2's score.
    [SerializeField] private TMP_Text RoundEndText; // Text element to display a message when the round ends.
    [SerializeField] private TMP_Text gameTimerText; // Text element to show the remaining game time.

    [SerializeField] public GameObject roundEndTextObject; // GameObject used for the round end message (probably enabling/disabling visibility).

    [SerializeField] public int player1IngredientCount = 0; // Tracks Player 1's collected ingredients.
    [SerializeField] public int player2IngredientCount = 0; // Tracks Player 2's collected ingredients.
    private int totalScore; // Stores the combined score of both players.

    [SerializeField] public float remainingTime; // Tracks the time left in the game round.
    private bool winStateMet = false; // Boolean to check if win conditions are met.

    // Update is called once per frame (every frame)
    void Update()
    {
        // Update the UI with the current number of ingredients for each player.
        Player1ScoreText.text = "Ingredients Collected: " + player1IngredientCount;
        Player2ScoreText.text = "Ingredients Collected: " + player2IngredientCount;

        // Calculate the total score (sum of both players' collected ingredients).
        totalScore = player1IngredientCount + player2IngredientCount;

        // Decrease the remaining game time, using Time.deltaTime to account for real-time.
        remainingTime -= Time.deltaTime;

        // Convert remaining time to minutes and seconds.
        int minutes = Mathf.FloorToInt(remainingTime / 60); // Minutes component.
        int seconds = Mathf.FloorToInt(remainingTime % 60); // Seconds component.

        // Update the game timer text on the UI, formatted as MM:SS.
        gameTimerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);

        // If the remaining time is less than or equal to 4 seconds...
        if (remainingTime <= 4)
        {
            // Display the game state (win/lose) text.
            gameStateText();

            // Call the gameEnd method to handle the end of the game.
            gameEnd();
        }
    }

    // Coroutine to handle the game ending and transition to another scene after a delay.
    private IEnumerator gameEnd()
    {
        // Wait for (remainingTime - 1) seconds before executing the next line.
        yield return new WaitForSeconds(remainingTime - 1);

        // Load the scene named "MVPScene" when the round ends.
        SceneManager.LoadScene("MVPScene");
    }

    // Method to handle what happens when players enter the end zone.
    // This checks if the players have collected enough ingredients to win.
    public void EndZoneEntry(int roundRequiredScore)
    {
        // If the total score is less than the required score for the round...
        if (totalScore < roundRequiredScore)
        {
            // Display "Not Enough Ingredients" and exit the function.
            RoundEndText.text = "Not Enough Ingredients";
            return;
        }

        // If enough ingredients were collected, set winStateMet to true.
        winStateMet = true;

        // Display the appropriate game state text.
        gameStateText();
    }

    // Method to update the round end text based on whether the win condition was met.
    public void gameStateText()
    {
        // If the win condition is met, display "Witches Win".
        if (winStateMet == true)
        {
            RoundEndText.text = "Witches Win";
        }
        // If the win condition is not met, display "Witches Lose".
        else if (winStateMet == false)
        {
            RoundEndText.text = "Witches Lose";
        }
    }

    // Method to increase Player 1's ingredient count by 1.
    public void player1IncreaseIngredient()
    {
        player1IngredientCount++;
    }

    // Method to increase Player 2's ingredient count by 1.
    public void player2IncreaseIngredient()
    {
        player2IngredientCount++;
    }
}
