using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class InGameButtonUIIScript : MonoBehaviour
{
    public void AchievementScene()
    {
        LevelManager.Instance.LoadScene("AchievementsScene", "CrossFade");
        MusicManager.Instance.PlayMusic("AchievementBGM");

    }

    public void MainMenu()
    {
        LevelManager.Instance.LoadScene("MainMenu", "CrossFade");
        MusicManager.Instance.PlayMusic("MainMenuBGM");

    }

    public void Quit()
    {
        Application.Quit();
    }
}