using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// Handles visual transitions when the fish is opened with the knife.
/// Attach this to the root of each fish prefab.
/// </summary>
public class FishOpeningVisuals : MonoBehaviour
{
    [Header("Fish Parts")]
    public GameObject topSkinObject;      // The outer layer that gets removed
    public GameObject innerFishObject;    // The interior revealed after opening

    [Header("Animation Settings")]
    [Tooltip("Duration of the opening animation in seconds")]
    public float animationDuration = 0.8f;
    [Tooltip("Animation curve for the fade effect")]
    public AnimationCurve fadeCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    [Tooltip("Should the skin slide/scale away?")]
    public bool useSlideAnimation = true;
    [Tooltip("Direction to slide the skin (normalized vector)")]
    public Vector2 slideDirection = new Vector2(0, 1); // Slide up by default

    [Header("Optional Effects")]
    public ParticleSystem openingParticles;
    public string openingSoundName;       // Reserved for future audio integration

    private bool hasOpened = false;
    private Coroutine currentAnimation;

    /// <summary>
    /// Ensure the prefab is in its default (closed) state.
    /// Called from FishBoardController when a fish prefab is instantiated or reset.
    /// </summary>
    public void ResetVisuals()
    {
        hasOpened = false;

        // Stop any running animation
        if (currentAnimation != null)
        {
            StopCoroutine(currentAnimation);
            currentAnimation = null;
        }

        if (topSkinObject != null)
        {
            topSkinObject.SetActive(true);
            // Reset transform and alpha
            ResetSkinTransform();
        }

        if (innerFishObject != null)
        {
            innerFishObject.SetActive(false);
            // Reset interior alpha
            ResetInteriorTransform();
        }
    }

    /// <summary>
    /// Trigger the visual change when the fish is opened with the knife.
    /// </summary>
    public void PlayOpen()
    {
        if (hasOpened)
            return;

        hasOpened = true;

        // Start the animation coroutine
        if (currentAnimation != null)
        {
            StopCoroutine(currentAnimation);
        }
        currentAnimation = StartCoroutine(AnimateOpening());
    }

    private IEnumerator AnimateOpening()
    {
        // Get components for animation
        CanvasGroup skinCanvasGroup = GetOrAddCanvasGroup(topSkinObject);
        CanvasGroup interiorCanvasGroup = GetOrAddCanvasGroup(innerFishObject);
        RectTransform skinRect = topSkinObject != null ? topSkinObject.GetComponent<RectTransform>() : null;

        // Store initial positions
        Vector3 skinStartPos = skinRect != null ? skinRect.localPosition : Vector3.zero;
        Vector3 skinStartScale = skinRect != null ? skinRect.localScale : Vector3.one;

        // Activate interior immediately but make it transparent
        if (innerFishObject != null)
        {
            innerFishObject.SetActive(true);
            if (interiorCanvasGroup != null)
            {
                interiorCanvasGroup.alpha = 0f;
            }
        }

        float elapsed = 0f;

        while (elapsed < animationDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / animationDuration);
            float curveValue = fadeCurve.Evaluate(t);

            // Animate skin fading out
            if (skinCanvasGroup != null)
            {
                skinCanvasGroup.alpha = 1f - curveValue; // Fade out
            }

            // Animate interior fading in
            if (interiorCanvasGroup != null)
            {
                interiorCanvasGroup.alpha = curveValue; // Fade in
            }

            // Animate skin sliding/scaling away
            if (useSlideAnimation && skinRect != null)
            {
                // Slide in direction
                Vector2 slideOffset = slideDirection * (curveValue * 50f); // Adjust 50f for slide distance
                skinRect.localPosition = skinStartPos + new Vector3(slideOffset.x, slideOffset.y, 0);

                // Optional: Scale down as it slides
                float scale = 1f - (curveValue * 0.3f); // Scale down to 70%
                skinRect.localScale = skinStartScale * scale;
            }

            yield return null;
        }

        // Ensure final state
        if (skinCanvasGroup != null)
        {
            skinCanvasGroup.alpha = 0f;
        }
        if (interiorCanvasGroup != null)
        {
            interiorCanvasGroup.alpha = 1f;
        }

        // Hide skin completely after animation
        if (topSkinObject != null)
        {
            topSkinObject.SetActive(false);
        }

        // Play particles if available
        if (openingParticles != null)
        {
            openingParticles.Play();
        }

        // Hook your audio system here if needed:
        // if (!string.IsNullOrEmpty(openingSoundName))
        // {
        //     AudioManager.Instance.PlaySound(openingSoundName);
        // }

        currentAnimation = null;
    }

    private CanvasGroup GetOrAddCanvasGroup(GameObject obj)
    {
        if (obj == null)
            return null;

        CanvasGroup cg = obj.GetComponent<CanvasGroup>();
        if (cg == null)
        {
            cg = obj.AddComponent<CanvasGroup>();
        }
        return cg;
    }

    private void ResetSkinTransform()
    {
        if (topSkinObject == null)
            return;

        RectTransform rect = topSkinObject.GetComponent<RectTransform>();
        if (rect != null)
        {
            rect.localPosition = Vector3.zero;
            rect.localScale = Vector3.one;
        }

        CanvasGroup cg = topSkinObject.GetComponent<CanvasGroup>();
        if (cg != null)
        {
            cg.alpha = 1f;
        }
    }

    private void ResetInteriorTransform()
    {
        if (innerFishObject == null)
            return;

        CanvasGroup cg = innerFishObject.GetComponent<CanvasGroup>();
        if (cg != null)
        {
            cg.alpha = 0f;
        }
    }
}
