using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviour
{
    [SerializeField] private TMP_Text Player1ScoreText;
    [SerializeField] private TMP_Text Player2ScoreText;
    [SerializeField] public int player1IngredientCount = 0;
    [SerializeField] public int player2IngredientCount = 0;
   

    // Start is called before the first frame update

    void Update()
    {
        Player1ScoreText.text = "Ingredients Collected: " + player1IngredientCount;
        Player2ScoreText.text = "Ingredients Collected: " + player2IngredientCount;
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
