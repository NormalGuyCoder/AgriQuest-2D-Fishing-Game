using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class EndangeredFishCardUI : MonoBehaviour
{
    [Header("Card UI Elements")]
    public Image fishIconImage;
    public TMPro.TextMeshProUGUI fishNameText;
    public TMPro.TextMeshProUGUI scientificNameText;
    public TMPro.TextMeshProUGUI statusText;
    public TMPro.TextMeshProUGUI descriptionText;
    public Button viewDetailsButton;
    public Image statusBadge;

    private EndangeredFishDefinition fishData;
    private EndangeredFishLibraryController libraryController;

    void Start()
    {
        if (viewDetailsButton != null)
        {
            viewDetailsButton.onClick.AddListener(OnCardClicked);
        }
    }

    public void SetupCard(EndangeredFishDefinition fish, EndangeredFishLibraryController controller)
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
            fishNameText.text = fish.fishName;
        }

        if (scientificNameText != null)
        {
            scientificNameText.text = fish.scientificName;
        }

        if (statusText != null)
        {
            statusText.text = fish.GetStatusText();
        }

        // Set status badge color
        if (statusBadge != null)
        {
            switch (fish.status)
            {
                case EndangeredFishDefinition.ConservationStatus.Vulnerable:
                    statusBadge.color = new Color(1f, 0.8f, 0.2f); // Yellow
                    break;
                case EndangeredFishDefinition.ConservationStatus.Endangered:
                    statusBadge.color = new Color(1f, 0.5f, 0.1f); // Orange
                    break;
                case EndangeredFishDefinition.ConservationStatus.CriticallyEndangered:
                    statusBadge.color = new Color(0.9f, 0.2f, 0.2f); // Red
                    break;
                case EndangeredFishDefinition.ConservationStatus.ExtinctInWild:
                    statusBadge.color = new Color(0.5f, 0.5f, 0.5f); // Gray
                    break;
            }
        }

        if (descriptionText != null)
        {
            descriptionText.text = fish.description;
        }

        // Update button text
        if (viewDetailsButton != null)
        {
            TMPro.TextMeshProUGUI buttonText = viewDetailsButton.GetComponentInChildren<TMPro.TextMeshProUGUI>();
            if (buttonText != null)
            {
                buttonText.text = $"Learn More";
            }
        }
    }

    public void OnCardClicked()
    {
        if (libraryController != null && fishData != null)
        {
            libraryController.OnEndangeredFishSelected(fishData);
        }
    }
}



