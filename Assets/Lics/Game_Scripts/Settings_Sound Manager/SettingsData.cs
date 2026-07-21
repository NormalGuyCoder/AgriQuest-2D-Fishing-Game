using System;

[Serializable]
public class SettingsData
{
    public float musicVolume = 0.5f;
    public float sfxVolume = 0.5f;

    public SettingsData() { }

    public SettingsData(float musicVol, float sfxVol)
    {
        musicVolume = musicVol;
        sfxVolume = sfxVol;
    }
}