using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance;

    [SerializeField]
    private SoundLibrary sfxLibrary;
    [SerializeField]
    private AudioSource sfx2DSource;

    private float baseVolume = 0.5f;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    private void Start()
    {
        if (SettingsManager.Instance != null)
        {
            baseVolume = SettingsManager.Instance.SFXVolume;
            sfx2DSource.volume = baseVolume;
            SettingsManager.Instance.OnSFXVolumeChanged += UpdateVolume;
        }
        else
        {
            sfx2DSource.volume = baseVolume;
        }
    }

    public void UpdateVolume(float newVolume)
    {
        baseVolume = newVolume;
        sfx2DSource.volume = baseVolume;
    }

    public void PlaySound3D(AudioClip clip, Vector3 pos)
    {
        if (clip != null)
        {
            AudioSource.PlayClipAtPoint(clip, pos, baseVolume);
        }
    }

    public void PlaySound3D(string soundName, Vector3 pos)
    {
        PlaySound3D(sfxLibrary.GetClipFromName(soundName), pos);
    }

    public void PlaySound2D(string soundName)
    {
        AudioClip clip = sfxLibrary.GetClipFromName(soundName);
        if (clip != null)
        {
            sfx2DSource.PlayOneShot(clip, baseVolume);
        }
    }

    private void OnDestroy()
    {
        if (SettingsManager.Instance != null)
        {
            SettingsManager.Instance.OnSFXVolumeChanged -= UpdateVolume;
        }
    }
}