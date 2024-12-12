using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ingredient : collectableObject
{
    public GameManager Manager;

    // Array to store sprites
    public Sprite[] sprites;

    // Reference to the SpriteRenderer component
    private SpriteRenderer spriteRenderer;

    public void Start()
    {
        Manager = FindFirstObjectByType<GameManager>();

        // Get the SpriteRenderer component
        spriteRenderer = GetComponent<SpriteRenderer>();

        // Randomly select a sprite from the array and assign it
        if (sprites.Length > 0 && spriteRenderer != null)
        {
            Sprite randomSprite = sprites[Random.Range(0, sprites.Length)];
            spriteRenderer.sprite = randomSprite;
        }
    }

    public override void Player1CollectItem()
    {
        base.Player1CollectItem();
        Manager.player1IncreaseIngredient(1);
    }

    public override void Player2CollectItem()
    {
        base.Player2CollectItem();
        Manager.player2IncreaseIngredient(1);
    }
}
