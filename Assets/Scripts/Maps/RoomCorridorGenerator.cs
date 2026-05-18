// RoomCorridorGenerator.cs
// ========================
// Generates a dungeon layout using a Room-and-Corridor approach.
//
// ── WHAT THIS REPLACES ───────────────────────────────────────────────────────
// The original MapGenerator placed small, same-sized tile prefabs on a grid.
// Every "room" was one 22×22 unit tile — no variety in size or shape.
//
// This generator instead places two kinds of pieces:
//   • ROOMS  — large, hand-designed prefabs (any visual shape the designer wants).
//              L-shaped hallways, big open arenas, winding corridors — all valid.
//   • CORRIDORS — short connector tiles that bridge the gap between room doorways.
//
// ── HOW THE ALGORITHM WORKS ──────────────────────────────────────────────────
// This uses the same queue-based expansion as MapGenerator, just adapted for rooms.
//
//   1. Place the starting room at grid position (0, 0).
//   2. Every doorway on that room becomes an "open exit" — a pending connection.
//   3. Loop: dequeue one open exit, pick a random room that fits (has the matching
//      incoming doorway), spawn corridor tiles in the gap, place the room.
//   4. Add that new room's OTHER doorways to the queue.
//   5. Once minRooms is reached, stop branching and cap leftover exits with dead-ends.
//
// ── GRID SPACE VS. WORLD SPACE ───────────────────────────────────────────────
// Rooms live on a coarse integer grid (Vector2Int positions like (-1, 0), (0, 2), etc.)
// Their actual Unity world position = gridPosition × roomSpacing.
//
// Corridor tiles live in the gap between two room centers. Their world positions
// are calculated by evenly interpolating between the two room centers.
//
// ── WHAT THE DESIGNER CONTROLS ───────────────────────────────────────────────
//   mapSize      → scales total room count (1 = small floor, 10 = large floor)
//   roomSpacing  → distance between room centers in world units
//                  Rule of thumb: set this to (your room's visual size) + (corridor tiles × tileSpacing)
//                  Example: 44f room + 2 corridors at 22f each = 88f roomSpacing
//   tileSpacing  → distance between corridor tile centers (keep at 22f to match existing corridors)

using System.Collections.Generic;
using UnityEngine;

public class RoomCorridorGenerator : MonoBehaviour
{
    // ─── DESIGNER CONTROLS ───────────────────────────────────────────────────

    [Header("Designer Controls")]
    [Tooltip("Scales total room count. 1 = ~4–6 rooms, 10 = ~40–60 rooms.")]
    [Range(1, 10)]
    public int mapSize = 5;

    // ─── ROOM CONFIGURATION ──────────────────────────────────────────────────

    [Space]
    [Header("Room Configuration")]

    // All possible room types the generator can pick from.
    // Populate this in the Inspector with your RoomData ScriptableObjects.
    // Include every room variant: combat rooms, treasure rooms, dead-ends, etc.
    [Tooltip("All room templates the generator can place. Include dead-end rooms here too.")]
    public RoomData[] roomDatas;

    // The room placed at position (0, 0) — the player's spawn point for the floor.
    // This should be a room with RoomType = Start and doorways on as many sides as you want.
    [Tooltip("The starting room placed at the center. Usually a safe, enemy-free spawn room.")]
    public RoomData startingRoom;

    // ─── CORRIDOR CONFIGURATION ──────────────────────────────────────────────

    [Space]
    [Header("Corridor Configuration")]

    // Reuses the existing TileData system for corridor tiles.
    // Assign your North-South hallway prefab here (the one with openings top and bottom).
    [Tooltip("Corridor tile used for vertical connections (North↔South). Uses existing TileData.")]
    public TileData corridorNS;

    // Assign your East-West hallway prefab here (the one with openings left and right).
    [Tooltip("Corridor tile used for horizontal connections (East↔West). Uses existing TileData.")]
    public TileData corridorEW;

    // ─── SPACING REFERENCES — DO NOT TOUCH ───────────────────────────────────

    [Space]
    [Header("Spacing — DO NOT TOUCH")]

    // World-space distance between the centers of two adjacent rooms.
    // This must be large enough that room prefabs don't visually overlap.
    // Formula: roomSpacing = roomVisualSize + (corridorCount × tileSpacing)
    // With default tileSpacing=22 and roomSpacing=88: (88/22)-1 = 3 corridor tiles between rooms.
    [Tooltip("World-space distance between room centers. Must be a multiple of tileSpacing.")]
    public float roomSpacing = 88f;

    // Distance between corridor tile centers — matches the original tileSpacing in MapGenerator.
    // Keep this at 22f unless you're resizing all existing tile prefabs.
    [Tooltip("World-space distance between corridor tiles. Keep at 22 to match existing tiles.")]
    public float tileSpacing = 22f;

    [Tooltip("Parent transform that keeps all spawned objects organized in the hierarchy.")]
    public Transform tileParent;

