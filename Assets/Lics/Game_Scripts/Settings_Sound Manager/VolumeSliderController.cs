using UnityEngine;
using UnityEngine.UI;

public class VolumeSliderController : MonoBehaviour
{
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider sfxSlider;

    private void Start()
    {
        if (SettingsManager.Instance != null)
        {
            if (musicSlider != null)
            {
                musicSlider.value = SettingsManager.Instance.MusicVolume;
                musicSlider.onValueChanged.AddListener(SetMusicVolume);
            }

            if (sfxSlider != null)
            {
                sfxSlider.value = SettingsManager.Instance.SFXVolume;
                sfxSlider.onValueChanged.AddListener(SetSFXVolume);
            }
        }
        else
        {
            if (musicSlider != null)
            {
                musicSlider.value = 0.5f;
                musicSlider.onValueChanged.AddListener(SetMusicVolume);
            }

            if (sfxSlider != null)
            {
                sfxSlider.value = 0.5f;
                sfxSlider.onValueChanged.AddListener(SetSFXVolume);
            }
        }
    }

    private void SetMusicVolume(float volume)
    {
        if (SettingsManager.Instance != null)
        {
            SettingsManager.Instance.SetMusicVolume(volume);
        }
    }

    private void SetSFXVolume(float volume)
    {
        if (SettingsManager.Instance != null)
        {
            SettingsManager.Instance.SetSFXVolume(volume);
        }
    }

    private void OnDestroy()
    {
        if (musicSlider != null)
            musicSlider.onValueChanged.RemoveListener(SetMusicVolume);

        if (sfxSlider != null)
            sfxSlider.onValueChanged.RemoveListener(SetSFXVolume);
    }
}