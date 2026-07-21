using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// A flexible back button script that can navigate to any selected scene.
/// Attach this to a Button GameObject to enable scene navigation.
/// </summary>
public class BackButton : MonoBehaviour
{
    [Header("Target Scene")]
    [Tooltip("The name of the scene to load when the back button is pressed")]
    public string targetSceneName = "";

    [Header("Options")]
    [Tooltip("Show a confirmation dialog before leaving the scene")]
    public bool showConfirmationDialog = false;
    
    [Tooltip("Optional: Assign a prefab for confirmation dialog")]
    public GameObject confirmationDialogPrefab;

    [Header("Advanced")]
    [Tooltip("If true, will try to use DeboningSceneController.ShowMainMenu() if target scene matches main menu")]
    public bool useSceneController = true;

    private Button button;

    void Start()
    {
        button = GetComponent<Button>();
        if (button != null)
        {
            button.onClick.AddListener(OnBackClicked);
        }
        else
        {
            Debug.LogWarning("BackButton: Button component not found on this GameObject. Please attach this script to a GameObject with a Button component.");
        }
    }

    /// <summary>
    /// Called when the back button is clicked
    /// </summary>
    private void OnBackClicked()
    {
        if (string.IsNullOrEmpty(targetSceneName))
        {
            Debug.LogError("BackButton: Target scene name is not set! Please assign a scene name in the Inspector.");
            return;
        }

        if (showConfirmationDialog && confirmationDialogPrefab != null)
        {
            ShowConfirmationDialog();
        }
        else
        {
            // Go back directly without confirmation
            LoadTargetScene();
        }
    }

    /// <summary>
    /// Shows a confirmation dialog before leaving the scene
    /// </summary>
    private void ShowConfirmationDialog()
    {
        if (confirmationDialogPrefab == null)
        {
            Debug.LogWarning("BackButton: Confirmation dialog prefab is not assigned! Loading scene directly.");
            LoadTargetScene();
            return;
        }

        // Find canvas or use current transform's root
        Transform parent = transform.root;
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas != null)
        {
            parent = canvas.transform;
        }

        // Instantiate the confirmation dialog
        GameObject dialogInstance = Instantiate(confirmationDialogPrefab, parent);

        // Try to find Yes and No buttons (handle different possible names)
        Button yesButton = FindButtonInChildren(dialogInstance, "YesButton", "ConfirmButton", "OKButton");
        Button noButton = FindButtonInChildren(dialogInstance, "NoButton", "CancelButton", "CloseButton");

        if (yesButton != null)
        {
            yesButton.onClick.AddListener(() =>
            {
                LoadTargetScene();
                if (dialogInstance != null)
                    Destroy(dialogInstance);
            });
        }
        else
        {
            Debug.LogWarning("BackButton: Could not find Yes/Confirm button in dialog!");
        }

        if (noButton != null)
        {
            noButton.onClick.AddListener(() =>
            {
                if (dialogInstance != null)
                    Destroy(dialogInstance);
            });
        }
    }

    /// <summary>
    /// Finds a button component in children by checking multiple possible names
    /// </summary>
    private Button FindButtonInChildren(GameObject parent, params string[] possibleNames)
    {
        foreach (string name in possibleNames)
        {
            Transform found = parent.transform.Find(name);
            if (found != null)
            {
                Button button = found.GetComponent<Button>();
                if (button != null)
                    return button;
            }
        }
        return null;
    }

    /// <summary>
    /// Loads the target scene specified in the Inspector
    /// </summary>
    private void LoadTargetScene()
    {
        // Try to use DeboningSceneController if available and target is main menu
        if (useSceneController && DeboningSceneController.Instance != null)
        {
            string currentSceneName = SceneManager.GetActiveScene().name;
            DeboningSceneController controller = DeboningSceneController.Instance;
            
            // Check if target scene matches the main menu scene name
            if (targetSceneName == controller.mainMenuSceneName)
            {
                controller.ShowMainMenu();
                return;
            }
        }

        // Fallback: Load scene directly
        try
        {
            Debug.Log($"BackButton: Loading scene '{targetSceneName}'");
            SceneManager.LoadScene(targetSceneName);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"BackButton: Failed to load scene '{targetSceneName}'. Error: {e.Message}");
            Debug.LogError("BackButton: Please ensure the scene is added to Build Settings and the name is correct!");
        }
    }

    /// <summary>
    /// Public method to programmatically set the target scene
    /// </summary>
    public void SetTargetScene(string sceneName)
    {
        targetSceneName = sceneName;
    }

    /// <summary>
    /// Public method to programmatically trigger the back action
    /// </summary>
    public void GoBack()
    {
        OnBackClicked();
    }
}






