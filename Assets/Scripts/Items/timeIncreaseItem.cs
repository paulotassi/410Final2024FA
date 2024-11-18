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
    public override void Player1CollectItem()
    {
        base.Player1CollectItem();
        Manager.increaseGameTime(15);
    }

    public override void Player2CollectItem()
    {
        base.Player2CollectItem();
        Manager.increaseGameTime(15);
    }
}
