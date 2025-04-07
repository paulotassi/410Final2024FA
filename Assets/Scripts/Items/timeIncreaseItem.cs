using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class timeIncreaseItem : collectableObject
{
    public GameManager Manager;

    public void Start()
    {
        Manager = FindFirstObjectByType<GameManager>();
    }
    public override void Player1CollectItem(Collider2D playerCollided)
    {
        base.Player1CollectItem(playerCollided);
        Manager.IncreaseGameTime(15);
    }

    public override void Player2CollectItem(Collider2D playerCollided)
    {
        base.Player2CollectItem(playerCollided);
        Manager.IncreaseGameTime(15);
    }
}
