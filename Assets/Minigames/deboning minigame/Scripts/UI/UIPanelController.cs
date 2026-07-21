using UnityEngine;

public class UIPanelController : MonoBehaviour
{
    public GameObject root;
    public CanvasGroup canvasGroup; // optional

    public void Open()
    {
        if (root != null) root.SetActive(true);
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 1f;
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
        }
    }

    public void Close()
    {
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }
        if (root != null) root.SetActive(false);
    }
}