using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;

public class GameManager : MonoBehaviour
{
    [SerializeField] private TMP_Text Player1ScoreText;
    [SerializeField] private TMP_Text Player2ScoreText;
    [SerializeField] private TMP_Text WinText;
    [SerializeField] private TMP_Text timerDisplay;
    [SerializeField] public int player1IngredientCount = 0;
    [SerializeField] public int player2IngredientCount = 0;
    [SerializeField] public float currentTime;
    [SerializeField] public int totalTime = 60;

// Start is called before the first frame update

    void Update()
    {
        Player1ScoreText.text = "Ingredients Collected: " + player1IngredientCount;
        Player2ScoreText.text = "Ingredients Collected: " + player2IngredientCount;
        
        currentTime += Time.deltaTime;

        int minutes = Mathf.FloorToInt((totalTime/60) - (currentTime) * -1);

        int seconds = totalTime - Mathf.FloorToInt(currentTime % 60) ;

        string timerString = string.Format("{0:00}:{1:00}", minutes, seconds);

        timerDisplay.text = timerString;

    }

    public void player1IncreaseIngredient()
    {
        player2IngredientCount++;
    }
    public void player2IncreaseIngredient()
    {
        player2IngredientCount++;
    }
}
