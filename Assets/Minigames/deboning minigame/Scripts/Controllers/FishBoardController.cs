using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class FishBoardController : MonoBehaviour
{
    [Header("References")]
    public Transform boneMarkersParent;
    public GameObject boneButtonPrefab;
    public Image fishSpriteImage; // UI Image component for fish sprite
    public Transform fishPrefabParent; // Optional parent to instantiate fish prefab with BoneMarkers
    public Button fishBoardButton; // Clickable button for the fish board (to open fish)
    public Image cutLineImage; // Visual cut line that appears when fish is opened (optional)

    [Header("Opening Animation")]
    public float cutLineAnimationDuration = 0.5f;
    public AnimationCurve cutLineAnimationCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    private FishDefinition currentFish;
    private BoneButton[] boneButtons;
    private GameObject activeFishInstance;
    private bool isFishOpened = false;
    private FishOpeningVisuals fishOpeningVisuals; // Reference to the opening visuals component

    void Start()
    {
        // Set up fish board button if it exists
        if (fishBoardButton != null)
        {
            fishBoardButton.onClick.AddListener(OnFishBoardClicked);
        }

        // Hide cut line initially
        if (cutLineImage != null)
        {
            cutLineImage.gameObject.SetActive(false);
        }
    }

    public void SetupFish(FishDefinition fish)
    {
        if (fish == null)
        {
            Debug.LogError("FishBoardController: Cannot setup null fish!");
            return;
        }

        currentFish = fish;
        isFishOpened = false; // Reset opened state
        
        // Clear previous bones/buttons
        if (boneButtons != null)
        {
            foreach (var button in boneButtons)
            {
                if (button != null)
                    Destroy(button.gameObject);
            }
        }
        boneButtons = null;

        // Clear previous prefab instance
        if (activeFishInstance != null)
        {
            Destroy(activeFishInstance);
            activeFishInstance = null;
        }
        fishOpeningVisuals = null; // Clear reference

        // Hide cut line
        if (cutLineImage != null)
        {
            cutLineImage.gameObject.SetActive(false);
        }

        // Prefer prefab with BoneMarker components if provided
        if (fish.fishPrefab != null)
        {
            if (fishSpriteImage != null)
            {
                fishSpriteImage.enabled = false; // Hide static sprite if prefab is used
            }
            BuildFromPrefabMarkers();
        }
        else
        {
            // Set fish sprite
            if (fishSpriteImage != null && fish.fishSprite != null)
            {
                fishSpriteImage.enabled = true;
                fishSpriteImage.sprite = fish.fishSprite;
            }
            else if (fishSpriteImage == null)
            {
                Debug.LogWarning("FishBoardController: Fish Sprite Image is not assigned!");
            }

            // Spawn bone markers from data (legacy mode)
            SpawnBoneMarkers();
        }
    }

    private void SpawnBoneMarkers()
    {
        if (currentFish == null)
        {
            Debug.LogError("FishBoardController: Current fish is null!");
            return;
        }
        
        if (boneButtonPrefab == null)
        {
            Debug.LogError("FishBoardController: Bone Button Prefab is not assigned! Assign it in the Inspector.");
            return;
        }

        if (boneMarkersParent == null)
        {
            Debug.LogError("FishBoardController: Bone Markers Parent is not assigned! Assign it in the Inspector.");
            return;
        }

        boneButtons = new BoneButton[currentFish.bones.Count];

        for (int i = 0; i < currentFish.bones.Count; i++)
        {
            BoneData bone = currentFish.bones[i];
            
            // Create bone button
            GameObject boneObj = Instantiate(boneButtonPrefab, boneMarkersParent);
            boneObj.name = "Bone_" + bone.name;

            // Set position (assuming board is 600x400, centered at origin)
            // Convert pixel coordinates to world coordinates
            float worldX = (bone.x - 300f) / 100f; // Adjust based on your scale
            float worldY = (200f - bone.y) / 100f; // Flip Y for Unity coordinate system
            
            boneObj.transform.localPosition = new Vector3(worldX, worldY, 0);
            
            // Set size
            RectTransform rect = boneObj.GetComponent<RectTransform>();
            if (rect != null)
            {
                rect.sizeDelta = new Vector2(bone.width, bone.height);
            }

            // Setup BoneButton component
            BoneButton boneButton = boneObj.GetComponent<BoneButton>();
            if (boneButton == null)
            {
                boneButton = boneObj.AddComponent<BoneButton>();
            }
            
            boneButton.Initialize(bone, currentFish);
            boneButtons[i] = boneButton;
        }
    }

    private void BuildFromPrefabMarkers()
    {
        if (currentFish == null)
        {
            Debug.LogError("FishBoardController: Current fish is null!");
            return;
        }

        // Determine parent for prefab instance
        Transform parent = fishPrefabParent != null ? fishPrefabParent : (boneMarkersParent != null ? boneMarkersParent.parent : this.transform);
        if (parent == null)
        {
            parent = this.transform;
        }

        // Instantiate fish prefab
        activeFishInstance = Instantiate(currentFish.fishPrefab, parent);
        activeFishInstance.name = "FishPrefab_" + currentFish.displayName;

        // Find FishOpeningVisuals component on the prefab
        fishOpeningVisuals = activeFishInstance.GetComponent<FishOpeningVisuals>();
        if (fishOpeningVisuals != null)
        {
            fishOpeningVisuals.ResetVisuals(); // Ensure fish starts in closed state
        }
        else
        {
            Debug.LogWarning($"FishBoardController: No FishOpeningVisuals component found on prefab '{currentFish.displayName}'. Opening animation will not work.");
        }

        // Find BoneMarker components in children
        var markers = activeFishInstance.GetComponentsInChildren<BoneMarker>(true);
        if (markers == null || markers.Length == 0)
        {
            Debug.LogWarning("FishBoardController: No BoneMarker components found in fish prefab '" + currentFish.displayName + "'.");
        }

        // Rebuild fish bones list from markers (runtime only)
        currentFish.bones = new List<BoneData>();

        int spineCount = 0, ribCount = 0, pinCount = 0;
        boneButtons = new BoneButton[markers.Length];
        for (int i = 0; i < markers.Length; i++)
        {
            BoneMarker marker = markers[i];

            // Ensure there is a RectTransform for sizing
            RectTransform rect = marker.GetComponent<RectTransform>();
            if (rect == null)
            {
                rect = marker.gameObject.AddComponent<RectTransform>();
            }

            // Create BoneData based on marker
            BoneType mappedType = BoneType.Pin;
            switch (marker.boneType)
            {
                case BoneMarker.BoneType.Spine: mappedType = BoneType.Spine; spineCount++; break;
                case BoneMarker.BoneType.Rib: mappedType = BoneType.Rib; ribCount++; break;
                case BoneMarker.BoneType.Pin: mappedType = BoneType.Pin; pinCount++; break;
            }

            // Use current local position within a 600x400 logical board where (0,0) is center
            Vector2 size = rect.sizeDelta;
            Vector3 lp = rect.localPosition;
            float x = 300f + lp.x * 100f; // inverse of legacy conversion
            float y = 200f - lp.y * 100f;

            var boneData = new BoneData(string.IsNullOrEmpty(marker.markerName) ? marker.gameObject.name : marker.markerName,
                                        x,
                                        y,
                                        size.x,
                                        size.y,
                                        mappedType);
            currentFish.bones.Add(boneData);

            // Ensure marker has a Button/Image and BoneButton to handle clicks
            Button button = marker.GetComponent<Button>();
            if (button == null)
            {
                button = marker.gameObject.AddComponent<Button>();
            }
            Image img = marker.GetComponent<Image>();
            if (img == null)
            {
                img = marker.gameObject.AddComponent<Image>();
                img.color = new Color(1f, 1f, 1f, 0.3f);
            }

            BoneButton boneButton = marker.GetComponent<BoneButton>();
            if (boneButton == null)
            {
                boneButton = marker.gameObject.AddComponent<BoneButton>();
            }
            boneButton.Initialize(boneData, currentFish);
            boneButtons[i] = boneButton;
        }

        // Update declared counts for UI at runtime
        currentFish.spineBoneCount = spineCount;
        currentFish.ribBoneCount = ribCount;
        currentFish.pinBoneCount = pinCount;
    }

    public void ResetBones()
    {
        if (currentFish != null)
        {
            currentFish.ResetBones();
            isFishOpened = false;
            if (boneButtons != null)
            {
                foreach (var button in boneButtons)
                {
                    if (button != null)
                        button.SetRemoved(false);
                }
            }
        }

        // Reset opening visuals
        if (fishOpeningVisuals != null)
        {
            fishOpeningVisuals.ResetVisuals();
        }

        // Hide cut line
        if (cutLineImage != null)
        {
            cutLineImage.gameObject.SetActive(false);
        }
    }

    private void OnFishBoardClicked()
    {
        if (GameManager.Instance != null && !isFishOpened)
        {
            GameManager.Instance.OnFishBoardClicked();
        }
    }

    public void PlayFishOpeningAnimation()
    {
        if (isFishOpened)
            return;

        isFishOpened = true;
        StartCoroutine(AnimateFishOpening());
    }

    private System.Collections.IEnumerator AnimateFishOpening()
    {
        // Show cut line if available
        if (cutLineImage != null)
        {
            cutLineImage.gameObject.SetActive(true);
            
            // Animate cut line appearing (fade in and scale)
            RectTransform cutLineRect = cutLineImage.GetComponent<RectTransform>();
            CanvasGroup cutLineCanvasGroup = cutLineImage.GetComponent<CanvasGroup>();
            if (cutLineCanvasGroup == null)
            {
                cutLineCanvasGroup = cutLineImage.gameObject.AddComponent<CanvasGroup>();
            }

            float elapsed = 0f;
            Vector3 startScale = new Vector3(0f, 1f, 1f);
            Vector3 endScale = Vector3.one;
            float startAlpha = 0f;
            float endAlpha = 1f;

            while (elapsed < cutLineAnimationDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / cutLineAnimationDuration;
                float curveValue = cutLineAnimationCurve.Evaluate(t);

                if (cutLineRect != null)
                {
                    cutLineRect.localScale = Vector3.Lerp(startScale, endScale, curveValue);
                }

                if (cutLineCanvasGroup != null)
                {
                    cutLineCanvasGroup.alpha = Mathf.Lerp(startAlpha, endAlpha, curveValue);
                }

                yield return null;
            }

            // Ensure final state
            if (cutLineRect != null)
            {
                cutLineRect.localScale = endScale;
            }
            if (cutLineCanvasGroup != null)
            {
                cutLineCanvasGroup.alpha = endAlpha;
            }
        }

        // Trigger the fish opening visuals (hide skin, show interior)
        if (fishOpeningVisuals != null)
        {
            fishOpeningVisuals.PlayOpen();
        }
        else
        {
            Debug.LogWarning("FishBoardController: FishOpeningVisuals is null! Make sure your fish prefab has the FishOpeningVisuals component.");
        }
    }
}




