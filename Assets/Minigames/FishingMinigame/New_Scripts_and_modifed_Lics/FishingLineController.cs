using UnityEngine;

public class FishingLineController : MonoBehaviour
{
    public enum FishingState
    {
        Idle,
        Casting,
        Descending,
        Reeling
    }

    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float retractionSpeed = 10f;
    public float rotationSpeed = 200f;
    public float maxYPosition = -9f;
    public float maxXPosition = 8f;
    public float minXPosition = -8f;
    public LayerMask obstacles;

    [Header("Fishing Settings")]
    public float maxFishingTime = 30f;
    private float currentFishingTime = 0f;
    public LayerMask fishLayer;
    public float fishCatchRadius = 1f;
    private bool hasCaughtFish = false;

    [Header("Line Settings")]
    public Transform hook;
    public LineRenderer lineRenderer;

    [Header("Camera Settings")]
    public FishingCameraFollow cameraFollow;

    [Header("Audio Settings")]
    [SerializeField] private AudioSource fishingMusicSource;
    [SerializeField] private AudioClip fishingMusic;
    [SerializeField] private float musicFadeSpeed = 1f;
    [SerializeField] private BackgroundMusicManager backgroundMusicManager;
    private float targetMusicVolume = 0f;

    [Header("Encyclopedia")]
    [SerializeField] private FishEncyclopedia encyclopedia;
    [SerializeField] private FishData[] availableFish;

    [Header("UI")]
    [SerializeField] private FishCaughtUI fishCaughtUI;
    [SerializeField] private EndangeredFishWarningUI endangeredFishWarningUI;

    private FishingState currentState = FishingState.Idle;
    private Vector3 startPoint;
    private Vector2 moveDirection;
    private bool isMoving = false;
    private GameObject caughtFish;
    private FishData pendingFishDecision;

    public FishingState CurrentState => currentState;

    void Start()
    {
        startPoint = hook.position;
        UpdateLine();

        if (cameraFollow == null)
        {
            cameraFollow = Camera.main.GetComponent<FishingCameraFollow>();
            if (cameraFollow == null)
            {
                Debug.LogWarning("FishingCameraFollow component not found on main camera!");
            }
        }

        if (fishingMusicSource == null)
        {
            fishingMusicSource = gameObject.AddComponent<AudioSource>();
            fishingMusicSource.playOnAwake = false;
            fishingMusicSource.loop = true;
            fishingMusicSource.volume = 0f;
        }

        if (fishingMusic != null)
        {
            fishingMusicSource.clip = fishingMusic;
        }
        else
        {
            Debug.LogWarning("No fishing music clip assigned!");
        }

        if (backgroundMusicManager == null)
        {
            backgroundMusicManager = FindObjectOfType<BackgroundMusicManager>();
            if (backgroundMusicManager == null)
            {
                Debug.LogWarning("BackgroundMusicManager not found in scene!");
            }
        }

        if (encyclopedia == null)
        {
            encyclopedia = FindObjectOfType<FishEncyclopedia>();
            if (encyclopedia == null)
            {
                Debug.LogWarning("FishEncyclopedia not found in scene!");
            }
        }

        if (FishingStats.Instance != null)
        {
            FishingStats.Instance.StartFishingSession();
        }
    }

    private void OnDestroy()
    {
        if (FishingStats.Instance != null)
        {
            FishingStats.Instance.EndFishingSession();
        }
    }

    void Update()
    {
        if (fishingMusicSource != null)
        {
            float globalVolume = SettingsManager.Instance != null ? SettingsManager.Instance.MusicVolume : 1f;
            float actualTarget = targetMusicVolume * globalVolume;

            if (fishingMusicSource.volume != actualTarget)
            {
                fishingMusicSource.volume = Mathf.MoveTowards(fishingMusicSource.volume, actualTarget, musicFadeSpeed * Time.deltaTime);
            }

            if (fishingMusicSource.volume > 0 && !fishingMusicSource.isPlaying)
            {
                fishingMusicSource.Play();
            }
            else if (fishingMusicSource.volume <= 0 && fishingMusicSource.isPlaying)
            {
                fishingMusicSource.Stop();
            }
        }

        switch (currentState)
        {
            case FishingState.Idle:
                break;

            case FishingState.Casting:
                currentState = FishingState.Descending;
                currentFishingTime = 0f;
                hasCaughtFish = false;
                break;

            case FishingState.Descending:
                HandleDescending();
                break;

            case FishingState.Reeling:
                HandleReeling();
                break;
        }

        UpdateLine();
    }

