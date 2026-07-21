using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTransitionManager : MonoBehaviour
{
    public void GoToScalingMinigame()
    {
        // TODO: Replace with actual scaling scene name
        SceneManager.LoadScene("ScalingMinigameScene");
    }

    public void GoToServingMinigame()
    {
        // TODO: Replace with actual serving scene name
        SceneManager.LoadScene("ServingMinigameScene");
    }

    public void GoToFishingMinigame()
    {
        SceneManager.LoadScene("FishingScene");
    }
} 