    // ─── PRIVATE STATE ────────────────────────────────────────────────────────

    // Calculated from mapSize at runtime, never exposed to the Inspector.
    private int minRooms;
    private int maxRooms;

    // Maps each room grid position to the RoomData placed there.
    // Used to:  (a) avoid placing two rooms on the same slot
    //           (b) look up what's at a grid position during generation
    private Dictionary<Vector2Int, RoomData> placedRooms = new Dictionary<Vector2Int, RoomData>();

    // Queue of pending connections — doorways on placed rooms that haven't been filled yet.
    // Each OpenExit stores: WHERE to place the next room, and WHICH doorway direction it needs.
    // We reuse the OpenExit class from the original system — it hasn't changed.
    private Queue<OpenExit> openExits = new Queue<OpenExit>();

    // ─── ENTRY POINT ─────────────────────────────────────────────────────────

    void Start()
    {
        // Scale min/max room counts from the designer-facing mapSize value.
        // mapSize=1 → 4–6 rooms  |  mapSize=5 → 20–30 rooms  |  mapSize=10 → 40–60 rooms
        minRooms = mapSize * 4;
        maxRooms = mapSize * 6;

        GenerateMap();
    }

    // ─── CORE GENERATION LOOP ────────────────────────────────────────────────

    void GenerateMap()
    {
        // Step 1: Place the starting room at the origin of the room grid.
        PlaceRoom(startingRoom, Vector2Int.zero);

        // Step 2: Queue every doorway on the starting room as a pending connection.
        //         Pass null for entryDir because the starting room has no "entry" side.
        AddDoorways(Vector2Int.zero, startingRoom.doorways, null);

        // Step 3: Main expansion loop.
        //         Keep processing pending doorways until we hit maxRooms or run out of exits.
        while (openExits.Count > 0 && placedRooms.Count < maxRooms)
        {
            OpenExit open = openExits.Dequeue();

            // If another room already filled this slot (two paths converged), skip it.
            if (placedRooms.ContainsKey(open.position)) continue;

            if (placedRooms.Count < minRooms)
            {
                // ── BUILD PHASE ──
                // Still growing the dungeon. Pick any multi-doorway room that has the
                // required entry direction. Single-doorway (dead-end) rooms are excluded
                // here so branches keep expanding until we hit minRooms.

                RoomData selected = GetRandomMatchingRoom(open.neededExit, allowDeadEnds: false);
                if (selected == null) continue;

                // Spawn the corridor tiles bridging the gap from the previous room to here.
                SpawnCorridors(open.position, open.neededExit);

                PlaceRoom(selected, open.position);

                // Queue this room's remaining doorways for future expansion.
                AddDoorways(open.position, selected.doorways, open.neededExit);
            }
            else
            {
                // ── CAP PHASE ──
                // Hit minRooms. Stop branching — seal every remaining open doorway
                // with a dead-end room (a room that has exactly one doorway, facing inward).
                // This ensures every corridor ends at a closed room, not an open void.

                RoomData deadEnd = GetSingleDoorwayRoom(open.neededExit);
                if (deadEnd == null) continue;

                SpawnCorridors(open.position, open.neededExit);
                PlaceRoom(deadEnd, open.position);

                // Intentionally do NOT call AddDoorways — the branch ends here.
            }
        }
    }

    // ─── PLACEMENT HELPERS ───────────────────────────────────────────────────

    // Spawns a room prefab at the given room-grid position and records it.
    //
    // Room grid positions are integers like (0, 0), (1, 0), (-1, 2), etc.
    // To convert to Unity world space: multiply by roomSpacing.
    // Example: grid (2, -1) × roomSpacing 88 = world (176, -88, 0).
    void PlaceRoom(RoomData room, Vector2Int gridPos)
    {
        Vector3 worldPos = new Vector3(gridPos.x * roomSpacing, gridPos.y * roomSpacing, 0f);
        Instantiate(room.roomPrefab, worldPos, Quaternion.identity, tileParent);
        placedRooms.Add(gridPos, room);
    }

