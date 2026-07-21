using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class FishCaughtUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject fishCaughtPanel;
    [SerializeField] private Image fishImage;
    [SerializeField] private TextMeshProUGUI fishNameText;
    [SerializeField] private TextMeshProUGUI scientificNameText;
    [SerializeField] private TextMeshProUGUI fishDescriptionText;
    [SerializeField] private TextMeshProUGUI fishValueText;
    [SerializeField] private Button closeButton;

    [Header("Stats Reference")]
    [SerializeField] private SimpleFishingStats fishingStats; // Can be assigned in inspector or found automatically

    private bool isPanelVisible = false;

    public bool IsVisible => isPanelVisible;

    private void Awake()
    {
        // Ensure panel is hidden immediately when the component awakens
        // This prevents it from showing on game start
        if (fishCaughtPanel != null)
        {
            fishCaughtPanel.SetActive(false);
            isPanelVisible = false;
        }
    }

    private void Start()
    {
        Debug.Log("FishCaughtUI Start - Checking references:");
        Debug.Log($"Panel: {(fishCaughtPanel != null ? "Found" : "Missing")}");
        Debug.Log($"Image: {(fishImage != null ? "Found" : "Missing")}");
        Debug.Log($"Name Text: {(fishNameText != null ? "Found" : "Missing")}");
        Debug.Log($"Scientific Name Text: {(scientificNameText != null ? "Found" : "Missing")}");
        Debug.Log($"Description Text: {(fishDescriptionText != null ? "Found" : "Missing")}");
        Debug.Log($"Value Text: {(fishValueText != null ? "Found" : "Missing")}");
        Debug.Log($"Close Button: {(closeButton != null ? "Found" : "Missing")}");

        // Try to find SimpleFishingStats (including inactive ones)
        FindFishingStats();

        if (closeButton != null)
        {
            // Remove any existing listeners to avoid duplicates
            closeButton.onClick.RemoveAllListeners();
            closeButton.onClick.AddListener(OnCloseButtonClicked);
            Debug.Log("FishCaughtUI: Close button listener added.");
        }
        else
        {
            Debug.LogWarning("FishCaughtUI: Close button is null! You can still close the panel with Escape key, but timer resume may not work properly.");
        }
        
        // Initialize panel state without triggering timer resume
        // Only hide if it's not already being shown
        if (!isPanelVisible)
        {
            InitializePanel();
        }
    }

    private void OnEnable()
    {
        // Ensure close button listener is set when enabled
        if (closeButton != null)
        {
            closeButton.onClick.RemoveAllListeners();
            closeButton.onClick.AddListener(OnCloseButtonClicked);
        }
    }

    private void Update()
    {
        // Allow closing with Escape key if close button is missing
        if (isPanelVisible && Input.GetKeyDown(KeyCode.Escape))
        {
            Debug.Log("Escape key pressed - closing fish caught panel.");
            HidePanel();
        }
    }

    public void ShowFishCaught(FishData fish)
    {
        Debug.Log($"ShowFishCaught called with fish: {(fish != null ? fish.fishName : "null")}");
        
        if (fish == null)
        {
            Debug.LogWarning("ShowFishCaught called with null fish!");
            return;
        }

        if (fishCaughtPanel == null)
        {
            Debug.LogError("Fish Caught Panel reference is missing!");
            return;
        }

        // Try to find SimpleFishingStats again in case it wasn't found at Start
        if (fishingStats == null)
        {
            FindFishingStats();
        }

        // Pause the timer BEFORE showing the panel
        PauseTimer();
        
        // Show the panel
        fishCaughtPanel.SetActive(true);
        isPanelVisible = true;
        
        // Update UI elements
        if (fishImage != null) 
        {
            fishImage.sprite = fish.fishSprite;
            Debug.Log($"Setting fish image: {(fish.fishSprite != null ? "Sprite found" : "Sprite missing")}");
        }
        if (fishNameText != null) 
        {
            fishNameText.text = fish.fishName;
            Debug.Log($"Setting fish name: {fish.fishName}");
        }
        if (scientificNameText != null) 
        {
            scientificNameText.text = fish.scientificName;
            Debug.Log($"Setting fish scientific name: {fish.scientificName}");
        }
        if (fishDescriptionText != null) 
        {
            fishDescriptionText.text = fish.description;
            Debug.Log($"Setting fish description: {fish.description}");
        }
        if (fishValueText != null) 
        {
            float multiplier = fish.IsEndangeredSpecies() && EconomyManager.Instance != null
                ? EconomyManager.Instance.endangeredPremiumMultiplier
                : 1f;

            if (multiplier <= 0f)
            {
                multiplier = 1f;
            }

            float displayValue = fish.GetSuggestedSaleValue(multiplier);
            string warningSuffix = fish.IsEndangeredSpecies() ? " (Endangered payout)" : string.Empty;
            fishValueText.text = $"Value: {displayValue:0} coins{warningSuffix}";
            Debug.Log($"Setting fish value: {displayValue} (multiplier {multiplier})");
        }

        Debug.Log("Fish caught panel is now visible. Timer should be paused.");
    }

    private void OnCloseButtonClicked()
    {
        Debug.Log("Close button clicked - hiding panel and resuming timer.");
        HidePanel();
    }

    private void InitializePanel()
    {
        // Initialize panel state without affecting timer
        // Only hide if it's not currently being shown
        if (fishCaughtPanel != null && !isPanelVisible)
        {
            fishCaughtPanel.SetActive(false);
            isPanelVisible = false;
            Debug.Log("Fish caught panel initialized as hidden.");
        }
    }

    private void FindFishingStats()
    {
        // Find SimpleFishingStats if not assigned in inspector
        if (fishingStats == null)
        {
            // Try to find it (including inactive GameObjects)
            fishingStats = FindObjectOfType<SimpleFishingStats>(true);
            if (fishingStats == null)
            {
                // Try one more time without the inactive flag
                fishingStats = FindObjectOfType<SimpleFishingStats>();
            }
            
            if (fishingStats == null)
            {
                Debug.LogWarning("FishCaughtUI: SimpleFishingStats not found! Timer pause/resume will not work. " +
                    "Make sure SimpleFishingStats component exists in the scene, or assign it in the Inspector.");
            }
            else
            {
                Debug.Log("FishCaughtUI: Found SimpleFishingStats automatically.");
            }
        }
    }

    public void HidePanel()
    {
        // Only hide and resume if the panel is actually visible
        if (fishCaughtPanel != null && isPanelVisible)
        {
            fishCaughtPanel.SetActive(false);
            isPanelVisible = false;
            Debug.Log("Fish caught panel hidden");
            
            // Resume the timer AFTER hiding the panel
            ResumeTimer();
        }
        // Don't resume timer if panel wasn't visible - it was never paused
    }

    private void PauseTimer()
    {
        // Try to find it if null
        if (fishingStats == null)
        {
            FindFishingStats();
        }
        
        if (fishingStats != null)
        {
            fishingStats.PauseTimer();
            Debug.Log("FishCaughtUI: Timer paused.");
        }
        else
        {
            Debug.LogWarning("FishCaughtUI: Cannot pause timer - SimpleFishingStats reference is null! " +
                "The panel will still show, but the timer will continue running.");
        }
    }

    private void ResumeTimer()
    {
        // Try to find it if null
        if (fishingStats == null)
        {
            FindFishingStats();
        }
        
        if (fishingStats != null)
        {
            fishingStats.ResumeTimer();
            Debug.Log("FishCaughtUI: Timer resumed.");
        }
        else
        {
            Debug.LogWarning("FishCaughtUI: Cannot resume timer - SimpleFishingStats reference is null!");
        }
    }

    private void OnDestroy()
    {
        // Ensure timer is resumed if panel is destroyed while visible
        if (isPanelVisible)
        {
            ResumeTimer();
        }
    }
} 