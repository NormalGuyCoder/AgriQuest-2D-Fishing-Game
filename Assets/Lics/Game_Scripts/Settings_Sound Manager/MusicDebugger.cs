using UnityEngine;

public class MusicDebugger : MonoBehaviour
{
    void Start()
    {
        Debug.Log("=== Music System Debug ===");
        
        // Check Audio Manager
        GameObject audioManager = GameObject.Find("Audio Manager");
        if (audioManager != null)
        {
            Debug.Log($"Found Audio Manager: {audioManager.name}");
            
            // Check MusicManager
            MusicManager musicManager = audioManager.GetComponent<MusicManager>();
            if (musicManager != null)
            {
                Debug.Log($"Found MusicManager script");
            }
            else
            {
                Debug.LogError("No MusicManager script on Audio Manager!");
            }
            
            // Check MusicLibrary
            MusicLibrary library = audioManager.GetComponentInChildren<MusicLibrary>();
            if (library != null)
            {
                Debug.Log($"Found MusicLibrary with {library.tracks.Length} tracks");
                
                // Check for Greenville
                bool hasGreenville = false;
                foreach (var track in library.tracks)
                {
                    if (track.trackName == "Greenville")
                    {
                        hasGreenville = true;
                        Debug.Log($"Found 'Greenville' track! Clip: {track.clip?.name ?? "NULL"}");
                    }
                }
                
                if (!hasGreenville)
                {
                    Debug.LogError("'Greenville' NOT found in MusicLibrary!");
                }
            }
            else
            {
                Debug.LogError("No MusicLibrary found!");
            }
            
            // Check for AudioSource
            AudioSource[] audioSources = audioManager.GetComponentsInChildren<AudioSource>();
            Debug.Log($"Found {audioSources.Length} AudioSource components:");
            foreach (var source in audioSources)
            {
                Debug.Log($"- {source.gameObject.name}: clip={source.clip?.name ?? "None"}");
            }
        }
        else
        {
            Debug.LogError("No Audio Manager found in scene!");
        }
    }
}