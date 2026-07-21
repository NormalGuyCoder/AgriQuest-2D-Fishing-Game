using System.Collections;
using UnityEngine;

public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance;

    [Header("Audio Settings")]
    [SerializeField] private string mixerGroup = "Music";
    
    private AudioSource musicSource;
    private MusicLibrary musicLibrary;
    private float baseVolume = 0.5f;
    private Coroutine currentFadeCoroutine;

    private void Awake()
    {
        // Singleton pattern
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        Instance = this;
        DontDestroyOnLoad(gameObject);
        
        // Initialize audio source
        InitializeAudioSource();
    }

    private void InitializeAudioSource()
    {
        // Try to find existing AudioSource on this GameObject
        musicSource = GetComponent<AudioSource>();
        
        if (musicSource == null)
        {
            // Try to find child named "MusicSource"
            Transform musicSourceChild = transform.Find("MusicSource");
            if (musicSourceChild != null)
            {
                musicSource = musicSourceChild.GetComponent<AudioSource>();
            }
        }
        
        // If still null, create a new one
        if (musicSource == null)
        {
            GameObject audioGO = new GameObject("MusicAudioSource");
            audioGO.transform.SetParent(transform);
            musicSource = audioGO.AddComponent<AudioSource>();
        }
        
        // Configure AudioSource
        musicSource.playOnAwake = false;
        musicSource.loop = true;
        musicSource.volume = baseVolume;
        
        Debug.Log($"MusicManager: AudioSource initialized on '{musicSource.gameObject.name}'");
    }

    private void Start()
    {
        // Find MusicLibrary
        musicLibrary = GetComponentInChildren<MusicLibrary>();
        if (musicLibrary == null)
        {
            Debug.LogError("MusicManager: No MusicLibrary found!");
            return;
        }
        
        Debug.Log($"MusicManager: Found library with {musicLibrary.tracks.Length} tracks");
        
        // Subscribe to volume changes
        if (SettingsManager.Instance != null)
        {
            baseVolume = SettingsManager.Instance.MusicVolume;
            SettingsManager.Instance.OnMusicVolumeChanged += UpdateVolume;
        }
    }

    public void PlayMusic(string trackName, float fadeDuration = 0.5f)
    {
        // Ensure we have an AudioSource
        if (musicSource == null)
        {
            InitializeAudioSource();
            if (musicSource == null)
            {
                Debug.LogError($"MusicManager: Failed to initialize AudioSource!");
                return;
            }
        }
        
        // Ensure we have a MusicLibrary
        if (musicLibrary == null)
        {
            musicLibrary = GetComponentInChildren<MusicLibrary>();
            if (musicLibrary == null)
            {
                Debug.LogError($"MusicManager: No MusicLibrary found!");
                return;
            }
        }
        
        Debug.Log($"MusicManager: Looking for track '{trackName}' in library...");
        
        // Get the audio clip
        AudioClip nextTrack = musicLibrary.GetClipFromName(trackName);
        
        if (nextTrack == null)
        {
            Debug.LogError($"Music track '{trackName}' not found!");
            Debug.Log($"Available tracks: {musicLibrary.tracks.Length}");
            
            // List all tracks
            foreach (var track in musicLibrary.tracks)
            {
                if (track.clip != null)
                    Debug.Log($"- '{track.trackName}' -> {track.clip.name}");
                else
                    Debug.Log($"- '{track.trackName}' -> NULL CLIP!");
            }
            return;
        }
        
        Debug.Log($"MusicManager: Found '{trackName}', starting playback...");
        
        // Start crossfade
        if (currentFadeCoroutine != null)
        {
            StopCoroutine(currentFadeCoroutine);
        }
        currentFadeCoroutine = StartCoroutine(AnimateMusicCrossfade(nextTrack, fadeDuration));
    }

    IEnumerator AnimateMusicCrossfade(AudioClip nextTrack, float fadeDuration = 0.5f)
    {
        if (musicSource == null) yield break;

        float percent = 0;

        // Fade out current track
        while (percent < 1)
        {
            percent += Time.deltaTime * 1 / fadeDuration;
            musicSource.volume = Mathf.Lerp(baseVolume, 0, percent);
            yield return null;
        }

        // Switch track
        musicSource.clip = nextTrack;
        musicSource.Play();
        Debug.Log($"Now playing: {nextTrack.name}");

        // Fade in new track
        percent = 0;
        while (percent < 1)
        {
            percent += Time.deltaTime * 1 / fadeDuration;
            musicSource.volume = Mathf.Lerp(0, baseVolume, percent);
            yield return null;
        }

        currentFadeCoroutine = null;
    }

    public void UpdateVolume(float newVolume)
    {
        baseVolume = newVolume;
        if (musicSource != null)
        {
            musicSource.volume = baseVolume;
        }
    }

    private void OnDestroy()
    {
        if (SettingsManager.Instance != null)
        {
            SettingsManager.Instance.OnMusicVolumeChanged -= UpdateVolume;
        }
    }
}