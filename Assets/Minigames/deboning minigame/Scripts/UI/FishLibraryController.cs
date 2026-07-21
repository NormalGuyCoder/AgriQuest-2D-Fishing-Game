using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

public class FishLibraryController : MonoBehaviour
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
    public List<FishDefinition> availableFish = new List<FishDefinition>();

    [Header("Learning Tips")]
    public TMPro.TextMeshProUGUI beginnerTipText;
    public TMPro.TextMeshProUGUI intermediateTipText;
    public TMPro.TextMeshProUGUI advancedTipText;

    private List<GameObject> spawnedCards = new List<GameObject>();
    private int currentPage = 0;

    void Start()
    {
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

        int totalFish = availableFish != null ? availableFish.Count : 0;
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
                FishDefinition fish = availableFish[i];
                if (fish != null)
                {
                    CreateFishCard(fish);
                }
            }
        }

        UpdatePaginationControls(totalFish);
    }

    private void CreateFishCard(FishDefinition fish)
    {
        GameObject cardObj = Instantiate(fishCardPrefab, fishCardsContainer);
        cardObj.name = "Card_" + fish.displayName;

        FishCardUI cardUI = cardObj.GetComponent<FishCardUI>();
        if (cardUI == null)
        {
            cardUI = cardObj.AddComponent<FishCardUI>();
        }

        cardUI.SetupCard(fish, this);
        spawnedCards.Add(cardObj);
    }

    public void OnFishSelected(FishDefinition fish)
    {
        // Show detailed fish info screen or start game
        FishDetailPanel detailPanel = GetComponentInChildren<FishDetailPanel>(true);
        if (detailPanel != null)
        {
            detailPanel.ShowFishDetails(fish);
        }
        else
        {
            // If no detail panel, start game directly
            StartDeboningFish(fish);
        }
    }

    private void ChangePage(int delta)
    {
        int totalFish = availableFish != null ? availableFish.Count : 0;
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

    public void StartDeboningFish(FishDefinition fish)
    {
        if (DeboningSceneController.Instance != null && GameManager.Instance != null)
        {
            DeboningSceneController.Instance.StartDeboningGame(fish);
            
            // Hide library, show game panel
            MainMenuController mainMenu = FindObjectOfType<MainMenuController>();
            if (mainMenu != null && mainMenu.deboningGamePanel != null)
            {
                mainMenu.deboningGamePanel.SetActive(true);
                gameObject.SetActive(false);
            }
        }
    }

    public void OnBackToMenu()
    {
        MainMenuController mainMenu = FindObjectOfType<MainMenuController>();
        if (mainMenu != null)
        {
            mainMenu.ShowMainMenu();
        }
    }
}

