using UnityEngine;
using UnityEngine.SceneManagement;

public class DeboningSceneController : MonoBehaviour
{
    public static DeboningSceneController Instance { get; private set; }

    [Header("Scene Names")]
    public string deboningSceneName = "DeboningScene";
    public string mainMenuSceneName = "DeboningMainScene"; // Your main menu scene name

    public enum SceneType
    {
        MainMenu,
        FishLibrary,
        EndangeredFishLibrary,
        DeboningGame
    }

    private SceneType currentScene;
    private FishDefinition pendingFish;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        // Only set to MainMenu if we're actually in the main menu scene
        string currentSceneName = SceneManager.GetActiveScene().name;
        if (currentSceneName == mainMenuSceneName || string.IsNullOrEmpty(currentSceneName))
        {
            currentScene = SceneType.MainMenu;
        }
        
        // If we're in the deboning scene and have a pending fish, start it
        if (currentSceneName == deboningSceneName && pendingFish != null)
        {
            // Wait one frame to ensure GameManager is initialized
            StartCoroutine(BeginDeboningDelayed());
        }
    }

    public void ShowMainMenu()
    {
        currentScene = SceneType.MainMenu;
        // Only load the scene if we're NOT already in it
        if (!string.IsNullOrEmpty(mainMenuSceneName))
        {
            string currentSceneName = SceneManager.GetActiveScene().name;
            if (currentSceneName != mainMenuSceneName)
            {
                SceneManager.LoadScene(mainMenuSceneName);
            }
            // If we're already in MainScene, don't reload it (just update state)
        }
    }

    public void ShowFishLibrary()
    {
        currentScene = SceneType.FishLibrary;
    }

    public void ShowEndangeredFishLibrary()
    {
        currentScene = SceneType.EndangeredFishLibrary;
    }

    public void StartDeboningGame(FishDefinition fish)
    {
        if (fish == null)
        {
            Debug.LogError("DeboningSceneController: Cannot start game with null fish!");
            return;
        }

        pendingFish = fish;
        currentScene = SceneType.DeboningGame;

        // Check if we're already in the deboning scene
        if (SceneManager.GetActiveScene().name == deboningSceneName)
        {
            BeginDeboning();
        }
        else
        {
            // Load the deboning scene
            SceneManager.sceneLoaded += HandleSceneLoaded;
            SceneManager.LoadScene(deboningSceneName);
        }
    }

    private void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == deboningSceneName)
        {
            SceneManager.sceneLoaded -= HandleSceneLoaded;
            // Wait one frame to ensure all objects are initialized
            StartCoroutine(BeginDeboningDelayed());
        }
    }

    private System.Collections.IEnumerator BeginDeboningDelayed()
    {
        // Wait one frame for all Awake/Start methods to complete
        yield return null;
        BeginDeboning();
    }

    private void BeginDeboning()
    {
        if (GameManager.Instance == null)
        {
            Debug.LogError("DeboningSceneController: GameManager not found in deboning scene! Make sure GameManager exists in the scene.");
            return;
        }

        if (pendingFish == null)
        {
            Debug.LogWarning("DeboningSceneController: No pending fish to start. Make sure fish is selected from library.");
            return;
        }

        Debug.Log($"Starting deboning game with: {pendingFish.displayName}");
        GameManager.Instance.StartGameWithFish(pendingFish);
    }

    public SceneType GetCurrentScene()
    {
        return currentScene;
    }
}
