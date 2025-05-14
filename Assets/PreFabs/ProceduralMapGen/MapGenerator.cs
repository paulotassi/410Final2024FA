// MapGenerator.cs
using System.Collections.Generic;
using UnityEngine;

public class MapGenerator : MonoBehaviour
{
    public TileData[] tileDatas;                     // All available tile types (set in Inspector)
    public TileData startingTile;                    // The tile to start with
    public int maxTiles = 10;                        // Total number of tiles to generate
    public float tileSpacing = 22f;                  // Space between tiles in world units
    public Transform tileParent;                     // Parent object to keep hierarchy clean

    private Dictionary<Vector2Int, TileData> placedTiles = new Dictionary<Vector2Int, TileData>();
    private Queue<OpenExit> openExits = new Queue<OpenExit>();

    void Start()
    {
        GenerateMap();
    }


    void GenerateMap()
    {
        Vector2Int startPos = Vector2Int.zero;
        PlaceTile(startingTile, startPos);

        // Add exits of the starting tile to the open exit list
        foreach (Direction exit in startingTile.exits)
        {
            openExits.Enqueue(new OpenExit(GetOffsetPosition(startPos, exit), GetOppositeDirection(exit)));
        }

        // Generate the map by filling in exits
        while (openExits.Count > 0 && placedTiles.Count < maxTiles)
        {
            OpenExit open = openExits.Dequeue();

            // Skip if tile already exists at this position
            if (placedTiles.ContainsKey(open.position)) continue;

            // Find all tile options that contain the needed exit
            List<TileData> matchingTiles = new List<TileData>();
            foreach (TileData tile in tileDatas)
            {
                if (tile.exits.Contains(open.neededExit))
                {
                    matchingTiles.Add(tile);
                }
            }

            if (matchingTiles.Count == 0) continue; // No match, skip

            // Pick a random matching tile
            TileData selectedTile = matchingTiles[Random.Range(0, matchingTiles.Count)];
            PlaceTile(selectedTile, open.position);

            // Add new open exits from this tile, except the one that was just filled
            foreach (Direction newExit in selectedTile.exits)
            {
                if (newExit == open.neededExit) continue; // Don’t loop back

                Vector2Int newPos = GetOffsetPosition(open.position, newExit);
                openExits.Enqueue(new OpenExit(newPos, GetOppositeDirection(newExit)));
            }
        }

        // Close remaining exits with single-exit tiles
        while (openExits.Count > 0)
        {
            OpenExit finalOpen = openExits.Dequeue();

            if (placedTiles.ContainsKey(finalOpen.position)) continue;

            TileData capTile = GetSingleExitTile(finalOpen.neededExit);
            if (capTile != null)
            {
                PlaceTile(capTile, finalOpen.position);
            }
        }
    }

    void PlaceTile(TileData tileData, Vector2Int gridPosition)
    {
        Vector3 worldPos = new Vector3(gridPosition.x * tileSpacing, gridPosition.y * tileSpacing,0 );
        GameObject tile = Instantiate(tileData.tilePrefab, worldPos, Quaternion.identity, tileParent);
        //tile.name = tileData.tileName + "_" + gridPosition;
        placedTiles.Add(gridPosition, tileData);
    }

    // Gets the grid offset based on a direction
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

    // Reverses the direction (needed for exit compatibility)
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

    // Picks a tile with only one exit in the specified direction
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
