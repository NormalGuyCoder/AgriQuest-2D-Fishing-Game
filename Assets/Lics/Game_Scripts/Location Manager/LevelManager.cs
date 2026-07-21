using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Linq;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance;

    [Header("Scene Loading")]
    public Slider progressBar;
    public GameObject transitionsContainer;

    private string nextSceneSpawnPoint = "default";
    private SceneTransition[] transitions;

    // Track if we're waiting for spawn
    private bool waitingForSpawn = false;
    private float maxWaitTime = 2f; // Maximum time to wait for spawn

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            SceneManager.sceneLoaded += OnSceneLoaded;

            // Subscribe to spawn complete event
            PlayerSpawner.OnSpawnComplete += OnPlayerSpawnComplete;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        PlayerSpawner.OnSpawnComplete -= OnPlayerSpawnComplete;
    }

    void Start()
    {
        if (transitionsContainer != null)
        {
            transitions = transitionsContainer.GetComponentsInChildren<SceneTransition>();
        }
    }

    public void LoadScene(string sceneName, string transitionName, string spawnPoint = "default")
    {
        nextSceneSpawnPoint = spawnPoint;
        Debug.Log($"LevelManager: Loading '{sceneName}' with spawn point '{spawnPoint}'");

        // Save current scene and position before loading
        SaveBeforeSceneChange(sceneName);

        StartCoroutine(LoadSceneAsync(sceneName, transitionName));
    }

    public string GetSpawnPoint()
    {
        return nextSceneSpawnPoint;
    }

    public void ClearSpawnData()
    {
        nextSceneSpawnPoint = "default";
        Debug.Log("LevelManager: Spawn data cleared");
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log($"LevelManager: Scene '{scene.name}' loaded, spawn point: '{nextSceneSpawnPoint}'");

        // Don't automatically reset spawn point - let PlayerSpawner use it first
        // It will be cleared by PlayerSpawner after spawning
    }

    void OnPlayerSpawnComplete()
    {
        Debug.Log($"[TIMING] LevelManager: Player spawn complete received");
        waitingForSpawn = false;
    }

    IEnumerator LoadSceneAsync(string sceneName, string transitionName)
    {
        Debug.Log($"[TIMING] LevelManager.LoadSceneAsync START");

        SceneTransition transition = null;

        if (transitions != null)
        {
            transition = transitions.FirstOrDefault(t => t.name == transitionName);
        }

        // Start transition IN
        if (transition != null)
        {
            yield return transition.AnimateTransitionIn();
        }

        // Show loading bar
        if (progressBar != null)
        {
            progressBar.gameObject.SetActive(true);
        }

        // Load scene asynchronously
        AsyncOperation scene = SceneManager.LoadSceneAsync(sceneName);
        scene.allowSceneActivation = false;

        // Wait for scene to load
        while (scene.progress < 0.9f)
        {
            if (progressBar != null)
            {
                progressBar.value = scene.progress;
            }
            yield return null;
        }

        // Scene is loaded, allow activation
        scene.allowSceneActivation = true;

        // IMPORTANT: Wait for scene to be fully active
        yield return null;

        // Now wait for PlayerSpawner to complete
        waitingForSpawn = true;
        float waitStartTime = Time.time;

        Debug.Log($"[TIMING] LevelManager: Waiting for player spawn...");

        // Wait for spawn complete or timeout
        while (waitingForSpawn && Time.time - waitStartTime < maxWaitTime)
        {
            yield return null;
        }

        if (waitingForSpawn)
        {
            Debug.LogWarning($"[TIMING] LevelManager: Timeout waiting for player spawn after {maxWaitTime}s");
            waitingForSpawn = false;
        }
        else
        {
            Debug.Log($"[TIMING] LevelManager: Player spawn confirmed, continuing...");
        }

        // Hide loading bar
        if (progressBar != null)
        {
            progressBar.gameObject.SetActive(false);
        }

        // Transition OUT
        if (transition != null)
        {
            yield return transition.AnimateTransitionOut();
        }

        Debug.Log($"[TIMING] LevelManager.LoadSceneAsync END");
    }

    // Helper method for debugging
    public void DebugSpawnInfo()
    {
        Debug.Log($"LevelManager Debug - Current spawn point: '{nextSceneSpawnPoint}'");
    }

    // Add this method to LevelManager.cs
    private void SaveBeforeSceneChange(string nextScene)
    {
        if (LocationManager.Instance != null)
        {
            // Try to get the player before we unload the scene
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                LocationManager.Instance.SaveGame(
                    UnityEngine.SceneManagement.SceneManager.GetActiveScene().name,
                    player.transform.position
                );
            }
            else
            {
                Debug.LogWarning("Could not save player position - player not found");
            }
        }
        else
        {
            Debug.LogWarning("Location Manager not available for auto-save");
        }
    }
}