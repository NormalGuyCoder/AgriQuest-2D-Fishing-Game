using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class BackToLibraryButton : MonoBehaviour
{
    [Header("Options")]
    public bool showConfirmationDialog = false;
    public GameObject confirmationDialogPrefab; // Optional: Assign a prefab for confirmation dialog

    [Header("Scene Settings")]
    public string mainMenuSceneName = "DeboningMainScene"; // Fallback scene name if controller is null

    void Start()
    {
        Button button = GetComponent<Button>();
        if (button != null)
        {
            button.onClick.AddListener(OnBackClicked);
        }
    }

    private void OnBackClicked()
    {
        if (showConfirmationDialog && confirmationDialogPrefab != null)
        {
            ShowConfirmationDialog();
        }
        else
        {
            // Go back directly without confirmation
            ReturnToMainMenu();
        }
    }

    private void ShowConfirmationDialog()
    {
        if (confirmationDialogPrefab == null)
        {
            Debug.LogWarning("BackToLibraryButton: Confirmation dialog prefab is not assigned!");
            ReturnToMainMenu();
            return;
        }

        // Find canvas or use current transform's parent
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
                ReturnToMainMenu();
                if (dialogInstance != null)
                    Destroy(dialogInstance);
            });
        }
        else
        {
            Debug.LogWarning("BackToLibraryButton: Could not find Yes/Confirm button in dialog!");
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

    private void ReturnToMainMenu()
    {
        // Try to use DeboningSceneController first
        if (DeboningSceneController.Instance != null)
        {
            DeboningSceneController.Instance.ShowMainMenu();
            return;
        }

        // Fallback: Load main menu scene directly
        Debug.LogWarning("BackToLibraryButton: DeboningSceneController.Instance is null! Loading scene directly.");
        
        string sceneToLoad = mainMenuSceneName;
        
        // If mainMenuSceneName is empty, try default names
        if (string.IsNullOrEmpty(sceneToLoad))
        {
            sceneToLoad = "DeboningMainScene"; // Default fallback
        }

        try
        {
            SceneManager.LoadScene(sceneToLoad);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"BackToLibraryButton: Failed to load scene '{sceneToLoad}'. Error: {e.Message}");
            Debug.LogError("BackToLibraryButton: Please ensure the scene is added to Build Settings and the name is correct!");
        }
    }
}