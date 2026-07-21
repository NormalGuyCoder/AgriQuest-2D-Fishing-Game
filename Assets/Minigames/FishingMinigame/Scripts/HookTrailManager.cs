using UnityEngine;

public class HookTrailManager : MonoBehaviour
{
    private TrailRenderer trailRenderer;
    private LineRenderer lineRenderer;

    [SerializeField]
    private float width = 0.3f;

    [SerializeField]
    private int bufferTime = 3;

    [SerializeField]
    private Transform hookPoint;

    [Header("Line Settings")]
    public Color lineColor = Color.white;
    public int sortingOrder = 5;
    public Material lineMaterial;

    [Header("Position Settings")]
    public Vector3 startPositionOffset = Vector3.zero; // Offset from the start point
    public bool useCustomStartPosition = false; // Toggle to use custom start position
    public Transform customStartPoint; // Optional custom start point transform

    [Header("Trail Optimization (Multi-Monitor Fix)")]
    [Tooltip("Minimum distance between trail vertices. Higher values reduce vertices on large screens.")]
    [SerializeField] private float minVertexDistance = 0.2f;
    [Tooltip("Automatically adjust minVertexDistance based on screen resolution")]
    [SerializeField] private bool autoAdjustForResolution = true;

    private Transform startpoint;
    private bool propertiesDirty = true; // Track if properties need updating
    private float lastWidth;
    private int lastBufferTime;
    private Color lastLineColor;
    private int lastSortingOrder;

    private void Awake()
    {
        // Get or add TrailRenderer
        trailRenderer = GetComponentInChildren<TrailRenderer>();
        if (trailRenderer == null)
        {
            GameObject trailObj = new GameObject("Trail");
            trailObj.transform.SetParent(transform);
            trailRenderer = trailObj.AddComponent<TrailRenderer>();
            
            // Create a new material for the trail
            Material trailMaterial = new Material(Shader.Find("Unlit/Color"));
            trailMaterial.color = lineColor;
            trailRenderer.material = trailMaterial;
            
            ConfigureTrailRenderer();
        }
        else
        {
            // Configure existing TrailRenderer
            ConfigureTrailRenderer();
        }

        // Get or add LineRenderer
        lineRenderer = GetComponentInChildren<LineRenderer>();
        if (lineRenderer == null)
        {
            GameObject lineObj = new GameObject("Line");
            lineObj.transform.SetParent(transform);
            lineRenderer = lineObj.AddComponent<LineRenderer>();
            
            // Use provided material or create a new one
            if (lineMaterial == null)
            {
                lineMaterial = new Material(Shader.Find("Unlit/Color"));
                lineMaterial.color = lineColor;
            }
            lineRenderer.material = lineMaterial;
            
            ConfigureLineRenderer();
        }
        else
        {
            // Configure existing LineRenderer
            ConfigureLineRenderer();
        }

        // If hookPoint is not assigned, use this transform
        if (hookPoint == null)
        {
            hookPoint = transform;
        }

        // Initialize tracking variables
        lastWidth = width;
        lastBufferTime = bufferTime;
        lastLineColor = lineColor;
        lastSortingOrder = sortingOrder;
    }

    private void ConfigureTrailRenderer()
    {
        if (trailRenderer == null) return;

        // Calculate minVertexDistance based on resolution if enabled
        float adjustedMinVertexDistance = minVertexDistance;
        if (autoAdjustForResolution && Screen.width > 0)
        {
            // Adjust minVertexDistance based on screen height (larger screens = higher value)
            // Base adjustment: 1920p = 0.2f, scale proportionally
            float resolutionScale = Screen.height / 1080f;
            adjustedMinVertexDistance = Mathf.Clamp(minVertexDistance * resolutionScale, 0.1f, 1.0f);
        }

        trailRenderer.startWidth = width;
        trailRenderer.endWidth = width;
        trailRenderer.time = bufferTime;
        trailRenderer.startColor = lineColor;
        trailRenderer.endColor = lineColor;
        trailRenderer.sortingOrder = sortingOrder;
        
        // Optimization settings for multi-monitor support
        trailRenderer.minVertexDistance = adjustedMinVertexDistance;
        trailRenderer.alignment = LineAlignment.View; // View alignment works better across different resolutions
        trailRenderer.textureMode = LineTextureMode.Stretch;
        trailRenderer.shadowBias = 0.5f;
        trailRenderer.generateLightingData = false;
        
        // Disable shadows for better performance
        trailRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        trailRenderer.receiveShadows = false;
    }

    private void ConfigureLineRenderer()
    {
        if (lineRenderer == null) return;

        lineRenderer.startWidth = width;
        lineRenderer.endWidth = width;
        lineRenderer.positionCount = 2;
        lineRenderer.startColor = lineColor;
        lineRenderer.endColor = lineColor;
        lineRenderer.sortingOrder = sortingOrder;
        lineRenderer.generateLightingData = false;
        lineRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        lineRenderer.receiveShadows = false;
    }

    private void Update()
    {
        // Only update TrailRenderer properties if they've changed
        if (trailRenderer != null)
        {
            // Check if properties have changed
            if (propertiesDirty || 
                lastWidth != width || 
                lastBufferTime != bufferTime || 
                lastLineColor != lineColor || 
                lastSortingOrder != sortingOrder)
            {
                ConfigureTrailRenderer();
                
                // Update tracking variables
                lastWidth = width;
                lastBufferTime = bufferTime;
                lastLineColor = lineColor;
                lastSortingOrder = sortingOrder;
                propertiesDirty = false;
            }
        }

        // LineRenderer position updates (this needs to happen every frame)
        if (lineRenderer != null)
        {
            // Only update properties if they've changed
            if (propertiesDirty || 
                lastWidth != width || 
                lastLineColor != lineColor || 
                lastSortingOrder != sortingOrder)
            {
                lineRenderer.startWidth = width;
                lineRenderer.endWidth = width;
                lineRenderer.startColor = lineColor;
                lineRenderer.endColor = lineColor;
                lineRenderer.sortingOrder = sortingOrder;
            }

            lineRenderer.enabled = true;

            if (startpoint != null && hookPoint != null)
            {
                // Calculate the start position based on settings
                Vector3 startPos;
                if (useCustomStartPosition && customStartPoint != null)
                {
                    startPos = customStartPoint.position;
                }
                else
                {
                    startPos = startpoint.position + startPositionOffset;
                }
                
                lineRenderer.SetPosition(0, startPos);
                lineRenderer.SetPosition(1, hookPoint.position);
            }
        }
    }

    private void OnEnable()
    {
        // Reconfigure when enabled to handle resolution changes
        propertiesDirty = true;
        if (trailRenderer != null)
        {
            ConfigureTrailRenderer();
        }
        if (lineRenderer != null)
        {
            ConfigureLineRenderer();
        }
    }

    public void enableRetractionLine(Transform newStartPoint)
    {
        if (trailRenderer != null)
        {
            trailRenderer.enabled = false;
        }
        
        if (lineRenderer != null)
        {
            lineRenderer.enabled = true;
        }
        
        startpoint = newStartPoint;
    }

    // Helper method to set the start position offset
    public void SetStartPositionOffset(Vector3 offset)
    {
        startPositionOffset = offset;
    }

    // Helper method to set a custom start point
    public void SetCustomStartPoint(Transform newStartPoint)
    {
        customStartPoint = newStartPoint;
        useCustomStartPosition = true;
    }

    // Helper method to reset to default start point
    public void ResetStartPoint()
    {
        useCustomStartPosition = false;
        customStartPoint = null;
    }
} 