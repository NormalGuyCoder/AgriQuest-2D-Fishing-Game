using UnityEngine;
using System.IO;

public class SettingsManager : MonoBehaviour
{
    public static SettingsManager Instance { get; private set; }

    private SettingsData currentSettings;
    private string savePath;

    public float MusicVolume => currentSettings.musicVolume;
    public float SFXVolume => currentSettings.sfxVolume;

    public event System.Action<float> OnMusicVolumeChanged;
    public event System.Action<float> OnSFXVolumeChanged;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        savePath = Path.Combine(Application.persistentDataPath, "game_settings.json");
        LoadSettings();
    }

    private void Start()
    {
        if (MusicManager.Instance != null)
        {
            MusicManager.Instance.UpdateVolume(currentSettings.musicVolume);
        }

        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.UpdateVolume(currentSettings.sfxVolume);
        }
    }

    public void SetMusicVolume(float volume)
    {
        currentSettings.musicVolume = Mathf.Clamp01(volume);
        OnMusicVolumeChanged?.Invoke(currentSettings.musicVolume);
        SaveSettings();

        if (MusicManager.Instance != null)
        {
            MusicManager.Instance.UpdateVolume(currentSettings.musicVolume);
        }
    }

    public void SetSFXVolume(float volume)
    {
        currentSettings.sfxVolume = Mathf.Clamp01(volume);
        OnSFXVolumeChanged?.Invoke(currentSettings.sfxVolume);
        SaveSettings();

        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.UpdateVolume(currentSettings.sfxVolume);
        }
    }

    private void SaveSettings()
    {
        try
        {
            string json = JsonUtility.ToJson(currentSettings, true);
            File.WriteAllText(savePath, json);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to save settings: {e.Message}");
        }
    }

    private void LoadSettings()
    {
        try
        {
            if (File.Exists(savePath))
            {
                string json = File.ReadAllText(savePath);
                currentSettings = JsonUtility.FromJson<SettingsData>(json);
            }
            else
            {
                currentSettings = new SettingsData();
                SaveSettings();
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to load settings: {e.Message}");
            currentSettings = new SettingsData();
        }
    }

    public void ResetToDefaults()
    {
        currentSettings = new SettingsData();
        SaveSettings();

        OnMusicVolumeChanged?.Invoke(currentSettings.musicVolume);
        OnSFXVolumeChanged?.Invoke(currentSettings.sfxVolume);
    }
}