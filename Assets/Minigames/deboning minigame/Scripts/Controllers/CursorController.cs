using UnityEngine;

/// <summary>
/// Manages cursor changes based on selected tool.
/// Attach this to a GameObject in your scene (or add to GameManager).
/// </summary>
public class CursorController : MonoBehaviour
{
    public static CursorController Instance { get; private set; }

    [Header("Cursor Textures")]
    [Tooltip("Cursor sprite when knife is selected")]
    public Texture2D knifeCursor;
    [Tooltip("Cursor sprite when tweezers are selected")]
    public Texture2D tweezersCursor;
    [Tooltip("Default cursor (when no tool selected)")]
    public Texture2D defaultCursor;

    [Header("Cursor Settings")]
    [Tooltip("Hotspot offset from top-left corner (where the click point is)")]
    public Vector2 cursorHotspot = new Vector2(0, 0);
    [Tooltip("Should cursor be visible?")]
    public bool showCursor = true;
    [Tooltip("Cursor size multiplier (1.0 = original size, 2.0 = double size, 0.5 = half size)")]
    [Range(0.5f, 5f)]
    public float cursorSize = 1.5f;

    [Header("Cursor Rotation")]
    [Tooltip("Rotation angle for knife cursor (in degrees)")]
    [Range(0, 360)]
    public float knifeRotation = 0f;
    [Tooltip("Rotation angle for tweezers cursor (in degrees)")]
    [Range(0, 360)]
    public float tweezersRotation = 0f;

    private ToolType currentTool = ToolType.Knife;
    private Texture2D rotatedKnifeCursor;
    private Texture2D rotatedTweezersCursor;
    private Texture2D scaledKnifeCursor;
    private Texture2D scaledTweezersCursor;

    // Track previous values to detect changes
    private float prevCursorSize = 1.5f;
    private float prevKnifeRotation = 0f;
    private float prevTweezersRotation = 0f;
    private Vector2 prevCursorHotspot = Vector2.zero;
    private bool useCustomHotspot = false;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        // Initialize previous values
        prevCursorSize = cursorSize;
        prevKnifeRotation = knifeRotation;
        prevTweezersRotation = tweezersRotation;
        prevCursorHotspot = cursorHotspot;
        useCustomHotspot = (cursorHotspot != Vector2.zero);

        // Create rotated and scaled versions of cursors if needed
        RefreshCursors();
        
