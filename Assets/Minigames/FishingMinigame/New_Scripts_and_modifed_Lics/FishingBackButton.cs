using UnityEngine;

public class FishingBackButton : MonoBehaviour
{
    public void ReturnIsland()
    {
        LevelManager.Instance.LoadScene("Saltyshore", "CrossFade");
        MusicManager.Instance.PlayMusic("BeachsideBGM");

    }
}
