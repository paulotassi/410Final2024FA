// MapGenerator.cs
using System.Collections.Generic;
using UnityEngine;

public class MapGenerator : MonoBehaviour
{
    [Header("Designer Controls")]
    [Tooltip("Controls the overall size of the generated map. 1 = small, 10 = large")]
    [Range(1, 10)]
    public int mapSize = 5;

    [Space]
    [Header("References - DO NOT TOUCH")]
    [Tooltip("DO NOT TOUCH - All available tile types, assigned by a programmer")]
    public TileData[] tileDatas;
    [Tooltip("DO NOT TOUCH - The tile placed at the center of the map")]
    public TileData startingTile;
    [Tooltip("DO NOT TOUCH - Distance between tiles in world units")]
    public float tileSpacing = 22f;
    [Tooltip("DO NOT TOUCH - Parent object that keeps the tile hierarchy clean")]
    public Transform tileParent;

    // Calculated from mapSize at runtime, never exposed to the Inspector
    private int minTiles;
    private int maxTiles;

    private Dictionary<Vector2Int, TileData> placedTiles = new Dictionary<Vector2Int, TileData>();
    private Queue<OpenExit> openExits = new Queue<OpenExit>();

    void Start()
    {
        // Translate the designer-facing mapSize into internal tile counts
        minTiles = mapSize * 20;
        maxTiles = mapSize * 30;

        GenerateMap();
    }

    void GenerateMap()
    {
        Vector2Int startPos = Vector2Int.zero;
        PlaceTile(startingTile, startPos);

        // Always populate exits from the starting tile first
        AddExits(startPos, startingTile.exits, null);

        // Keep filling exits until we hit maxTiles or run out of exits
        while (openExits.Count > 0 && placedTiles.Count < maxTiles)
        {
            OpenExit open = openExits.Dequeue();

            // Skip if a tile already exists at this position
            if (placedTiles.ContainsKey(open.position)) continue;

            if (placedTiles.Count < minTiles)
            {
                // Still building - exclude cap tiles until minTiles is reached
                bool reachedMin = placedTiles.Count >= minTiles; // flag passed into tile picker
                TileData selected = GetRandomMatchingTile(open.neededExit, reachedMin);
                if (selected == null) continue;

                PlaceTile(selected, open.position);
                AddExits(open.position, selected.exits, open.neededExit);
            }
            else
            {
                // Hit minTiles - cap remaining exits with dead-end tiles
                TileData capTile = GetSingleExitTile(open.neededExit);
                if (capTile != null)
                {
                    PlaceTile(capTile, open.position);
                }
            }
        }
    }

    // Places a tile prefab at the given grid position
    void PlaceTile(TileData tileData, Vector2Int gridPosition)
    {
        Vector3 worldPos = new Vector3(gridPosition.x * tileSpacing, gridPosition.y * tileSpacing, 0);
        Instantiate(tileData.tilePrefab, worldPos, Quaternion.identity, tileParent);
        placedTiles.Add(gridPosition, tileData);
    }

    // Adds a tile's exits to the open exit queue, skipping the entrance we just came from
    void AddExits(Vector2Int pos, List<Direction> exits, Direction? entryDirection)
    {
        foreach (Direction exit in exits)
        {
            // Don't loop back through the entrance we arrived from
            if (entryDirection.HasValue && exit == entryDirection.Value) continue;

            Vector2Int neighborPos = GetOffsetPosition(pos, exit);
            openExits.Enqueue(new OpenExit(neighborPos, GetOppositeDirection(exit)));
        }
    }

    // Returns a random tile that has the required exit direction
    // If allowCapTiles is false, single-exit tiles are excluded from the pool
    TileData GetRandomMatchingTile(Direction neededExit, bool allowCapTiles)
    {
        List<TileData> matches = new List<TileData>();

        foreach (TileData tile in tileDatas)
        {
            if (!tile.exits.Contains(neededExit)) continue;

            // Before minTiles is reached, skip any tile with only one exit
            if (!allowCapTiles && tile.exits.Count == 1) continue;

            matches.Add(tile);
        }

        if (matches.Count == 0) return null;
        return matches[Random.Range(0, matches.Count)];
    }

    // Returns the grid position one step in the given direction
    Vector2Int GetOffsetPosition(Vector2Int pos, Direction dir)
    {
        switch (dir)
        {
            case Direction.North: return pos + new Vector2Int(0, 1);
            case Direction.South: return pos + new Vector2Int(0, -1);
            case Direction.East: return pos + new Vector2Int(1, 0);
            case Direction.West: return pos + new Vector2Int(-1, 0);
            default: return pos;
        }
    }

    // Flips a direction to its opposite
    Direction GetOppositeDirection(Direction dir)
    {
        switch (dir)
        {
            case Direction.North: return Direction.South;
            case Direction.South: return Direction.North;
            case Direction.East: return Direction.West;
            case Direction.West: return Direction.East;
            default: return dir;
        }
    }

    // Returns a dead-end tile with only one exit in the specified direction
    TileData GetSingleExitTile(Direction dir)
    {
        foreach (TileData tile in tileDatas)
        {
            if (tile.exits.Count == 1 && tile.exits[0] == dir)
            {
                return tile;
            }
        }
        return null;
    }
}