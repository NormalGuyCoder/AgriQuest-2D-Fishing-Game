using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;
using Newtonsoft.Json;

public class FishImporterEditor : EditorWindow
{
    private string jsonFilePath = "";
    private string outputFolderPath = "Assets/Minigames/DEBONING MINIGAME/Data/FishDefinitions";
    private bool createScene = false;

    [MenuItem("Fish Deboner/Import Fish JSON")]
    public static void ShowWindow()
    {
        GetWindow<FishImporterEditor>("Fish JSON Importer");
    }

    void OnGUI()
    {
        GUILayout.Label("Fish JSON Importer", EditorStyles.boldLabel);
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

        createScene = EditorGUILayout.Toggle("Auto-create Scene", createScene);

        EditorGUILayout.Space();

        if (GUILayout.Button("Import", GUILayout.Height(30)))
        {
            ImportFishJSON();
        }

        EditorGUILayout.Space();
        EditorGUILayout.HelpBox("JSON Format:\n{\n  \"fish\": [\n    {\n      \"fishType\": \"Tilapia\",\n      \"displayName\": \"Tilapia\",\n      \"difficulty\": 1,\n      \"educationalInfo\": \"...\",\n      \"bones\": [...]\n    }\n  ]\n}", MessageType.Info);
    }

    private void ImportFishJSON()
    {
        if (string.IsNullOrEmpty(jsonFilePath) || !File.Exists(jsonFilePath))
        {
            EditorUtility.DisplayDialog("Error", "Please select a valid JSON file!", "OK");
            return;
        }

        try
        {
            string jsonContent = File.ReadAllText(jsonFilePath);
            FishJSONData data = JsonConvert.DeserializeObject<FishJSONData>(jsonContent);

            if (data == null || data.fish == null || data.fish.Count == 0)
            {
                EditorUtility.DisplayDialog("Error", "JSON file is invalid or contains no fish data!", "OK");
                return;
            }

            // Create output folder if it doesn't exist
            string fullOutputPath = Path.Combine(Application.dataPath, outputFolderPath.Replace("Assets/", ""));
            if (!Directory.Exists(fullOutputPath))
            {
                Directory.CreateDirectory(fullOutputPath);
            }

            int successCount = 0;
            foreach (var fishData in data.fish)
            {
                FishDefinition fishDefinition = ScriptableObject.CreateInstance<FishDefinition>();
                fishDefinition.fishType = fishData.fishType;
                fishDefinition.displayName = fishData.displayName;
                
                // Convert difficulty int to enum
                if (fishData.difficulty == 1)
                    fishDefinition.difficulty = FishDefinition.DifficultyLevel.Beginner;
                else if (fishData.difficulty == 2)
                    fishDefinition.difficulty = FishDefinition.DifficultyLevel.Intermediate;
                else
                    fishDefinition.difficulty = FishDefinition.DifficultyLevel.Advanced;

                fishDefinition.educationalInfo = fishData.educationalInfo;
                fishDefinition.description = fishData.description ?? fishData.educationalInfo;
                fishDefinition.timeLimitSeconds = fishData.timeLimitSeconds;
                fishDefinition.spineBoneCount = fishData.spineBoneCount;
                fishDefinition.ribBoneCount = fishData.ribBoneCount;
                fishDefinition.pinBoneCount = fishData.pinBoneCount;
                fishDefinition.habitat = fishData.habitat ?? "";
                fishDefinition.cookingTips = fishData.cookingTips ?? "";

                // Convert bone data
                fishDefinition.bones = new List<BoneData>();
                foreach (var boneData in fishData.bones)
                {
                    BoneType boneType = BoneType.Pin;
                    if (boneData.boneType == "Spine")
                        boneType = BoneType.Spine;
                    else if (boneData.boneType == "Rib")
                        boneType = BoneType.Rib;
                    else if (boneData.boneType == "Pin")
                        boneType = BoneType.Pin;

                    BoneData bone = new BoneData(
                        boneData.name,
                        boneData.x,
                        boneData.y,
                        boneData.width,
                        boneData.height,
                        boneType
                    );
                    fishDefinition.bones.Add(bone);
                }

                // Save asset
                string assetPath = Path.Combine(outputFolderPath, fishData.fishType + ".asset");
                AssetDatabase.CreateAsset(fishDefinition, assetPath);
                successCount++;
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            EditorUtility.DisplayDialog("Success", $"Imported {successCount} fish definition(s) successfully!", "OK");

            if (createScene)
            {
                CreatePlayableScene();
            }
        }
        catch (System.Exception e)
        {
            EditorUtility.DisplayDialog("Error", "Failed to import JSON: " + e.Message, "OK");
            Debug.LogError("Import error: " + e);
        }
    }

    private void CreatePlayableScene()
    {
        EditorUtility.DisplayDialog("Info", "Scene creation feature coming soon!\nPlease set up the scene manually using the MainScene.unity template.", "OK");
    }

    [System.Serializable]
    private class FishJSONData
    {
        public List<FishJSONEntry> fish;
    }

    [System.Serializable]
    private class FishJSONEntry
    {
        public string fishType;
        public string displayName;
        public int difficulty;
        public string educationalInfo;
        public string description;
        public int timeLimitSeconds;
        public int spineBoneCount;
        public int ribBoneCount;
        public int pinBoneCount;
        public string habitat;
        public string cookingTips;
        public List<BoneJSONEntry> bones;
    }

    [System.Serializable]
    private class BoneJSONEntry
    {
        public string name;
        public float x;
        public float y;
        public float width;
        public float height;
        public string boneType;
    }
}

