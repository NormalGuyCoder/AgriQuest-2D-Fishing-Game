using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

/// <summary>
/// Shared fish library controller that uses FishCatalogEntry.
/// Can be used by both deboning and freshwater fishing minigames.
/// </summary>
public class SharedFishLibraryController : MonoBehaviour
{
    [Header("UI References")]
    public Transform fishCardsContainer; // Parent for spawning fish cards
    public GameObject fishCardPrefab; // Prefab for fish card UI
    public Button backToMenuButton;
    public TMPro.TextMeshProUGUI libraryTitleText;

    [Header("Pagination")]
    [Min(1)]
    public int itemsPerPage = 8;
    public Button nextPageButton;
    public Button previousPageButton;
    public TMPro.TextMeshProUGUI pageIndicatorText;

    [Header("Fish Database")]
    [Tooltip("The shared fish catalog database containing all fish entries")]
    public FishCatalogDatabase fishCatalogDatabase;
    [Tooltip("Optional: Manually assign fish entries if not using database")]
    public List<FishCatalogEntry> availableFish = new List<FishCatalogEntry>();

    [Header("Learning Tips")]
    public TMPro.TextMeshProUGUI beginnerTipText;
    public TMPro.TextMeshProUGUI intermediateTipText;
    public TMPro.TextMeshProUGUI advancedTipText;

    private List<GameObject> spawnedCards = new List<GameObject>();
    private int currentPage = 0;
    private List<FishCatalogEntry> allFishEntries = new List<FishCatalogEntry>();

    void Start()
    {
        // Load fish entries from database if available
        if (fishCatalogDatabase != null)
        {
            allFishEntries = new List<FishCatalogEntry>(fishCatalogDatabase.Entries);
            Debug.Log($"SharedFishLibraryController: Loaded {allFishEntries.Count} fish from catalog database.");
        }
        else if (availableFish != null && availableFish.Count > 0)
        {
            allFishEntries = new List<FishCatalogEntry>(availableFish);
            Debug.Log($"SharedFishLibraryController: Using manually assigned fish list ({allFishEntries.Count} fish).");
        }
        else
        {
            Debug.LogWarning("SharedFishLibraryController: No fish catalog database or fish list assigned!");
        }

        if (backToMenuButton != null)
        {
            backToMenuButton.onClick.AddListener(OnBackToMenu);
        }

        if (nextPageButton != null)
        {
            nextPageButton.onClick.AddListener(() => ChangePage(1));
        }

        if (previousPageButton != null)
        {
            previousPageButton.onClick.AddListener(() => ChangePage(-1));
        }

        RefreshLibrary();
    }

    public void RefreshLibrary()
    {
        if (itemsPerPage < 1)
        {
            itemsPerPage = 1;
        }

        int totalFish = allFishEntries != null ? allFishEntries.Count : 0;
        int totalPages = totalFish == 0 ? 1 : Mathf.CeilToInt(totalFish / (float)itemsPerPage);
        currentPage = Mathf.Clamp(currentPage, 0, totalPages - 1);

        // Clear existing cards
        foreach (GameObject card in spawnedCards)
        {
            if (card != null)
                Destroy(card);
        }
        spawnedCards.Clear();

        // Spawn cards for each fish
        if (fishCardPrefab != null && fishCardsContainer != null)
        {
            int startIndex = currentPage * itemsPerPage;
            int endIndex = Mathf.Min(startIndex + itemsPerPage, totalFish);

            for (int i = startIndex; i < endIndex; i++)
            {
                FishCatalogEntry fish = allFishEntries[i];
                if (fish != null)
                {
                    CreateFishCard(fish);
                }
            }
        }

        UpdatePaginationControls(totalFish);
    }

    private void CreateFishCard(FishCatalogEntry fish)
    {
        GameObject cardObj = Instantiate(fishCardPrefab, fishCardsContainer);
        cardObj.name = "Card_" + fish.displayName;

        // Try to get existing SharedFishCardUI component, or add one
        SharedFishCardUI cardUI = cardObj.GetComponent<SharedFishCardUI>();
        if (cardUI == null)
        {
            cardUI = cardObj.AddComponent<SharedFishCardUI>();
        }

        cardUI.SetupCard(fish, this);
        spawnedCards.Add(cardObj);
    }

    public void OnFishSelected(FishCatalogEntry fish)
    {
        // Show detailed fish info screen
        SharedFishDetailPanel detailPanel = GetComponentInChildren<SharedFishDetailPanel>(true);
        if (detailPanel != null)
        {
            detailPanel.ShowFishDetails(fish);
        }
        else
        {
            Debug.Log($"Selected fish: {fish.displayName} ({fish.scientificName})");
        }
    }

    private void ChangePage(int delta)
    {
        int totalFish = allFishEntries != null ? allFishEntries.Count : 0;
        int totalPages = totalFish == 0 ? 1 : Mathf.CeilToInt(totalFish / (float)itemsPerPage);
        currentPage = Mathf.Clamp(currentPage + delta, 0, Mathf.Max(0, totalPages - 1));
        RefreshLibrary();
    }

