using UnityEngine;

public class MenuUIManager : MonoBehaviour
{
    [Header("UI References")]
    public GameObject menuCanvas;

    [Header("Settings")]
    public KeyCode toggleKey = KeyCode.Escape;
    public bool startWithMenuHidden = true;

    void Start()
    {
        // Initialize menu state
        if (menuCanvas != null && startWithMenuHidden)
        {
            menuCanvas.SetActive(false);
        }
    }

    void Update()
    {
        // Toggle menu with ESC key
        if (Input.GetKeyDown(toggleKey))
        {
            ToggleMenu();
        }
    }

    public void ToggleMenu()
    {
        if (menuCanvas != null)
        {
            bool newState = !menuCanvas.activeSelf;
            menuCanvas.SetActive(newState);
            Debug.Log($"Menu {(newState ? "opened" : "closed")}");
        }
    }

    // Public methods for UI buttons to call
    public void OpenMenu()
    {
        if (menuCanvas != null)
        {
            menuCanvas.SetActive(true);
        }
    }

    public void CloseMenu()
    {
        if (menuCanvas != null)
        {
            menuCanvas.SetActive(false);
        }
    }
}