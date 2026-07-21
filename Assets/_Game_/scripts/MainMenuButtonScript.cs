// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;
// using UnityEngine.SceneManagement;

// public class MainMenuButtonScript : MonoBehaviour
// {
//    public void Play()
//    {
//        LevelManager.Instance.LoadScene("Riverside","CrossFade");
//        MusicManager.Instance.PlayMusic("RiversideBGM");

//    }
//    public void MainMenu()
//    {
//        LevelManager.Instance.LoadScene("MainMenu", "CrossFade");
//        MusicManager.Instance.PlayMusic("MainMenuBGM");

//    }

//    public void ReturnIsland()
//    {
//        LevelManager.Instance.LoadScene("Beachside", "CrossFade");
//        MusicManager.Instance.PlayMusic("BeachsideBGM");

//    }
//    public void ReturnDnsc()
//    {
//        LevelManager.Instance.LoadScene("DNSC_entrance", "CrossFade");
//        MusicManager.Instance.PlayMusic("DNSCBGM");

//    }
//    public void BCsound()
//    {
//        SoundManager.Instance.PlaySound2D("buttonc");
//    }
//    public void Hsound()
//    {
//        SoundManager.Instance.PlaySound2D("hoverc");
//    }
//    public void Credit()
//    {
//        LevelManager.Instance.LoadScene("CreditScene", "CrossFade");
//        MusicManager.Instance.PlayMusic("CreditsBGM");

//    }

//    public void Quit()
//    {
//        Application.Quit();
//    }
    
//     public void DebonineScene()
//    {
//        LevelManager.Instance.LoadScene("Deboning Scene", "CrossFade");
//        MusicManager.Instance.PlayMusic("DeboningBGM");

//    }
//    public void AchievementScene()
//    {
//      LevelManager.Instance.LoadScene("AchievementsScene", "CrossFade");
//        MusicManager.Instance.PlayMusic("AchievementBGM");

//    }
//    public void DiscordLink()
//    {
//        Application.OpenURL("https://discord.gg/ngQxxTwBxv");
//    }
//    public void DeboningMain()
//    {
//        LevelManager.Instance.LoadScene("DeboningMainScene", "CrossFade");
//        MusicManager.Instance.PlayMusic("DeboningBGM");

//    }



//    public void NewGame()
//    {
//        // Play sound
//        BCsound();

//        // Clear spawn tracking data for fresh start
//        if (LevelManager.Instance != null)
//        {
//            LevelManager.Instance.ClearSpawnData();
//        }

//        // Reset all game data
//        if (DataResetManager.Instance != null)
//        {
//            DataResetManager.Instance.ResetAllGameData();
//        }

//        // Load Riverside scene - player will spawn at default position
//        LevelManager.Instance.LoadScene("Auditorium", "CrossFade");
//        MusicManager.Instance.PlayMusic("RiversideBGM");
//    }

//    public void ContinueGame()
//    {
//        // Play sound
//        BCsound();

//        // Check if there's existing save data
//        bool hasSaveData = false;
//        if (DataResetManager.Instance != null)
//        {
//            hasSaveData = DataResetManager.Instance.HasExistingSaveData();
//        }

//        if (hasSaveData)
//        {
//            Debug.Log("Loading existing game...");
//            // Load the last scene the player was in
//            // You might want to save/load the last scene separately
//            LevelManager.Instance.LoadScene("Riverside", "CrossFade");
//        }
//        else
//        {
//            Debug.Log("No save data found, starting New Game");
//            NewGame(); // Start fresh
//        }
//    }

//    private void ShowNewGameConfirmation()
//    {
//        // You can implement a proper confirmation dialog here
//        // For now, we'll just show a debug message and reset
//        Debug.Log("Starting new game - all progress will be reset!");

//        // If you have a confirmation panel, show it here
//        // For example: confirmationPanel.SetActive(true);

//        // For now, proceed with reset
//        ResetAndPlay();
//    }

//    private void ResetAndPlay()
//    {
//        // Reset all data
//        if (DataResetManager.Instance != null)
//        {
//            DataResetManager.Instance.ResetAllGameData();
//        }

//        // Then load the scene
//        Play();
//    }
// }