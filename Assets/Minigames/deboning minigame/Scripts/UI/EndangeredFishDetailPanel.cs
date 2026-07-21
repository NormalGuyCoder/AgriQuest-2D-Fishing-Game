using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class EndangeredFishDetailPanel : MonoBehaviour
{
    [Header("Panel References")]
    public GameObject detailPanel;
    public Button backButton;

    [Header("Fish Info Display")]
    public Image fishImage;
    public TMPro.TextMeshProUGUI fishNameText;
    public TMPro.TextMeshProUGUI scientificNameText;
    public TMPro.TextMeshProUGUI descriptionText;
    public TMPro.TextMeshProUGUI statusText;
    public TMPro.TextMeshProUGUI iucnStatusText;
    public TMPro.TextMeshProUGUI populationTrendText;

    [Header("Information Panels")]
    public TMPro.TextMeshProUGUI habitatText;
    public TMPro.TextMeshProUGUI distributionText;
    public TMPro.TextMeshProUGUI threatsText;
    public TMPro.TextMeshProUGUI importanceText;
    public TMPro.TextMeshProUGUI conservationEffortsText;

    [Header("Learn More Panel")]
    public GameObject learnMorePanel;
    public TMPro.TextMeshProUGUI learnMoreTitleText;
    public TMPro.TextMeshProUGUI learnMoreCopyText;
    public TMPro.TextMeshProUGUI learnMoreActionText;

    private EndangeredFishDefinition currentFish;

    void Start()
    {
        if (backButton != null)
        {
            backButton.onClick.AddListener(OnBackClicked);
        }

        if (detailPanel != null)
        {
            detailPanel.SetActive(false);
        }
    }

    public void ShowFishDetails(EndangeredFishDefinition fish)
    {
        currentFish = fish;

        if (detailPanel != null)
        {
            detailPanel.SetActive(true);
        }

        // Update all UI elements
        if (fishImage != null && fish.fishImage != null)
        {
            fishImage.sprite = fish.fishImage;
        }

        if (fishNameText != null)
        {
            fishNameText.text = fish.fishName;
        }

        if (scientificNameText != null)
        {
            scientificNameText.text = fish.scientificName;
        }

        if (descriptionText != null)
        {
            descriptionText.text = fish.description;
        }

        if (statusText != null)
        {
            statusText.text = $"Conservation Status: {fish.GetStatusText()}";
        }

        if (iucnStatusText != null)
        {
            iucnStatusText.text = $"IUCN Status: {fish.iucnStatus}";
        }

        if (populationTrendText != null)
        {
            populationTrendText.text = $"Population Trend: {fish.populationTrend}";
        }

        if (habitatText != null)
        {
            habitatText.text = $"Habitat: {fish.habitat}";
        }

        if (distributionText != null)
        {
            distributionText.text = $"Distribution: {fish.distribution}";
        }

        if (threatsText != null)
        {
            threatsText.text = $"Threats: {fish.threats}";
        }

        if (importanceText != null)
        {
            importanceText.text = $"Importance: {fish.importance}";
        }

        if (conservationEffortsText != null)
        {
            conservationEffortsText.text = $"Conservation Efforts: {fish.conservationEfforts}";
        }

        UpdateLearnMoreSection(fish);
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

    private void UpdateLearnMoreSection(EndangeredFishDefinition fish)
    {
        bool hasLearnMore = !string.IsNullOrWhiteSpace(fish.learnMoreCopy);

        if (learnMorePanel != null)
        {
            learnMorePanel.SetActive(hasLearnMore);
        }

        if (learnMoreTitleText != null)
        {
            learnMoreTitleText.text = !string.IsNullOrWhiteSpace(fish.learnMoreHeadline)
                ? fish.learnMoreHeadline
                : $"Learn more about {fish.fishName}";
        }

        if (learnMoreCopyText != null)
        {
            learnMoreCopyText.text = hasLearnMore
                ? fish.learnMoreCopy
                : "Currently no additional learning material is available for this species.";
        }

        if (learnMoreActionText != null)
        {
            learnMoreActionText.text = !string.IsNullOrWhiteSpace(fish.learnMoreAction)
                ? $"What you can do: {fish.learnMoreAction}"
                : string.Empty;
        }
    }
}



