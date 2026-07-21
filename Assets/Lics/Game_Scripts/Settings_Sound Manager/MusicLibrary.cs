using UnityEngine;

[System.Serializable]
public struct MusicTrack
{
    public string trackName;
    public AudioClip clip;
}

public class MusicLibrary : MonoBehaviour
{
    public MusicTrack[] tracks;

    void Start()
    {
        // Debug: Log what tracks we have
        if (tracks == null || tracks.Length == 0)
        {
            Debug.LogWarning("MusicLibrary: No tracks assigned! Check Inspector.");
        }
        else
        {
            Debug.Log($"MusicLibrary: Loaded {tracks.Length} tracks");
        }
    }

    public AudioClip GetClipFromName(string trackName)
    {
        if (tracks == null)
        {
            Debug.LogError("MusicLibrary: Tracks array is null!");
            return null;
        }

        foreach (var track in tracks)
        {
            // Check for null track names
            if (string.IsNullOrEmpty(track.trackName))
            {
                Debug.LogWarning("MusicLibrary: Found track with empty name!");
                continue;
            }

            // Exact match
            if (track.trackName == trackName)
            {
                if (track.clip == null)
                {
                    Debug.LogWarning($"MusicLibrary: Track '{trackName}' has null AudioClip!");
                }
                return track.clip;
            }
        }

        // Try case-insensitive match
        foreach (var track in tracks)
        {
            if (!string.IsNullOrEmpty(track.trackName) && 
                track.trackName.ToLower() == trackName.ToLower())
            {
                Debug.LogWarning($"MusicLibrary: Found '{trackName}' with different case (expected: '{trackName}', actual: '{track.trackName}')");
                return track.clip;
            }
        }

        return null;
    }

    // Debug method to list all tracks
    [ContextMenu("List All Tracks")]
    public void ListAllTracks()
    {
        Debug.Log("=== MusicLibrary Track List ===");
        if (tracks == null)
        {
            Debug.Log("Tracks array is null!");
            return;
        }

        for (int i = 0; i < tracks.Length; i++)
        {
            var track = tracks[i];
            string status = track.clip != null ? "✓" : "✗ (NULL CLIP)";
            Debug.Log($"{i}: '{track.trackName}' {status}");
        }
    }
}