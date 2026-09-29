using System;
using UnityEngine;

// All shop tuning lives here: costs per tier, when the shield/stun unlock, and the skin.
// Edit the asset at Assets/Resources/UpgradeConfig.asset.
[CreateAssetMenu(menuName = "Upgrade Config", fileName = "UpgradeConfig")]
public class UpgradeConfig : ScriptableObject
{
    [Serializable]
    public class Entry
    {
        public string displayName = "Upgrade";
        [TextArea] public string description;
        [Tooltip("Ingredient type that pays for this upgrade")]
        public IngredientType ingredient;
        [Tooltip("The buff the player gets. Each tier applies the effect once more")]
        public BuffType buff;
        [Tooltip("Cost of each tier, in order. Two entries = a two-tier upgrade")]
        public int[] tierCosts = { 5, 10 };
        [Tooltip("Tiers of this upgrade count toward the Requires Upgrades Owned unlocks of other entries")]
        public bool countsTowardUnlocks;
        [Tooltip("Total tiers of the counts-toward-unlocks upgrades needed before this one can be bought. 0 = buyable from the start")]
        public int requiresUpgradesOwned;
        [Tooltip("For abilities that start locked (shield, stun): tier 1 unlocks the ability itself and only tiers after that apply the buff")]
        public bool firstTierUnlocksAbility;
    }

    [Header("Upgrades (Shield and Stun unlock automatically, then these tiers make them stronger)")]
    public Entry[] upgrades;

    [Header("Witch skin (final upgrade - needs every upgrade at max tier)")]
    public string skinName = "Witch Skin";
    public IngredientType skinIngredient = IngredientType.Eyeball;
    [Min(1)] public int skinCost = 10;
    [Tooltip("Animator used by the witch when the skin is equipped")]
    public RuntimeAnimatorController skinAnimator;
    [Tooltip("Optional static sprite for the skin (used if there is no animator)")]
    public Sprite skinSprite;

    private static UpgradeConfig instance;

    public static UpgradeConfig Instance
    {
        get
        {
            if (instance == null) instance = Resources.Load<UpgradeConfig>("UpgradeConfig");
            return instance;
        }
    }

    public Entry Find(BuffType buff)
    {
        foreach (Entry e in upgrades)
            if (e.buff == buff) return e;
        return null;
    }
}
