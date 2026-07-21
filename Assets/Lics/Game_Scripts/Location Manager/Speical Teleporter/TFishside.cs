using UnityEngine;
using UnityEngine.SceneManagement;

public class TFishside : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            // Save the player's position before switching scenes
            collision.GetComponent<Player>()?.SavePlayerPosition();

            // Now load the new scene
            SceneManager.LoadScene("FishingSea");
            MusicManager.Instance.PlayMusic("FishingBGM");

        }
    }
}
