using UnityEngine;

public class SimpleTitleTester : MonoBehaviour
{
    [Header("Test Settings")]
    public string testSceneName = "Greenville";
    public bool showTitleOnStart = false;
    
    [Header("Debug Controls")]
    [SerializeField] private bool showAcademyHill = false;
    [SerializeField] private bool showAuditorium = false;
    [SerializeField] private bool showFreshFinds = false;
    [SerializeField] private bool showGreenville = false;
    [SerializeField] private bool showGreenville2 = false;
    [SerializeField] private bool showMainMenu = false;
    [SerializeField] private bool showNashStore = false;
    [SerializeField] private bool showSaltshore = false;
    [SerializeField] private bool showSoriaStore = false;
    [SerializeField] private bool showCustomTitle = false;
    [SerializeField] private bool debugState = false;
    [SerializeField] private bool forceStop = false;
    [SerializeField] private bool testPosition = false;
    
    [Header("Custom Title")]
    public string customTitle = "Test Scene";
    public float customDuration = 6f;

    void Start()
    {
        if (showTitleOnStart && SceneTitleManager.Instance != null)
        {
            SceneTitleManager.Instance.ShowTitleForScene(testSceneName);
        }
    }

    void Update()
    {
        // Test with keyboard keys
        if (Input.GetKeyDown(KeyCode.F1))
        {
            ShowTestTitle("Academy Hill");
        }
        if (Input.GetKeyDown(KeyCode.F2))
        {
            ShowTestTitle("Auditorium");
        }
        if (Input.GetKeyDown(KeyCode.F3))
        {
            ShowTestTitle("Fresh Finds");
        }
        if (Input.GetKeyDown(KeyCode.F4))
        {
            ShowTestTitle("Greenville");
        }
        if (Input.GetKeyDown(KeyCode.F5))
        {
            ShowTestTitle("Greenville...");
        }
        if (Input.GetKeyDown(KeyCode.F6))
        {
            ShowTestTitle("MainMenu");
        }
        if (Input.GetKeyDown(KeyCode.F7))
        {
            ShowTestTitle("Nash's Store");
        }
        if (Input.GetKeyDown(KeyCode.F8))
        {
            ShowTestTitle("Saltshore");
        }
        if (Input.GetKeyDown(KeyCode.F9))
        {
            ShowTestTitle("Soria's Store");
        }
        if (Input.GetKeyDown(KeyCode.F10))
        {
            ShowCustomTitle();
        }
        if (Input.GetKeyDown(KeyCode.F11) && SceneTitleManager.Instance != null)
        {
            SceneTitleManager.Instance.DebugCurrentState();
        }
        if (Input.GetKeyDown(KeyCode.F12) && SceneTitleManager.Instance != null)
        {
            SceneTitleManager.Instance.ForceStopCurrentTitle();
        }
        if (Input.GetKeyDown(KeyCode.P) && SceneTitleManager.Instance != null)
        {
            TestPosition();
        }
    }

    void OnValidate()
    {
        if (showAcademyHill)
        {
            showAcademyHill = false;
            ShowTestTitle("Academy Hill");
        }
        if (showAuditorium)
        {
            showAuditorium = false;
            ShowTestTitle("Auditorium");
        }
        if (showFreshFinds)
        {
            showFreshFinds = false;
            ShowTestTitle("Fresh Finds");
        }
        if (showGreenville)
        {
            showGreenville = false;
            ShowTestTitle("Greenville");
        }
        if (showGreenville2)
        {
            showGreenville2 = false;
            ShowTestTitle("Greenville...");
        }
        if (showMainMenu)
        {
            showMainMenu = false;
            ShowTestTitle("MainMenu");
        }
        if (showNashStore)
        {
            showNashStore = false;
            ShowTestTitle("Nash's Store");
        }
        if (showSaltshore)
        {
            showSaltshore = false;
            ShowTestTitle("Saltshore");
        }
        if (showSoriaStore)
        {
            showSoriaStore = false;
            ShowTestTitle("Soria's Store");
        }
        if (showCustomTitle)
        {
            showCustomTitle = false;
            ShowCustomTitle();
        }
        if (debugState)
        {
            debugState = false;
            DebugState();
        }
        if (forceStop)
        {
            forceStop = false;
            ForceStopTitle();
        }
        if (testPosition)
        {
            testPosition = false;
            TestPosition();
        }
    }

    void ShowTestTitle(string sceneName)
    {
        if (SceneTitleManager.Instance != null)
        {
            SceneTitleManager.Instance.ShowTitleForScene(sceneName);
            Debug.Log($"SimpleTitleTester: Showing title for: {sceneName}");
        }
        else
        {
            Debug.LogWarning("SimpleTitleTester: SceneTitleManager.Instance is null!");
        }
    }

    void ShowCustomTitle()
    {
        if (SceneTitleManager.Instance != null)
        {
            SceneTitleManager.Instance.ShowCustomTitle(customTitle, customDuration);
            Debug.Log($"SimpleTitleTester: Showing custom title: {customTitle} for {customDuration}s");
        }
        else
        {
            Debug.LogWarning("SimpleTitleTester: SceneTitleManager.Instance is null!");
        }
    }
    
    void DebugState()
    {
        if (SceneTitleManager.Instance != null)
        {
            SceneTitleManager.Instance.DebugCurrentState();
        }
        else
        {
            Debug.LogWarning("SimpleTitleTester: SceneTitleManager.Instance is null!");
        }
    }
    
    void ForceStopTitle()
    {
        if (SceneTitleManager.Instance != null)
        {
            SceneTitleManager.Instance.ForceStopCurrentTitle();
            Debug.Log("SimpleTitleTester: Force stopped current title");
        }
        else
        {
            Debug.LogWarning("SimpleTitleTester: SceneTitleManager.Instance is null!");
        }
    }
    
    void TestPosition()
    {
        if (SceneTitleManager.Instance != null)
        {
            // Test different positions
            SceneTitleManager.Instance.ShowCustomTitle("Position Test: Top", 3f);
            Debug.Log("SimpleTitleTester: Testing position - top of screen");
        }
    }
}