        // Set initial cursor
        UpdateCursor(ToolType.Knife);
    }

    void Update()
    {
        // Check if values changed during Play Mode
        bool needsRefresh = false;

        if (cursorSize != prevCursorSize)
        {
            needsRefresh = true;
            prevCursorSize = cursorSize;
        }

        if (knifeRotation != prevKnifeRotation)
        {
            needsRefresh = true;
            prevKnifeRotation = knifeRotation;
        }

        if (tweezersRotation != prevTweezersRotation)
        {
            needsRefresh = true;
            prevTweezersRotation = tweezersRotation;
        }

        if (cursorHotspot != prevCursorHotspot)
        {
            needsRefresh = true;
            prevCursorHotspot = cursorHotspot;
            useCustomHotspot = (cursorHotspot != Vector2.zero);
        }

        if (needsRefresh)
        {
            RefreshCursors();
            // Update current cursor with new settings
            UpdateCursor(currentTool);
        }
    }

    /// <summary>
    /// Called when values change in Inspector (Edit Mode)
    /// </summary>
    void OnValidate()
    {
        // Only regenerate if in Play Mode
        if (Application.isPlaying)
        {
            RefreshCursors();
            if (currentTool != ToolType.Knife && currentTool != ToolType.Tweezers)
            {
                UpdateCursor(ToolType.Knife);
            }
            else
            {
                UpdateCursor(currentTool);
            }
        }
    }

    /// <summary>
    /// Refresh/regenerate all cursors (call this when settings change)
    /// </summary>
    public void RefreshCursors()
    {
        // Clean up old textures
        CleanupRotatedTextures();
        CleanupScaledTextures();

        // Recreate rotated cursors
        CreateRotatedCursors();

        // Recreate scaled cursors
        CreateScaledCursors();
    }

    void OnDestroy()
    {
        // Clean up rotated and scaled textures
        CleanupRotatedTextures();
        CleanupScaledTextures();
        // Reset cursor when script is destroyed
        ResetCursor();
    }

    private void CreateRotatedCursors()
    {
        // Create rotated knife cursor if rotation is set (even if 0, to allow for changes)
        if (knifeCursor != null)
        {
            if (knifeRotation != 0f)
            {
                rotatedKnifeCursor = RotateTexture(knifeCursor, knifeRotation);
            }
            // If rotation is 0, rotatedKnifeCursor should be null (use original)
        }

        // Create rotated tweezers cursor if rotation is set
        if (tweezersCursor != null)
        {
            if (tweezersRotation != 0f)
            {
                rotatedTweezersCursor = RotateTexture(tweezersCursor, tweezersRotation);
            }
            // If rotation is 0, rotatedTweezersCursor should be null (use original)
        }
    }

    private void CleanupRotatedTextures()
    {
        if (rotatedKnifeCursor != null)
        {
            DestroyImmediate(rotatedKnifeCursor);
            rotatedKnifeCursor = null;
        }

        if (rotatedTweezersCursor != null)
        {
            DestroyImmediate(rotatedTweezersCursor);
            rotatedTweezersCursor = null;
        }
    }

    private void CleanupScaledTextures()
    {
        if (scaledKnifeCursor != null)
        {
            DestroyImmediate(scaledKnifeCursor);
            scaledKnifeCursor = null;
        }

        if (scaledTweezersCursor != null)
        {
            DestroyImmediate(scaledTweezersCursor);
            scaledTweezersCursor = null;
        }
    }

    private void CreateScaledCursors()
    {
        // Clean up old scaled textures first
        CleanupScaledTextures();

        // Scale knife cursor if size is not 1.0
        if (knifeCursor != null && cursorSize != 1.0f)
        {
            Texture2D sourceTexture = (rotatedKnifeCursor != null) ? rotatedKnifeCursor : knifeCursor;
            scaledKnifeCursor = ScaleTexture(sourceTexture, cursorSize);
        }

        // Scale tweezers cursor if size is not 1.0
        if (tweezersCursor != null && cursorSize != 1.0f)
        {
            Texture2D sourceTexture = (rotatedTweezersCursor != null) ? rotatedTweezersCursor : tweezersCursor;
            scaledTweezersCursor = ScaleTexture(sourceTexture, cursorSize);
        }
    }

    private Texture2D ScaleTexture(Texture2D source, float scale)
    {
        if (source == null)
            return null;

        // Make sure texture is readable
        if (!IsTextureReadable(source))
        {
            Debug.LogWarning($"CursorController: Cannot scale texture '{source.name}' - it's not readable!");
            return source;
        }

        int newWidth = Mathf.RoundToInt(source.width * scale);
        int newHeight = Mathf.RoundToInt(source.height * scale);

        // Ensure minimum size
        if (newWidth < 1) newWidth = 1;
        if (newHeight < 1) newHeight = 1;

        // Create scaled texture
        Texture2D scaled = new Texture2D(newWidth, newHeight, TextureFormat.RGBA32, false);

        // Scale using bilinear sampling
        for (int y = 0; y < newHeight; y++)
        {
            for (int x = 0; x < newWidth; x++)
            {
                // Map to original texture coordinates
                float u = (float)x / newWidth;
                float v = (float)y / newHeight;

                float srcX = u * source.width;
                float srcY = v * source.height;

                Color color = SampleTextureBilinear(source, srcX, srcY);
                scaled.SetPixel(x, y, color);
            }
        }

        scaled.Apply();
        return scaled;
    }

    private Texture2D RotateTexture(Texture2D originalTexture, float angleDegrees)
    {
        if (originalTexture == null)
            return null;

        // Make sure texture is readable
        if (!IsTextureReadable(originalTexture))
        {
            Debug.LogWarning($"CursorController: Cannot rotate texture '{originalTexture.name}' - it's not readable!");
            return originalTexture;
        }

        int width = originalTexture.width;
        int height = originalTexture.height;
        
        // Create new texture for rotated version
        Texture2D rotated = new Texture2D(width, height, TextureFormat.RGBA32, false);
        
        // Calculate rotation in radians
        float angleRad = angleDegrees * Mathf.Deg2Rad;
        float cos = Mathf.Cos(angleRad);
        float sin = Mathf.Sin(angleRad);
        
        // Center point
        float centerX = width / 2f;
        float centerY = height / 2f;
        
        // Rotate each pixel
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                // Calculate position relative to center
                float dx = x - centerX;
                float dy = y - centerY;
                
                // Apply rotation
                float rotatedX = dx * cos - dy * sin + centerX;
                float rotatedY = dx * sin + dy * cos + centerY;
                
                // Sample original texture (with bilinear filtering)
                Color color = SampleTextureBilinear(originalTexture, rotatedX, rotatedY);
                rotated.SetPixel(x, y, color);
            }
        }
        
        rotated.Apply();
        return rotated;
    }

    private Color SampleTextureBilinear(Texture2D tex, float x, float y)
    {
        // Clamp coordinates
        x = Mathf.Clamp(x, 0, tex.width - 1);
        y = Mathf.Clamp(y, 0, tex.height - 1);
        
        // Get integer coordinates
        int x1 = Mathf.FloorToInt(x);
        int y1 = Mathf.FloorToInt(y);
        int x2 = Mathf.Min(x1 + 1, tex.width - 1);
        int y2 = Mathf.Min(y1 + 1, tex.height - 1);
        
        // Get fractional parts
        float fx = x - x1;
        float fy = y - y1;
        
        // Sample four corners
        Color c11 = tex.GetPixel(x1, y1);
        Color c21 = tex.GetPixel(x2, y1);
        Color c12 = tex.GetPixel(x1, y2);
        Color c22 = tex.GetPixel(x2, y2);
        
        // Bilinear interpolation
        Color c1 = Color.Lerp(c11, c21, fx);
        Color c2 = Color.Lerp(c12, c22, fx);
        return Color.Lerp(c1, c2, fy);
    }

    /// <summary>
    /// Update cursor based on selected tool
    /// </summary>
    public void UpdateCursor(ToolType tool)
    {
        currentTool = tool;

        if (!showCursor)
        {
            Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
            return;
        }

        Texture2D cursorTexture = null;

        switch (tool)
        {
            case ToolType.Knife:
                // Priority: scaled > rotated > original
                if (scaledKnifeCursor != null)
                    cursorTexture = scaledKnifeCursor;
                else if (rotatedKnifeCursor != null)
                    cursorTexture = rotatedKnifeCursor;
                else
                    cursorTexture = knifeCursor;
                break;
            case ToolType.Tweezers:
                // Priority: scaled > rotated > original
                if (scaledTweezersCursor != null)
                    cursorTexture = scaledTweezersCursor;
                else if (rotatedTweezersCursor != null)
                    cursorTexture = rotatedTweezersCursor;
                else
                    cursorTexture = tweezersCursor;
                break;
            default:
                cursorTexture = defaultCursor;
                break;
        }

        if (cursorTexture != null)
        {
            // Check if texture is readable (required for cursor)
            if (!IsTextureReadable(cursorTexture))
            {
                Debug.LogWarning($"CursorController: Texture '{cursorTexture.name}' is not readable. Please enable 'Read/Write Enabled' in import settings.");
                // Try to use default cursor instead
                if (defaultCursor != null && IsTextureReadable(defaultCursor))
                {
                    Cursor.SetCursor(defaultCursor, cursorHotspot, CursorMode.Auto);
                }
                else
                {
                    Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
                }
                return;
            }

            // Calculate hotspot (click point) - scale it based on cursor size
            Vector2 hotspot = CalculateHotspot(cursorTexture, tool);
            Cursor.SetCursor(cursorTexture, hotspot, CursorMode.Auto);
        }
        else
        {
            // Fallback to default cursor if texture not assigned
            if (defaultCursor != null && IsTextureReadable(defaultCursor))
            {
                Cursor.SetCursor(defaultCursor, cursorHotspot, CursorMode.Auto);
            }
            else
            {
                Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
            }
        }
    }

    /// <summary>
    /// Calculate the hotspot (click point) for the cursor
    /// </summary>
    private Vector2 CalculateHotspot(Texture2D texture, ToolType tool)
    {
        if (texture == null)
            return cursorHotspot;

        // If custom hotspot is set (not zero), use it
        if (useCustomHotspot && cursorHotspot != Vector2.zero)
        {
            // Scale the hotspot based on cursor size if texture is scaled
            if (cursorSize != 1.0f)
            {
                return cursorHotspot * cursorSize;
            }
            return cursorHotspot;
        }

        // Otherwise, use automatic calculation based on tool type
        Vector2 hotspot = new Vector2(texture.width / 2f, texture.height / 2f);

        // Adjust based on tool type for better feel
        switch (tool)
        {
            case ToolType.Knife:
                // Knife tip is usually at bottom-center
                hotspot = new Vector2(texture.width / 2f, texture.height * 0.8f);
                break;
            case ToolType.Tweezers:
                // Tweezers tip is usually at bottom-center
                hotspot = new Vector2(texture.width / 2f, texture.height * 0.8f);
                break;
        }

        return hotspot;
    }

    /// <summary>
    /// Check if texture is readable (required for cursor)
    /// </summary>
    private bool IsTextureReadable(Texture2D texture)
    {
        if (texture == null)
            return false;

        try
        {
            // Try to access pixel data - if this fails, texture is not readable
            texture.GetPixel(0, 0);
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Reset cursor to default
    /// </summary>
    public void ResetCursor()
    {
        if (defaultCursor != null && IsTextureReadable(defaultCursor))
        {
            Cursor.SetCursor(defaultCursor, cursorHotspot, CursorMode.Auto);
        }
        else
        {
            Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
        }
    }
}

