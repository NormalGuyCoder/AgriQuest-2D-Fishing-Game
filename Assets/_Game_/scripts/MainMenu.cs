using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class MainMenu : MonoBehaviour
{
    [Header("Menu Buttons")]
    [SerializeField] private Button startButton;
    [SerializeField] private Button optionsButton;
    [SerializeField] private Button quitButton;

    [Header("Menu Panels")]
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject optionsPanel;

    [Header("Options Settings")]
    [SerializeField] private Slider volumeSlider;
    [SerializeField] private Toggle fullscreenToggle;

    private void Start()
    {
        // Add listeners to buttons
        if (startButton != null)
            startButton.onClick.AddListener(StartGame);
        
        if (optionsButton != null)
            optionsButton.onClick.AddListener(OpenOptions);
        
        if (quitButton != null)
            quitButton.onClick.AddListener(QuitGame);

        // Initialize options panel as hidden
        if (optionsPanel != null)
            optionsPanel.SetActive(false);
        
        if (mainMenuPanel != null)
            mainMenuPanel.SetActive(true);

        // Initialize options settings
        LoadSettings();
    }

    public void StartGame()
    {
        SceneManager.LoadScene("Farmhouse"); // Load the farmhouse scene
    }

    public void OpenOptions()
    {
        if (mainMenuPanel != null)
            mainMenuPanel.SetActive(false);
        if (optionsPanel != null)
            optionsPanel.SetActive(true);
    }

    public void CloseOptions()
    {
        if (mainMenuPanel != null)
            mainMenuPanel.SetActive(true);
        if (optionsPanel != null)
            optionsPanel.SetActive(false);
        
        SaveSettings();
    }

    public void QuitGame()
    {
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }

    private void LoadSettings()
    {
        if (volumeSlider != null)
            volumeSlider.value = PlayerPrefs.GetFloat("GameVolume", 1f);
        
        if (fullscreenToggle != null)
            fullscreenToggle.isOn = PlayerPrefs.GetInt("Fullscreen", 1) == 1;
    }

    private void SaveSettings()
    {
        if (volumeSlider != null)
            PlayerPrefs.SetFloat("GameVolume", volumeSlider.value);
        
        if (fullscreenToggle != null)
        {
            PlayerPrefs.SetInt("Fullscreen", fullscreenToggle.isOn ? 1 : 0);
            Screen.fullScreen = fullscreenToggle.isOn;
        }
        
        PlayerPrefs.Save();
    }
} 