using UnityEngine;

public abstract class PlayerState
{
    protected PlayerController player;

    public PlayerState(PlayerController player)
    {
        this.player = player;
    }

    public abstract void Enter();   // Called when entering a state
    public abstract void Update();  // Called every frame
    public abstract void FixedUpdate(); // Called every physics frame
    public abstract void Exit();    // Called when exiting a state
}