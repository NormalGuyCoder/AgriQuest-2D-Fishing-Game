using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

public class EndangeredFishLibraryController : MonoBehaviour
{
    [Header("UI References")]
    public Transform fishCardsContainer;
    public GameObject endangeredFishCardPrefab;
    public Button backToMenuButton;
    public TMPro.TextMeshProUGUI libraryTitleText;
    public TMPro.TextMeshProUGUI libraryDescriptionText;

    [Header("Pagination")]
    [Min(1)]
    public int itemsPerPage = 8;
    public Button nextPageButton;
    public Button previousPageButton;
    public TMPro.TextMeshProUGUI pageIndicatorText;

    [Header("Endangered Fish Database")]
    public List<EndangeredFishDefinition> endangeredFish = new List<EndangeredFishDefinition>();

    [Header("Detail Panel")]
    [SerializeField]
    private EndangeredFishDetailPanel detailPanel;

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

        if (libraryDescriptionText != null)
        {
            libraryDescriptionText.text = "Learn about endangered fish species. These species are protected and should not be caught or deboned. Understand their importance and conservation efforts.";
        }

        RefreshLibrary();
    }

    public void RefreshLibrary()
    {
        if (itemsPerPage < 1)
        {
            itemsPerPage = 1;
        }

        int totalFish = endangeredFish != null ? endangeredFish.Count : 0;
        int totalPages = totalFish == 0 ? 1 : Mathf.CeilToInt(totalFish / (float)itemsPerPage);
        currentPage = Mathf.Clamp(currentPage, 0, totalPages - 1);

        // Clear existing cards
        foreach (GameObject card in spawnedCards)
        {
            if (card != null)
                Destroy(card);
        }
        spawnedCards.Clear();

        // Spawn cards for each endangered fish
        if (endangeredFishCardPrefab != null && fishCardsContainer != null)
        {
            int startIndex = currentPage * itemsPerPage;
            int endIndex = Mathf.Min(startIndex + itemsPerPage, totalFish);

            for (int i = startIndex; i < endIndex; i++)
            {
                EndangeredFishDefinition fish = endangeredFish[i];
                if (fish != null)
                {
                    CreateEndangeredFishCard(fish);
                }
            }
        }

        UpdatePaginationControls(totalFish);
    }

    private void CreateEndangeredFishCard(EndangeredFishDefinition fish)
    {
        GameObject cardObj = Instantiate(endangeredFishCardPrefab, fishCardsContainer);
        cardObj.name = "Card_" + fish.fishName;

        EndangeredFishCardUI cardUI = cardObj.GetComponent<EndangeredFishCardUI>();
        if (cardUI == null)
        {
            cardUI = cardObj.AddComponent<EndangeredFishCardUI>();
        }

        cardUI.SetupCard(fish, this);
        spawnedCards.Add(cardObj);
    }

    public void OnEndangeredFishSelected(EndangeredFishDefinition fish)
    {
        if (detailPanel == null)
        {
            detailPanel = GetComponentInChildren<EndangeredFishDetailPanel>(true);
        }

        if (detailPanel != null)
        {
            detailPanel.ShowFishDetails(fish);
        }
        else
        {
            Debug.LogWarning("EndangeredFishLibraryController: No EndangeredFishDetailPanel assigned or found in children.");
        }
    }

    private void ChangePage(int delta)
    {
        int totalFish = endangeredFish != null ? endangeredFish.Count : 0;
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
        MainMenuController mainMenu = FindObjectOfType<MainMenuController>();
        if (mainMenu != null)
        {
            mainMenu.ShowMainMenu();
        }
    }
}


