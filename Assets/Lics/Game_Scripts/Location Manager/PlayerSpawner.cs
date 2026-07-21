using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlayerSpawner : MonoBehaviour
{
    [System.Serializable]
    public class SpawnLocation
    {
        public string id;
        public Transform location;
    }

    [Header("Spawn Locations")]
    public List<SpawnLocation> spawnLocations = new List<SpawnLocation>();
    public string playerTag = "Player";
    public float spawnDelay = 0.2f;

    [Header("Gizmo Settings")]
    public bool showGizmos = true;
    public Color defaultGizmoColor = new Color(0, 0.5f, 1f, 0.5f);
    public Color selectedGizmoColor = new Color(0, 1f, 1f, 0.8f);
    public float gizmoSize = 0.5f;
    public bool showLabels = true;

    public static event System.Action OnSpawnComplete;

    private void OnEnable()
    {
        Debug.Log($"[TIMING] PlayerSpawner.OnEnable() - Frame {Time.frameCount}, Time {Time.time}");
    }

    void Awake()
    {
        Debug.Log($"[TIMING] PlayerSpawner.Awake() - Frame {Time.frameCount}, Time {Time.time}");
    }

    void Start()
    {
        Debug.Log($"[TIMING] PlayerSpawner.Start() - Frame {Time.frameCount}, Time {Time.time}");
        Debug.Log($"PlayerSpawner: Started in scene '{gameObject.scene.name}'");

        StartCoroutine(InitializeAndSpawn());
    }

    IEnumerator InitializeAndSpawn()
    {
        Debug.Log($"[TIMING] PlayerSpawner.InitializeAndSpawn started - Frame {Time.frameCount}");

        yield return null;

        yield return new WaitForSeconds(spawnDelay);

        FindAndSpawnPlayer();

        // Clear the spawn point after using it
        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.ClearSpawnData();
            Debug.Log($"PlayerSpawner: Cleared spawn point data");
        }

        OnSpawnComplete?.Invoke();
        Debug.Log($"[TIMING] PlayerSpawner: Spawn complete notified - Frame {Time.frameCount}");
    }

    void FindAndSpawnPlayer()
    {
        Debug.Log($"[TIMING] PlayerSpawner.FindAndSpawnPlayer - Frame {Time.frameCount}");

        // Debug: List all available spawn points
        Debug.Log($"PlayerSpawner: Available spawn points in scene '{gameObject.scene.name}':");
        foreach (var spawn in spawnLocations)
        {
            if (spawn.location != null)
                Debug.Log($"  - '{spawn.id}' at {spawn.location.position}");
        }

        GameObject player = GameObject.FindGameObjectWithTag(playerTag);
        if (player == null)
        {
            Debug.LogError($"PlayerSpawner: No player found with tag '{playerTag}'");
            return;
        }

        // NEW: Check if we just teleported. If so, ignore the save data and use the default spawn point.
        bool justTeleported = PlayerPrefs.GetInt("JustTeleported", 0) == 1;

        // UPDATED: Now using LocationManager instead of GameSaveSystem
        // But skip it if we are teleporting!
        if (!justTeleported && 
            LocationManager.Instance != null &&
            LocationManager.Instance.HasSaveData() &&
            LocationManager.Instance.GetSaveData().sceneName == gameObject.scene.name)
        {
            LocationSaveData saveData = LocationManager.Instance.GetSaveData();
            player.transform.position = saveData.playerPosition;
            Debug.Log($"PlayerSpawner: Loaded from save - Position: {saveData.playerPosition}");

            // Don't use spawn point since we loaded from save
            if (LevelManager.Instance != null)
            {
                LevelManager.Instance.ClearSpawnData();
            }

            OnSpawnComplete?.Invoke();
            return;
        }

        // Reset the teleport flag after we've decided whether to use save data
        if (justTeleported)
        {
            PlayerPrefs.SetInt("JustTeleported", 0);
            PlayerPrefs.Save();
            Debug.Log("PlayerSpawner: Teleport flag detected and reset. Using default spawn point.");
        }

        string spawnId = GetSpawnId();
        Debug.Log($"PlayerSpawner: Attempting to spawn at '{spawnId}' (Player current position: {player.transform.position})");

        Transform spawnTransform = FindSpawnLocation(spawnId);

        if (spawnTransform != null)
        {
            player.transform.position = spawnTransform.position;
            player.transform.rotation = spawnTransform.rotation;
            Debug.Log($"PlayerSpawner: SUCCESS - Player moved to spawn point '{spawnId}' at {spawnTransform.position}");
        }
        else
        {
            Debug.LogWarning($"PlayerSpawner: FAILED - No spawn point found for '{spawnId}', player remains at {player.transform.position}");
        }
    }

    string GetSpawnId()
    {
        if (LevelManager.Instance != null)
        {
            string spawnId = LevelManager.Instance.GetSpawnPoint();
            Debug.Log($"PlayerSpawner.GetSpawnId(): Returning '{spawnId}' from LevelManager");
            return spawnId;
        }

        Debug.LogWarning("PlayerSpawner.GetSpawnId(): No LevelManager found, using 'default'");
        return "default";
    }

    Transform FindSpawnLocation(string spawnId)
    {
        Debug.Log($"PlayerSpawner: Looking for spawn location '{spawnId}'");

        foreach (var spawn in spawnLocations)
        {
            if (spawn.id == spawnId && spawn.location != null)
            {
                Debug.Log($"PlayerSpawner: Found spawn location for '{spawnId}' at {spawn.location.position}");
                return spawn.location;
            }
        }

        Debug.LogWarning($"PlayerSpawner: No spawn point found with id '{spawnId}'");
        return null;
    }

    // Test method for debugging - call this to test spawn points manually
    void Update()
    {
        // Test: Press T to manually trigger spawn at a specific point
        if (Input.GetKeyDown(KeyCode.T))
        {
            string testSpawnId = "Entrance"; // Change this to test different IDs
            Debug.Log($"Manual spawn test: Looking for '{testSpawnId}'");

            GameObject player = GameObject.FindGameObjectWithTag(playerTag);
            if (player)
            {
                Transform spawn = FindSpawnLocation(testSpawnId);
                if (spawn)
                {
                    player.transform.position = spawn.position;
                    Debug.Log($"Manual spawn SUCCESS to {spawn.position}");
                }
                else
                {
                    Debug.Log($"Manual spawn FAILED - No spawn point '{testSpawnId}' found");
                }
            }
        }

        // Press Y to debug LevelManager info
        if (Input.GetKeyDown(KeyCode.Y) && LevelManager.Instance != null)
        {
            LevelManager.Instance.DebugSpawnInfo();
        }
    }

    void OnDrawGizmos()
    {
        if (!showGizmos) return;

        Gizmos.color = defaultGizmoColor;

        foreach (var spawn in spawnLocations)
        {
            if (spawn.location != null)
            {
                Gizmos.DrawSphere(spawn.location.position, gizmoSize);

                Vector3 forward = spawn.location.forward;
                Gizmos.DrawLine(spawn.location.position, spawn.location.position + forward * gizmoSize * 1.5f);

                DrawCircle(spawn.location.position, gizmoSize * 0.8f, 12);
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        if (!showGizmos) return;

        Gizmos.color = selectedGizmoColor;

        foreach (var spawn in spawnLocations)
        {
            if (spawn.location != null)
            {
                Gizmos.DrawWireSphere(spawn.location.position, gizmoSize * 1.1f);

                if (showLabels && !string.IsNullOrEmpty(spawn.id))
                {
#if UNITY_EDITOR
                    UnityEditor.Handles.Label(spawn.location.position + Vector3.up * gizmoSize * 0.5f, 
                        $"Spawn: {spawn.id}", 
                        new GUIStyle() { 
                            normal = new GUIStyleState() { textColor = Color.white },
                            fontSize = 10
                        });
#endif
                }
            }
        }
    }

    void DrawCircle(Vector3 center, float radius, int segments)
    {
        float angleStep = 360f / segments;
        Vector3 prevPoint = center + new Vector3(radius, 0, 0);

        for (int i = 1; i <= segments; i++)
        {
            float angle = angleStep * i;
            Vector3 nextPoint = center + new Vector3(
                Mathf.Cos(angle * Mathf.Deg2Rad) * radius,
                0,
                Mathf.Sin(angle * Mathf.Deg2Rad) * radius
            );

            Gizmos.DrawLine(prevPoint, nextPoint);
            prevPoint = nextPoint;
        }
    }

#if UNITY_EDITOR
    void OnValidate()
    {
        gizmoSize = Mathf.Max(0.1f, gizmoSize);
    }
#endif
}