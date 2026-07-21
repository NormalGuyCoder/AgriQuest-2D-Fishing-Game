using UnityEngine;
using UnityEngine.UI;

public class ToolController : MonoBehaviour
{
    [Header("Tool Buttons")]
    public Button knifeButton;
    public Button tweezersButton;

    [Header("Tool Visuals")]
    public Image knifeIcon;
    public Image tweezersIcon;

    [Header("Cursor Control")]
    public CursorController cursorController; // Optional: assign if you want cursor changes

    private ToolType currentTool = ToolType.Knife;

    void Start()
    {
        SetTool(ToolType.Knife);

        if (knifeButton != null)
            knifeButton.onClick.AddListener(() => SetTool(ToolType.Knife));

        if (tweezersButton != null)
            tweezersButton.onClick.AddListener(() => SetTool(ToolType.Tweezers));

        // Find cursor controller if not assigned
        if (cursorController == null)
        {
            cursorController = CursorController.Instance;
        }
    }

    public void SetTool(ToolType tool)
    {
        currentTool = tool;
        UpdateToolVisuals();
        UpdateCursor();
    }

    public ToolType GetCurrentTool()
    {
        return currentTool;
    }

    private void UpdateToolVisuals()
    {
        // Highlight selected tool, dim unselected
        if (knifeIcon != null)
        {
            knifeIcon.color = (currentTool == ToolType.Knife) ? Color.white : Color.gray;
        }

        if (tweezersIcon != null)
        {
            tweezersIcon.color = (currentTool == ToolType.Tweezers) ? Color.white : Color.gray;
        }
    }

    private void UpdateCursor()
    {
        // Update cursor if cursor controller exists
        if (cursorController != null)
        {
            cursorController.UpdateCursor(currentTool);
        }
        else if (CursorController.Instance != null)
        {
            CursorController.Instance.UpdateCursor(currentTool);
        }
    }
}

public enum ToolType
{
    Knife,
    Tweezers
}




