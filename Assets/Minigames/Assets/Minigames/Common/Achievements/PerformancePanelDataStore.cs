using UnityEngine;
using System.IO;

/// <summary>
/// Persists performance panel data (unified analytics + evaluation) separately.
/// Prevents loss of performance history when achievements data is cleared.
/// </summary>
public class PerformancePanelDataStore : MonoBehaviour
{
    public static PerformancePanelDataStore Instance { get; private set; }

    [SerializeField] private string saveFileName = "PerformancePanel.json";
    [SerializeField] private bool savePrettyJson = true;

    private PerformancePanelData cachedData;

    private string SaveFilePath => Path.Combine(Application.persistentDataPath, saveFileName);

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadFromDisk();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public bool HasSavedData => cachedData != null;

    public UnifiedAnalyticsReport GetSavedUnifiedReport()
    {
        return cachedData?.unifiedAnalytics?.ToReport();
    }

    public PlayerPerformanceEvaluation GetSavedEvaluation()
    {
        return cachedData?.performanceEvaluation?.ToEvaluation();
    }

    public void SaveSnapshot(UnifiedAnalyticsReport report, PlayerPerformanceEvaluation evaluation)
    {
        if (report == null && evaluation == null)
            return;

        var unifiedSnapshot = UnifiedAnalyticsSnapshot.FromReport(report);
        var evaluationSnapshot = PerformanceEvaluationSnapshot.FromEvaluation(evaluation);

        if (unifiedSnapshot == null && evaluationSnapshot == null)
        {
            Debug.Log("PerformancePanelDataStore: No performance data to save.");
            return;
        }

        cachedData = new PerformancePanelData
        {
            lastUpdatedUtc = System.DateTime.UtcNow.ToString("o"),
            unifiedAnalytics = unifiedSnapshot,
            performanceEvaluation = evaluationSnapshot
        };

        string json = JsonUtility.ToJson(cachedData, savePrettyJson);
        try
        {
            File.WriteAllText(SaveFilePath, json);
            Debug.Log($"PerformancePanelDataStore: Saved performance data to {SaveFilePath}");
        }
        catch (System.Exception ex)
        {
            Debug.LogWarning($"PerformancePanelDataStore: Failed to save performance data. {ex.Message}");
        }
    }

    private void LoadFromDisk()
    {
        if (!File.Exists(SaveFilePath))
            return;

        try
        {
            string json = File.ReadAllText(SaveFilePath);
            if (string.IsNullOrEmpty(json))
                return;

            cachedData = JsonUtility.FromJson<PerformancePanelData>(json);
        }
        catch (System.Exception ex)
        {
            Debug.LogWarning($"PerformancePanelDataStore: Failed to load performance data. {ex.Message}");
        }
    }

    /// <summary>
    /// Completely delete the performance panel save file and clear the cached snapshot.
    /// Use this as part of a full "New Game" reset so old performance data does not carry over.
    /// </summary>
    public void DeleteSavedPerformancePanel()
    {
        cachedData = null;

        try
        {
            if (File.Exists(SaveFilePath))
            {
                File.Delete(SaveFilePath);
                Debug.Log($"PerformancePanelDataStore: Deleted performance panel save file at {SaveFilePath}");
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogWarning($"PerformancePanelDataStore: Failed to delete performance panel save file. {ex.Message}");
        }
    }
}

[System.Serializable]
public class PerformancePanelData
{
    public string lastUpdatedUtc;
    public UnifiedAnalyticsSnapshot unifiedAnalytics;
    public PerformanceEvaluationSnapshot performanceEvaluation;
}


