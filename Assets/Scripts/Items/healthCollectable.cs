using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class healthCollectable : collectableObject
{
    public GameManager Manager;
    public int healthIncrease;

    public void Start()
    {
        Manager = FindFirstObjectByType<GameManager>();


    }
    public override void Player1CollectItem()
    {

        base.Player1CollectItem();
        Manager.player1IncreaseIngredient();
        Manager.player1GameObject.GetComponent<PlayerHealth>().TakeDamage(-healthIncrease);
    }

    public override void Player2CollectItem()
    {
        base.Player2CollectItem();
        Manager.player2IncreaseIngredient();
        Manager.player2GameObject.GetComponent<PlayerHealth>().TakeDamage(-healthIncrease);
    }
}
