using System.Collections.Generic;
using UnityEngine; 
public class OpenExit
{
    public Vector2Int position;     // Where a new tile should be placed
    public Direction neededExit;    // Which direction that tile needs to have (to connect)

    public OpenExit(Vector2Int position, Direction neededExit)
    {
        this.position = position;
        this.neededExit = neededExit;
    }
}

public enum Direction
{
    North,
    East,
    South,
    West
}
