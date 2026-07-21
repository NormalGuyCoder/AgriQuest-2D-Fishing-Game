using UnityEngine;

public class CreditsBackButtonScript : MonoBehaviour
{
    public void MainMenu()
    {
        LevelManager.Instance.LoadScene("MainMenu", "CrossFade");
        MusicManager.Instance.PlayMusic("MainMenuBGM");

    }
}
