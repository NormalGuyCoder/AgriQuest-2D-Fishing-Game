using UnityEngine;

public class AutoSaveManager : MonoBehaviour
{
    [Header("Auto-Save Settings")]
    public bool saveOnTeleport = true;
    public bool saveOnExit = true;

    void Start()
    {
        // Subscribe to save events
        if (saveOnTeleport)
        {
            // Save when teleporting (via SceneTeleporter)
            var teleporters = FindObjectsOfType<SceneTeleporter>();
            foreach (var tp in teleporters)
            {
                // Add listener if not already present
                // This is a simple approach - you might need to modify SceneTeleporter
                // to have an event for teleporting
            }
        }
    }

    public void ManualSave()
    {
        if (LocationManager.Instance != null)
        {
            LocationManager.Instance.SaveCurrentGameState();
        }
    }

    public void LoadFromSave()
    {
        if (LocationManager.Instance != null && LocationManager.Instance.HasSaveData())
        {
            LocationSaveData saveData = LocationManager.Instance.GetSaveData();

            // Load the saved scene
            if (LevelManager.Instance != null)
            {
                // We'll need to handle this based on your scene loading system
                Debug.Log($"Would load scene: {saveData.sceneName}");
            }
        }
    }

    void OnDestroy()
    {
        if (saveOnExit && LocationManager.Instance != null)
        {
            LocationManager.Instance.SaveCurrentGameState();
        }
    }
}