    private void HandleDescending()
    {
        currentFishingTime += Time.deltaTime;

        if (Input.GetKeyDown(KeyCode.Space) || currentFishingTime >= maxFishingTime || hasCaughtFish)
        {
            currentState = FishingState.Reeling;
            return;
        }

        if (!hasCaughtFish)
        {
            CheckForFish();
        }

        RaycastHit2D raycastHit2D = Physics2D.Raycast(hook.position, -hook.up, 1f, obstacles);

        if (raycastHit2D.collider == null)
        {
            float horizontalInput = Input.GetAxisRaw("Horizontal");

            Vector2 forwardDirection = -hook.up;
            Vector2 rightDirection = hook.right;

            moveDirection = (forwardDirection + rightDirection * horizontalInput).normalized * moveSpeed;
            hook.position += new Vector3(moveDirection.x, moveDirection.y, 0) * Time.deltaTime;

            Vector3 position = hook.position;
            position.y = Mathf.Min(position.y, maxYPosition);
            position.x = Mathf.Clamp(position.x, minXPosition, maxXPosition);
            hook.position = position;

            isMoving = true;

            if (Input.GetKey(KeyCode.A))
            {
                hook.Rotate(Vector3.forward * -rotationSpeed * Time.deltaTime);
            }
            else if (Input.GetKey(KeyCode.D))
            {
                hook.Rotate(Vector3.forward * rotationSpeed * Time.deltaTime);
            }
        }
        else
        {
            isMoving = false;
        }
    }

    private void CheckForFish()
    {
        Collider2D[] fishColliders = Physics2D.OverlapCircleAll(hook.position, fishCatchRadius, fishLayer);

        if (fishColliders.Length > 0)
        {
            GameObject fish = fishColliders[0].gameObject;
            CatchFish(fish);
        }
    }

    private void CatchFish(GameObject fish)
    {
        if (!hasCaughtFish && fish != null)
        {
            hasCaughtFish = true;
            caughtFish = fish;

            if (fish.GetComponent<Collider2D>() != null)
                fish.GetComponent<Collider2D>().enabled = false;

            if (fish.GetComponent<Rigidbody2D>() != null)
            {
                Rigidbody2D rb = fish.GetComponent<Rigidbody2D>();
                rb.linearVelocity = Vector2.zero;
                rb.angularVelocity = 0f;
                rb.simulated = false;
            }

            if (fish.GetComponent<FishController>() != null)
                fish.GetComponent<FishController>().enabled = false;

            if (fish.GetComponent<FishMovement>() != null)
                fish.GetComponent<FishMovement>().enabled = false;

            fish.transform.SetParent(hook);
            fish.transform.localPosition = new Vector3(0, -0.5f, 0);
            fish.transform.localRotation = Quaternion.Euler(0, 0, 90f);

            FishData fishData = GetFishDataForCaughtFish(fish);

            if (fishData != null && fishCaughtUI != null)
            {
                SimpleFishingStats simpleStats = FindObjectOfType<SimpleFishingStats>();
                if (simpleStats != null && fishData != null)
                {
                    simpleStats.OnFishCaught(fishData);
                }
            }

            currentState = FishingState.Reeling;
        }
    }

    private FishData GetFishDataForCaughtFish(GameObject fish)
    {
        Fish fishComponent = fish.GetComponent<Fish>();
        if (fishComponent != null)
        {
            return fishComponent.GetFishData();
        }

        if (availableFish != null)
        {
            string fishName = fish.name.Replace("(Clone)", "").Trim();
            foreach (FishData fishData in availableFish)
            {
                if (fishData != null && fishData.fishName == fishName)
                {
                    return fishData;
                }
            }
        }

        return null;
    }

    private void HandleReeling()
    {
        Vector2 direction = startPoint - hook.position;
        float distance = direction.magnitude;

        if (distance > 0.1f)
        {
            moveDirection = direction.normalized * retractionSpeed;
            hook.position += new Vector3(moveDirection.x, moveDirection.y, 0) * Time.deltaTime;

            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;
            Quaternion targetRotation = Quaternion.Euler(0, 0, angle);
            hook.rotation = Quaternion.RotateTowards(hook.rotation, targetRotation, rotationSpeed * Time.deltaTime);

            if (caughtFish != null)
            {
                caughtFish.transform.localPosition = new Vector3(0, -0.5f, 0);
                caughtFish.transform.localRotation = Quaternion.Euler(0, 0, 90f);
            }
        }
        else
        {
            hook.position = startPoint;

            if (caughtFish != null)
            {
                caughtFish.transform.SetParent(null);
                FishData fishData = GetFishDataForCaughtFish(caughtFish);

                Destroy(caughtFish);
                caughtFish = null;
                hasCaughtFish = false;

                HandleCaughtFish(fishData);
            }

            currentState = FishingState.Idle;
            SetHookIdleState();

            if (cameraFollow != null)
            {
                cameraFollow.SetTarget(transform);
            }

            if (fishingMusicSource != null)
            {
                targetMusicVolume = 0f;
            }

            if (backgroundMusicManager != null)
            {
                backgroundMusicManager.ResumeBackgroundMusic();
            }
        }
    }

