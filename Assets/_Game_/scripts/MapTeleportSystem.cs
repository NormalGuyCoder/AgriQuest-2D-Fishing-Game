using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class MapTeleportSystem : MonoBehaviour
{
    [Header("Menu Setup")]
    [Tooltip("Drag your TeleportMenuWindow Panel here")]
    public GameObject teleportMenuPanel;

    [Tooltip("Key to open/close the teleport menu")]
    public KeyCode toggleMenuKey = KeyCode.T;

    [Header("Audio & Effects Setup")]
    [Tooltip("Sounds to play randomly right before teleporting")]
    public AudioClip[] teleportLeaveSounds;
    [Tooltip("Sounds to play randomly when arriving in a new scene")]
    public AudioClip[] teleportArriveSounds;
    
    [Tooltip("A CanvasGroup used for fading the screen. Drag an image panel here that covers the screen.")]
    public CanvasGroup fadeCanvasGroup;
    public float fadeDuration = 1f;

    private AudioSource audioSource;
    private bool isTeleporting = false;

    private void Start()
    {
        // Setup audio source automatically
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        // Make sure the menu starts hidden
        if (teleportMenuPanel != null)
        {
            teleportMenuPanel.SetActive(false);
        }

        // NEW: Connect to SettingsManager for SFX volume control
        if (SettingsManager.Instance != null)
        {
            UpdateSFXVolume(SettingsManager.Instance.SFXVolume);
            SettingsManager.Instance.OnSFXVolumeChanged += UpdateSFXVolume;
        }

        // Handle arrival fade-in and sound
        if (fadeCanvasGroup != null)
        {
            fadeCanvasGroup.alpha = 1f; // Start fully dark
            StartCoroutine(FadeIn());
        }

        // Play the arrival sound ONLY if the player just used the teleport feature
        if (PlayerPrefs.GetInt("JustTeleported", 0) == 1)
        {
            // Note: We don't reset the flag here anymore, so the PlayerSpawner can see it too.
            // It will be reset after the arrival sound finishes or in the PlayerSpawner.
            
            if (audioSource != null && teleportArriveSounds != null && teleportArriveSounds.Length > 0)
            {
                AudioClip randomArriveSound = teleportArriveSounds[Random.Range(0, teleportArriveSounds.Length)];
                if (randomArriveSound != null)
                {
                    audioSource.PlayOneShot(randomArriveSound);
                }
            }
        }
    }

    private void Update()
    {
        // Don't allow menu toggling if we are currently teleporting
        if (isTeleporting) return;

        // Check if the player pressed the map/teleport key
        if (Input.GetKeyDown(toggleMenuKey))
        {
            ToggleMenu();
        }
    }

    public void ToggleMenu()
    {
        if (teleportMenuPanel != null && !isTeleporting)
        {
            if (teleportMenuPanel == this.gameObject)
            {
                Debug.LogError("SETUP ERROR: You dragged the Canvas into the 'Teleport Menu Panel' slot! Please drag your TeleportMenuWindow panel into it instead.");
                return;
            }
            teleportMenuPanel.SetActive(!teleportMenuPanel.activeSelf);
        }
    }

    public void CloseMenu()
    {
        if (teleportMenuPanel != null)
        {
            if (teleportMenuPanel == this.gameObject)
            {
                Debug.LogError("SETUP ERROR: You dragged the Canvas into the 'Teleport Menu Panel' slot! Please drag your TeleportMenuWindow panel into it instead.");
                return;
            }
            teleportMenuPanel.SetActive(false);
        }
    }

    // This is the magic function for your buttons!
    public void TeleportTo(string targetSceneName)
    {
        if (isTeleporting) return; // Prevent clicking multiple times
        
        Debug.Log($"Teleporting to: {targetSceneName}");
        CloseMenu(); // Close the menu before loading

        // Start the teleport sequence (fade out, play sound, load scene)
        StartCoroutine(TeleportSequence(targetSceneName));
    }

    private IEnumerator TeleportSequence(string targetSceneName)
    {
        isTeleporting = true;
        AudioClip selectedLeaveSound = null;

        // 1. Play leave sound
        if (audioSource != null && teleportLeaveSounds != null && teleportLeaveSounds.Length > 0)
        {
            selectedLeaveSound = teleportLeaveSounds[Random.Range(0, teleportLeaveSounds.Length)];
            if (selectedLeaveSound != null)
            {
                audioSource.PlayOneShot(selectedLeaveSound);
            }
        }

        // 2. Fade out screen
        if (fadeCanvasGroup != null)
        {
            // Block raycasts so player can't click things while fading
            fadeCanvasGroup.blocksRaycasts = true; 
            
            float timer = 0f;
            while (timer < fadeDuration)
            {
                timer += Time.deltaTime;
                fadeCanvasGroup.alpha = Mathf.Lerp(0f, 1f, timer / fadeDuration);
                yield return null;
            }
            fadeCanvasGroup.alpha = 1f;
        }
        else
        {
            // If no fade is setup, just wait a moment for the sound to play
            if (selectedLeaveSound != null)
            {
                yield return new WaitForSeconds(selectedLeaveSound.length);
            }
        }

        // 3. Load the scene
        // Set a flag so the next scene knows we teleported and should play the arrive sound
        PlayerPrefs.SetInt("JustTeleported", 1);
        PlayerPrefs.Save();

        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.LoadScene(targetSceneName, "CrossFade", "default");
        }
        else if (SceneController.instance != null)
        {
            SceneController.instance.LoadScene(targetSceneName);
        }
        else
        {
            SceneManager.LoadScene(targetSceneName);
        }
    }

    private IEnumerator FadeIn()
    {
        // Fade in screen upon arrival
        if (fadeCanvasGroup != null)
        {
            float timer = 0f;
            while (timer < fadeDuration)
            {
                timer += Time.deltaTime;
                fadeCanvasGroup.alpha = Mathf.Lerp(1f, 0f, timer / fadeDuration);
                yield return null;
            }
            fadeCanvasGroup.alpha = 0f;
            fadeCanvasGroup.blocksRaycasts = false; // Allow clicking again
        }
    }

    // NEW: Volume update methods to respond to settings changes
    private void UpdateSFXVolume(float volume)
    {
        if (audioSource != null)
        {
            audioSource.volume = volume;
        }
    }

    private void OnDestroy()
    {
        // Unsubscribe from events to prevent memory leaks or errors
        if (SettingsManager.Instance != null)
        {
            SettingsManager.Instance.OnSFXVolumeChanged -= UpdateSFXVolume;
        }
    }
}
