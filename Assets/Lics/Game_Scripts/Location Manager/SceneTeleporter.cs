using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTeleporter : MonoBehaviour
{
    [Header("Destination Settings")]
    public string targetScene = "";
    public string spawnAt = "default";

    [Header("Trigger Area")]
    public Vector2 triggerSize = new Vector2(1, 1);

    [Header("Trigger Visualization")]
    public bool alwaysShowGizmo = true;
    public Color gizmoColor = new Color(0, 1, 0, 0.3f);

    [Header("Transition Settings")]
    public string transitionName = "CrossFade";
    public string musicName = "";

    private BoxCollider2D triggerCollider;

    void Awake()
    {
        triggerCollider = GetComponent<BoxCollider2D>();
        if (triggerCollider == null)
        {
            triggerCollider = gameObject.AddComponent<BoxCollider2D>();
        }

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

    void OnDrawGizmosSelected()
    {
        if (!alwaysShowGizmo)
        {
            Gizmos.color = new Color(0, 1, 0, 0.3f);

            Vector3 colliderCenter = transform.position + new Vector3(triggerSize.x * 0.5f, triggerSize.y * 0.5f, 0);
            Vector3 worldSize = new Vector3(triggerSize.x, triggerSize.y, 0.1f);

            Gizmos.DrawCube(colliderCenter, worldSize);
        }
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
            Teleport();
        }
    }

    public void Teleport()
    {
        if (string.IsNullOrEmpty(targetScene))
            return;

        // Auto-save before teleporting
        if (LocationManager.Instance != null)
        {
            LocationManager.Instance.SaveCurrentGameState();
        }

        if (!string.IsNullOrEmpty(musicName) && MusicManager.Instance != null)
            MusicManager.Instance.PlayMusic(musicName);

        if (LevelManager.Instance != null)
            LevelManager.Instance.LoadScene(targetScene, transitionName, spawnAt);
        else
            SceneManager.LoadScene(targetScene);
    }
}