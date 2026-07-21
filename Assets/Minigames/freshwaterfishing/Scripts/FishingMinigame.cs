using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

public class FishingMinigame : MonoBehaviour
{
    /// <summary>
    /// This is the main fishing minigame code. It controls the states of fishing,
    /// then when in the minigame state it controls the movement of the catching
    /// bar and selects which fish we are catching.
    /// </summary>

    //These bools are just for keeping track of the state of the minigame we are in
    private bool lineCast = false;
    private bool nibble = false;
    public bool reelingFish = false;
    private bool minigameActive = false; //Whether the minigame is currently active/enabled
    private Animator animator; // Fish State Animation

    private FreshwaterFish currentFishOnLine; //Reference to the current fish we are catching (FreshwaterFish class is in Fishfesh.cs)

    //These are references for the gameobjects used in the UI
    [Header("Setup References")]
    //The catching bar is the green bar that you put ontop of the fish to catch it
    [SerializeField] private GameObject catchingbar;
    private Vector3 catchingBarLoc;
    private Rigidbody2D catchingBarRB;

    //This is the fish on the UI that you are chasing to catch
    [SerializeField] private GameObject fishBar;
    private FishingMinigame_FishTrigger fishTrigger; //Reference to this script on the fish
    private bool inTrigger = false; //Whether or not the fish is inside the "catchingbar"

    private float catchPercentage = 0f; //0-100 how much you have caught the fish
    private float timeOutOfBar = 0f; //How long the fish has been out of the catching bar
    private bool showingGettingAwayWarning = false; //Whether we're currently showing the getting away warning
    private Coroutine fishEscapeCoroutine = null; //Reference to the fish escape coroutine
    [SerializeField] private Slider catchProgressBar; //The bar on the right that shows how much you have caught

    //Score tracking
    private float catchStartTime = 0f; //Time when reeling started (for calculating catch time)

    [SerializeField] private GameObject thoughtBubbles;
    [SerializeField] private GameObject minigameCanvas;

    //Prompt panel for showing messages to the player
    [Header("Prompt Panel")]
    [SerializeField] private GameObject promptPanel; //The panel/window that shows messages
    [SerializeField] private UnityEngine.UI.Text promptText; //The text component that displays the message

    //Catch result popup window
    [Header("Catch Result Popup")]
    [SerializeField] private GameObject catchResultPanel; //The popup window that shows catch result
    [SerializeField] private UnityEngine.UI.Text fishNameText; //Text showing the fish name
    [SerializeField] private UnityEngine.UI.Text fishInfoText; //Text showing educational information
    [SerializeField] private UnityEngine.UI.Image fishImageDisplay; //Image showing the caught fish sprite
    [SerializeField] private UnityEngine.UI.Button closeButton; //Button to close the popup

    [Header("Settings")]
    [SerializeField] private KeyCode fishingKey = KeyCode.Space; //Key used to play
    [SerializeField] private float catchMultiplier = 10f; //Higher means catch fish faster x
    [SerializeField] private float catchingForce; //How much force to push the catchingbar up by
    [SerializeField] private float nibbleResponseTime = 3f; //How many seconds player has to press Space after nibble (default: 3 seconds)
    [SerializeField] private float fishEscapeTime = 4f; //How many seconds the fish can be out of bar before escaping (default: 4 seconds)

    [Header("Scene Management")]
    [SerializeField] private bool returnToPreviousScene = true; //If true, allows returning to previous scene after fishing
    [SerializeField] private string returnSceneName = "MainScene"; //Name of scene to return to (set this to your main game scene)
    [SerializeField] private KeyCode returnKey = KeyCode.Escape; //Key to press to return to previous scene

    // NEW: Reference to DetailedFishInventory
    [Header("Inventory Integration")]
    [SerializeField] private bool recordToDetailedInventory = true;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioSource bgmSource;
    [SerializeField] private AudioClip backgroundMusic;
    [SerializeField] private AudioClip castLineSound;
    [SerializeField] private AudioClip reelingSound;
    [SerializeField] private AudioClip nibbleSound;
    [SerializeField] private AudioClip fishCaughtSound;
    [SerializeField] private AudioClip fishGotAwaySound;
    [SerializeField] private AudioClip minigameFinishSound;

