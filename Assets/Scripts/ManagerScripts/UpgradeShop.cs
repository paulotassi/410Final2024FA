using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

// Title screen shop: spend banked ingredients on permanent upgrades.
// Each upgrade is paid for with its matching ingredient type.
public class UpgradeShop : MonoBehaviour
{
    [Serializable]
    public class UpgradeOption
    {
        public string displayName = "Upgrade";
        [TextArea] public string description;
        [Tooltip("Ingredient type that pays for this upgrade")]
        public IngredientType ingredient;
        [Tooltip("The buff the player gets")]
        public BuffType buff;
        [Tooltip("How many of the ingredient it costs")]
        [Min(1)] public int cost = 5;

        [Header("UI (assigned in the scene)")]
        public TMP_Text nameText;
        public TMP_Text costText;
        public Button buyButton;
    }

    [Header("Panel")]
    public GameObject panel;
    public TMP_Text bankText;
    public Button firstSelected;

    [Header("Upgrades - change the costs here")]
    public UpgradeOption[] upgrades;

    private void Awake()
    {
        for (int i = 0; i < upgrades.Length; i++)
        {
            int index = i;
            if (upgrades[i].buyButton != null)
                upgrades[i].buyButton.onClick.AddListener(() => Buy(index));
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

    private void Buy(int index)
    {
        UpgradeOption o = upgrades[index];
        SaveManager.TryPurchase(o.buff, o.ingredient, o.cost);
        Refresh();
    }

    public void Refresh()
    {
        if (bankText != null)
        {
            string text = "";
            foreach (IngredientType t in Enum.GetValues(typeof(IngredientType)))
                text += t + ": " + SaveManager.Banked(t) + "    ";
            bankText.text = text;
        }

        foreach (UpgradeOption o in upgrades)
        {
            bool owned = SaveManager.HasUpgrade(o.buff);
            int have = SaveManager.Banked(o.ingredient);

            if (o.nameText != null)
                o.nameText.text = o.displayName + (string.IsNullOrEmpty(o.description) ? "" : "\n<size=70%>" + o.description + "</size>");
            if (o.costText != null)
                o.costText.text = owned ? "Owned" : o.cost + " " + o.ingredient + " (have " + have + ")";
            if (o.buyButton != null)
            {
                o.buyButton.interactable = !owned && have >= o.cost;
                TMP_Text label = o.buyButton.GetComponentInChildren<TMP_Text>(true);
                if (label != null) label.text = owned ? "Owned" : "Buy";
            }
        }
    }
}
