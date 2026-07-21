// MainMenuMusicPlayer.cs
using UnityEngine;

public class MainMenuMusicPlayer : MonoBehaviour
{
    void Start()
    {
        if (MusicManager.Instance != null)
        {
            MusicManager.Instance.PlayMusic("MainMenuBGM");
        }
    }
}