    private void Start()
    {
        catchingBarRB = catchingbar.GetComponent<Rigidbody2D>(); //Get reference to the Rigidbody on the catchingbar
        catchingBarLoc = catchingbar.GetComponent<RectTransform>().localPosition; //Use this to reset the catchingbars position to the bottom of the "water"
        animator = GetComponentInChildren<Animator>(); // Para maka animate ta
        Debug.Log(animator);
        animator.SetTrigger("idlefish");
        //Ensure minigame canvas starts inactive
        if (minigameCanvas != null)
        {
            minigameCanvas.SetActive(false);
        }

        //Ensure prompt panel starts inactive
        if (promptPanel != null)
        {
            promptPanel.SetActive(false);
        }

        //Ensure catch result panel starts inactive
        if (catchResultPanel != null)
        {
            catchResultPanel.SetActive(false);
        }

        //Setup close button listener
        if (closeButton != null)
        {
            closeButton.onClick.AddListener(CloseCatchResult);
        }

        //Initialize minigame state - always active in fishing scene
        minigameActive = true;

        // NEW: Ensure DetailedFishInventory is available
        EnsureDetailedInventory();

        // Ensure AudioSource exists
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        // Setup BGM Source
        if (bgmSource == null)
        {
            bgmSource = gameObject.AddComponent<AudioSource>();
        }

        // NEW: Connect to SettingsManager for volume control
        if (SettingsManager.Instance != null)
        {
            UpdateMusicVolume(SettingsManager.Instance.MusicVolume);
            UpdateSFXVolume(SettingsManager.Instance.SFXVolume);
            SettingsManager.Instance.OnMusicVolumeChanged += UpdateMusicVolume;
            SettingsManager.Instance.OnSFXVolumeChanged += UpdateSFXVolume;
        }

        if (bgmSource != null && backgroundMusic != null)
        {
            bgmSource.clip = backgroundMusic;
            bgmSource.loop = true;
            bgmSource.Play();
        }
    }

    // NEW: Ensure DetailedFishInventory exists
    private void EnsureDetailedInventory()
    {
        if (DetailedFishInventory.Instance == null)
        {
            Debug.Log("Creating DetailedFishInventory for freshwater fishing...");
            GameObject inventoryObj = new GameObject("DetailedFishInventory");
            inventoryObj.AddComponent<DetailedFishInventory>();
            DontDestroyOnLoad(inventoryObj);
        }
    }

    private void Update()
    {
        //Don't process input if catch result panel is open
        if (IsCatchResultPanelOpen())
        {
            //Allow returning to previous scene even when catch panel is open
            if (returnToPreviousScene && Input.GetKeyDown(returnKey))
            {
                ReturnToPreviousScene();
            }
            return;
        }

        //Don't process input if minigame is not active
        if (!minigameActive)
        {
            return;
        }

        //Check for return to previous scene
        if (returnToPreviousScene && Input.GetKeyDown(returnKey) && !reelingFish && !lineCast)
        {
            ReturnToPreviousScene();
            return;
        }

        if (!reelingFish)
        { //If we arent currently in the reeling stage
            if (Input.GetKeyDown(fishingKey) && !lineCast)
            { //This is if we are doing nothing and are ready to cast a line
                CastLine();
                
            }
            else if (Input.GetKeyDown(fishingKey) && lineCast && !nibble)
            { //This is if the line has cast and we reel in before we get a nibble
                StopAllCoroutines(); //Stops the WaitForNibble timer
                lineCast = false; //Resets the line being cast
                animator.SetTrigger("endfish");
                if (audioSource != null) audioSource.Stop();

                //Resets the thought bubbles
                thoughtBubbles.GetComponent<Animator>().SetTrigger("Reset");
                thoughtBubbles.SetActive(false);

                //Hide prompt panel
                HidePrompt();

            }
            else if (Input.GetKeyDown(fishingKey) && lineCast && nibble)
            { //This is if we reel in while there is a nibble
                StopAllCoroutines(); //Stops the LineBreak timer
                StartReeling();
            }
        }
        else
        { //This is when we are in the stage where we are fighitng for the fish
            if (Input.GetKey(fishingKey))
            { //If we press space
                catchingBarRB.AddForce(Vector2.up * catchingForce * Time.deltaTime, ForceMode2D.Force); //Add force to lift the bar
            }
        }

        //If the fish is in our trigger box
        if (inTrigger && reelingFish)
        {
            catchPercentage += catchMultiplier * Time.deltaTime;
            //Reset escape timer when fish is back in bar
            if (showingGettingAwayWarning)
            {
                StopFishEscapeTimer();
            }
        }
        else if (reelingFish && !inTrigger)
        {
            //Fish is out of bar - decrease catch percentage
            catchPercentage -= catchMultiplier * Time.deltaTime;

            //Start tracking escape time if not already
            if (!showingGettingAwayWarning)
            {
                StartFishEscapeTimer();
            }
        }

        //Changes fish from silhoutte to colour over time
        var fishColor = Color.Lerp(Color.black, Color.white, Map(0, 100, 0, 1, catchPercentage));
        fishBar.GetComponent<Image>().color = fishColor;

        //Clamps our percentage between 0 and 100
        catchPercentage = Mathf.Clamp(catchPercentage, 0, 100);
        catchProgressBar.value = catchPercentage;
        if (catchPercentage >= 100)
        { //Fish is caught if percentage is full
            catchPercentage = 0;
            FishCaught();
        }
    }

