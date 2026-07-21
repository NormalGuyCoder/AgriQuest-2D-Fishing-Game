using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

/// <summary>
/// Simple utility to mirror wallet/debt status to a UI panel.
/// Hook up TMP labels + a progress fill and drop this on a canvas panel.
/// </summary>
public class DebtStatusPanel : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI walletText;
    public TextMeshProUGUI debtText;
    public TextMeshProUGUI statusText;
    public Image debtProgressFill;
    public Slider debtProgressSlider;
    public TextMeshProUGUI progressPercentText;

    [Header("Progress Bar Background (Optional)")]
    public Image progressBarBackground;
    public RectTransform progressBarFillRect;

    [Header("Additional Stats")]
    public TextMeshProUGUI totalEarnedText;
    public TextMeshProUGUI totalPaidText;
    public TextMeshProUGUI startingDebtText;

    private bool isSubscribed = false;
    private float cachedStartingDebt = 0f;

    private void OnEnable()
    {
        Subscribe();
        RefreshUI();
    }

    private void Start()
    {
        if (!isSubscribed)
        {
            StartCoroutine(FindEconomyManagerAndSubscribe());
        }
        RefreshUI();
    }

    private void OnDisable()
    {
        Unsubscribe();
    }

    private IEnumerator FindEconomyManagerAndSubscribe()
    {
        while (EconomyManager.Instance == null)
        {
            yield return new WaitForSeconds(0.1f);
        }

        Subscribe();
        RefreshUI();
    }

    private void Subscribe()
    {
        if (EconomyManager.Instance != null && !isSubscribed)
        {
            EconomyManager.Instance.OnEconomyChanged += HandleEconomyChanged;
            isSubscribed = true;
        }
    }

    private void Unsubscribe()
    {
        if (EconomyManager.Instance != null && isSubscribed)
        {
            EconomyManager.Instance.OnEconomyChanged -= HandleEconomyChanged;
            isSubscribed = false;
        }
    }

    private void HandleEconomyChanged(float wallet, float debt)
    {
        UpdateTexts(wallet, debt);
    }

    /// <summary>
    /// Public method to force refresh the UI. Call this when showing the panel.
    /// </summary>
    public void RefreshUI()
    {
        if (EconomyManager.Instance == null)
            return;

        UpdateTexts(EconomyManager.Instance.CurrentWallet, EconomyManager.Instance.CurrentDebt);
    }

    private void UpdateTexts(float wallet, float debt)
    {
        // Cache starting debt for progress calculations
        if (EconomyManager.Instance != null && cachedStartingDebt <= 0f)
        {
            cachedStartingDebt = EconomyManager.Instance.startingDebt;
        }

        if (walletText != null)
        {
            walletText.text = $"Wallet: ₱{wallet:0}";
        }

        if (debtText != null)
        {
            debtText.text = $"Debt Remaining: ₱{debt:0}";
        }

        // Calculate progress correctly
        float startingDebt = cachedStartingDebt > 0 ? cachedStartingDebt : 
                            (EconomyManager.Instance != null ? EconomyManager.Instance.startingDebt : 5000f);
        
        float debtPaid = startingDebt - debt;
        float progress = startingDebt > 0 ? Mathf.Clamp01(debtPaid / startingDebt) : 0f;
        float percentPaid = progress * 100f;

        if (statusText != null)
        {
            if (debt <= 0f)
            {
                statusText.text = "Status: Debt Cleared! You are free.";
            }
            else if (percentPaid < 10f)
            {
                statusText.text = "Status: Just started - keep fishing!";
            }
            else if (percentPaid < 25f)
            {
                statusText.text = "Status: Making early progress!";
            }
            else if (percentPaid < 40f)
            {
                statusText.text = "Status: Good momentum! Keep it up!";
            }
            else if (percentPaid < 50f)
            {
                statusText.text = "Status: Almost halfway there!";
            }
            else if (percentPaid < 60f)
            {
                statusText.text = "Status: Halfway done! Great work!";
            }
            else if (percentPaid < 75f)
            {
                statusText.text = "Status: Over halfway! You can do it!";
            }
            else if (percentPaid < 90f)
            {
                statusText.text = "Status: Almost free! Final push!";
            }
            else
            {
                statusText.text = "Status: So close! Just a bit more!";
            }
        }

        // Update progress percentage text
        if (progressPercentText != null)
        {
            progressPercentText.text = $"{percentPaid:F1}% Paid";
            progressPercentText.color = GetProgressColor(progress);
        }

        // Update fill image (for Image.type = Filled)
        if (debtProgressFill != null)
        {
            debtProgressFill.fillAmount = progress;
            debtProgressFill.color = GetProgressColor(progress);
        }

        // Update slider
        if (debtProgressSlider != null)
        {
            debtProgressSlider.minValue = 0f;
            debtProgressSlider.maxValue = 1f;
            debtProgressSlider.value = progress;
            debtProgressSlider.interactable = false;
            
            // Try to hide the handle if it exists (handles can make slider look wrong at low values)
            Transform handleArea = debtProgressSlider.transform.Find("Handle Slide Area");
            if (handleArea != null)
            {
                Transform handle = handleArea.Find("Handle");
                if (handle != null)
                {
                    handle.gameObject.SetActive(false);
                }
            }
        }
        
        // Debug log to help diagnose slider issues
        Debug.Log($"[DebtStatusPanel] StartingDebt: {startingDebt}, CurrentDebt: {debt}, DebtPaid: {debtPaid}, Progress: {progress:F4} ({percentPaid:F1}%), Wallet: {wallet}");

        // Alternative: Scale progress bar rect directly
        if (progressBarFillRect != null)
        {
            Vector2 anchorMax = progressBarFillRect.anchorMax;
            anchorMax.x = progress;
            progressBarFillRect.anchorMax = anchorMax;
        }

        // Update additional stats if available
        if (totalEarnedText != null && EconomyManager.Instance != null)
        {
            totalEarnedText.text = $"Total Earned: ₱{EconomyManager.Instance.TotalEarned:0}";
        }

        if (totalPaidText != null && EconomyManager.Instance != null)
        {
            totalPaidText.text = $"Debt Paid: ₱{EconomyManager.Instance.TotalDebtPaid:0}";
        }

        if (startingDebtText != null)
        {
            startingDebtText.text = $"Starting Debt: ₱{startingDebt:0}";
        }
    }

    private Color GetProgressColor(float progress)
    {
        if (progress <= 0.1f) return new Color(0.8f, 0f, 0f);
        if (progress <= 0.2f) return new Color(0.85f, 0.25f, 0f);
        if (progress <= 0.3f) return new Color(0.9f, 0.45f, 0f);
        if (progress <= 0.4f) return new Color(0.9f, 0.65f, 0.05f);
        if (progress <= 0.5f) return new Color(0.9f, 0.85f, 0.1f);
        if (progress <= 0.6f) return new Color(0.75f, 0.9f, 0.1f);
        if (progress <= 0.7f) return new Color(0.55f, 0.9f, 0.15f);
        if (progress <= 0.8f) return new Color(0.35f, 0.85f, 0.2f);
        if (progress <= 0.9f) return new Color(0.2f, 0.8f, 0.25f);
        return new Color(0.05f, 0.7f, 0.2f);
    }

    /// <summary>
    /// Optional button hook to pay all available funds toward debt.
    /// </summary>
    public void PayAllDebt()
    {
        if (EconomyManager.Instance == null)
            return;

        EconomyManager.Instance.PayDebtFromWallet(EconomyManager.Instance.CurrentWallet);
    }

    /// <summary>
    /// Optional button to pay a fixed chunk (e.g., assign via UnityEvent).
    /// </summary>
    public void PayFixedAmount(float amount)
    {
        EconomyManager.Instance?.PayDebtFromWallet(amount);
    }
}

