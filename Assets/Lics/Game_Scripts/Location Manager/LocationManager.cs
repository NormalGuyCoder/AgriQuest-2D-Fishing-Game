using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;

[System.Serializable]
public class LocationSaveData
{
    public string sceneName = "";
    public Vector3 playerPosition;
    public string lastSaveTime;
}

public class LocationManager : MonoBehaviour
{
    public static LocationManager Instance;

    private string saveFilePath;
    private LocationSaveData currentSaveData;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            // Set up save file path
            saveFilePath = Path.Combine(Application.persistentDataPath, "savegame.json");
            Debug.Log($"Save file path: {saveFilePath}");

            // Try to load existing save
            LoadGame();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void SaveGame(string sceneName, Vector3 playerPosition)
    {
        if (currentSaveData == null)
            currentSaveData = new LocationSaveData();

        currentSaveData.sceneName = sceneName;
        currentSaveData.playerPosition = playerPosition;
        currentSaveData.lastSaveTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

        string jsonData = JsonUtility.ToJson(currentSaveData, true);
        File.WriteAllText(saveFilePath, jsonData);

        Debug.Log($"Game saved: Scene={sceneName}, Position={playerPosition}");
    }

    public void SaveCurrentGameState()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            SaveGame(
                UnityEngine.SceneManagement.SceneManager.GetActiveScene().name,
                player.transform.position
            );
        }
    }

    public void LoadGame()
    {
        if (File.Exists(saveFilePath))
        {
            try
            {
                string jsonData = File.ReadAllText(saveFilePath);
                currentSaveData = JsonUtility.FromJson<LocationSaveData>(jsonData);
                Debug.Log($"Game loaded: Scene={currentSaveData.sceneName}");
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to load save: {e.Message}");
                currentSaveData = new LocationSaveData();
            }
        }
        else
        {
            currentSaveData = new LocationSaveData();
            Debug.Log("No save file found, creating new save data");
        }
    }

    public bool HasSaveData()
    {
        return currentSaveData != null && !string.IsNullOrEmpty(currentSaveData.sceneName);
    }

    public LocationSaveData GetSaveData()
    {
        return currentSaveData;
    }

    public string GetLastSceneName()
    {
        return currentSaveData?.sceneName ?? "";
    }

    public void DeleteSave()
    {
        if (File.Exists(saveFilePath))
        {
            File.Delete(saveFilePath);
            currentSaveData = new LocationSaveData();
            Debug.Log("Save file deleted");
        }
    }

    void OnApplicationQuit()
    {
        SaveCurrentGameState();
        Debug.Log("Auto-saved on application quit");
    }
}