    //Called to cast our line
    private void CastLine()
    {
        lineCast = true;
        thoughtBubbles.SetActive(true);
        // Play casting sound
        if (audioSource != null && castLineSound != null)
        {
            audioSource.clip = castLineSound;
            audioSource.Play();
        }
        animator.SetTrigger("startfish");


        StartCoroutine(WaitForNibble(10));
    }

    //Wait a random time to get a nibble
    private IEnumerator WaitForNibble(float maxWaitTime)
    {
        yield return new WaitForSeconds(Random.Range(maxWaitTime * 0.25f, maxWaitTime)); //Wait between 25% of maxWaitTime and the maxWaitTime
        thoughtBubbles.GetComponent<Animator>().SetTrigger("Alert"); //Show the alert thoughtbubble
        nibble = true;

        // Play nibble sound
        if (audioSource != null && nibbleSound != null)
        {
            audioSource.PlayOneShot(nibbleSound);
        }

        //Show the hurry prompt
        ShowPrompt("Hurry! Press Space to reel in!");

        StartCoroutine(LineBreak(nibbleResponseTime)); //If we dont respond in time, break the line
    }

    //Used to start the minigame
    private void StartReeling()
    {
        reelingFish = true;

        // Play reeling sound
        if (audioSource != null && reelingSound != null)
        {
            audioSource.clip = reelingSound;
            audioSource.Play();
        }

        //Record start time for catch duration
        catchStartTime = Time.time;

        nibble = false;
        lineCast = false;

        //Hide thought bubbles when starting to reel
        if (thoughtBubbles != null)
        {
            thoughtBubbles.GetComponent<Animator>().SetTrigger("Reset");
            thoughtBubbles.SetActive(false);
        }

        //Hide prompt panel when starting to reel
        HidePrompt();

        //Reset escape tracking
        timeOutOfBar = 0f;
        showingGettingAwayWarning = false;
        StopFishEscapeTimer();

        //Activate canvas first to ensure it shows even if there are setup errors
        if (minigameCanvas != null)
        {
            minigameCanvas.SetActive(true);
        }
        else
        {
            Debug.LogError("MinigameCanvas is not assigned in FishingMinigame!");
            return;
        }

        //Set up the fish we are catching
        currentFishOnLine = FishManager.GetRandomFishWeighted();

        if (currentFishOnLine == null)
        {
            Debug.LogError("Failed to get a fish from FishManager!");
            //Fallback to non-weighted random
            currentFishOnLine = FishManager.GetRandomFish();
            if (currentFishOnLine == null)
            {
                Debug.LogError("No fish available in FishManager!");
                return;
            }
        }

        //Get fish sprite from resources
        var tempSprite = Resources.Load<Sprite>($"FishSprites/{currentFishOnLine.spriteID}");

        if (tempSprite == null)
        {
            Debug.LogError($"Failed to load sprite: FishSprites/{currentFishOnLine.spriteID}");
            //Continue anyway, but log the error
        }

        //Set up fish bar if it exists
        if (fishBar != null)
        {
            var fishImage = fishBar.GetComponent<Image>();
            if (fishImage != null && tempSprite != null)
            {
                fishImage.sprite = tempSprite;

                //Changes the width and height of the fishBar to accomodate for wider sprites
                var rectTransform = fishBar.GetComponent<RectTransform>();
                if (rectTransform != null && tempSprite != null)
                {
                    var w = Map(0, 32, 0, 100, tempSprite.texture.width);
                    var h = Map(0, 32, 0, 100, tempSprite.texture.height);
                    rectTransform.sizeDelta = new Vector2(w, h);
                }
            }
        }
        else
        {
            Debug.LogError("FishBar is not assigned in FishingMinigame!");
        }
    }

