using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;

public class PracticeMPG : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public TileData[] tileDatas;        // All possible tiles
    public TileData startingTile;       // Starting tile
    public int maxTiles = 10;           // Max tiles to generate
    public float tileSpacing = 22f;     // Distance between tiles
    public Transform tileParent;        // Parent for spawned tiles

    private Dictionary<Vector2Int, TileData> placedTiles =  new Dictionary<Vector2Int, TileData>();
    private Queue<OpenExit> openExits = new Queue<OpenExit>();
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public Vector2Int GetOffSetPosition(Vector2Int pos, Direction dir, Vector2Int tileSize)
    {
        switch (dir)
        {
            case Direction.North: return pos + new Vector2Int(0, tileSize.y);
            case Direction.South: return pos + new Vector2Int(0, -tileSize.y);
            case Direction.East: return pos + new Vector2Int(tileSize.x, 0);
            case Direction.West: return pos + new Vector2Int(-tileSize.x, 0);
            default: return pos;
        }
    }

    public Direction GetOppositeDirection(Direction originaldir)
    {
        switch(originaldir)
        {
                case Direction.North: return Direction.South;
                case Direction.South: return Direction.North;
                case Direction.East: return Direction.West;
                case Direction.West: return Direction.East;
                default: return originaldir;

        }
    }

    public void GenerateMap()
    {
        Vector2Int startingPos = new Vector2Int(0, 0);
        PlaceANewTile(startingTile, startingPos);

        foreach (Direction exit in startingTile.exits)
        {
            Vector2Int exitPos = GetOffSetPosition(startingPos, exit, startingTile.size);
            Direction neededExitInNewTile = GetOppositeDirection(exit);
            openExits.Enqueue(new OpenExit(exitPos, neededExitInNewTile));
        }

        int tilesPlaced = 1; // we already placed the starting tile

        // Keep placing normal tiles until we get too close to the limit
        while (openExits.Count > 0 && tilesPlaced < maxTiles)
        {
            // If the number of remaining tiles is less than or equal to
            // the number of open exits, stop branching and break out.
            if ((maxTiles - tilesPlaced) <= openExits.Count)
            {
                break;
            }

            OpenExit currentExit = openExits.Dequeue();
            TileData newTile = FindTileWithExit(currentExit.neededExit, currentExit.position);

            if (newTile == null) continue;

            PlaceANewTile(newTile, currentExit.position);
            tilesPlaced++;

            foreach (Direction exit in newTile.exits)
            {
                if (exit == GetOppositeDirection(currentExit.neededExit)) continue;

                Vector2Int exitPos = GetOffSetPosition(currentExit.position, exit, newTile.size);
                openExits.Enqueue(new OpenExit(exitPos, GetOppositeDirection(exit)));
            }
        }

        // At this point we must cap off the rest
        while (openExits.Count > 0)
        {
            OpenExit finalOpen = openExits.Dequeue();

            if (placedTiles.ContainsKey(finalOpen.position)) continue;

            TileData capTile = GetSingleExitTile(finalOpen.neededExit);
            if (capTile != null)
            {
                PlaceANewTile(capTile, finalOpen.position);
                tilesPlaced++;
            }
        }

    }

    public TileData FindTileWithExit(Direction requiredDirection, Vector2Int position)
    {
        if (placedTiles.ContainsKey(position))
        {
            TileData existingTile = placedTiles[position];
            if (existingTile.exits.Contains(requiredDirection))
            {
                return existingTile; // ✅ compatible, keep the old one
            }
            else
            {
                return null; // ❌ incompatible, skip this spot
            }
        }

        List<TileData> candidateTiles = new List<TileData>();

        for (int i = 0; i < tileDatas.Length; i++)
        {
            TileData currentTileDate = tileDatas[i];
            if (currentTileDate == null) continue;
            if (currentTileDate.exits == null) continue;

            if (currentTileDate.exits.Contains(requiredDirection))
            {
                candidateTiles.Add(currentTileDate);
            }  

        }
        // If nothing fits, we signal "no tile" by returning null
        if (candidateTiles.Count == 0)
                return null;

            int index = UnityEngine.Random.Range(0, candidateTiles.Count);

            return candidateTiles[index]; //Produces a SINGLE tile
    }
    public void PlaceANewTile(TileData tile, Vector2Int tilePos)
    {
        if (placedTiles.ContainsKey(tilePos))
        {
            // Tile already exists here, don’t place a duplicate prefab
            return;
        }

        Vector3 worldPos = new Vector3(tilePos.x * tileSpacing, tilePos.y * tileSpacing, 0);
        GameObject newTile = Instantiate(tile.tilePrefab, worldPos, Quaternion.identity, tileParent);

        placedTiles[tilePos] = tile; // record it
    }

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
