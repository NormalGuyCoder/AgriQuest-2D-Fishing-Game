using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Events;

public class FishEncyclopedia : MonoBehaviour
{
    public static FishEncyclopedia Instance { get; private set; }
    
    [Header("UI References")]
    [SerializeField] private GameObject encyclopediaPanel;
    [SerializeField] private Transform fishListContent;
    [SerializeField] private GameObject fishEntryPrefab;
    [SerializeField] private Image selectedFishImage;
    [SerializeField] private TextMeshProUGUI selectedFishName;
    [SerializeField] private TextMeshProUGUI selectedFishScientificName;
    [SerializeField] private TextMeshProUGUI selectedFishDescription;
    [SerializeField] private TextMeshProUGUI selectedFishStats;
    [SerializeField] private Button closeButton;
    [SerializeField] private Button[] categoryButtons;

    [Header("Fish Data")]
    [SerializeField] private List<FishData> allFish = new List<FishData>();
    [SerializeField] private string[] fishCategories = { "All", "Common", "Rare", "Legendary" };

    private List<FishData> discoveredFish = new List<FishData>();
    private FishData currentlySelectedFish;
    private string currentCategory = "All";

    public UnityEvent<FishData> onFishDiscovered = new UnityEvent<FishData>();
    public UnityEvent<FishData> onFishCaught = new UnityEvent<FishData>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        // Initialize UI
        if (encyclopediaPanel != null)
        {
            encyclopediaPanel.SetActive(false);
        }

        // Setup button listeners
        if (closeButton != null)
        {
            closeButton.onClick.AddListener(ToggleEncyclopedia);
        }

        // Setup category buttons
        for (int i = 0; i < categoryButtons.Length; i++)
        {
            string category = fishCategories[i];
            categoryButtons[i].onClick.AddListener(() => FilterByCategory(category));
        }

        // Load discovered fish from PlayerPrefs
        LoadDiscoveredFish();
    }

    private void Update()
    {
        // Toggle encyclopedia with 'E' key
        if (Input.GetKeyDown(KeyCode.E))
        {
            ToggleEncyclopedia();
        }
    }

    public void ToggleEncyclopedia()
    {
        if (encyclopediaPanel != null)
        {
            bool isActive = !encyclopediaPanel.activeSelf;
            encyclopediaPanel.SetActive(isActive);
            
            if (isActive)
            {
                RefreshFishList();
            }
        }
    }

    public void AddDiscoveredFish(FishData fish)
    {
        if (fish != null && !discoveredFish.Contains(fish))
        {
            discoveredFish.Add(fish);
            fish.isDiscovered = true;
            onFishDiscovered.Invoke(fish);
            SaveDiscoveredFish();
            RefreshFishList();
        }
    }

    public void OnFishCaught(FishData fish)
    {
        if (fish != null)
        {
            onFishCaught.Invoke(fish);
        }
    }

    private void RefreshFishList()
    {
        // Clear current list
        foreach (Transform child in fishListContent)
        {
            Destroy(child.gameObject);
        }

        // Filter fish based on category
        var filteredFish = FilterFishByCategory(currentCategory);

        // Populate list
        foreach (var fish in filteredFish)
        {
            GameObject entry = Instantiate(fishEntryPrefab, fishListContent);
            FishEntryUI entryUI = entry.GetComponent<FishEntryUI>();
            
            if (entryUI != null)
            {
                entryUI.Setup(fish, () => SelectFish(fish));
            }
        }
    }

    private List<FishData> FilterFishByCategory(string category)
    {
        if (category == "All")
        {
            return new List<FishData>(discoveredFish);
        }

        return new List<FishData>(discoveredFish).FindAll(fish =>
        {
            switch (category)
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

    private void FilterByCategory(string category)
    {
        currentCategory = category;
        RefreshFishList();
    }

    private void SelectFish(FishData fish)
    {
        currentlySelectedFish = fish;
        UpdateSelectedFishUI();
    }

    private void UpdateSelectedFishUI()
    {
        if (currentlySelectedFish == null) return;

        if (selectedFishImage != null)
        {
            selectedFishImage.sprite = currentlySelectedFish.fishSprite;
        }

        if (selectedFishName != null)
        {
            selectedFishName.text = currentlySelectedFish.fishName;
        }

        if (selectedFishScientificName != null)
        {
            selectedFishScientificName.text = currentlySelectedFish.scientificName;
        }

        if (selectedFishDescription != null)
        {
            selectedFishDescription.text = currentlySelectedFish.description;
        }

        if (selectedFishStats != null)
        {
            selectedFishStats.text = $"Size: {currentlySelectedFish.minSize}-{currentlySelectedFish.maxSize} cm\n" +
                                   $"Value: {currentlySelectedFish.baseValue} coins\n" +
                                   $"Rarity: {(currentlySelectedFish.rarity * 100):F0}%\n" +
                                   $"Catch Difficulty: {(currentlySelectedFish.catchDifficulty * 100):F0}%\n" +
                                   $"Preferred Depth: {currentlySelectedFish.preferredDepth}m\n" +
                                   $"Habitats: {string.Join(", ", currentlySelectedFish.habitats)}\n" +
                                   $"Seasons: {string.Join(", ", currentlySelectedFish.seasons)}";
        }
    }

    private void SaveDiscoveredFish()
    {
        string discoveredFishJson = JsonUtility.ToJson(new SerializableFishList(discoveredFish));
        PlayerPrefs.SetString("DiscoveredFish", discoveredFishJson);
        PlayerPrefs.Save();
    }

    private void LoadDiscoveredFish()
    {
        if (PlayerPrefs.HasKey("DiscoveredFish"))
        {
            string json = PlayerPrefs.GetString("DiscoveredFish");
            SerializableFishList fishList = JsonUtility.FromJson<SerializableFishList>(json);
            discoveredFish.Clear();
            foreach (FishData fish in fishList.fishList)
            {
                discoveredFish.Add(fish);
            }
        }
    }

    /// <summary>
    /// Clear all discovered fish progress and remove the persisted data.
    /// Use this when starting a brand new game so the encyclopedia is empty again.
    /// </summary>
    public void ResetDiscoveredFish()
    {
        foreach (var fish in discoveredFish)
        {
            if (fish != null)
            {
                fish.isDiscovered = false;
            }
        }

        discoveredFish.Clear();

        PlayerPrefs.DeleteKey("DiscoveredFish");
        PlayerPrefs.Save();
    }

    public List<FishData> GetAllFish()
    {
        return new List<FishData>(allFish);
    }

    public List<FishData> GetDiscoveredFish()
    {
        return new List<FishData>(discoveredFish);
    }

    public bool IsFishDiscovered(FishData fish)
    {
        return discoveredFish.Contains(fish);
    }
}

[System.Serializable]
public class SerializableFishList
{
    public List<FishData> fishList;

    public SerializableFishList(IEnumerable<FishData> fish)
    {
        fishList = new List<FishData>(fish);
    }
} 