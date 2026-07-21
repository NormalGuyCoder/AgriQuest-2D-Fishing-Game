using UnityEngine;
using UnityEngine.UI;

public class MainMenuController : MonoBehaviour
{
    [Header("UI Panels")]
    public GameObject mainMenuPanel;
    public GameObject fishLibraryPanel;
    public GameObject endangeredLibraryPanel;
    public GameObject deboningGamePanel;
    public GameObject scorePanel;

    [Header("Main Menu Buttons")]
    public Button openFishLibraryButton;
    public Button openEndangeredLibraryButton;
    public Button exitButton;

    [Header("Title Elements")]
    public TMPro.TextMeshProUGUI titleText;
    public TMPro.TextMeshProUGUI subtitleText;

    private DeboningSceneController sceneController;

    void Start()
    {
        sceneController = DeboningSceneController.Instance;
        if (sceneController == null)
        {
            GameObject sceneControllerObj = new GameObject("DeboningSceneController");
            sceneController = sceneControllerObj.AddComponent<DeboningSceneController>();
        }

        // Set up buttons
        if (openFishLibraryButton != null)
        {
            openFishLibraryButton.onClick.AddListener(OnOpenFishLibrary);
        }

        if (openEndangeredLibraryButton != null)
        {
            openEndangeredLibraryButton.onClick.AddListener(OnOpenEndangeredLibrary);
        }

        if (exitButton != null)
        {
            exitButton.onClick.AddListener(OnExitGame);
        }

        // Show main menu by default
        ShowMainMenu();
    }

    public void ShowMainMenu()
    {
        SetActivePanel(mainMenuPanel);
        
        // Refresh score panel if it exists (in case we're returning from a game)
        if (scorePanel != null)
        {
            ScorePanelController scorePanelController = scorePanel.GetComponent<ScorePanelController>();
            if (scorePanelController != null)
            {
                scorePanelController.UpdateDisplay();
            }
        }
        
        // Don't call sceneController.ShowMainMenu() when we're already in MainScene
        // ShowMainMenu() is only for loading the scene when coming from another scene
        // Since we're using panels in the same scene, we just show/hide panels
    }

    public void OnOpenFishLibrary()
    {
        if (fishLibraryPanel != null)
        {
            SetActivePanel(fishLibraryPanel);
            FishLibraryController libraryController = fishLibraryPanel.GetComponent<FishLibraryController>();
            if (libraryController != null)
            {
                libraryController.RefreshLibrary();
            }
            if (sceneController != null)
            {
                sceneController.ShowFishLibrary();
            }
        }
    }

    public void OnOpenEndangeredLibrary()
    {
        if (endangeredLibraryPanel != null)
        {
            SetActivePanel(endangeredLibraryPanel);
            EndangeredFishLibraryController libraryController = endangeredLibraryPanel.GetComponent<EndangeredFishLibraryController>();
            if (libraryController != null)
            {
                libraryController.RefreshLibrary();
            }
            if (sceneController != null)
            {
                sceneController.ShowEndangeredFishLibrary();
            }
        }
    }

    public void OnBackToMainMenu()
    {
        ShowMainMenu();
    }

    public void OnExitGame()
    {
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }

    public void OnOpenScorePanel()
    {
        if (scorePanel != null)
        {
            SetActivePanel(scorePanel);
            ScorePanelController scorePanelController = scorePanel.GetComponent<ScorePanelController>();
            if (scorePanelController != null)
            {
                scorePanelController.RefreshScoreDisplay();
            }
            else
            {
                Debug.LogWarning("MainMenuController: ScorePanelController component not found on score panel! Make sure the ScorePanelController script is attached to the score panel GameObject.");
            }
        }
        else
        {
            Debug.LogError("MainMenuController: Score Panel is not assigned! Please assign the score panel GameObject in the Inspector.");
        }
    }

    private void SetActivePanel(GameObject activePanel)
    {
        if (mainMenuPanel != null) mainMenuPanel.SetActive(activePanel == mainMenuPanel);
        if (fishLibraryPanel != null) fishLibraryPanel.SetActive(activePanel == fishLibraryPanel);
        if (endangeredLibraryPanel != null) endangeredLibraryPanel.SetActive(activePanel == endangeredLibraryPanel);
        if (deboningGamePanel != null) deboningGamePanel.SetActive(activePanel == deboningGamePanel);
        if (scorePanel != null) scorePanel.SetActive(activePanel == scorePanel);
    }
}
