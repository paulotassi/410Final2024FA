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
    [Tooltip("Scales every tile (walls, backdrop, hallways) and the spacing between them. Above 1 = roomier maps. Enemies keep their normal size")]
    [Range(0.75f, 2f)]
    public float tileScale = 1.25f;
    float Spacing => tileSpacing * tileScale;
    [Tooltip("DO NOT TOUCH - Parent object that keeps the tile hierarchy clean")]
    public Transform tileParent;

    // Calculated from mapSize at runtime, never exposed to the Inspector
    private int minTiles;
    private int maxTiles;

    private Dictionary<Vector2Int, TileData> placedTiles = new Dictionary<Vector2Int, TileData>();
    private Queue<OpenExit> openExits = new Queue<OpenExit>();

    [Tooltip("Optional - prefab for the exit marker (needs a trigger collider + EndZone). If empty, a simple green marker is built at runtime")]
    public GameObject exitPrefab;

    // Path-finding data, filled in after generation
    private Vector2Int exitCell;
    private Dictionary<Vector2Int, int> distanceToExit = new Dictionary<Vector2Int, int>();
    private Dictionary<Vector2Int, GameObject> placedObjects = new Dictionary<Vector2Int, GameObject>();
    // Tile prefabs are painted off-origin, so the visual center of a tile isn't at its grid position.
    // Measured from the first placed tile.
    private Vector3 tileCenterOffset = Vector3.zero;
    public bool Generated { get; private set; }
    public Vector3 ExitWorldPosition { get; private set; }

    void Start()
    {
        // The title screen picks Small/Large by setting the size before loading this scene
        if (GameSettings.proceduralMode && GameSettings.proceduralMapSize > 0)
            mapSize = GameSettings.proceduralMapSize;

        // Translate the designer-facing mapSize into internal tile counts
        minTiles = mapSize * 20;
        maxTiles = mapSize * 30;

        GenerateMap();
        SetUpExit();
        PlacePlayersAtStart();
        Generated = true;
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

        // If we stopped at maxTiles, close off any doors that still lead nowhere
        while (openExits.Count > 0)
        {
            OpenExit open = openExits.Dequeue();
            if (placedTiles.ContainsKey(open.position)) continue;

            TileData capTile = GetSingleExitTile(open.neededExit);
            if (capTile != null) PlaceTile(capTile, open.position);
        }
    }

    // Places a tile prefab at the given grid position
    void PlaceTile(TileData tileData, Vector2Int gridPosition)
    {
        Vector3 worldPos = new Vector3(gridPosition.x * Spacing, gridPosition.y * Spacing, 0);
        GameObject instance = Instantiate(tileData.tilePrefab, worldPos, Quaternion.identity, tileParent);
        instance.transform.localScale *= tileScale;
        // enemies are children of the tile: undo the scaling on them so only the space grows
        foreach (EnemyHealth enemy in instance.GetComponentsInChildren<EnemyHealth>(true))
            if (enemy.transform != instance.transform)
                enemy.transform.localScale /= tileScale;
        placedTiles[gridPosition] = tileData;
        placedObjects[gridPosition] = instance;

        if (placedTiles.Count == 1)
        {
            // Measure where the tile's walls actually sit relative to its origin
            Bounds bounds = new Bounds();
            bool hasBounds = false;
            foreach (UnityEngine.Tilemaps.TilemapRenderer tr in instance.GetComponentsInChildren<UnityEngine.Tilemaps.TilemapRenderer>())
            {
                // measure the wall layer only (the background layer behind it is not part of the tile's footprint)
                if (tr.GetComponent<Collider2D>() == null) continue;
                if (!hasBounds) { bounds = tr.bounds; hasBounds = true; }
                else bounds.Encapsulate(tr.bounds);
            }
            if (hasBounds) tileCenterOffset = new Vector3(bounds.center.x - worldPos.x, bounds.center.y - worldPos.y, 0f);
        }
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
            if (tile.isExit || !tile.exits.Contains(neededExit)) continue;

            // Before minTiles is reached, skip any tile with only one exit
            if (!allowCapTiles && tile.exits.Count == 1) continue;

            matches.Add(tile);
        }

        return PickWeighted(matches);
    }

    // Picks one tile from the list, favoring higher weights
    TileData PickWeighted(List<TileData> options)
    {
        if (options.Count == 0) return null;

        int total = 0;
        foreach (TileData t in options) total += Mathf.Max(1, t.weight);

        int roll = Random.Range(0, total);
        foreach (TileData t in options)
        {
            roll -= Mathf.Max(1, t.weight);
            if (roll < 0) return t;
        }
        return options[options.Count - 1];
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

    //======================================================
    // Exit, spawn and path guidance
    //======================================================

    // Picks a random dead-end tile (other than the start) as the exit and computes tile distances back to it
    void SetUpExit()
    {
        List<Vector2Int> deadEnds = new List<Vector2Int>();
        foreach (KeyValuePair<Vector2Int, TileData> kv in placedTiles)
        {
            if (kv.Key != Vector2Int.zero && kv.Value.exits.Count == 1)
                deadEnds.Add(kv.Key);
        }

        if (deadEnds.Count > 0)
        {
            exitCell = deadEnds[Random.Range(0, deadEnds.Count)];
        }
        else
        {
            // No dead ends were generated - fall back to any tile other than the start
            List<Vector2Int> others = new List<Vector2Int>(placedTiles.Keys);
            others.Remove(Vector2Int.zero);
            exitCell = others.Count > 0 ? others[Random.Range(0, others.Count)] : Vector2Int.zero;
        }

        ExitWorldPosition = CellToWorld(exitCell);
        BuildDistanceMap();

        // Prefer a dedicated exit tile (has its own end zone); otherwise drop in a plain marker
        if (!SwapInExitTile())
            CreateExitMarker();
    }

    // Replaces the tile at exitCell with an exit tile that has the same single opening
    bool SwapInExitTile()
    {
        TileData current = placedTiles[exitCell];
        if (current.exits.Count != 1) return false;

        List<TileData> options = new List<TileData>();
        foreach (TileData tile in tileDatas)
        {
            if (tile.isExit && tile.exits.Count == 1 && tile.exits[0] == current.exits[0])
                options.Add(tile);
        }

        TileData exitTile = PickWeighted(options);
        if (exitTile == null) return false;

        Destroy(placedObjects[exitCell]);
        PlaceTile(exitTile, exitCell);
        return true;
    }

    // Breadth-first search outward from the exit over connected tiles (fewest tiles = shortest path)
    void BuildDistanceMap()
    {
        distanceToExit.Clear();
        Queue<Vector2Int> frontier = new Queue<Vector2Int>();
        distanceToExit[exitCell] = 0;
        frontier.Enqueue(exitCell);

        while (frontier.Count > 0)
        {
            Vector2Int cell = frontier.Dequeue();
            foreach (Direction dir in placedTiles[cell].exits)
            {
                Vector2Int next = GetOffsetPosition(cell, dir);
                if (distanceToExit.ContainsKey(next) || !IsConnected(next, GetOppositeDirection(dir))) continue;

                distanceToExit[next] = distanceToExit[cell] + 1;
                frontier.Enqueue(next);
            }
        }
    }

    // True if a tile exists at pos and has an exit facing the given direction
    bool IsConnected(Vector2Int pos, Direction facing)
    {
        return placedTiles.TryGetValue(pos, out TileData tile) && tile.exits.Contains(facing);
    }

    void CreateExitMarker()
    {
        GameObject marker;
        if (exitPrefab != null)
        {
            marker = Instantiate(exitPrefab, ExitWorldPosition, Quaternion.identity, tileParent);
        }
        else
        {
            marker = new GameObject("ExitMarker");
            marker.transform.SetParent(tileParent);
            marker.transform.position = ExitWorldPosition;
            marker.transform.localScale = new Vector3(3f, 3f, 1f);

            Texture2D white = Texture2D.whiteTexture;
            SpriteRenderer sr = marker.AddComponent<SpriteRenderer>();
            sr.sprite = Sprite.Create(white, new Rect(0, 0, white.width, white.height), new Vector2(0.5f, 0.5f), white.width);
            sr.color = new Color(0.2f, 1f, 0.3f, 0.6f);
            sr.sortingOrder = 5;

            BoxCollider2D col = marker.AddComponent<BoxCollider2D>();
            col.isTrigger = true;
            marker.AddComponent<EndZone>();
        }
    }

    // Moves the players to the starting tile
    void PlacePlayersAtStart()
    {
        Vector3 start = CellToWorld(Vector2Int.zero);
        foreach (GameObject p in GameObject.FindGameObjectsWithTag("Player"))
            p.transform.position = start + new Vector3(-1f, 0f, 0f);
        foreach (GameObject p in GameObject.FindGameObjectsWithTag("Player2"))
            p.transform.position = start + new Vector3(1f, 0f, 0f);
    }

    Vector3 CellToWorld(Vector2Int cell)
    {
        return new Vector3(cell.x * Spacing, cell.y * Spacing, 0f) + tileCenterOffset;
    }

    Vector2Int WorldToCell(Vector3 world)
    {
        world -= tileCenterOffset;
        return new Vector2Int(Mathf.RoundToInt(world.x / Spacing), Mathf.RoundToInt(world.y / Spacing));
    }

    // Returns the point (on the edge of the tile the position is in) where the player should head next
    // to take the fewest-tiles route to the exit. Returns false if the position isn't inside a known tile.
    public Vector2Int CellOf(Vector3 worldPos)
    {
        return WorldToCell(worldPos);
    }

    // True if worldPos is inside the given tile, at least margin units away from its edges
    public bool IsInsideCell(Vector3 worldPos, Vector2Int cell, float margin)
    {
        Vector3 d = worldPos - CellToWorld(cell);
        float half = Spacing * 0.5f - margin;
        return Mathf.Abs(d.x) <= half && Mathf.Abs(d.y) <= half;
    }

    public bool TryGetGuidePoint(Vector2Int cell, out Vector3 point)
    {
        point = Vector3.zero;
        if (!Generated) return false;

        if (!distanceToExit.TryGetValue(cell, out int here)) return false;

        // Inside the exit tile: point straight at the exit
        if (cell == exitCell)
        {
            point = ExitWorldPosition;
            return true;
        }

        bool found = false;
        int best = here;
        Direction bestDir = Direction.North;
        foreach (Direction dir in placedTiles[cell].exits)
        {
            Vector2Int next = GetOffsetPosition(cell, dir);
            if (distanceToExit.TryGetValue(next, out int d) && d < best && IsConnected(next, GetOppositeDirection(dir)))
            {
                best = d;
                bestDir = dir;
                found = true;
            }
        }

        if (!found) return false;

        Vector2Int step = GetOffsetPosition(Vector2Int.zero, bestDir);
        point = CellToWorld(cell) + new Vector3(step.x, step.y, 0f) * (Spacing * 0.5f);
        return true;
    }

    // Returns a dead-end tile with only one exit in the specified direction
    TileData GetSingleExitTile(Direction dir)
    {
        List<TileData> caps = new List<TileData>();
        foreach (TileData tile in tileDatas)
        {
            if (!tile.isExit && tile.exits.Count == 1 && tile.exits[0] == dir)
                caps.Add(tile);
        }
        return PickWeighted(caps);
    }
}