using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HUDController : MonoBehaviour
{
    [Header("Score Display")]
    public TextMeshProUGUI scoreText;
    
    [Header("Timer Display")]
    public TextMeshProUGUI timerText;
    
    [Header("Cleanliness Bar")]
    [Tooltip("Use Slider component (recommended) or Image component for the bar")]
    public Slider cleanlinessBarSlider; // Preferred: Slider component
    public Image cleanlinessBar; // Alternative: Image component (must be type "Filled")
    [Tooltip("Alternative: Use RectTransform width-based bar (always works)")]
    public RectTransform cleanlinessBarRect; // Fallback: RectTransform for width-based bar
    public float maxBarWidth = 200f; // Max width for RectTransform-based bar
    public TextMeshProUGUI cleanlinessText;
    
    [Header("Mistakes Display")]
    public TextMeshProUGUI mistakesText;
    
    [Header("Level Complete UI")]
    public GameObject levelCompletePanel;
    public TextMeshProUGUI levelCompleteTitle;
    public TextMeshProUGUI educationalInfoText;
    public TextMeshProUGUI timeBonusText;
    public Button nextLevelButton;
    public Button restartButton;
    
    [Header("Pause Menu")]
    public GameObject pauseMenu;
    
    [Header("Game Complete")]
    public GameObject gameCompletePanel;
    public TextMeshProUGUI finalScoreText;
    
    [Header("Time Up")]
    public GameObject timeUpPanel;
    
    [Header("Points Popup")]
    public GameObject pointsPopupPrefab;
    public Transform popupParent;

    [Header("Fish Opening Prompt")]
    public GameObject fishOpeningPromptPanel; // Panel showing "Click to open fish with knife"
    public TMPro.TextMeshProUGUI fishOpeningPromptText;

    [Header("Message Display")]
    public TMPro.TextMeshProUGUI messageText; // Temporary message display (e.g., "Fish opened!")
    public float defaultMessageDuration = 2f;

    void Start()
    {
        if (levelCompletePanel != null)
            levelCompletePanel.SetActive(false);
        if (pauseMenu != null)
            pauseMenu.SetActive(false);
        if (gameCompletePanel != null)
            gameCompletePanel.SetActive(false);
        if (timeUpPanel != null)
            timeUpPanel.SetActive(false);
        if (fishOpeningPromptPanel != null)
            fishOpeningPromptPanel.SetActive(false);
        if (messageText != null)
            messageText.gameObject.SetActive(false);
    }

    public void UpdateUI(int score, int cleanliness, int timeRemaining, int mistakes)
    {
        if (scoreText != null)
            scoreText.text = "Score: " + score.ToString();

        if (timerText != null)
        {
            int minutes = timeRemaining / 60;
            int seconds = timeRemaining % 60;
            timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
        }

        // Update cleanliness bar using multiple methods (whichever is assigned)
        float cleanlinessPercent = cleanliness / 100f;
        
        // Method 1: Slider component (recommended - easiest and most reliable)
        if (cleanlinessBarSlider != null)
        {
            cleanlinessBarSlider.value = cleanlinessPercent;
        }
        // Method 2: Image component with fillAmount
        else if (cleanlinessBar != null)
        {
            cleanlinessBar.fillAmount = cleanlinessPercent;
            
            // Debug log to help diagnose issues
            if (cleanlinessBar.type != Image.Type.Filled)
            {
                Debug.LogWarning("HUDController: CleanlinessBar Image type is not set to 'Filled'! Current type: " + cleanlinessBar.type + ". Please set it to 'Filled' in the Inspector, or use a Slider component instead.");
            }
        }
        // Method 3: RectTransform width-based (fallback - always works)
        else if (cleanlinessBarRect != null)
        {
            float currentWidth = maxBarWidth * cleanlinessPercent;
            cleanlinessBarRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, currentWidth);
        }
        else
        {
            Debug.LogWarning("HUDController: No cleanliness bar assigned! Please assign either a Slider, Image, or RectTransform to one of the cleanliness bar fields in the Inspector.");
        }

        if (cleanlinessText != null)
            cleanlinessText.text = "Cleanliness: " + cleanliness + "%";

        if (mistakesText != null)
            mistakesText.text = "Mistakes: " + mistakes;
    }

    public void ShowPointsPopup(Vector3 worldPosition, int points)
    {
        if (pointsPopupPrefab != null && popupParent != null)
        {
            GameObject popup = Instantiate(pointsPopupPrefab, popupParent);
            popup.transform.position = worldPosition;
            
            TextMeshProUGUI popupText = popup.GetComponentInChildren<TextMeshProUGUI>();
            if (popupText != null)
            {
                popupText.text = "+" + points;
            }

            // Start animation coroutine - it will handle destruction
            StartCoroutine(AnimatePopup(popup));
        }
    }

    private System.Collections.IEnumerator AnimatePopup(GameObject popup)
    {
        // Check if popup is valid before starting
        if (popup == null)
        {
            yield break;
        }

        Vector3 startPos = popup.transform.position;
        float elapsed = 0f;
        float duration = 1.5f;

        while (elapsed < duration)
        {
            // Check if popup was destroyed (can happen if scene changes)
            if (popup == null)
            {
                yield break;
            }

            elapsed += Time.deltaTime;
            
            // Check again before accessing transform
            if (popup != null)
            {
                popup.transform.position = startPos + Vector3.up * (elapsed * 50f);
                
                CanvasGroup canvasGroup = popup.GetComponent<CanvasGroup>();
                if (canvasGroup != null)
                {
                    canvasGroup.alpha = 1f - (elapsed / duration);
                }
            }
            
            yield return null;
        }

        // Destroy popup after animation completes
        if (popup != null)
        {
            Destroy(popup);
        }
    }

    public void ShowLevelComplete(FishDefinition fish, int timeBonus)
    {
        if (levelCompletePanel != null)
        {
            levelCompletePanel.SetActive(true);
            
            if (levelCompleteTitle != null)
                levelCompleteTitle.text = fish.displayName + " Complete!";
            
            if (educationalInfoText != null)
                educationalInfoText.text = fish.educationalInfo;
            
            if (timeBonusText != null)
                timeBonusText.text = "Time Bonus: +" + timeBonus + " points";
            
            // Next Level button now goes back to fish selection
            if (nextLevelButton != null)
            {
                // Update button text
                TextMeshProUGUI nextButtonText = nextLevelButton.GetComponentInChildren<TextMeshProUGUI>();
                if (nextButtonText != null)
                {
                    nextButtonText.text = "Select Another Fish";
                }
                
                nextLevelButton.onClick.RemoveAllListeners();
                nextLevelButton.onClick.AddListener(() => {
                    levelCompletePanel.SetActive(false);
                    // Return to main menu to select another fish
                    if (DeboningSceneController.Instance != null)
                    {
                        DeboningSceneController.Instance.ShowMainMenu();
                    }
                    else
                    {
                        // Fallback: reload main menu scene directly
                        UnityEngine.SceneManagement.SceneManager.LoadScene("MainScene");
                    }
                });
            }
            
            // Restart button restarts the current fish
            if (restartButton != null)
            {
                // Update button text
                TextMeshProUGUI restartButtonText = restartButton.GetComponentInChildren<TextMeshProUGUI>();
                if (restartButtonText != null)
                {
                    restartButtonText.text = "Restart";
                }
                
                restartButton.onClick.RemoveAllListeners();
                restartButton.onClick.AddListener(() => {
                    levelCompletePanel.SetActive(false);
                    if (GameManager.Instance != null)
                    {
                        GameManager.Instance.RestartLevel();
                    }
                });
            }
        }
    }

    public void ShowPauseMenu()
    {
        if (pauseMenu != null)
            pauseMenu.SetActive(true);
    }

    public void HidePauseMenu()
    {
        if (pauseMenu != null)
            pauseMenu.SetActive(false);
    }

    public void ShowGameComplete()
    {
        if (gameCompletePanel != null)
        {
            gameCompletePanel.SetActive(true);
            // Final score will be updated by GameManager
        }
    }

    public void ShowTimeUp()
    {
        if (timeUpPanel != null)
            timeUpPanel.SetActive(true);
    }

    public void ShowFishOpeningPrompt(bool show)
    {
        if (fishOpeningPromptPanel != null)
        {
            fishOpeningPromptPanel.SetActive(show);
        }

        if (fishOpeningPromptText != null && show)
        {
            fishOpeningPromptText.text = "Click the fish with the knife to open it first!";
        }
    }

    public void ShowMessage(string message, float duration = 0f)
    {
        if (messageText == null)
            return;

        if (duration <= 0f)
        {
            duration = defaultMessageDuration;
        }

        messageText.text = message;
        messageText.gameObject.SetActive(true);
        StartCoroutine(HideMessageAfterDelay(duration));
    }

    private System.Collections.IEnumerator HideMessageAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (messageText != null)
        {
            messageText.gameObject.SetActive(false);
        }
    }
}

