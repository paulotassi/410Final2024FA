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
    public override void Player1CollectItem(Collider2D playerCollided)
    {

        base.Player1CollectItem(playerCollided);
        Manager.player1GameObject.GetComponent<PlayerHealth>().TakeDamage(-healthIncrease);
    }

    public override void Player2CollectItem(Collider2D playerCollided)
    {
        base.Player2CollectItem(playerCollided);
        Manager.player2GameObject.GetComponent<PlayerHealth>().TakeDamage(-healthIncrease);
    }
}
