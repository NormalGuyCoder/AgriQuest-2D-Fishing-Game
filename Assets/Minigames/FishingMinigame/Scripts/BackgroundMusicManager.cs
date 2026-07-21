using UnityEngine;

public class BackgroundMusicManager : MonoBehaviour
{
    [Header("Audio Settings")]
    [SerializeField] private AudioSource backgroundMusicSource;
    [SerializeField] private AudioClip backgroundMusic;
    [SerializeField] private float fadeSpeed = 1f;
    
    private float targetVolume = 1f;
    private float savedVolume = 1f;
    private bool isPaused = false;

    private void Awake()
    {
        // Create AudioSource if not assigned
        if (backgroundMusicSource == null)
        {
            backgroundMusicSource = gameObject.AddComponent<AudioSource>();
            backgroundMusicSource.playOnAwake = true;
            backgroundMusicSource.loop = true;
            backgroundMusicSource.volume = 1f;
        }
    }

    private void Start()
    {
        // Set the background music clip
        if (backgroundMusic != null)
        {
            backgroundMusicSource.clip = backgroundMusic;
            backgroundMusicSource.Play();
        }
        else
        {
            Debug.LogWarning("No background music clip assigned to BackgroundMusicManager!");
        }
    }

    private void Update()
    {
        // Smoothly adjust volume based on target volume AND global settings
        float globalVolume = SettingsManager.Instance != null ? SettingsManager.Instance.MusicVolume : 1f;
        float actualTarget = targetVolume * globalVolume;

        if (backgroundMusicSource.volume != actualTarget)
        {
            backgroundMusicSource.volume = Mathf.MoveTowards(backgroundMusicSource.volume, actualTarget, fadeSpeed * Time.deltaTime);
        }
    }

    public void PauseBackgroundMusic()
    {
        if (!isPaused)
        {
            savedVolume = backgroundMusicSource.volume;
            targetVolume = 0f;
            isPaused = true;
        }
    }

    public void ResumeBackgroundMusic()
    {
        if (isPaused)
        {
            targetVolume = savedVolume;
            isPaused = false;
        }
    }
} 