    //This breaks the line if we are waiting for a response too long
    private IEnumerator LineBreak(float lineBreakTime)
    {
        yield return new WaitForSeconds(lineBreakTime);

        //Only break the line if we're still waiting (not already reeling)
        if (!reelingFish && nibble)
        {
            Debug.Log("Line Broke! You didn't press Space in time.");
            animator.SetTrigger("endfish");
            if (audioSource != null) audioSource.Stop();
            if (audioSource != null && fishGotAwaySound != null) audioSource.PlayOneShot(fishGotAwaySound);

            //Show failure message
            ShowPrompt("Too slow! The fish got away. Try again!");

            //Disable thought bubbles
            if (thoughtBubbles != null)
            {
                thoughtBubbles.GetComponent<Animator>().SetTrigger("Reset");
                thoughtBubbles.SetActive(false);
            }

            lineCast = false;
            nibble = false;

            //Hide the failure message after a few seconds
            StartCoroutine(HidePromptAfterDelay(2f));
        }
    }

    //Called from the FishingMinigame_FishTrigger script
    public void FishInBar()
    {
        inTrigger = true;
        //Stop escape timer when fish is back in bar
        if (showingGettingAwayWarning)
        {
            StopFishEscapeTimer();
        }
    }

    //Called from the FishingMinigame_FishTrigger script
    public void FishOutOfBar()
    {
        inTrigger = false;
        //Start escape timer when fish leaves the bar (only if we're reeling)
        if (reelingFish && !showingGettingAwayWarning)
        {
            StartFishEscapeTimer();
        }
    }

    //Called when the catchpercentage hits 100
    public void FishCaught()
    {
        if (currentFishOnLine == null)
        { //This picks a new fish if the old one is lost by chance
            currentFishOnLine = FishManager.GetRandomFish();
        }
        animator.SetTrigger("endfish");
        Debug.Log($"Caught a: {currentFishOnLine.name}");

        //Calculate catch time
        float catchTime = Time.time - catchStartTime;

        // Record to DetailedFishInventory (NEW)
        if (recordToDetailedInventory && DetailedFishInventory.Instance != null)
        {
            DetailedFishInventory.Instance.AddFreshwaterCatchRecord(currentFishOnLine);
            Debug.Log($"✅ Recorded {currentFishOnLine.name} to DetailedFishInventory");
        }
        else if (recordToDetailedInventory)
        {
            Debug.LogWarning("DetailedFishInventory.Instance is null! Cannot record catch.");
        }

        //Record score in FreshwaterFishingScoreManager
        if (FreshwaterFishingScoreManager.Instance != null && currentFishOnLine != null)
        {
            //Use baseCost as gold value, and calculate experience points (baseCost / 2)
            int experiencePoints = Mathf.Max(1, currentFishOnLine.baseCost / 2);
            int goldValue = currentFishOnLine.baseCost;

            string fishId = currentFishOnLine.name.ToLower().Replace(" ", "_");

            FreshwaterFishingScoreManager.Instance.RecordCatch(
                currentFishOnLine.name,
                fishId,
                experiencePoints,
                goldValue,
                catchTime
            );

            // Resolve catalog entry if possible (needed for sustainability + economy)
            FishCatalogEntry catalogEntry = null;
            if (FishInventoryManager.Instance != null)
            {
                catalogEntry = FishInventoryManager.Instance.GetCatalogEntry(fishId)
                    ?? FishInventoryManager.Instance.GetTotals(fishId)?.catalogEntry;
            }

            // Also record to SustainableFishingMetrics for unified analytics
            if (SustainableFishingMetrics.Instance != null)
            {
                bool isEndangered = catalogEntry != null && catalogEntry.isEndangered;

                SustainableFishingMetrics.Instance.RecordCatch(
                    fishId,
                    currentFishOnLine.name,
                    FishCatchSource.FreshwaterFishing,
                    isEndangered
                );
            }

            // Record to FishInventoryManager for unified inventory
            if (FishInventoryManager.Instance != null)
            {
                FishInventoryManager.Instance.RecordCatch(
                    catalogEntry,
                    1,
                    FishCatchSource.FreshwaterFishing,
                    fishId
                );
            }

            // Feed the shared economy so the player earns cash toward their debt
            if (EconomyManager.Instance != null)
            {
                EconomyManager.Instance.RecordFishSale(
                    catalogEntry,
                    currentFishOnLine.name,
                    FishCatchSource.FreshwaterFishing,
                    goldValue,
                    soldAfterDeboning: false);
            }
        }

        reelingFish = false; //No longer reeling in a fish

        //Stop escape timer
        StopFishEscapeTimer();

        //Reset the thought bubbles
        thoughtBubbles.SetActive(false);
        thoughtBubbles.GetComponent<Animator>().SetTrigger("Reset");
        minigameCanvas.SetActive(false); //Disable the fishing canvas
        catchingbar.transform.localPosition = catchingBarLoc; //Reset the catching bars position

        //Hide prompt panel
        HidePrompt();

        //Show catch result popup
        ShowCatchResult(currentFishOnLine);

        if (audioSource != null) audioSource.Stop();

        // Play fish caught sound
        if (audioSource != null && fishCaughtSound != null)
        {
            audioSource.PlayOneShot(fishCaughtSound);
        }
    }


