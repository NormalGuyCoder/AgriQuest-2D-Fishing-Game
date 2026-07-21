using UnityEngine;

public class DeboningExitButtonSCript : MonoBehaviour
{
    public void ExitDeboning()
    {
        LevelManager.Instance.LoadScene("Fresh Finds", "CrossFade");
        MusicManager.Instance.PlayMusic("Fresh Finds");

    }
}
