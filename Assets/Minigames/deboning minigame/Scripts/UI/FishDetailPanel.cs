using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class FishDetailPanel : MonoBehaviour
{
    [Header("Panel References")]
    public GameObject detailPanel;
    public Button backButton;
    public Button startDeboningButton;

    [Header("Fish Info Display")]
    public Image fishIconImage;
    public TMPro.TextMeshProUGUI fishNameText;
    public TMPro.TextMeshProUGUI descriptionText;
    public TMPro.TextMeshProUGUI difficultyText;
    public TMPro.TextMeshProUGUI timeLimitText;
    public TMPro.TextMeshProUGUI totalBonesText;

    [Header("Challenge Details Panel")]
    public TMPro.TextMeshProUGUI challengeDetailsText;

    [Header("Cooking Info Panel")]
    public TMPro.TextMeshProUGUI habitatText;
    public TMPro.TextMeshProUGUI cookingTipsText;

    private FishDefinition currentFish;

    void Start()
    {
        if (backButton != null)
        {
            backButton.onClick.AddListener(OnBackClicked);
        }

        if (startDeboningButton != null)
        {
            startDeboningButton.onClick.AddListener(OnStartDeboning);
        }

        if (detailPanel != null)
        {
            detailPanel.SetActive(false);
        }
    }

    public void ShowFishDetails(FishDefinition fish)
    {
        currentFish = fish;

        if (detailPanel != null)
        {
            detailPanel.SetActive(true);
        }

        // Update all UI elements
        if (fishIconImage != null && fish.fishIcon != null)
        {
            fishIconImage.sprite = fish.fishIcon;
        }

        if (fishNameText != null)
        {
            fishNameText.text = fish.displayName;
        }

        if (descriptionText != null)
        {
            descriptionText.text = fish.description;
        }

        if (difficultyText != null)
        {
            difficultyText.text = $"Difficulty: {fish.GetDifficultyText()}";
        }

        if (timeLimitText != null)
        {
            timeLimitText.text = $"Time Limit: {fish.timeLimitSeconds}s";
        }

        if (totalBonesText != null)
        {
            totalBonesText.text = $"Total Bones: {fish.GetTotalBoneCount()}";
        }

        if (challengeDetailsText != null)
        {
            challengeDetailsText.text = $"Difficulty: {fish.GetDifficultyText()}\n" +
                                        $"Time Limit: {fish.timeLimitSeconds}s\n" +
                                        $"Total Bones: {fish.GetTotalBoneCount()}";
        }

        if (habitatText != null)
        {
            habitatText.text = $"Habitat: {fish.habitat}";
        }

        if (cookingTipsText != null)
        {
            cookingTipsText.text = fish.cookingTips;
        }

        // Update button text
        if (startDeboningButton != null)
        {
            TMPro.TextMeshProUGUI buttonText = startDeboningButton.GetComponentInChildren<TMPro.TextMeshProUGUI>();
            if (buttonText != null)
            {
                buttonText.text = $"Start Deboning {fish.displayName}!";
            }
        }
    }

    public void HideFishDetails()
    {
        if (detailPanel != null)
        {
            detailPanel.SetActive(false);
        }
    }

    private void OnBackClicked()
    {
        HideFishDetails();
    }

    private void OnStartDeboning()
    {
        if (currentFish != null && DeboningSceneController.Instance != null)
        {
            HideFishDetails();
            DeboningSceneController.Instance.StartDeboningGame(currentFish);
            
            // Hide library, show game
            MainMenuController mainMenu = FindObjectOfType<MainMenuController>();
            if (mainMenu != null && mainMenu.deboningGamePanel != null)
            {
                mainMenu.deboningGamePanel.SetActive(true);
                
                FishLibraryController library = FindObjectOfType<FishLibraryController>();
                if (library != null)
                {
                    library.gameObject.SetActive(false);
                }
            }
        }
    }
}

