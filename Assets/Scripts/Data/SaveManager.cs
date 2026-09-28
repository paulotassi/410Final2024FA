using System;
using System.IO;
using UnityEngine;

// Persistent progress: ingredients banked from procedural runs, purchased upgrades and the boss unlock.
[Serializable]
public class SaveData
{
    public int[] bankedIngredients = new int[Enum.GetValues(typeof(IngredientType)).Length];
    public int totalBanked;   // lifetime total, never reduced by spending (drives the boss unlock)
    public bool bossUnlocked;
    public int[] upgradeTiers = new int[Enum.GetValues(typeof(BuffType)).Length];
    public bool skinOwned;
    public bool skinEquipped;
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
        if (data.upgradeTiers == null || data.upgradeTiers.Length != buffCount)
        {
            Array.Resize(ref data.upgradeTiers, buffCount);
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

    //======================================================
    // Upgrades
    //======================================================

    public static int Tier(BuffType buff)
    {
        return Data.upgradeTiers[(int)buff];
    }

    // Total tiers owned across the upgrades that count toward unlocking the shield and stun
    public static int UnlockProgress()
    {
        UpgradeConfig config = UpgradeConfig.Instance;
        if (config == null) return 0;

        int total = 0;
        foreach (UpgradeConfig.Entry e in config.upgrades)
            if (e.countsTowardUnlocks) total += Tier(e.buff);
        return total;
    }

    public static bool IsUnlocked(UpgradeConfig.Entry entry)
    {
        return UnlockProgress() >= entry.requiresUpgradesOwned;
    }

    public static bool IsMaxed(UpgradeConfig.Entry entry)
    {
        return Tier(entry.buff) >= entry.tierCosts.Length;
    }

    // Cost of the next tier, or -1 if already maxed
    public static int NextCost(UpgradeConfig.Entry entry)
    {
        int tier = Tier(entry.buff);
        return tier < entry.tierCosts.Length ? entry.tierCosts[tier] : -1;
    }

    public static bool CanAfford(UpgradeConfig.Entry entry)
    {
        int cost = NextCost(entry);
        return cost >= 0 && Banked(entry.ingredient) >= cost;
    }

    // Buys the next tier of an upgrade with its matching ingredient
    public static bool TryPurchase(UpgradeConfig.Entry entry)
    {
        if (!IsUnlocked(entry) || IsMaxed(entry) || !CanAfford(entry)) return false;

        Data.bankedIngredients[(int)entry.ingredient] -= NextCost(entry);
        Data.upgradeTiers[(int)entry.buff]++;
        Save();
        return true;
    }

    // The shield and stun abilities start locked; buying tier 1 of their entry unlocks them
    public static bool ShieldUnlocked => AbilityUnlocked(BuffType.ShieldExtension);
    public static bool StunUnlocked => AbilityUnlocked(BuffType.StunMultiplier);

    private static bool AbilityUnlocked(BuffType buff)
    {
        UpgradeConfig config = UpgradeConfig.Instance;
        UpgradeConfig.Entry entry = config != null ? config.Find(buff) : null;
        if (entry == null || !entry.firstTierUnlocksAbility) return true;
        return Tier(buff) >= 1;
    }

    // How many times the buff itself should be applied for the tiers owned
    public static int BuffApplications(UpgradeConfig.Entry entry)
    {
        int tiers = Tier(entry.buff);
        return entry.firstTierUnlocksAbility ? Mathf.Max(0, tiers - 1) : tiers;
    }

    //======================================================
    // Skin
    //======================================================

    public static bool AllUpgradesMaxed()
    {
        UpgradeConfig config = UpgradeConfig.Instance;
        if (config == null) return false;

        foreach (UpgradeConfig.Entry e in config.upgrades)
            if (!IsMaxed(e)) return false;
        return true;
    }

    public static bool TryBuySkin()
    {
        UpgradeConfig config = UpgradeConfig.Instance;
        if (config == null || Data.skinOwned || !AllUpgradesMaxed() || Banked(config.skinIngredient) < config.skinCost) return false;

        Data.bankedIngredients[(int)config.skinIngredient] -= config.skinCost;
        Data.skinOwned = true;
        Data.skinEquipped = true;
        Save();
        return true;
    }

    public static void SetSkinEquipped(bool equipped)
    {
        if (!Data.skinOwned) return;
        Data.skinEquipped = equipped;
        Save();
    }

    public static void ResetProgress()
    {
        data = new SaveData();
        Save();
    }
}
