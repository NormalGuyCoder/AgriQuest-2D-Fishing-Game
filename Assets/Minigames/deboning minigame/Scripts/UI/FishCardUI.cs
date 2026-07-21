using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class FishCardUI : MonoBehaviour
{
    [Header("Card UI Elements")]
    public Image fishIconImage;
    public TMPro.TextMeshProUGUI fishNameText;
    public TMPro.TextMeshProUGUI difficultyText;
    public TMPro.TextMeshProUGUI descriptionText;
    public TMPro.TextMeshProUGUI timeLimitText;
    public TMPro.TextMeshProUGUI boneCountText;
    public TMPro.TextMeshProUGUI spineCountText;
    public TMPro.TextMeshProUGUI ribCountText;
    public TMPro.TextMeshProUGUI pinCountText;
    public TMPro.TextMeshProUGUI habitatText;
    public Button selectButton;
    public Image difficultyBadge; // For color coding

    private FishDefinition fishData;
    private FishLibraryController libraryController;

    void Start()
    {
        if (selectButton != null)
        {
            selectButton.onClick.AddListener(OnCardClicked);
        }
    }

    public void SetupCard(FishDefinition fish, FishLibraryController controller)
    {
        fishData = fish;
        libraryController = controller;

        // Update UI elements
        if (fishIconImage != null && fish.fishIcon != null)
        {
            fishIconImage.sprite = fish.fishIcon;
        }

        if (fishNameText != null)
        {
            fishNameText.text = fish.displayName;
        }

        if (difficultyText != null)
        {
            difficultyText.text = fish.GetDifficultyText();
        }

        // Set difficulty badge color
        if (difficultyBadge != null)
        {
            switch (fish.difficulty)
            {
                case FishDefinition.DifficultyLevel.Beginner:
                    difficultyBadge.color = new Color(0.2f, 0.8f, 0.3f); // Green
                    break;
                case FishDefinition.DifficultyLevel.Intermediate:
                    difficultyBadge.color = new Color(1f, 0.8f, 0.2f); // Yellow/Orange
                    break;
                case FishDefinition.DifficultyLevel.Advanced:
                    difficultyBadge.color = new Color(0.9f, 0.2f, 0.2f); // Red
                    break;
            }
        }

        if (descriptionText != null)
        {
            descriptionText.text = fish.description;
        }

        if (timeLimitText != null)
        {
            timeLimitText.text = $"Time Limit: {fish.timeLimitSeconds}s";
        }

        if (boneCountText != null)
        {
            boneCountText.text = $"Bones: {fish.GetTotalBoneCount()}";
        }

        if (spineCountText != null)
        {
            spineCountText.text = $"Main: {fish.spineBoneCount}";
        }

        if (ribCountText != null)
        {
            ribCountText.text = $"Ribs: {fish.ribBoneCount}";
        }

        if (pinCountText != null)
        {
            pinCountText.text = $"Pins: {fish.pinBoneCount}";
        }

        if (habitatText != null)
        {
            habitatText.text = fish.habitat;
        }

        // Update button text
        if (selectButton != null)
        {
            TMPro.TextMeshProUGUI buttonText = selectButton.GetComponentInChildren<TMPro.TextMeshProUGUI>();
            if (buttonText != null)
            {
                buttonText.text = $"Select {fish.displayName}";
            }
        }
    }

    public void OnCardClicked()
    {
        if (libraryController != null && fishData != null)
        {
            libraryController.OnFishSelected(fishData);
        }
    }
}


