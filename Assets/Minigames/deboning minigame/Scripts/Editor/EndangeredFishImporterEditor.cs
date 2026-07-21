using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;
using Newtonsoft.Json;

public class EndangeredFishImporterEditor : EditorWindow
{
    private string jsonFilePath = "";
    private string outputFolderPath = "Assets/deboning mini game/debs/DEBONING MINIGAME/Data/FishDefinitions/endangered fishes";

    [MenuItem("Fish Deboner/Import Endangered Fish JSON")]
    public static void ShowWindow()
    {
        GetWindow<EndangeredFishImporterEditor>("Endangered Fish JSON Importer");
    }

    void OnGUI()
    {
        GUILayout.Label("Endangered Fish JSON Importer", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        EditorGUILayout.LabelField("JSON File Path:", EditorStyles.boldLabel);
        EditorGUILayout.BeginHorizontal();
        jsonFilePath = EditorGUILayout.TextField(jsonFilePath);
        if (GUILayout.Button("Browse", GUILayout.Width(60)))
        {
            string path = EditorUtility.OpenFilePanel("Select JSON File", Application.dataPath, "json");
            if (!string.IsNullOrEmpty(path))
            {
                jsonFilePath = path;
            }
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();

        EditorGUILayout.LabelField("Output Folder:", EditorStyles.boldLabel);
        EditorGUILayout.BeginHorizontal();
        outputFolderPath = EditorGUILayout.TextField(outputFolderPath);
        if (GUILayout.Button("Browse", GUILayout.Width(60)))
        {
            string path = EditorUtility.SaveFolderPanel("Select Output Folder", "Assets", "");
            if (!string.IsNullOrEmpty(path))
            {
                // Convert to relative path
                if (path.StartsWith(Application.dataPath))
                {
                    outputFolderPath = "Assets" + path.Substring(Application.dataPath.Length);
                }
                else
                {
                    outputFolderPath = path;
                }
            }
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();

        if (GUILayout.Button("Import", GUILayout.Height(30)))
        {
            ImportEndangeredFishJSON();
        }

        EditorGUILayout.Space();
        EditorGUILayout.HelpBox("JSON Format:\n{\n  \"endangeredFish\": [\n    {\n      \"fishName\": \"Tawilis\",\n      \"scientificName\": \"Sardinella tawilis\",\n      \"description\": \"...\",\n      \"status\": 2,\n      \"iucnStatus\": \"Endangered\",\n      ...\n    }\n  ]\n}", MessageType.Info);
    }

    private void ImportEndangeredFishJSON()
    {
        if (string.IsNullOrEmpty(jsonFilePath) || !File.Exists(jsonFilePath))
        {
            EditorUtility.DisplayDialog("Error", "Please select a valid JSON file!", "OK");
            return;
        }

        try
        {
            string jsonContent = File.ReadAllText(jsonFilePath);
            EndangeredFishJSONData data = JsonConvert.DeserializeObject<EndangeredFishJSONData>(jsonContent);

            if (data == null || data.endangeredFish == null || data.endangeredFish.Count == 0)
            {
                EditorUtility.DisplayDialog("Error", "JSON file is invalid or contains no endangered fish data!", "OK");
                return;
            }

            // Create output folder if it doesn't exist
            string fullOutputPath = Path.Combine(Application.dataPath, outputFolderPath.Replace("Assets/", ""));
            if (!Directory.Exists(fullOutputPath))
            {
                Directory.CreateDirectory(fullOutputPath);
            }

            int successCount = 0;
            foreach (var fishData in data.endangeredFish)
            {
                EndangeredFishDefinition fishDefinition = ScriptableObject.CreateInstance<EndangeredFishDefinition>();
                
                // Basic information
                fishDefinition.fishName = fishData.fishName;
                fishDefinition.scientificName = fishData.scientificName;
                fishDefinition.description = fishData.description ?? "";
                
                // Conservation status
                // Convert status int to enum (0=Vulnerable, 1=Vulnerable, 2=Endangered, 3=CriticallyEndangered/Extinct)
                if (fishData.status == 0 || fishData.status == 1)
                    fishDefinition.status = EndangeredFishDefinition.ConservationStatus.Vulnerable;
                else if (fishData.status == 2)
                    fishDefinition.status = EndangeredFishDefinition.ConservationStatus.Endangered;
                else if (fishData.status == 3)
                    fishDefinition.status = EndangeredFishDefinition.ConservationStatus.CriticallyEndangered;
                else
                    fishDefinition.status = EndangeredFishDefinition.ConservationStatus.Vulnerable;
                
                fishDefinition.iucnStatus = fishData.iucnStatus ?? "";
                fishDefinition.populationTrend = fishData.populationTrend ?? "";
                
                // Habitat & Distribution
                fishDefinition.habitat = fishData.habitat ?? "";
                fishDefinition.distribution = fishData.distribution ?? "";
                
                // Threats & Importance
                fishDefinition.threats = fishData.threats ?? "";
                fishDefinition.importance = fishData.importance ?? "";
                
                // Conservation Efforts
                fishDefinition.conservationEfforts = fishData.conservationEfforts ?? "";
                
                // Visual fields (fishImage and fishIcon) will need to be assigned manually
                // or you can add image path support here if needed

                // Save asset - use fishName as filename (sanitize it)
                string safeFileName = fishData.fishName.Replace(" ", "_").Replace("/", "_").Replace("\\", "_");
                string assetPath = Path.Combine(outputFolderPath, safeFileName + ".asset");
                AssetDatabase.CreateAsset(fishDefinition, assetPath);
                successCount++;
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            EditorUtility.DisplayDialog("Success", $"Imported {successCount} endangered fish definition(s) successfully!\n\nFiles saved to:\n{outputFolderPath}", "OK");
        }
        catch (System.Exception e)
        {
            EditorUtility.DisplayDialog("Error", "Failed to import JSON: " + e.Message, "OK");
            Debug.LogError("Import error: " + e);
        }
    }

    [System.Serializable]
    private class EndangeredFishJSONData
    {
        public List<EndangeredFishJSONEntry> endangeredFish;
    }

    [System.Serializable]
    private class EndangeredFishJSONEntry
    {
        public string fishName;
        public string scientificName;
        public string description;
        public int status; // 0=Vulnerable, 1=Vulnerable, 2=Endangered, 3=CriticallyEndangered/Extinct
        public string iucnStatus;
        public string populationTrend;
        public string habitat;
        public string distribution;
        public string threats;
        public string importance;
        public string conservationEfforts;
    }
}

