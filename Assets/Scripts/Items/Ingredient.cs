using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ingredient : collectableObject
{
    public GameManager Manager;

    public void Start()
    {
        Manager = FindFirstObjectByType<GameManager>();
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
