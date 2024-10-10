using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class GameManager : MonoBehaviour
{
    [SerializeField] private TMP_Text Player1ScoreText;
    [SerializeField] private TMP_Text Player2ScoreText;
    [SerializeField] private TMP_Text RoundEndText;
    [SerializeField] private TMP_Text gameTimerText;
    [SerializeField] public GameObject roundEndTextObject;
    [SerializeField] public int player1IngredientCount = 0;
    [SerializeField] public int player2IngredientCount = 0;
    [SerializeField] public int totalScore;

    [SerializeField] public float remainingTime;
    [SerializeField] private bool winStateMet = false;
   

    // Start is called before the first frame update

    void Update()
    {
        Player1ScoreText.text = "Ingredients Collected: " + player1IngredientCount;
        Player2ScoreText.text = "Ingredients Collected: " + player2IngredientCount;
        totalScore = player1IngredientCount + player2IngredientCount;
        remainingTime -= Time.deltaTime;
        
        int minutes = Mathf.FloorToInt(remainingTime / 60);
        int seconds = Mathf.FloorToInt(remainingTime % 60);
        gameTimerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
        if (remainingTime <= 4)
        {
            gameStateText();
            gameEnd();
        }
    }

    private IEnumerator gameEnd()
    {

        yield return new WaitForSeconds(remainingTime - 1);
        SceneManager.LoadScene("MVPScene");

    }
    public void EndZoneEntry(int roundRequiredScore)
    {
        if (totalScore < roundRequiredScore)
        {
            RoundEndText.text = "Not Enough Ingredients";
            return;
            
        }
        winStateMet = true;
        gameStateText();
    }
    public void gameStateText()
    {
        if (winStateMet == true)
        {
            RoundEndText.text = "Witches Win";
        }
        else if (winStateMet == false) 
        {
            RoundEndText.text = "Witches Lose";
        }
    }



    public void player1IncreaseIngredient()
    {
        player1IngredientCount++;
    }
    public void player2IncreaseIngredient()
    {
        player2IngredientCount++;
    }

   
}
