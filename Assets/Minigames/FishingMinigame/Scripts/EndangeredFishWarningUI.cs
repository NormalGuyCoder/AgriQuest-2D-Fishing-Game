using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Shows a confirmation dialog whenever the player hooks an endangered or prohibited fish.
/// </summary>
public class EndangeredFishWarningUI : MonoBehaviour
{
    [Header("Panel References")]
    [SerializeField] private GameObject warningPanel;
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI bodyText;
    [SerializeField] private Image fishPreviewImage;
    [SerializeField] private Button keepButton;
    [SerializeField] private Button releaseButton;

    private Action keepCallback;
    private Action releaseCallback;

    private void Awake()
    {
        if (warningPanel != null)
        {
            warningPanel.SetActive(false);
        }

        if (keepButton != null)
        {
            keepButton.onClick.AddListener(OnKeepPressed);
        }

        if (releaseButton != null)
        {
            releaseButton.onClick.AddListener(OnReleasePressed);
        }
    }

    public void ShowWarning(FishData fishData, Action onKeep, Action onRelease)
    {
        keepCallback = onKeep;
        releaseCallback = onRelease;

        if (warningPanel == null)
        {
            Debug.LogWarning("EndangeredFishWarningUI: Warning panel is not assigned.");
            OnKeepPressed();
            return;
        }

        warningPanel.SetActive(true);

        if (titleText != null)
        {
            titleText.text = $"{fishData.fishName} is endangered!";
        }

        if (bodyText != null)
        {
            string customCopy = !string.IsNullOrWhiteSpace(fishData.conservationWarningCopy)
                ? fishData.conservationWarningCopy
                : "This species is not recommended for fishing. Keeping it will yield a high payout to help with debt, but it harms marine sustainability.";

            bodyText.text = customCopy + "\n\nDo you still want to keep this catch?";
        }

        if (fishPreviewImage != null)
        {
            fishPreviewImage.sprite = fishData.fishSprite;
            fishPreviewImage.enabled = fishData.fishSprite != null;
        }
    }

    public void Hide()
    {
        if (warningPanel != null)
        {
            warningPanel.SetActive(false);
        }

        keepCallback = null;
        releaseCallback = null;
    }

    private void OnKeepPressed()
    {
        keepCallback?.Invoke();
        Hide();
    }

    private void OnReleasePressed()
    {
        releaseCallback?.Invoke();
        Hide();
    }
}

