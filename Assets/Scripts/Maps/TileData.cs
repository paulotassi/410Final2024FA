using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "TileData")]
public class TileData : ScriptableObject
{
    public string tileName;
    public GameObject tilePrefab;
    public Vector2Int size = Vector2Int.one;
    public Vector2[] exitVectors = new Vector2[4];
    public List<Direction> exits;  // The directions this tile connects to (e.g., North and East)

    [Tooltip("Relative chance of being picked among tiles with the same exits")]
    [Min(1)] public int weight = 2;
    [Tooltip("Dead-end tile containing the level's end zone. Never picked randomly; swapped in over one dead-end by the generator")]
    public bool isExit = false;
}
