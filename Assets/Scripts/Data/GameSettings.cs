using UnityEngine;

public static class GameSettings 
{
    public static bool singlePlayerMode = true;
    public static bool competetiveMode = false;
    public static bool arcadeMode = false;

    // Procedural (roguelike) run settings, set by the title screen before loading the procedural scene
    public static bool proceduralMode = false;
    public static int proceduralMapSize = 0;   // 0 = use the MapGenerator's own inspector value
}
