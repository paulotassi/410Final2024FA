using System;
using System.IO;
using UnityEngine;

// Persistent progress: ingredients banked from procedural runs and the boss unlock.
[Serializable]
public class SaveData
{
    public int[] bankedIngredients = new int[Enum.GetValues(typeof(IngredientType)).Length];
    public int totalBanked;   // lifetime total, never reduced by spending (drives the boss unlock)
    public bool bossUnlocked;
    public bool[] ownedUpgrades = new bool[Enum.GetValues(typeof(BuffType)).Length];
}

public static class SaveManager
{
    // Total banked ingredients needed before the boss level unlocks
    public const int BossUnlockTotal = 50;
    // Share of a run's ingredients kept when the player dies or runs out of time
    public const float FailKeepFraction = 0.5f;

    private static SaveData data;
    private static string SavePath => Path.Combine(Application.persistentDataPath, "save.json");

    public static SaveData Data
    {
        get
        {
            if (data == null) Load();
            return data;
        }
    }

    public static bool BossUnlocked => Data.bossUnlocked;

    public static void Load()
    {
        data = null;
        try
        {
            if (File.Exists(SavePath))
                data = JsonUtility.FromJson<SaveData>(File.ReadAllText(SavePath));
        }
        catch (Exception e)
        {
            Debug.LogWarning("Could not read save file: " + e.Message);
        }

        if (data == null) data = new SaveData();
        int typeCount = Enum.GetValues(typeof(IngredientType)).Length;
        if (data.bankedIngredients == null || data.bankedIngredients.Length != typeCount)
        {
            Array.Resize(ref data.bankedIngredients, typeCount);
        }

        int buffCount = Enum.GetValues(typeof(BuffType)).Length;
        if (data.ownedUpgrades == null || data.ownedUpgrades.Length != buffCount)
        {
            Array.Resize(ref data.ownedUpgrades, buffCount);
        }
    }

    public static void Save()
    {
        try
        {
            File.WriteAllText(SavePath, JsonUtility.ToJson(Data, true));
        }
        catch (Exception e)
        {
            Debug.LogWarning("Could not write save file: " + e.Message);
        }
    }

    // Adds a run's ingredients to the bank. fraction is 1 for an escape, FailKeepFraction for a failed run.
    // Returns the number of ingredients actually banked.
    public static int BankRun(int[] runCounts, float fraction)
    {
        int banked = 0;
        for (int i = 0; i < runCounts.Length && i < Data.bankedIngredients.Length; i++)
        {
            int amount = Mathf.FloorToInt(runCounts[i] * fraction);
            Data.bankedIngredients[i] += amount;
            banked += amount;
        }

        Data.totalBanked += banked;
        if (Data.totalBanked >= BossUnlockTotal) Data.bossUnlocked = true;
        Save();
        return banked;
    }

    public static int Banked(IngredientType type)
    {
        return Data.bankedIngredients[(int)type];
    }

    public static bool HasUpgrade(BuffType buff)
    {
        return Data.ownedUpgrades[(int)buff];
    }

    // Spends `cost` of the matching ingredient on a one-time upgrade. Returns false if it can't be afforded.
    public static bool TryPurchase(BuffType buff, IngredientType ingredient, int cost)
    {
        if (HasUpgrade(buff) || Banked(ingredient) < cost) return false;

        Data.bankedIngredients[(int)ingredient] -= cost;
        Data.ownedUpgrades[(int)buff] = true;
        Save();
        return true;
    }

    public static void ResetProgress()
    {
        data = new SaveData();
        Save();
    }
}
