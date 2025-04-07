using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum IngredientType { Eyeball, Herb, Spider, Finger, FrogLeg } // Example types

public class Ingredient : collectableObject
{
    public GameManager Manager;
    public IngredientType ingredientType; // Each prefab has a predefined type
    public GameObject[] ingredients; // Array of ingredient prefabs

    public void Start()
    {
        Manager = FindFirstObjectByType<GameManager>();

        if (ingredients.Length > 0)
        {
            // Pick a random prefab
            GameObject newIngredientPrefab = ingredients[Random.Range(0, ingredients.Length)];

            // Instantiate it at the current position and rotation
            GameObject newIngredient = Instantiate(newIngredientPrefab, transform.position, transform.rotation);

            // Destroy the placeholder ingredient object
            Destroy(gameObject);
        }
    }

    public override void Player1CollectItem(Collider2D playerCollided)
    {
        base.Player1CollectItem(playerCollided);
        Manager.Player1IncreaseIngredient(ingredientType, playerCollided.gameObject);
    }

    public override void Player2CollectItem(Collider2D playerCollided)
    {
        base.Player2CollectItem(playerCollided);
        Manager.Player2IncreaseIngredient(ingredientType, playerCollided.gameObject);
    }
}
