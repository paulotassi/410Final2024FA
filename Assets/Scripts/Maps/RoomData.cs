// RoomData.cs
// ============
// A ScriptableObject that describes one type of room in the dungeon.
//
// HOW TO CREATE ONE (for designers/new programmers):
//   Right-click in the Project window → Create → Map → RoomData
//   Fill in the prefab and choose which sides have doorways.
//   You'll want to make several of these: CombatRoom_Small, TreasureRoom_Large, BossArena, etc.
//
// KEY CONCEPT — ScriptableObjects:
//   A ScriptableObject is a data container that lives as an asset in your Project folder.
//   Unlike a MonoBehaviour (which must be on a GameObject in a scene), a ScriptableObject
//   just holds data. This means you can reference the same RoomData in multiple scenes
//   without duplicating anything. Think of it as a shared spreadsheet row for one room type.

using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Map/RoomData")]
public class RoomData : ScriptableObject
{
    [Header("Identity")]
    [Tooltip("Friendly name shown in the Inspector — for your own reference only.")]
    public string roomName;

    // This is the actual GameObject that gets spawned into the scene when this room is placed.
    // Design this prefab however you want: L-shaped, large open area, narrow corridor bend, etc.
    // The PCG generator does NOT care about the visual shape — it only reads the doorways list.
    // That means you can make any crazy shape you want in the prefab and the generator
    // will still connect it correctly as long as the doorways match up.
    [Tooltip("The prefab to instantiate. Can be any shape — L, T, large square, irregular, etc.")]
    public GameObject roomPrefab;

    [Header("Roguelike Room Category")]
    // This tag is not used by the generator itself — it's here for FUTURE SYSTEMS.
    // When you eventually add an enemy spawner, loot tables, or event triggers,
    // they can read this field to know what kind of experience to create in this room.
    // Example: an enemy spawner checks RoomType == Combat before placing enemies.
    [Tooltip("What kind of room this is. Used by gameplay systems — not by the generator.")]
    public RoomType roomType = RoomType.Combat;

    [Header("Connection Points (Doorways)")]
    // This list tells the generator which sides of this room have door openings.
    // The generator matches these up so connected rooms always face each other correctly.
    //
    // EXAMPLE — a straight vertical hallway room would have: [North, South]
    // EXAMPLE — a dead-end treasure room would have: [South]  (one entrance, no exits)
    // EXAMPLE — a crossroads room would have: [North, East, South, West]
    //
    // IMPORTANT: Make sure your prefab's visual doorways actually match this list.
    // If the data says North but the wall has no opening, players will see a wall.
    [Tooltip("Which sides of this room have openings. Must match the prefab's visual doorways.")]
    public List<Direction> doorways;
}

// ─────────────────────────────────────────────────────────────────────────────
// RoomType Enum
// ─────────────────────────────────────────────────────────────────────────────
// This categorizes every room so gameplay systems know what to do inside it.
// Add new types here as the roguelike features grow (e.g., Puzzle, Shrine, NPC).

public enum RoomType
{
    Start,      // Where the player spawns at the beginning of a run floor
    Combat,     // Normal room — enemies spawn here
    Treasure,   // Reward room — ingredients, items, or powerups
    Shop,       // Spend meta-currency on witch upgrades (future feature)
    Boss,       // End-of-floor boss encounter

    Empty,
    Exit        // Portal/door that leads to the next floor
}
