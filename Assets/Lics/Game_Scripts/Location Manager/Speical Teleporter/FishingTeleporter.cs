using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Teleporter script that sends the player to the fishing minigame scene.
/// Place this on a GameObject with a Collider2D (set as Trigger) at the fishing location.
/// When the player enters, they'll be teleported to the fishing minigame scene.
/// </summary>
public class FishingTeleporter : MonoBehaviour
{
    [Header("Scene Settings")]
    [SerializeField] private string fishingSceneName = "freshwaterminigame"; // Changed from "FRESHWATERMINIGAME" to "freshwaterminigame")
    [SerializeField] private bool useSceneName = true; //If true, uses scene name. If false, uses scene index
    
    [Header("Trigger Settings")]
    [SerializeField] private string playerTag = "Player"; //Tag of the player GameObject
    [SerializeField] private KeyCode interactKey = KeyCode.E; //Key to press to teleport (optional, set to None for auto-teleport)
    [SerializeField] private bool autoTeleport = false; //If true, teleports immediately on enter. If false, requires key press
    
    [Header("Visual Feedback")]
    [SerializeField] private GameObject teleporterIcon; //Visual indicator (shining icon) that shows teleporter location
    [SerializeField] private GameObject promptPanel; //Optional UI panel to show "Press E to Fish" message
    [SerializeField] private UnityEngine.UI.Text promptText; //Text component for the prompt
    
    private bool playerInRange = false;
    private GameObject playerObject = null;
    
    private void Start() {
        //If using collider, make sure we have one
        Collider2D col = GetComponent<Collider2D>();
        if (col == null) {
            Debug.LogWarning("FishingTeleporter: No Collider2D found! Adding a CircleCollider2D as trigger.");
            CircleCollider2D circleCol = gameObject.AddComponent<CircleCollider2D>();
            circleCol.radius = 2f;
            circleCol.isTrigger = true;
        } else {
            col.isTrigger = true;
        }
        
        //Hide prompt panel initially
        if (promptPanel != null) {
            promptPanel.SetActive(false);
        }
        
        //Show teleporter icon
        if (teleporterIcon != null) {
            teleporterIcon.SetActive(true);
        }
    }
    
    private void Update() {
        //If player is in range and not auto-teleporting
        if (playerInRange && !autoTeleport) {
            //Check for interact key press
            if (Input.GetKeyDown(interactKey)) {
                TeleportToFishingScene();
            }
        }
    }
    
    //Called when player enters trigger
    private void OnTriggerEnter2D(Collider2D other) {
        if (other.CompareTag(playerTag)) {
            playerInRange = true;
            playerObject = other.gameObject;
            
            if (autoTeleport) {
                //Teleport immediately
                TeleportToFishingScene();
            } else {
                //Show prompt to press key
                ShowPrompt();
            }
        }
    }
    
    //Called when player exits trigger
    private void OnTriggerExit2D(Collider2D other) {
        if (other.CompareTag(playerTag)) {
            playerInRange = false;
            playerObject = null;
            HidePrompt();
        }
    }
    
    private void ShowPrompt() {
        if (promptPanel != null) {
            promptPanel.SetActive(true);
            
            if (promptText != null && interactKey != KeyCode.None) {
                promptText.text = $"Press {interactKey} to Fish";
            }
        }
    }
    
    private void HidePrompt() {
        if (promptPanel != null) {
            promptPanel.SetActive(false);
        }
    }
    
    //Teleport player to fishing scene
    private void TeleportToFishingScene() {
        //Save player position or other data if needed (optional)
        //PlayerPrefs.SetFloat("PlayerX", playerObject.transform.position.x);
        //PlayerPrefs.SetFloat("PlayerY", playerObject.transform.position.y);
        
        //Load the fishing scene
        if (useSceneName) {
            if (string.IsNullOrEmpty(fishingSceneName)) {
                Debug.LogError("Fishing Scene Name is not set in FishingTeleporter!");
                return;
            }
            
            //Check if scene exists in build settings
            if (!IsSceneInBuildSettings(fishingSceneName)) {
                Debug.LogError($"Scene '{fishingSceneName}' is not in Build Settings! " +
                    "Please go to File -> Build Settings and add the scene to the list.");
                return;
            }
            
            SceneManager.LoadScene(fishingSceneName);
        } else {
            Debug.LogWarning("Scene index loading not implemented. Please use scene name instead.");
        }
    }
    
    //Check if scene is in build settings
    private bool IsSceneInBuildSettings(string sceneName) {
        for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++) {
            string scenePath = SceneUtility.GetScenePathByBuildIndex(i);
            string sceneNameInBuild = System.IO.Path.GetFileNameWithoutExtension(scenePath);
            if (sceneNameInBuild == sceneName) {
                return true;
            }
        }
        return false;
    }
    
    //Visualize trigger area in editor
    private void OnDrawGizmosSelected() {
        Gizmos.color = Color.cyan;
        Collider2D col = GetComponent<Collider2D>();
        if (col != null && col is CircleCollider2D) {
            CircleCollider2D circleCol = col as CircleCollider2D;
            Gizmos.DrawWireSphere(transform.position, circleCol.radius);
        } else {
            Gizmos.DrawWireSphere(transform.position, 2f);
        }
    }
}

