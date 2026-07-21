using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MainMenuButtonsUIScript : MonoBehaviour
{
    [Header("UI References")]
    public GameObject newGameConfirmationPanel;
    public TMP_Text confirmationText;
    public Button confirmButton;
    public Button cancelButton;
    
    [Header("Confirmation Messages")]
    public string newGameMessage = "Starting a new game will erase all progress. Are you sure?";
    public string existingSaveMessage = "Starting a new game will overwrite existing save data. Are you sure?";

    void Start()
    {
        if (newGameConfirmationPanel != null)
        {
            newGameConfirmationPanel.SetActive(false);
        }
        
        if (confirmButton != null)
        {
            confirmButton.onClick.RemoveAllListeners();
            confirmButton.onClick.AddListener(ConfirmNewGame);
        }
        
        if (cancelButton != null)
        {
            cancelButton.onClick.RemoveAllListeners();
            cancelButton.onClick.AddListener(CancelNewGame);
        }
    }

    public void NewGame()
    {
        ShowNewGameConfirmation();
    }

    public void ContinueGame()
    {
        Debug.Log("ContinueGame button clicked");
        
        // Check if we have save data
        bool hasSaveData = DataResetManager.Instance != null && 
                          DataResetManager.Instance.HasExistingSaveData();
        
        if (!hasSaveData)
        {
            Debug.Log("No save data found, showing new game confirmation...");
            ShowNewGameConfirmation();
            return;
        }

        // Clear spawn data
        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.ClearSpawnData();
        }

        // Try to load saved location
        if (LocationManager.Instance != null && LocationManager.Instance.HasSaveData())
        {
            LocationSaveData saveData = LocationManager.Instance.GetSaveData();
            string savedScene = saveData.sceneName;

            if (!string.IsNullOrEmpty(savedScene))
            {
                LevelManager.Instance.LoadScene(savedScene, "CrossFade");

                if (savedScene.Contains("Riverside") || savedScene.Contains("Auditorium"))
                {
                    if (MusicManager.Instance != null)
                    {
                        MusicManager.Instance.PlayMusic("RiversideBGM");
                    }
                }
                else if (savedScene.Contains("Credit"))
                {
                    if (MusicManager.Instance != null)
                    {
                        MusicManager.Instance.PlayMusic("CreditsBGM");
                    }
                }

                Debug.Log($"Loaded saved scene: {savedScene}");
                return;
            }
        }

        // Fallback: Load default scene
        Debug.Log("Loading default starting scene...");
        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.LoadScene("Auditorium", "CrossFade");
            if (MusicManager.Instance != null)
            {
                MusicManager.Instance.PlayMusic("RiversideBGM");
            }
        }
    }

    private void ShowNewGameConfirmation()
    {
        bool hasSaveData = false;
        
        if (DataResetManager.Instance != null)
        {
            hasSaveData = DataResetManager.Instance.HasExistingSaveData();
        }
        
        if (confirmationText != null)
        {
            confirmationText.text = hasSaveData ? existingSaveMessage : newGameMessage;
        }
        
        if (newGameConfirmationPanel != null)
        {
            newGameConfirmationPanel.SetActive(true);
            Debug.Log("Showing new game confirmation panel");
        }
        else
        {
            Debug.LogWarning("newGameConfirmationPanel is null! Skipping confirmation.");
            ConfirmNewGame();
        }
    }

    private void ConfirmNewGame()
    {
        Debug.Log("ConfirmNewGame called - Starting new game");
        
        if (newGameConfirmationPanel != null)
        {
            newGameConfirmationPanel.SetActive(false);
        }

        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.ClearSpawnData();
        }

        if (DataResetManager.Instance != null)
        {
            Debug.Log("Starting new game with complete reset...");
            DataResetManager.Instance.ResetAllGameData();
        }
        else
        {
            Debug.LogError("DataResetManager.Instance is null!");
            // Try to load starting scene anyway
            if (LevelManager.Instance != null)
            {
                LevelManager.Instance.LoadScene("Auditorium", "CrossFade");
                if (MusicManager.Instance != null)
                {
                    MusicManager.Instance.PlayMusic("RiversideBGM");
                }
            }
        }
    }

    private void CancelNewGame()
    {
        Debug.Log("CancelNewGame called");
        
        if (newGameConfirmationPanel != null)
        {
            newGameConfirmationPanel.SetActive(false);
        }
    }

    public void Credit()
    {
        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.LoadScene("CreditScene", "CrossFade");
            if (MusicManager.Instance != null)
            {
                MusicManager.Instance.PlayMusic("CreditsBGM");
            }
        }
        else
        {
            Debug.LogWarning("LevelManager not found!");
        }
    }

    public void Quit()
    {
        Debug.Log("Quit game requested");
        Application.Quit();
    }

    public void DiscordLink()
    {
        Application.OpenURL("https://discord.gg/ngQxxTwBxv");
    }
}