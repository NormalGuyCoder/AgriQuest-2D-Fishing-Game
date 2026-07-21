using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class SceneTitleManager : MonoBehaviour
{
    public static SceneTitleManager Instance;

    [System.Serializable]
    public class SceneTitleEntry
    {
        public string sceneName;
        public string displayName;
        public float displayDuration = 6f;
    }

    [Header("UI Prefab")]
    public GameObject titleDisplayPrefab;

    [Header("Scene Titles")]
    public List<SceneTitleEntry> sceneTitles = new List<SceneTitleEntry>();

    [Header("Display Settings")]
    [Tooltip("Time to fade in")]
    [Range(0.5f, 2f)]
    public float fadeInTime = 1f;
    
    [Tooltip("Time to fade out")]
    [Range(0.5f, 2f)]
    public float fadeOutTime = 1f;
    
    [Tooltip("Default title duration (visible time)")]
    [Range(3f, 10f)]
    public float defaultTitleDuration = 6f;
    
    [Tooltip("Title text color")]
    public Color titleColor = Color.white;

    [Header("Background Settings")]
    [Tooltip("Padding around text for background")]
    public float backgroundPadding = 50f;
    
    [Tooltip("Background color")]
    public Color backgroundColor = new Color(0f, 0f, 0f, 0.7f);

    [Header("Position Settings")]
    [Tooltip("Position offset from screen center")]
    public Vector2 titlePosition = new Vector2(0f, 200f);
    
    [Tooltip("Use custom position instead of screen center")]
    public bool useCustomPosition = false;

    [Header("Debug")]
    public bool showDebugLogs = false; // Turned off by default to reduce console spam

    private bool isInitialized = false;
    private Coroutine currentTitleCoroutine;
    private GameObject currentTitleDisplay;
    private bool isDisplayingTitle = false;
    private bool sceneAlreadyLoaded = false;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            
            // Make sure we're a root object
            transform.SetParent(null);
            DontDestroyOnLoad(gameObject);
            
            isInitialized = true;
            if (showDebugLogs) Debug.Log("SceneTitleManager: Instance created and set to persist");
            
            // Subscribe to scene loaded event
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            if (showDebugLogs) Debug.LogWarning("SceneTitleManager: Extra instance found, destroying");
            Destroy(gameObject);
            return;
        }
    }

    void OnEnable()
    {
        if (!isInitialized) return;
        
        // Subscribe to spawn event
        PlayerSpawner.OnSpawnComplete += OnPlayerSpawned;
        if (showDebugLogs) Debug.Log("SceneTitleManager: Subscribed to spawn event");
    }

    void OnDisable()
    {
        if (!isInitialized) return;
        
        // Unsubscribe to prevent memory leaks
        PlayerSpawner.OnSpawnComplete -= OnPlayerSpawned;
        SceneManager.sceneLoaded -= OnSceneLoaded;
        
        if (showDebugLogs) Debug.Log("SceneTitleManager: Unsubscribed from events");
    }

    void OnDestroy()
    {
        if (isInitialized)
        {
            PlayerSpawner.OnSpawnComplete -= OnPlayerSpawned;
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
    }

    void Start()
    {
        if (!isInitialized) return;
        
        SetupDefaultTitles();
        if (showDebugLogs) Debug.Log("SceneTitleManager: Ready");
    }

    void SetupDefaultTitles()
    {
        // Only setup if empty
        if (sceneTitles.Count > 0) return;
        
        // Add all your scenes with appropriate display names
        sceneTitles.Add(new SceneTitleEntry { sceneName = "Academy Hill", displayName = "DNSC Academy Hill", displayDuration = defaultTitleDuration });
        sceneTitles.Add(new SceneTitleEntry { sceneName = "Auditorium", displayName = "DNSC Auditorium", displayDuration = defaultTitleDuration });
        sceneTitles.Add(new SceneTitleEntry { sceneName = "Greenville", displayName = "Greenvale", displayDuration = defaultTitleDuration });
        sceneTitles.Add(new SceneTitleEntry { sceneName = "Greenvilledonefish", displayName = "Greenvale", displayDuration = defaultTitleDuration });
        sceneTitles.Add(new SceneTitleEntry { sceneName = "Saltyshore", displayName = "Saltyshore", displayDuration = defaultTitleDuration });
        sceneTitles.Add(new SceneTitleEntry { sceneName = "Soria's Store", displayName = "Local Store Galore", displayDuration = defaultTitleDuration });
        sceneTitles.Add(new SceneTitleEntry { sceneName = "Nash's Store", displayName = "Bait Master", displayDuration = defaultTitleDuration });
        sceneTitles.Add(new SceneTitleEntry { sceneName = "FishingSea", displayName = "Saltwater Fishing Minigame", displayDuration = defaultTitleDuration });
        sceneTitles.Add(new SceneTitleEntry { sceneName = "Freshwaterminigame", displayName = "Freshwater Fishing Minigame", displayDuration = defaultTitleDuration });
        sceneTitles.Add(new SceneTitleEntry { sceneName = "DeboningScene", displayName = "Deboning Minigame", displayDuration = defaultTitleDuration });
        
        // Optional: Keep any existing scenes not in your list
        // sceneTitles.Add(new SceneTitleEntry { sceneName = "Fresh Finds", displayName = "Fresh Finds", displayDuration = defaultTitleDuration });
        // sceneTitles.Add(new SceneTitleEntry { sceneName = "MainMenu", displayName = "Main Menu", displayDuration = defaultTitleDuration });
        
        if (showDebugLogs) Debug.Log($"SceneTitleManager: Setup {sceneTitles.Count} titles");
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (!isInitialized) return;
        
        if (showDebugLogs) Debug.Log($"SceneTitleManager: Scene '{scene.name}' loaded");
        
        // Reset the flag when a new scene loads
        sceneAlreadyLoaded = false;
        
        // Wait a moment for the scene to settle, then show title
        StartCoroutine(ShowTitleForSceneWithDelay(scene.name, 1.0f));
    }

    private void OnPlayerSpawned()
    {
        if (!isInitialized) return;
        
        if (showDebugLogs) Debug.Log("SceneTitleManager: Player spawned event received");
        
        // Only show title from spawn event if we haven't already shown it from scene load
        if (!sceneAlreadyLoaded)
        {
            sceneAlreadyLoaded = true;
            ShowTitleForScene(SceneManager.GetActiveScene().name);
        }
    }

    IEnumerator ShowTitleForSceneWithDelay(string sceneName, float delay)
    {
        yield return new WaitForSeconds(delay);
        
        // Only show if not already shown from spawn event
        if (!sceneAlreadyLoaded)
        {
            sceneAlreadyLoaded = true;
            ShowTitleForScene(sceneName);
        }
    }

    public void ShowTitleForScene(string sceneName)
    {
        if (titleDisplayPrefab == null)
        {
            Debug.LogError("SceneTitleManager: No title display prefab assigned!");
            return;
        }

        // If already displaying a title, stop it and show the new one
        if (isDisplayingTitle)
        {
            if (showDebugLogs) Debug.Log("SceneTitleManager: Stopping current title to show new one");
            ForceStopCurrentTitle();
        }

        // Find title for this scene
        SceneTitleEntry title = null;
        foreach (var entry in sceneTitles)
        {
            if (entry.sceneName == sceneName)
            {
                title = entry;
                break;
            }
        }

        if (title == null)
        {
            // Create a default title for unknown scenes
            title = new SceneTitleEntry
            {
                sceneName = sceneName,
                displayName = sceneName,
                displayDuration = defaultTitleDuration
            };
            if (showDebugLogs) Debug.Log($"SceneTitleManager: Created default title for '{sceneName}'");
        }

        // Start the display coroutine
        if (currentTitleCoroutine != null)
        {
            StopCoroutine(currentTitleCoroutine);
        }
        
        currentTitleCoroutine = StartCoroutine(DisplayTitle(title));
    }

    IEnumerator DisplayTitle(SceneTitleEntry title)
    {
        isDisplayingTitle = true;
        
        if (showDebugLogs) Debug.Log($"SceneTitleManager: Displaying title for '{title.displayName}' for {title.displayDuration} seconds");

        // Find canvas
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            Debug.LogError("SceneTitleManager: No Canvas found in scene!");
            isDisplayingTitle = false;
            yield break;
        }

        // Create display
        GameObject display = Instantiate(titleDisplayPrefab, canvas.transform);
        currentTitleDisplay = display;
        
        // Get RectTransform and set position
        RectTransform rectTransform = display.GetComponent<RectTransform>();
        if (rectTransform != null)
        {
            rectTransform.localScale = Vector3.one;
            
            if (useCustomPosition)
            {
                // Use custom position from inspector
                rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
                rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
                rectTransform.pivot = new Vector2(0.5f, 0.5f);
                rectTransform.anchoredPosition = titlePosition;
            }
            else
            {
                // Center at top of screen (default)
                rectTransform.anchorMin = new Vector2(0.5f, 1f);
                rectTransform.anchorMax = new Vector2(0.5f, 1f);
                rectTransform.pivot = new Vector2(0.5f, 1f);
                rectTransform.anchoredPosition = new Vector2(0f, -50f); // 50 pixels from top
            }
            
            // Keep original size - don't set sizeDelta to Vector2.zero
        }

        // Find and setup text component
        TextMeshProUGUI textComp = display.GetComponentInChildren<TextMeshProUGUI>();
        if (textComp != null)
        {
            textComp.text = title.displayName;
            textComp.color = titleColor;
            textComp.alignment = TMPro.TextAlignmentOptions.Center;
            
            if (showDebugLogs) Debug.Log($"SceneTitleManager: Set text to '{title.displayName}'");
            
            // Adjust background to fit text if needed
            AdjustBackgroundToText(display, textComp);
        }
        else
        {
            Debug.LogError("SceneTitleManager: No TextMeshProUGUI found in prefab!");
            Destroy(display);
            isDisplayingTitle = false;
            yield break;
        }

        // Setup CanvasGroup for fading
        CanvasGroup canvasGroup = display.GetComponent<CanvasGroup>();
        if (canvasGroup == null) 
        {
            canvasGroup = display.AddComponent<CanvasGroup>();
        }
        canvasGroup.alpha = 0f;

        // Fade in
        float timer = 0f;
        while (timer < fadeInTime)
        {
            if (display == null || canvasGroup == null)
            {
                isDisplayingTitle = false;
                yield break;
            }
            
            timer += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(0f, 1f, timer / fadeInTime);
            yield return null;
        }
        
        if (display != null && canvasGroup != null)
        {
            canvasGroup.alpha = 1f;
        }

        if (showDebugLogs) Debug.Log($"SceneTitleManager: Title fully visible, waiting {title.displayDuration} seconds");

        // Wait for display duration
        float waitTimer = 0f;
        while (waitTimer < title.displayDuration)
        {
            if (display == null || canvasGroup == null)
            {
                isDisplayingTitle = false;
                yield break;
            }
            
            waitTimer += Time.deltaTime;
            yield return null;
        }

        // Fade out
        timer = 0f;
        while (timer < fadeOutTime)
        {
            if (display == null || canvasGroup == null)
            {
                isDisplayingTitle = false;
                yield break;
            }
            
            timer += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(1f, 0f, timer / fadeOutTime);
            yield return null;
        }

        // Ensure alpha is 0
        if (display != null && canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
        }
        
        // Wait one frame before destroying
        yield return null;
        
        // Destroy the display
        if (display != null)
        {
            Destroy(display);
        }
        
        if (currentTitleDisplay == display)
        {
            currentTitleDisplay = null;
        }
        
        if (showDebugLogs) Debug.Log("SceneTitleManager: Title faded out and destroyed");
        
        isDisplayingTitle = false;
        currentTitleCoroutine = null;
    }

    // Method to adjust background size to fit text
    void AdjustBackgroundToText(GameObject display, TextMeshProUGUI textComp)
    {
        // Find BackgroundImage
        Transform backgroundTransform = display.transform.Find("BackgroundImage");
        if (backgroundTransform == null)
        {
            // Try to find it by component
            UnityEngine.UI.Image backgroundImage = display.GetComponentInChildren<UnityEngine.UI.Image>();
            if (backgroundImage != null)
            {
                backgroundTransform = backgroundImage.transform;
            }
        }
        
        if (backgroundTransform != null)
        {
            RectTransform backgroundRect = backgroundTransform.GetComponent<RectTransform>();
            if (backgroundRect != null)
            {
                // Set background color
                UnityEngine.UI.Image backgroundImage = backgroundTransform.GetComponent<UnityEngine.UI.Image>();
                if (backgroundImage != null)
                {
                    backgroundImage.color = backgroundColor;
                }
                
                // Calculate text width (wait one frame for text to render)
                StartCoroutine(AdjustBackgroundDelayed(textComp, backgroundRect));
            }
        }
    }
    
    IEnumerator AdjustBackgroundDelayed(TextMeshProUGUI textComp, RectTransform backgroundRect)
    {
        // Wait for one frame so text can render and calculate its bounds
        yield return null;
        
        // Get the preferred width of the text
        float textWidth = textComp.preferredWidth;
        
        // Add padding
        float newWidth = Mathf.Max(500f, textWidth + (backgroundPadding * 2)); // Minimum 500 width
        
        // Set the background width (keep original height)
        backgroundRect.sizeDelta = new Vector2(newWidth, backgroundRect.sizeDelta.y);
        
        // Center the background behind the text
        backgroundRect.anchoredPosition = Vector2.zero;
    }

    // Method for manual testing
    public void ShowCustomTitle(string text, float duration = -1)
    {
        SceneTitleEntry customTitle = new SceneTitleEntry
        {
            displayName = text,
            displayDuration = duration > 0 ? duration : defaultTitleDuration
        };
        
        ShowTitleForScene("custom");
    }

    public void ForceStopCurrentTitle()
    {
        if (currentTitleCoroutine != null)
        {
            StopCoroutine(currentTitleCoroutine);
            currentTitleCoroutine = null;
        }
        
        if (currentTitleDisplay != null)
        {
            Destroy(currentTitleDisplay);
            currentTitleDisplay = null;
        }
        
        isDisplayingTitle = false;
        if (showDebugLogs) Debug.Log("SceneTitleManager: Force stopped current title");
    }

    public void DebugCurrentState()
    {
        Debug.Log($"=== SceneTitleManager Debug ===");
        Debug.Log($"Instance: {(Instance != null ? "Exists" : "NULL")}");
        Debug.Log($"Prefab: {(titleDisplayPrefab != null ? "Assigned" : "NULL")}");
        Debug.Log($"Current Display: {(currentTitleDisplay != null ? "Active" : "None")}");
        Debug.Log($"Is Displaying: {isDisplayingTitle}");
        Debug.Log($"Scene Already Loaded Flag: {sceneAlreadyLoaded}");
        Debug.Log($"Duration Settings: {defaultTitleDuration}s visible, {fadeInTime}s fade in, {fadeOutTime}s fade out");
        Debug.Log($"Total Time: {fadeInTime + defaultTitleDuration + fadeOutTime}s");
        Debug.Log($"Position: {(useCustomPosition ? $"Custom: {titlePosition}" : "Default (top center)")}");
        Debug.Log($"Registered Titles: {sceneTitles.Count}");
        
        foreach (var title in sceneTitles)
        {
            Debug.Log($"  - '{title.sceneName}' -> '{title.displayName}' ({title.displayDuration}s)");
        }
    }
}