    //Show prompt panel with a message
    private void ShowPrompt(string message)
    {
        if (promptPanel != null)
        {
            promptPanel.SetActive(true);

            if (promptText != null)
            {
                promptText.text = message;
            }
            else
            {
                Debug.LogWarning("PromptText is not assigned! Message: " + message);
            }
        }
        else
        {
            Debug.LogWarning("PromptPanel is not assigned! Message: " + message);
        }
    }

    //Hide prompt panel
    private void HidePrompt()
    {
        if (promptPanel != null)
        {
            promptPanel.SetActive(false);
        }
    }

    //Hide prompt panel after a delay
    private IEnumerator HidePromptAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        HidePrompt();
    }

    //Start tracking fish escape timer
    private void StartFishEscapeTimer()
    {
        if (fishEscapeCoroutine == null && reelingFish)
        {
            showingGettingAwayWarning = true;
            ShowPrompt("The fish is getting away!");
            fishEscapeCoroutine = StartCoroutine(FishEscapeTimer());
        }
    }

    //Stop tracking fish escape timer
    private void StopFishEscapeTimer()
    {
        if (fishEscapeCoroutine != null)
        {
            StopCoroutine(fishEscapeCoroutine);
            fishEscapeCoroutine = null;
        }
        showingGettingAwayWarning = false;
        timeOutOfBar = 0f;
    }

    //Coroutine that tracks how long the fish has been out of the bar
    private IEnumerator FishEscapeTimer()
    {
        timeOutOfBar = 0f;

        while (reelingFish && !inTrigger && timeOutOfBar < fishEscapeTime)
        {
            timeOutOfBar += Time.deltaTime;

            //Update warning message with countdown
            float timeRemaining = fishEscapeTime - timeOutOfBar;
            if (timeRemaining > 0)
            {
                ShowPrompt($"The fish is getting away! ({timeRemaining:F1}s)");
            }

            yield return null;
        }

        //If we've reached the escape time and fish is still out of bar
        if (reelingFish && !inTrigger && timeOutOfBar >= fishEscapeTime)
        {
            FishGotAway();
            animator.SetTrigger("endfish");
        }

        fishEscapeCoroutine = null;
        showingGettingAwayWarning = false;
    }

    //Called when the fish escapes
    private void FishGotAway()
    {
        reelingFish = false;

        if (audioSource != null) audioSource.Stop();
        if (audioSource != null && fishGotAwaySound != null) audioSource.PlayOneShot(fishGotAwaySound);

        //Reset catch timer
        catchStartTime = 0f;

        //Show failure message
        ShowPrompt("Darn it! The fish got away!");
        animator.SetTrigger("endfish");
        //Disable minigame canvas
        if (minigameCanvas != null)
        {
            minigameCanvas.SetActive(false);
        }

        //Reset catching bar position
        if (catchingbar != null)
        {
            catchingbar.transform.localPosition = catchingBarLoc;
        }

        //Reset catch percentage
        catchPercentage = 0f;
        if (catchProgressBar != null)
        {
            catchProgressBar.value = 0f;
        }

        //Reset thought bubbles
        if (thoughtBubbles != null)
        {
            thoughtBubbles.SetActive(false);
            thoughtBubbles.GetComponent<Animator>().SetTrigger("Reset");
        }

        //Reset states
        lineCast = false;
        nibble = false;
        inTrigger = false;

        Debug.Log("The fish got away!");

        //Hide the failure message after a few seconds
        StartCoroutine(HidePromptAfterDelay(3f));
    }

    //Show catch result popup with fish information
    private void ShowCatchResult(FreshwaterFish fish)
    {
        if (catchResultPanel == null)
        {
            Debug.LogError("CatchResultPanel is not assigned!");
            return;
        }

        //Activate the panel
        catchResultPanel.SetActive(true);

        //Set fish name
        if (fishNameText != null)
        {
            fishNameText.text = $"Caught: {fish.name}";
        }

        //Set educational information
        if (fishInfoText != null)
        {
            string educationalInfo = GetFishEducationalInfo(fish.name);
            fishInfoText.text = educationalInfo;
        }

        //Set fish image
        if (fishImageDisplay != null)
        {
            var fishSprite = Resources.Load<Sprite>($"FishSprites/{fish.spriteID}");
            if (fishSprite != null)
            {
                fishImageDisplay.sprite = fishSprite;
                fishImageDisplay.color = Color.white; //Make sure it's visible
            }
            else
            {
                Debug.LogWarning($"Could not load sprite: FishSprites/{fish.spriteID}");
            }
        }
    }

    //Close the catch result popup
    private void CloseCatchResult()
    {
        // Play minigame finish sound
        if (audioSource != null && minigameFinishSound != null)
        {
            audioSource.PlayOneShot(minigameFinishSound);
        }

        if (catchResultPanel != null)
        {
            catchResultPanel.SetActive(false);
        }
    }

    //Check if catch result panel is currently open
    private bool IsCatchResultPanelOpen()
    {
        return catchResultPanel != null && catchResultPanel.activeSelf;
    }

    //Return to previous scene
    private void ReturnToPreviousScene()
    {
        if (!string.IsNullOrEmpty(returnSceneName))
        {
            SceneManager.LoadScene(returnSceneName);
        }
        else
        {
            Debug.LogWarning("Return Scene Name is not set! Cannot return to previous scene.");
        }
    }

    //Get educational information about a fish (you can expand this dictionary or load from CSV)
    private string GetFishEducationalInfo(string fishName)
    {
        //Dictionary of educational information for each fish
        //You can expand this or load from CSV/JSON file
        switch (fishName.ToLower())
        {
            case "willy":
                return "Willy is a common freshwater fish known for its vibrant colors and playful nature. " +
                    "They typically inhabit slow-moving rivers and lakes, feeding on small insects and plant matter. " +
                    "These fish are popular among anglers due to their abundance and ease of catching.";

            case "pam":
                return "Pam is a medium-sized freshwater species that prefers deeper waters. " +
                    "They are known for their distinctive markings and are most active during dawn and dusk. " +
                    "Pam fish are omnivorous, feeding on both aquatic plants and small invertebrates.";

            case "shane":
                return "Shane is a resilient fish species that can adapt to various water conditions. " +
                    "They are often found in both rivers and lakes, making them a versatile catch. " +
                    "These fish are known for their strong fighting ability when caught on a line.";

            case "krobus":
                return "Krobus is a rarer freshwater fish with unique behavioral patterns. " +
                    "They prefer clear, well-oxygenated waters and are more challenging to catch. " +
                    "Krobus fish are prized for their distinctive appearance and are considered a trophy catch by many anglers.";

            case "linus":
                return "Linus is an elusive and highly sought-after freshwater species. " +
                    "These fish are known for their intelligence and cautious nature, making them one of the most difficult catches. " +
                    "They typically inhabit pristine waters and are indicators of a healthy aquatic ecosystem. " +
                    "Catching a Linus is considered a significant achievement among fishing enthusiasts.";

            default:
                return $"The {fishName} is a fascinating freshwater species found in local waters. " +
                    "Each fish plays an important role in maintaining the aquatic ecosystem's balance.";
        }
    }

    //Classic mapping script x
    private float Map(float a, float b, float c, float d, float x)
    {
        return (x - a) / (b - a) * (d - c) + c;
    }

    // NEW: Volume update methods to respond to settings changes
    private void UpdateMusicVolume(float volume)
    {
        if (bgmSource != null)
        {
            bgmSource.volume = volume;
        }
    }

    private void UpdateSFXVolume(float volume)
    {
        if (audioSource != null)
        {
            audioSource.volume = volume;
        }
    }

    private void OnDestroy()
    {
        // Unsubscribe from events to prevent memory leaks or errors
        if (SettingsManager.Instance != null)
        {
            SettingsManager.Instance.OnMusicVolumeChanged -= UpdateMusicVolume;
            SettingsManager.Instance.OnSFXVolumeChanged -= UpdateSFXVolume;
        }
    }
}