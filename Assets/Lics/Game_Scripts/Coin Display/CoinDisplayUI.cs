using UnityEngine;
using TMPro;
using System.Collections;

public class CoinDisplayUI : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private TMP_Text coinText;
    [SerializeField] private GameObject coinIcon;
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private float fadeInDuration = 0.5f;
    [SerializeField] private float fadeOutDuration = 0.3f;
    [SerializeField] private float visibleDuration = 2f;
    [SerializeField] private bool alwaysVisible = false;

    [Header("Animation")]
    [SerializeField] private bool animateOnChange = true;
    [SerializeField] private float scaleMultiplier = 1.2f;
    [SerializeField] private float scaleDuration = 0.3f;

    [Header("Data Source")]
    [Tooltip("Use EconomyManager wallet (synced with debt panel) instead of DetailedFishInventory coins")]
    [SerializeField] private bool useEconomyManagerWallet = true;

    private float currentDisplayedCoins = 0;
    private Coroutine updateCoroutine;
    private Vector3 originalScale;
    private bool isSubscribedToEconomy = false;
    private bool isSubscribedToInventory = false;

    private void Start()
    {
        Initialize();
        SubscribeToEvents();
        UpdateCoinDisplay(false);
    }

    private void Initialize()
    {
        if (coinText == null)
        {
            coinText = GetComponentInChildren<TMP_Text>();
        }

        if (canvasGroup == null)
        {
            canvasGroup = GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = gameObject.AddComponent<CanvasGroup>();
            }
        }

        if (coinIcon != null)
        {
            originalScale = coinIcon.transform.localScale;
        }
        else
        {
            originalScale = transform.localScale;
        }

        if (alwaysVisible)
        {
            canvasGroup.alpha = 1f;
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
        }
    }

    private void SubscribeToEvents()
    {
        if (useEconomyManagerWallet)
        {
            if (EconomyManager.Instance != null)
            {
                EconomyManager.Instance.OnEconomyChanged += OnEconomyChanged;
                isSubscribedToEconomy = true;
                currentDisplayedCoins = EconomyManager.Instance.CurrentWallet;
                UpdateCoinText();
            }
            else
            {
                StartCoroutine(FindEconomyManagerAndSubscribe());
            }
        }
        else
        {
            if (DetailedFishInventory.Instance != null)
            {
                DetailedFishInventory.Instance.OnCoinsChanged += OnCoinsChanged;
                isSubscribedToInventory = true;
            }
            else
            {
                StartCoroutine(FindInventoryAndSubscribe());
            }
        }
    }

    private IEnumerator FindEconomyManagerAndSubscribe()
    {
        while (EconomyManager.Instance == null)
        {
            yield return new WaitForSeconds(0.1f);
        }

        EconomyManager.Instance.OnEconomyChanged += OnEconomyChanged;
        isSubscribedToEconomy = true;

        currentDisplayedCoins = EconomyManager.Instance.CurrentWallet;
        UpdateCoinText();
    }

    private IEnumerator FindInventoryAndSubscribe()
    {
        while (DetailedFishInventory.Instance == null)
        {
            yield return new WaitForSeconds(0.1f);
        }

        DetailedFishInventory.Instance.OnCoinsChanged += OnCoinsChanged;
        isSubscribedToInventory = true;

        currentDisplayedCoins = DetailedFishInventory.Instance.Coins;
        UpdateCoinText();
    }

    private void OnEconomyChanged(float wallet, float debt)
    {
        if (updateCoroutine != null)
        {
            StopCoroutine(updateCoroutine);
        }

        updateCoroutine = StartCoroutine(UpdateCoinsWithAnimation(wallet));

        if (QuestManager.Instance != null)
        {
            QuestManager.Instance.UpdateCoins(Mathf.RoundToInt(wallet));
        }
    }

    private void OnCoinsChanged(int newCoinAmount)
    {
        if (updateCoroutine != null)
        {
            StopCoroutine(updateCoroutine);
        }

        updateCoroutine = StartCoroutine(UpdateCoinsWithAnimation(newCoinAmount));

        if (QuestManager.Instance != null)
        {
            QuestManager.Instance.UpdateCoins(newCoinAmount);
        }
    }

    private IEnumerator UpdateCoinsWithAnimation(float newCoinAmount)
    {
        if (!alwaysVisible)
        {
            yield return StartCoroutine(FadeCanvasGroup(canvasGroup, canvasGroup.alpha, 1f, fadeInDuration));
        }

        if (animateOnChange && coinText != null)
        {
            if (coinIcon != null)
            {
                yield return StartCoroutine(ScaleIcon(coinIcon.transform, scaleMultiplier));
            }

            yield return StartCoroutine(CountCoinsAnimation(currentDisplayedCoins, newCoinAmount));
        }
        else
        {
            currentDisplayedCoins = newCoinAmount;
            UpdateCoinText();
        }

        if (!alwaysVisible)
        {
            yield return new WaitForSeconds(visibleDuration);
            yield return StartCoroutine(FadeCanvasGroup(canvasGroup, 1f, 0f, fadeOutDuration));
        }
    }

    private IEnumerator CountCoinsAnimation(float startValue, float endValue)
    {
        float duration = 0.5f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            t = Mathf.SmoothStep(0f, 1f, t);

            float displayValue = Mathf.Lerp(startValue, endValue, t);
            coinText.text = FormatCoins(Mathf.RoundToInt(displayValue));

            yield return null;
        }

        currentDisplayedCoins = endValue;
        UpdateCoinText();
    }

    private IEnumerator ScaleIcon(Transform iconTransform, float targetMultiplier)
    {
        Vector3 startScale = originalScale;
        Vector3 targetScale = originalScale * targetMultiplier;

        // Scale up
        float elapsed = 0f;
        while (elapsed < scaleDuration / 2f)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / (scaleDuration / 2f);
            iconTransform.localScale = Vector3.Lerp(startScale, targetScale, t);
            yield return null;
        }

        // Scale back down
        elapsed = 0f;
        while (elapsed < scaleDuration / 2f)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / (scaleDuration / 2f);
            iconTransform.localScale = Vector3.Lerp(targetScale, originalScale, t);
            yield return null;
        }

        iconTransform.localScale = originalScale;
    }

    private IEnumerator FadeCanvasGroup(CanvasGroup group, float startAlpha, float endAlpha, float duration)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            group.alpha = Mathf.Lerp(startAlpha, endAlpha, t);
            yield return null;
        }

        group.alpha = endAlpha;

        // Enable/disable interaction based on visibility
        if (endAlpha > 0f)
        {
            group.interactable = true;
            group.blocksRaycasts = true;
        }
        else
        {
            group.interactable = false;
            group.blocksRaycasts = false;
        }
    }

    public void UpdateCoinDisplay(bool animate = true)
    {
        float currentCoins = 0f;

        if (useEconomyManagerWallet && EconomyManager.Instance != null)
        {
            currentCoins = EconomyManager.Instance.CurrentWallet;
        }
        else if (DetailedFishInventory.Instance != null)
        {
            currentCoins = DetailedFishInventory.Instance.Coins;
        }

        if (animate)
        {
            if (useEconomyManagerWallet)
            {
                OnEconomyChanged(currentCoins, 0);
            }
            else
            {
                OnCoinsChanged(Mathf.RoundToInt(currentCoins));
            }
        }
        else
        {
            currentDisplayedCoins = currentCoins;
            UpdateCoinText();

            if (alwaysVisible && canvasGroup != null)
            {
                canvasGroup.alpha = 1f;
                canvasGroup.interactable = true;
                canvasGroup.blocksRaycasts = true;
            }
        }
    }

    private void UpdateCoinText()
    {
        if (coinText != null)
        {
            coinText.text = FormatCoins(Mathf.RoundToInt(currentDisplayedCoins));
        }
    }

    private string FormatCoins(int amount)
    {
        return "₱" + amount.ToString("N0");
    }

    public void ShowCoinDisplay()
    {
        if (!alwaysVisible)
        {
            if (updateCoroutine != null)
            {
                StopCoroutine(updateCoroutine);
            }
            updateCoroutine = StartCoroutine(FadeCanvasGroup(canvasGroup, canvasGroup.alpha, 1f, fadeInDuration));
        }
    }

    public void HideCoinDisplay()
    {
        if (!alwaysVisible)
        {
            if (updateCoroutine != null)
            {
                StopCoroutine(updateCoroutine);
            }
            updateCoroutine = StartCoroutine(FadeCanvasGroup(canvasGroup, canvasGroup.alpha, 0f, fadeOutDuration));
        }
    }

    public void ToggleCoinDisplay()
    {
        if (alwaysVisible) return;

        if (canvasGroup.alpha > 0f)
        {
            HideCoinDisplay();
        }
        else
        {
            ShowCoinDisplay();
        }
    }

    private void OnDestroy()
    {
        if (isSubscribedToEconomy && EconomyManager.Instance != null)
        {
            EconomyManager.Instance.OnEconomyChanged -= OnEconomyChanged;
        }

        if (isSubscribedToInventory && DetailedFishInventory.Instance != null)
        {
            DetailedFishInventory.Instance.OnCoinsChanged -= OnCoinsChanged;
        }
    }

    public void ForceRefresh()
    {
        if (useEconomyManagerWallet && EconomyManager.Instance != null)
        {
            currentDisplayedCoins = EconomyManager.Instance.CurrentWallet;
        }
        else if (DetailedFishInventory.Instance != null)
        {
            currentDisplayedCoins = DetailedFishInventory.Instance.Coins;
        }
        UpdateCoinText();
    }

    public float GetCurrentDisplayedCoins()
    {
        return currentDisplayedCoins;
    }
}