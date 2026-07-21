using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Collections;

public class SellShopUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Transform fishListContainer;
    [SerializeField] private GameObject fishItemPrefab;
    [SerializeField] private TMP_Text coinsText;
    [SerializeField] private TMP_Text soldText;
    [SerializeField] private Button closeButton;

    private List<FishShopItem> currentFishItems = new List<FishShopItem>();

    private void Start()
    {
        if (closeButton != null)
        {
            closeButton.onClick.AddListener(CloseShop);
        }

        SetupContainerLayout();
        UpdateCoinsDisplay();
        UpdateSoldDisplay();
    }

    private void SetupContainerLayout()
    {
        if (fishListContainer != null)
        {
            var layoutGroup = fishListContainer.GetComponent<VerticalLayoutGroup>();
            if (layoutGroup == null)
            {
                layoutGroup = fishListContainer.gameObject.AddComponent<VerticalLayoutGroup>();
                layoutGroup.spacing = 10f;
                layoutGroup.childAlignment = TextAnchor.UpperLeft;
                layoutGroup.childControlHeight = true;
                layoutGroup.childControlWidth = true;
                layoutGroup.childForceExpandHeight = false;
                layoutGroup.childForceExpandWidth = true;
            }

            var sizeFitter = fishListContainer.GetComponent<ContentSizeFitter>();
            if (sizeFitter == null)
            {
                sizeFitter = fishListContainer.gameObject.AddComponent<ContentSizeFitter>();
                sizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            }
        }
    }

    public void RefreshFishList()
    {
        foreach (var item in currentFishItems)
        {
            if (item != null && item.gameObject != null)
            {
                Destroy(item.gameObject);
            }
        }
        currentFishItems.Clear();

        var inventory = DetailedFishInventory.Instance;
        if (inventory == null)
        {
            return;
        }

        var speciesCounts = inventory.SpeciesInventoryCounts;

        if (fishListContainer == null || fishItemPrefab == null)
        {
            return;
        }

        foreach (var species in speciesCounts)
        {
            string fishId = species.Key;
            int count = species.Value;

            if (count <= 0)
            {
                continue;
            }

            GameObject itemObj = Instantiate(fishItemPrefab, fishListContainer);

            if (itemObj == null)
            {
                continue;
            }

            itemObj.SetActive(true);

            FishShopItem fishItem = itemObj.GetComponent<FishShopItem>();

            if (fishItem != null)
            {
                string fishName = GetFishNameById(fishId);

                // Calculate total value from records using the public method
                int totalValue = CalculateTotalValueForSpecies(fishId);

                string description = $"Count: {count}\n" +
                                   $"Total Value: {totalValue} coins";

                fishItem.Setup(fishId, fishName, description, SellFish, SellAllFish);
                currentFishItems.Add(fishItem);
            }
        }

        UpdateCoinsDisplay();
        UpdateSoldDisplay();
        StartCoroutine(ForceLayoutRebuild());
    }

    private int CalculateTotalValueForSpecies(string fishId)
    {
        var inventory = DetailedFishInventory.Instance;
        if (inventory == null) return 0;

        // Use the public method GetRecordsByFishId instead of reflection
        var records = inventory.GetRecordsByFishId(fishId);
        if (records == null || records.Count == 0) return 0;

        int totalValue = 0;
        foreach (var record in records)
        {
            totalValue += Mathf.RoundToInt(record.actualValue);
        }

        return totalValue;
    }

    private IEnumerator ForceLayoutRebuild()
    {
        yield return null;

        if (fishListContainer != null)
        {
            var rectTransform = fishListContainer as RectTransform;
            if (rectTransform != null)
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(rectTransform);

                Transform parent = fishListContainer.parent;
                while (parent != null)
                {
                    var parentRect = parent as RectTransform;
                    if (parentRect != null)
                    {
                        LayoutRebuilder.ForceRebuildLayoutImmediate(parentRect);
                    }
                    parent = parent.parent;
                }
            }
        }
    }

    private void SellFish(string fishId)
    {
        var inventory = DetailedFishInventory.Instance;
        if (inventory == null) return;

        if (inventory.SellFish(fishId, 1, out float weightSold, out int coinsEarned))
        {
            RefreshFishList();
            RefreshDebtPanel();
        }
    }

    private void SellAllFish(string fishId)
    {
        var inventory = DetailedFishInventory.Instance;
        if (inventory == null) return;

        if (inventory.SpeciesInventoryCounts.ContainsKey(fishId))
        {
            int count = inventory.SpeciesInventoryCounts[fishId];
            if (count > 0)
            {
                if (inventory.SellFish(fishId, count, out float totalWeight, out int totalCoinsEarned))
                {
                    RefreshFishList();
                    RefreshDebtPanel();
                }
            }
        }
    }

    private void RefreshDebtPanel()
    {
        var debtPanel = FindObjectOfType<DebtStatusPanel>();
        if (debtPanel != null)
        {
            debtPanel.RefreshUI();
        }
    }

    private string GetFishNameById(string fishId)
    {
        switch (fishId.ToLower())
        {
            case "anchovy":
                return "Anchovy";
            case "bisugo":
                return "Bisugo (Threadfin Bream)";
            case "bluefin_tuna":
                return "Bluefin Tuna";
            case "cod":
                return "Cod";
            case "mackerel":
                return "Mackerel";
            case "sardine":
                return "Sardine";
            default:
                return FormatFishIdAsName(fishId);
        }
    }

    private string FormatFishIdAsName(string fishId)
    {
        string formatted = fishId.Replace("_", " ");
        formatted = System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(formatted.ToLower());
        return formatted;
    }

    private float GetBasePricePerKg(string fishId)
    {
        switch (fishId.ToLower())
        {
            case "bluefin_tuna":
                return 50f;
            case "cod":
                return 30f;
            case "mackerel":
                return 20f;
            case "bisugo":
                return 18f;
            case "anchovy":
                return 12f;
            case "sardine":
                return 10f;
            default:
                return 15f;
        }
    }

    public void UpdateCoinsDisplay()
    {
        if (coinsText != null && DetailedFishInventory.Instance != null)
        {
            coinsText.text = $"Coins: {DetailedFishInventory.Instance.Coins}";
        }
    }

    public void UpdateSoldDisplay()
    {
        if (soldText != null)
        {
            int soldCount = 0;
            if (QuestManager.Instance != null)
            {
                var saveData = typeof(QuestManager).GetField("saveData", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (saveData != null)
                {
                    var data = saveData.GetValue(QuestManager.Instance) as QuestSaveData;
                    if (data != null && data.inventory.ContainsKey("SoldFish"))
                    {
                        soldCount = data.inventory["SoldFish"];
                    }
                }
            }

            soldText.text = $"Fish Sold: {soldCount}";
        }
    }

    public void CloseShop()
    {
        gameObject.SetActive(false);

        var interactor = FindObjectOfType<SellShopInteractor>();
        if (interactor != null)
        {
            interactor.CloseShop();
        }
    }
}