    private void HandleCaughtFish(FishData fishData)
    {
        if (fishData == null) return;

        if (encyclopedia != null)
        {
            encyclopedia.AddDiscoveredFish(fishData);
        }

        if (fishData.RequiresCatchWarning() && endangeredFishWarningUI != null)
        {
            pendingFishDecision = fishData;
            endangeredFishWarningUI.ShowWarning(
                fishData,
                () => ResolveCatchDecision(true),
                () => ResolveCatchDecision(false));
        }
        else
        {
            ApplyCatchOutcome(fishData, true);
        }
    }

    private void ResolveCatchDecision(bool keepFish)
    {
        FishData decisionFish = pendingFishDecision;
        pendingFishDecision = null;

        if (decisionFish == null) return;

        ApplyCatchOutcome(decisionFish, keepFish);
    }

    private void ApplyCatchOutcome(FishData fishData, bool keepFish)
    {
        if (keepFish)
        {
            if (fishCaughtUI != null)
            {
                fishCaughtUI.ShowFishCaught(fishData);
            }

            if (DetailedFishInventory.Instance != null)
            {
                DetailedFishInventory.Instance.AddCatchRecord(fishData);
            }

            if (FishInventoryManager.Instance != null)
            {
                FishInventoryManager.Instance.RecordCatch(
                    fishData.sharedCatalogEntry,
                    1,
                    FishCatchSource.SaltwaterFishing,
                    fishData.GetSharedFishId());
            }

            bool isEndangered = fishData.IsEndangeredSpecies();

            if (SustainableFishingMetrics.Instance != null)
            {
                SustainableFishingMetrics.Instance.RecordCatch(
                    fishData.GetSharedFishId(),
                    fishData.fishName,
                    FishCatchSource.SaltwaterFishing,
                    isEndangered);

                if (isEndangered)
                {
                    SustainableFishingMetrics.Instance.RecordDecision(
                        $"Kept endangered {fishData.fishName}",
                        "Keeping endangered fish may help with debt payments but harms long-term sustainability.",
                        false,
                        DecisionSeverity.Critical);
                }
            }

            if (EconomyManager.Instance != null)
            {
                float baseSaleValue = fishData.GetSuggestedSaleValue(1f);
                EconomyManager.Instance.RecordFishSale(
                    fishData.sharedCatalogEntry,
                    fishData.fishName,
                    FishCatchSource.SaltwaterFishing,
                    baseSaleValue,
                    soldAfterDeboning: false,
                    overrideEndangered: isEndangered);
            }
        }
        else
        {
            if (DetailedFishInventory.Instance != null)
            {
                DetailedFishInventory.Instance.AddReleaseRecord(fishData);
            }

            if (SustainableFishingMetrics.Instance != null)
            {
                SustainableFishingMetrics.Instance.RecordDecision(
                    $"Released {fishData.fishName}",
                    "Releasing endangered fish protects biodiversity and keeps debt payments ethical.",
                    true,
                    DecisionSeverity.High);
            }
        }
    }

    void UpdateLine()
    {
        lineRenderer.SetPosition(0, startPoint);
        lineRenderer.SetPosition(1, hook.position);
    }

    public void OnFishCaught(GameObject fish)
    {
        if (!hasCaughtFish)
        {
            CatchFish(fish);
        }
    }

    private void SetHookIdleState()
    {
        hook.position = startPoint;
        hook.rotation = Quaternion.identity;
        Rigidbody2D hookRb = hook.GetComponent<Rigidbody2D>();
        if (hookRb != null)
        {
            hookRb.isKinematic = true;
            hookRb.linearVelocity = Vector2.zero;
            hookRb.angularVelocity = 0f;
            hookRb.constraints = RigidbodyConstraints2D.FreezeAll;
        }
    }

    private void SetHookActiveState()
    {
        Rigidbody2D hookRb = hook.GetComponent<Rigidbody2D>();
        if (hookRb != null)
        {
            hookRb.isKinematic = false;
            hookRb.constraints = RigidbodyConstraints2D.None;
        }
    }

    public void CastLine()
    {
        // NEW: Don't allow casting if the fish caught UI is still visible
        if (fishCaughtUI != null && fishCaughtUI.IsVisible)
        {
            Debug.Log("Cannot cast line: FishCaughtUI is still visible.");
            return;
        }

        if (caughtFish != null)
        {
            Destroy(caughtFish);
            caughtFish = null;
        }

        if (currentState == FishingState.Idle)
        {
            if (FishingStats.Instance != null)
            {
                FishingStats.Instance.StartFishingSession();
            }

            hasCaughtFish = false;
            SetHookActiveState();
            currentState = FishingState.Casting;

            if (cameraFollow != null)
            {
                cameraFollow.SetTarget(hook);
            }

            if (fishingMusicSource != null)
            {
                targetMusicVolume = 1f;
            }

            if (backgroundMusicManager != null)
            {
                backgroundMusicManager.PauseBackgroundMusic();
            }
        }
    }
}