    // Spawns corridor tiles in the gap between two adjacent room centers.
    //
    // HOW THE MATH WORKS:
    //   Rooms are roomSpacing apart. Corridors are tileSpacing apart.
    //   corridorCount = (roomSpacing / tileSpacing) - 1
    //   Example: roomSpacing=88, tileSpacing=22 → (88/22)-1 = 3 corridors.
    //
    //   Each corridor is placed by linearly interpolating (lerp) between the two
    //   room centers. The lerp parameter t goes from 1/(count+1) to count/(count+1),
    //   which evenly distributes the tiles across the gap.
    //
    // PARAMETERS:
    //   targetRoomGridPos — the grid position of the room being connected TO
    //   incomingDir       — the doorway direction the new room needs (i.e., where the
    //                       source room is relative to the new room)
    void SpawnCorridors(Vector2Int targetRoomGridPos, Direction incomingDir)
    {
        // Reconstruct the source room's grid position by stepping one cell in incomingDir.
        // If the new room needs a South doorway, the source is to its South.
        Vector2Int sourceGridPos = targetRoomGridPos + GetDirectionStep(incomingDir);

        Vector3 sourceWorld = new Vector3(sourceGridPos.x * roomSpacing, sourceGridPos.y * roomSpacing, 0f);
        Vector3 targetWorld = new Vector3(targetRoomGridPos.x * roomSpacing, targetRoomGridPos.y * roomSpacing, 0f);

        // Choose which corridor prefab to use based on the axis of travel.
        bool isVertical = (incomingDir == Direction.North || incomingDir == Direction.South);
        TileData corridorTile = isVertical ? corridorNS : corridorEW;

        if (corridorTile == null)
        {
            Debug.LogWarning($"RoomCorridorGenerator: No corridor tile assigned for {(isVertical ? "NS" : "EW")} direction.");
            return;
        }

        // How many corridor tiles fit between the two room centers.
        int corridorCount = Mathf.RoundToInt(roomSpacing / tileSpacing) - 1;

        for (int i = 1; i <= corridorCount; i++)
        {
            // t=0 is sourceWorld, t=1 is targetWorld.
            // We start at t = 1/(count+1) and step by 1/(count+1) each iteration,
            // so tiles are evenly spaced and never land on a room center.
            float t = (float)i / (corridorCount + 1);
            Vector3 corridorWorldPos = Vector3.Lerp(sourceWorld, targetWorld, t);

            Instantiate(corridorTile.tilePrefab, corridorWorldPos, Quaternion.identity, tileParent);
        }
    }

    // ─── EXIT / DOORWAY MANAGEMENT ───────────────────────────────────────────

    // Adds a newly placed room's unconnected doorways to the openExits queue.
    //
    // We skip the doorway we came in from (entryDir) because that connection is
    // already made — adding it again would create an infinite back-and-forth loop.
    //
    // For each doorway we DO queue:
    //   - neighborPos: the room-grid cell that sits one step through this doorway
    //   - neededExit:  the direction the neighbor room must have to face back at us
    //     (always the OPPOSITE of the direction we're going through)
    void AddDoorways(Vector2Int pos, List<Direction> doorways, Direction? entryDir)
    {
        foreach (Direction door in doorways)
        {
            if (entryDir.HasValue && door == entryDir.Value) continue;

            Vector2Int neighborPos = pos + GetDirectionStep(door);
            openExits.Enqueue(new OpenExit(neighborPos, GetOppositeDirection(door)));
        }
    }

    // ─── ROOM SELECTION ──────────────────────────────────────────────────────

    // Returns a random room from roomDatas that has the required doorway direction.
    //
    // allowDeadEnds: when false (build phase), rooms with only one doorway are
    // excluded so branches keep growing. When true (cap phase), they're included.
    RoomData GetRandomMatchingRoom(Direction required, bool allowDeadEnds)
    {
        List<RoomData> matches = new List<RoomData>();

        foreach (RoomData room in roomDatas)
        {
            if (room == null || room.doorways == null) continue;
            if (!room.doorways.Contains(required)) continue;
            if (!allowDeadEnds && room.doorways.Count == 1) continue;

            matches.Add(room);
        }

        if (matches.Count == 0) return null;
        return matches[Random.Range(0, matches.Count)];
    }

    // Returns a dead-end room — one with exactly one doorway facing the required direction.
    // Used during the cap phase to close off open branches cleanly.
    RoomData GetSingleDoorwayRoom(Direction required)
    {
        foreach (RoomData room in roomDatas)
        {
            if (room == null || room.doorways == null) continue;
            if (room.doorways.Count == 1 && room.doorways[0] == required)
                return room;
        }

        // If no matching dead-end exists, log a warning so designers can fix their RoomData setup.
        Debug.LogWarning($"RoomCorridorGenerator: No single-doorway room found for direction {required}. Add one to roomDatas.");
        return null;
    }

    // ─── DIRECTION UTILITIES ─────────────────────────────────────────────────

    // Returns a unit Vector2Int step in the given direction.
    // Used to move one cell in room-grid space (not world space).
    Vector2Int GetDirectionStep(Direction dir)
    {
        switch (dir)
        {
            case Direction.North: return new Vector2Int( 0,  1);
            case Direction.South: return new Vector2Int( 0, -1);
            case Direction.East:  return new Vector2Int( 1,  0);
            case Direction.West:  return new Vector2Int(-1,  0);
            default:              return Vector2Int.zero;
        }
    }

    Direction GetOppositeDirection(Direction dir)
    {
        switch (dir)
        {
            case Direction.North: return Direction.South;
            case Direction.South: return Direction.North;
            case Direction.East:  return Direction.West;
            case Direction.West:  return Direction.East;
            default:              return dir;
        }
    }
}
