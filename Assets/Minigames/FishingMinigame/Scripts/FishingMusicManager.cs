using UnityEngine;

public class FishingMusicManager : MonoBehaviour
{
    [Header("Audio Settings")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioClip fishingMusic;
    [SerializeField] private float fadeSpeed = 1f;
    
    private float targetVolume = 0f;
    private bool isPlaying = false;

    private void Awake()
    {
        // Create AudioSource if not assigned
        if (musicSource == null)
        {
            musicSource = gameObject.AddComponent<AudioSource>();
            musicSource.playOnAwake = false;
            musicSource.loop = true;
            musicSource.volume = 0f;
        }
    }

    private void Start()
    {
        // Set the fishing music clip
        if (fishingMusic != null)
        {
            musicSource.clip = fishingMusic;
        }
        else
        {
            Debug.LogWarning("No fishing music clip assigned to FishingMusicManager!");
        }
    }

    private void Update()
    {
        // Smoothly adjust volume based on target volume AND global settings
        float globalVolume = SettingsManager.Instance != null ? SettingsManager.Instance.MusicVolume : 1f;
        float actualTarget = targetVolume * globalVolume;

        if (musicSource.volume != actualTarget)
        {
            musicSource.volume = Mathf.MoveTowards(musicSource.volume, actualTarget, fadeSpeed * Time.deltaTime);
        }

        // Start/Stop music based on volume
        if (musicSource.volume > 0 && !isPlaying)
        {
            musicSource.Play();
            isPlaying = true;
        }
        else if (musicSource.volume <= 0 && isPlaying)
        {
            musicSource.Stop();
            isPlaying = false;
        }
    }

    public void StartFishingMusic()
    {
        targetVolume = 1f;
    }

    public void StopFishingMusic()
    {
        targetVolume = 0f;
    }
} 