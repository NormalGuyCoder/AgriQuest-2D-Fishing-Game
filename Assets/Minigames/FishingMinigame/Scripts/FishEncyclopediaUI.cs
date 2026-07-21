using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class FishEncyclopediaUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject encyclopediaPanel;
    [SerializeField] private Transform fishListContent;
    [SerializeField] private GameObject fishEntryPrefab;
    [SerializeField] private Button closeButton;
    [SerializeField] private Button openButton;
    [SerializeField] private TMP_Dropdown categoryDropdown;
    [SerializeField] private GameObject fishDetailsPanel;
    
    [Header("Fish Details UI")]
    [SerializeField] private Image selectedFishImage;
    [SerializeField] private TextMeshProUGUI selectedFishName;
    [SerializeField] private TextMeshProUGUI selectedFishScientificName;
    [SerializeField] private TextMeshProUGUI selectedFishDescription;
    [SerializeField] private TextMeshProUGUI selectedFishValue;
    [SerializeField] private TextMeshProUGUI selectedFishHabitat;
    [SerializeField] private TextMeshProUGUI selectedFishSeasons;

    private List<GameObject> fishEntries = new List<GameObject>();
    private string currentCategory = "All";

    private void Start()
    {
        if (closeButton != null)
        {
            closeButton.onClick.AddListener(HideEncyclopedia);
        }
        
        if (openButton != null)
        {
            openButton.onClick.AddListener(ShowEncyclopedia);
        }

        if (categoryDropdown != null)
        {
            categoryDropdown.onValueChanged.AddListener(OnCategoryChanged);
            SetupCategoryDropdown();
        }

        if (FishEncyclopedia.Instance != null)
        {
            FishEncyclopedia.Instance.onFishDiscovered.AddListener(OnFishDiscovered);
        }

        HideEncyclopedia();
        RefreshFishList();
    }

    private void SetupCategoryDropdown()
    {
        categoryDropdown.ClearOptions();
        List<string> categories = new List<string> { "All", "Common", "Rare", "Legendary" };
        categoryDropdown.AddOptions(categories);
    }

    private void OnCategoryChanged(int index)
    {
        currentCategory = categoryDropdown.options[index].text;
        RefreshFishList();
    }

    private void ShowEncyclopedia()
    {
        encyclopediaPanel.SetActive(true);
        RefreshFishList();
    }

    private void HideEncyclopedia()
    {
        encyclopediaPanel.SetActive(false);
        if (fishDetailsPanel != null)
        {
            fishDetailsPanel.SetActive(false);
        }
    }

    private void RefreshFishList()
    {
        // Clear existing entries
        foreach (var entry in fishEntries)
        {
            Destroy(entry);
        }
        fishEntries.Clear();

        if (FishEncyclopedia.Instance == null) return;

        // Get filtered list of fish based on category
        List<FishData> fishToShow = GetFilteredFishList();

        // Create entries for all fish
        foreach (var fish in fishToShow)
        {
            GameObject entry = Instantiate(fishEntryPrefab, fishListContent);
            
            // Set fish entry UI elements
            Image fishImage = entry.transform.Find("FishImage")?.GetComponent<Image>();
            TextMeshProUGUI nameText = entry.transform.Find("FishName")?.GetComponent<TextMeshProUGUI>();
            TextMeshProUGUI scientificNameText = entry.transform.Find("ScientificName")?.GetComponent<TextMeshProUGUI>();
            Button entryButton = entry.GetComponent<Button>();
            
            if (fishImage != null)
            {
                fishImage.sprite = fish.fishIcon ?? fish.fishSprite;
                fishImage.color = fish.isDiscovered ? Color.white : Color.black;
            }
            
            if (nameText != null)
            {
                nameText.text = fish.isDiscovered ? fish.fishName : "???";
            }

            if (scientificNameText != null)
            {
                scientificNameText.text = fish.isDiscovered ? fish.scientificName : "???";
            }
            
            if (entryButton != null)
            {
                entryButton.onClick.AddListener(() => ShowFishDetails(fish));
            }

            fishEntries.Add(entry);
        }
    }

    private List<FishData> GetFilteredFishList()
    {
        List<FishData> allFish = FishEncyclopedia.Instance.GetAllFish();
        
        if (currentCategory == "All")
        {
            return allFish;
        }

        return allFish.FindAll(fish =>
        {
            if (!fish.isDiscovered) return false;
            
            switch (currentCategory)
            {
                case "Common":
                    return fish.rarity < 0.3f;
                case "Rare":
                    return fish.rarity >= 0.3f && fish.rarity < 0.7f;
                case "Legendary":
                    return fish.rarity >= 0.7f;
                default:
                    return true;
            }
        });
    }

    private void ShowFishDetails(FishData fish)
    {
        if (fish == null || !fish.isDiscovered) return;

        fishDetailsPanel.SetActive(true);
        
        if (selectedFishImage != null) selectedFishImage.sprite = fish.fishSprite;
        if (selectedFishName != null) selectedFishName.text = fish.fishName;
        if (selectedFishScientificName != null) selectedFishScientificName.text = fish.scientificName;
        if (selectedFishDescription != null) selectedFishDescription.text = fish.description;
        if (selectedFishValue != null) selectedFishValue.text = $"Value: {fish.baseValue} coins";
        
        if (selectedFishHabitat != null)
        {
            string habitatText = "Habitat: " + string.Join(", ", fish.habitats);
            selectedFishHabitat.text = habitatText;
        }
        
        if (selectedFishSeasons != null)
        {
            string seasonsText = "Available in: " + string.Join(", ", fish.seasons);
            selectedFishSeasons.text = seasonsText;
        }
    }

    private void OnFishDiscovered(FishData fish)
    {
        RefreshFishList();
    }

    private void OnDestroy()
    {
        if (FishEncyclopedia.Instance != null)
        {
            FishEncyclopedia.Instance.onFishDiscovered.RemoveListener(OnFishDiscovered);
        }
    }
} 