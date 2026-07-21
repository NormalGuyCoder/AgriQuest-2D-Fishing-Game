// 11/4/2025 AI-Tag
// This was created with the help of Assistant, a Unity Artificial Intelligence product.

using UnityEngine;
using UnityEngine.UI;

public class CloseCurrentPanelButton : MonoBehaviour
{
    [SerializeField]
    private GameObject currentPanel;

    private void Start()
    {
        // Get the Button component attached to this GameObject
        Button button = GetComponent<Button>();
        if (button != null)
        {
            // Add the listener to the button's onClick event
            button.onClick.AddListener(CloseCurrentPanel);
        }
        else
        {
            Debug.LogWarning("Button component not found on the GameObject.");
        }
    }

    private void CloseCurrentPanel()
    {
        if (currentPanel != null)
        {
            // Deactivate the current panel
            currentPanel.SetActive(false);
        }
        else
        {
            Debug.LogWarning("Current panel is not assigned.");
        }
    }
}