    private void UpdatePaginationControls(int totalFish)
    {
        if (itemsPerPage < 1)
        {
            itemsPerPage = 1;
        }

        int totalPages = totalFish == 0 ? 1 : Mathf.CeilToInt(totalFish / (float)itemsPerPage);
        int clampedPage = Mathf.Clamp(currentPage, 0, Mathf.Max(0, totalPages - 1));
        if (clampedPage != currentPage)
        {
            currentPage = clampedPage;
        }

        if (previousPageButton != null)
        {
            previousPageButton.interactable = currentPage > 0 && totalFish > 0;
        }

        if (nextPageButton != null)
        {
            nextPageButton.interactable = totalFish > 0 && currentPage < totalPages - 1;
        }

        if (pageIndicatorText != null)
        {
            if (totalFish == 0)
            {
                pageIndicatorText.text = "No fish available";
            }
            else
            {
                pageIndicatorText.text = $"Page {currentPage + 1} / {totalPages}";
            }
        }
    }

    public void OnBackToMenu()
    {
        // Hide library panel
        gameObject.SetActive(false);
    }

    /// <summary>
    /// Get all available fish entries
    /// </summary>
    public List<FishCatalogEntry> GetAllFishEntries()
    {
        return new List<FishCatalogEntry>(allFishEntries);
    }

    /// <summary>
    /// Get a fish entry by ID
    /// </summary>
    public FishCatalogEntry GetFishById(string fishId)
    {
        if (fishCatalogDatabase != null)
        {
            return fishCatalogDatabase.GetEntry(fishId);
        }
        
        foreach (var fish in allFishEntries)
        {
            if (fish != null && fish.GetFishId() == fishId)
            {
                return fish;
            }
        }
        
        return null;
    }
}

/// <summary>
/// UI component for displaying a fish card in the shared library
/// </summary>
public class SharedFishCardUI : MonoBehaviour
{
    private FishCatalogEntry fish;
    private SharedFishLibraryController libraryController;

    [Header("UI References (Auto-assigned if present)")]
    public Image fishImage;
    public TextMeshProUGUI fishNameText;
    public TextMeshProUGUI scientificNameText;
    public Button cardButton;

    public void SetupCard(FishCatalogEntry fishEntry, SharedFishLibraryController controller)
    {
        fish = fishEntry;
        libraryController = controller;

        // Auto-find UI components if not assigned
        if (fishImage == null)
            fishImage = GetComponentInChildren<Image>();
        if (fishNameText == null)
            fishNameText = GetComponentInChildren<TextMeshProUGUI>();
        if (cardButton == null)
            cardButton = GetComponent<Button>();

        // Set fish image
        if (fishImage != null && fish != null)
        {
            fishImage.sprite = fish.iconSprite != null ? fish.iconSprite : fish.mainSprite;
        }

        // Set fish name
        if (fishNameText != null && fish != null)
        {
            fishNameText.text = fish.displayName;
        }

        // Set up button
        if (cardButton != null)
        {
            cardButton.onClick.RemoveAllListeners();
            cardButton.onClick.AddListener(OnCardClicked);
        }
    }

    private void OnCardClicked()
    {
        if (libraryController != null && fish != null)
        {
            libraryController.OnFishSelected(fish);
        }
    }
}

/// <summary>
/// Panel for displaying detailed fish information
/// </summary>
public class SharedFishDetailPanel : MonoBehaviour
{
    [Header("UI References")]
    public GameObject panel;
    public Image fishImage;
    public TextMeshProUGUI fishNameText;
    public TextMeshProUGUI scientificNameText;
    public TextMeshProUGUI descriptionText;
    public TextMeshProUGUI tagsText;
    public TextMeshProUGUI endangeredStatusText;
    public Button closeButton;

    public void ShowFishDetails(FishCatalogEntry fish)
    {
        if (panel != null)
        {
            panel.SetActive(true);
        }

        if (fishImage != null)
        {
            fishImage.sprite = fish.mainSprite != null ? fish.mainSprite : fish.iconSprite;
        }

        if (fishNameText != null)
        {
            fishNameText.text = fish.displayName;
        }

        if (scientificNameText != null)
        {
            scientificNameText.text = fish.scientificName;
        }

        if (descriptionText != null)
        {
            descriptionText.text = fish.description;
        }

        if (tagsText != null && fish.tags != null && fish.tags.Length > 0)
        {
            tagsText.text = "Tags: " + string.Join(", ", fish.tags);
        }

        if (endangeredStatusText != null)
        {
            endangeredStatusText.text = fish.isEndangered ? "⚠️ ENDANGERED SPECIES" : "✓ Common Species";
            endangeredStatusText.color = fish.isEndangered ? Color.red : Color.green;
        }

        if (closeButton != null)
        {
            closeButton.onClick.RemoveAllListeners();
            closeButton.onClick.AddListener(() => {
                if (panel != null) panel.SetActive(false);
            });
        }
    }
}

