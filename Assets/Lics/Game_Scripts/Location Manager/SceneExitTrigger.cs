using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneExitTrigger : MonoBehaviour
{
    [Header("Exit Settings")]
    public string exitId = "south_exit";
    public string targetScene = "Beachside";
    public string transitionName = "CrossFade";

    [Header("Trigger Area")]
    public Vector2 triggerSize = new Vector2(1, 1);

    [Header("Trigger Visualization")]
    public bool alwaysShowGizmo = true;
    public Color gizmoColor = new Color(1, 0, 0, 0.3f);

    [Header("Interaction")]
    public KeyCode interactKey = KeyCode.E;

    private BoxCollider2D triggerCollider;
    private bool playerInRange = false;

    void Awake()
    {
        triggerCollider = GetComponent<BoxCollider2D>();
        if (triggerCollider == null)
            triggerCollider = gameObject.AddComponent<BoxCollider2D>();

        SetupTrigger();
    }

    void Start()
    {
        triggerCollider.isTrigger = true;
    }

    void SetupTrigger()
    {
        triggerCollider.size = triggerSize;
        triggerCollider.offset = new Vector2(triggerSize.x * 0.5f, triggerSize.y * 0.5f);
    }

    void OnDrawGizmos()
    {
        if (!alwaysShowGizmo) return;

        Gizmos.color = gizmoColor;

        Vector3 colliderCenter = transform.position + new Vector3(triggerSize.x * 0.5f, triggerSize.y * 0.5f, 0);
        Vector3 worldSize = new Vector3(triggerSize.x, triggerSize.y, 0.1f);

        Gizmos.DrawCube(colliderCenter, worldSize);
    }

    void OnValidate()
    {
        BoxCollider2D collider = GetComponent<BoxCollider2D>();
        if (collider != null)
        {
            collider.size = triggerSize;
            collider.offset = new Vector2(triggerSize.x * 0.5f, triggerSize.y * 0.5f);
            collider.isTrigger = true;
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
        }
    }

    void Update()
    {
        if (playerInRange && Input.GetKeyDown(interactKey))
            ExitToScene();
    }

    public void ExitToScene()
    {
        if (string.IsNullOrEmpty(targetScene))
            return;

        if (LevelManager.Instance != null)
            LevelManager.Instance.LoadScene(targetScene, transitionName, exitId);
        else
            SceneManager.LoadScene(targetScene);
    }
}