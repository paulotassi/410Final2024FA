using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

// Title screen shop: spend banked ingredients on permanent upgrades.
// Costs, tiers and unlock rules are set in Assets/Resources/UpgradeConfig.asset.
public class UpgradeShop : MonoBehaviour
{
    [Serializable]
    public class Row
    {
        [Tooltip("Index into the Upgrade Config's upgrades list. -1 = the witch skin")]
        public int upgradeIndex;
        public TMP_Text nameText;
        public TMP_Text costText;
        public Button buyButton;
    }

    [Header("Panel")]
    public GameObject panel;
    public TMP_Text bankText;
    public Button firstSelected;

    [Header("Rows (costs live in Resources/UpgradeConfig)")]
    public Row[] rows;

    private void Awake()
    {
        for (int i = 0; i < rows.Length; i++)
        {
            int index = i;
            if (rows[i].buyButton != null)
                rows[i].buyButton.onClick.AddListener(() => Click(index));
        }
        if (panel != null) panel.SetActive(false);
    }

    public void Open()
    {
        panel.SetActive(true);
        Refresh();
        if (EventSystem.current != null && firstSelected != null)
            EventSystem.current.SetSelectedGameObject(firstSelected.gameObject);
    }

    public void Close()
    {
        panel.SetActive(false);
    }

    private void Click(int index)
    {
        UpgradeConfig config = UpgradeConfig.Instance;
        Row row = rows[index];

        if (row.upgradeIndex < 0)
        {
            if (SaveManager.Data.skinOwned) SaveManager.SetSkinEquipped(!SaveManager.Data.skinEquipped);
            else SaveManager.TryBuySkin();
        }
        else
        {
            SaveManager.TryPurchase(config.upgrades[row.upgradeIndex]);
        }
        Refresh();
    }

    public void Refresh()
    {
        UpgradeConfig config = UpgradeConfig.Instance;
        if (config == null)
        {
            if (bankText != null) bankText.text = "Missing Resources/UpgradeConfig";
            return;
        }

        if (bankText != null)
        {
            string text = "";
            foreach (IngredientType t in Enum.GetValues(typeof(IngredientType)))
                text += t + ": " + SaveManager.Banked(t) + "    ";
            bankText.text = text;
        }

        foreach (Row row in rows)
        {
            if (row.upgradeIndex < 0) RefreshSkin(config, row);
            else RefreshUpgrade(config.upgrades[row.upgradeIndex], row);
        }
    }

    private void RefreshUpgrade(UpgradeConfig.Entry entry, Row row)
    {
        bool unlocked = SaveManager.IsUnlocked(entry);
        bool maxed = SaveManager.IsMaxed(entry);
        int tier = SaveManager.Tier(entry.buff);

        SetTexts(row,
            entry.displayName + "  " + tier + "/" + entry.tierCosts.Length,
            entry.description,
            !unlocked ? "Locked: own " + entry.requiresUpgradesOwned + " upgrades (" + SaveManager.UnlockProgress() + "/" + entry.requiresUpgradesOwned + ")"
                : maxed ? "Max tier"
                : (entry.firstTierUnlocksAbility && tier == 0 ? "Unlock: " : "Tier " + (tier + 1) + ": ")
                    + SaveManager.NextCost(entry) + " " + entry.ingredient + " (have " + SaveManager.Banked(entry.ingredient) + ")");

        SetButton(row, unlocked && !maxed && SaveManager.CanAfford(entry), !unlocked ? "Locked" : maxed ? "Max" : (entry.firstTierUnlocksAbility && tier == 0 ? "Unlock" : "Buy"));
    }

    private void RefreshSkin(UpgradeConfig config, Row row)
    {
        SaveData data = SaveManager.Data;
        bool ready = SaveManager.AllUpgradesMaxed();

        SetTexts(row,
            config.skinName,
            "Final upgrade: a new look for your witch",
            data.skinOwned ? (data.skinEquipped ? "Equipped" : "Owned")
                : !ready ? "Locked: max every upgrade"
                : config.skinCost + " " + config.skinIngredient + " (have " + SaveManager.Banked(config.skinIngredient) + ")");

        if (data.skinOwned) SetButton(row, true, data.skinEquipped ? "Unequip" : "Equip");
        else SetButton(row, ready && SaveManager.Banked(config.skinIngredient) >= config.skinCost, ready ? "Buy" : "Locked");
    }

    private static void SetTexts(Row row, string title, string description, string cost)
    {
        if (row.nameText != null)
            row.nameText.text = title + (string.IsNullOrEmpty(description) ? "" : "\n<size=70%>" + description + "</size>");
        if (row.costText != null)
            row.costText.text = cost;
    }

    private static void SetButton(Row row, bool interactable, string label)
    {
        if (row.buyButton == null) return;
        row.buyButton.interactable = interactable;
        TMP_Text text = row.buyButton.GetComponentInChildren<TMP_Text>(true);
        if (text != null) text.text = label;
    }
}
