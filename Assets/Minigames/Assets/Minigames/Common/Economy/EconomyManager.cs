using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

/// <summary>
/// Tracks player earnings, wallet balance, and the outstanding debt that drives the story.
/// Attach this to a persistent bootstrap scene or let AchievementsSceneController spawn it on demand.
/// </summary>
[DefaultExecutionOrder(-250)]
public class EconomyManager : MonoBehaviour
{
    public static EconomyManager Instance { get; private set; }

    [Header("Story & Debt Settings")]
    [Tooltip("Initial debt the player must pay off to be free.")]
    [Min(0)]
    public int startingDebt = 5000;

    [Tooltip("Cash the player starts with before paying the debt.")]
    [Min(0)]
    public int startingWallet = 0;

    [Tooltip("Automatically funnel a percentage of each payout into the debt.")]
    public bool autoPayDebt = true;

    [Range(0f, 1f)]
    public float autoPaymentPercent = 0.5f;

    [Header("Payout Defaults")]
    [Tooltip("Fallback sell price when no catalog entry or custom value is provided.")]
    public int defaultCatchValue = 40;

    [Tooltip("Bonus added when a fish is sold after being deboned/processed.")]
    public int debonedBonusValue = 75;

    [Tooltip("Extra multiplier for endangered fish sales (lucrative but unethical).")]
    public float endangeredPremiumMultiplier = 2f;

    [Tooltip("Slight bonus for ethical, non-endangered sales.")]
    public float sustainableBonusMultiplier = 1.1f;

    [Header("Persistence")]
    public string saveFileName = "economy_state.json";

    private readonly List<EconomyTransaction> recentTransactions = new List<EconomyTransaction>();
    private float walletBalance;
    private float outstandingDebt;
    private float totalEarned;
    private float totalDebtPaid;

    public event Action<float, float> OnEconomyChanged; // wallet, debt

    private string SavePath => Path.Combine(Application.persistentDataPath, saveFileName);

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        LoadEconomyState();
        NotifyListeners();
    }

    #region Public API

    public float CurrentWallet => walletBalance;
    public float CurrentDebt => outstandingDebt;
    public float TotalEarned => totalEarned;
    public float TotalDebtPaid => totalDebtPaid;
    public float DebtProgress => startingDebt <= 0 ? 1f : Mathf.Clamp01(1f - (outstandingDebt / Mathf.Max(1f, startingDebt)));

    /// <summary>
    /// Records the sale of a fish (catch or deboned) and awards money accordingly.
    /// </summary>
    public EconomyPayoutResult RecordFishSale(
        FishCatalogEntry catalogEntry,
        string fallbackFishName,
        FishCatchSource source,
        float customBaseValue = -1f,
        bool soldAfterDeboning = false,
        bool overrideEndangered = false)
    {
        float basePrice = DetermineBasePrice(catalogEntry, customBaseValue, source, soldAfterDeboning);

        bool isEndangered = overrideEndangered || (catalogEntry != null && catalogEntry.isEndangered);
        if (isEndangered)
        {
            basePrice *= endangeredPremiumMultiplier;
        }
        else
        {
            basePrice *= sustainableBonusMultiplier;
        }

        int payout = Mathf.Max(1, Mathf.RoundToInt(basePrice));
        float debtPaid = AddFunds(
            payout,
            source,
            catalogEntry != null ? catalogEntry.GetFishId() : fallbackFishName,
            isEndangered,
            soldAfterDeboning);

        if (isEndangered && SustainableFishingMetrics.Instance != null)
        {
            SustainableFishingMetrics.Instance.RecordDecision(
                $"Sold endangered fish: {fallbackFishName}",
                "Selling endangered species may pay well but harms sustainability goals.",
                false,
                DecisionSeverity.Critical);
        }
        else if (!isEndangered && SustainableFishingMetrics.Instance != null)
        {
            SustainableFishingMetrics.Instance.RecordDecision(
                $"Sold sustainable catch: {fallbackFishName}",
                "Earning money through sustainable catches helps pay debts responsibly.",
                true,
                DecisionSeverity.Low);
        }

        var result = new EconomyPayoutResult
        {
            amountAwarded = payout,
            appliedEndangeredPremium = isEndangered,
            paidTowardDebt = debtPaid,
            walletAfterTransaction = walletBalance,
            debtRemaining = outstandingDebt
        };

        return result;
    }

    /// <summary>
    /// Attempts to pay debt directly from the player's wallet.
    /// </summary>
    public bool PayDebtFromWallet(float amount)
    {
        if (amount <= 0f || walletBalance <= 0f || outstandingDebt <= 0f)
            return false;

        float payment = Mathf.Min(amount, walletBalance, outstandingDebt);
        walletBalance -= payment;
        outstandingDebt -= payment;
        totalDebtPaid += payment;

        RecordTransaction(payment * -1f, 0f, "Manual debt payment", null, FishCatchSource.Unknown, false, false);
        SaveEconomyState();
        NotifyListeners();
        return true;
    }

    public IReadOnlyList<EconomyTransaction> GetRecentTransactions() => recentTransactions;

    #endregion

    #region Internal helpers

    private float DetermineBasePrice(FishCatalogEntry entry, float customBaseValue, FishCatchSource source, bool deboned)
    {
        float value = customBaseValue > 0 ? customBaseValue : 0f;

        if (entry != null)
        {
            value = entry.GetSellPrice(deboned, false, customBaseValue > 0 ? customBaseValue : defaultCatchValue);
        }

        if (value <= 0f)
        {
            value = defaultCatchValue;
        }

        if (deboned)
        {
            value += debonedBonusValue;
        }

        // Slightly adjust based on minigame to keep variety
        switch (source)
        {
            case FishCatchSource.Deboning:
                value *= 1.25f;
                break;
            case FishCatchSource.SaltwaterFishing:
                value *= 1.15f;
                break;
            case FishCatchSource.FreshwaterFishing:
                value *= 1f;
                break;
        }

        return value;
    }

    private float AddFunds(int payout, FishCatchSource source, string fishId, bool endangeredSale, bool processed)
    {
        walletBalance += payout;
        totalEarned += payout;

        float debtPayment = 0f;
        if (autoPayDebt && outstandingDebt > 0f && payout > 0)
        {
            debtPayment = Mathf.Min(outstandingDebt, payout * autoPaymentPercent);
            walletBalance -= debtPayment;
            outstandingDebt -= debtPayment;
            totalDebtPaid += debtPayment;
        }

        RecordTransaction(
            payout,
            debtPayment,
            $"Sold {fishId}",
            fishId,
            source,
            endangeredSale,
            processed);

        SaveEconomyState();
        NotifyListeners();
        return debtPayment;
    }

    private void RecordTransaction(
        float amount,
        float debtApplied,
        string description,
        string fishId,
        FishCatchSource source,
        bool endangeredSale,
        bool processed)
    {
        EconomyTransaction tx = new EconomyTransaction
        {
            timestamp = DateTime.Now.ToString("o"),
            description = description,
            amount = amount,
            debtApplied = debtApplied,
            fishId = fishId,
            source = source,
            endangeredSale = endangeredSale,
            processedSale = processed,
            balanceAfter = walletBalance,
            debtAfter = outstandingDebt
        };

        recentTransactions.Add(tx);
        if (recentTransactions.Count > 32)
        {
            recentTransactions.RemoveAt(0);
        }
    }

    private void LoadEconomyState()
    {
        if (!File.Exists(SavePath))
        {
            walletBalance = startingWallet;
            outstandingDebt = startingDebt;
            totalEarned = Mathf.Max(0, startingWallet);
            totalDebtPaid = 0f;
            recentTransactions.Clear();
            return;
        }

        try
        {
            string json = File.ReadAllText(SavePath);
            EconomySaveData data = JsonUtility.FromJson<EconomySaveData>(json);
            walletBalance = data.wallet;
            outstandingDebt = data.debt;
            totalEarned = data.totalEarned;
            totalDebtPaid = data.totalDebtPaid;
            recentTransactions.Clear();
            if (data.transactions != null)
            {
                recentTransactions.AddRange(data.transactions);
            }
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"EconomyManager: Failed to load save data. Resetting. {ex.Message}");
            walletBalance = startingWallet;
            outstandingDebt = startingDebt;
            totalEarned = Mathf.Max(0, startingWallet);
            totalDebtPaid = 0f;
            recentTransactions.Clear();
        }
    }

    private void SaveEconomyState()
    {
        EconomySaveData data = new EconomySaveData
        {
            wallet = walletBalance,
            debt = outstandingDebt,
            totalEarned = totalEarned,
            totalDebtPaid = totalDebtPaid,
            transactions = recentTransactions
        };

        try
        {
            string json = JsonUtility.ToJson(data, true);
            File.WriteAllText(SavePath, json);
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"EconomyManager: Failed to save state. {ex.Message}");
        }
    }

    private void NotifyListeners()
    {
        OnEconomyChanged?.Invoke(walletBalance, outstandingDebt);
    }

    private void OnApplicationQuit()
    {
        SaveEconomyState();
    }

    /// <summary>
    /// Reset the economy to starting values and delete the save file.
    /// </summary>
    public void ResetEconomy()
    {
        walletBalance = startingWallet;
        outstandingDebt = startingDebt;
        totalEarned = Mathf.Max(0, startingWallet);
        totalDebtPaid = 0f;
        recentTransactions.Clear();

        // Delete the save file
        if (File.Exists(SavePath))
        {
            File.Delete(SavePath);
        }

        SaveEconomyState(); // This will create a new file with starting values
        NotifyListeners();
        Debug.Log("EconomyManager: Economy reset to starting values.");
    }

    #endregion
}

[Serializable]
public class EconomyTransaction
{
    public string timestamp;
    public string description;
    public float amount;
    public float debtApplied;
    public string fishId;
    public FishCatchSource source;
    public bool endangeredSale;
    public bool processedSale;
    public float balanceAfter;
    public float debtAfter;
}

[Serializable]
public class EconomySaveData
{
    public float wallet;
    public float debt;
    public float totalEarned;
    public float totalDebtPaid;
    public List<EconomyTransaction> transactions;
}

public struct EconomyPayoutResult
{
    public int amountAwarded;
    public bool appliedEndangeredPremium;
    public float paidTowardDebt;
    public float walletAfterTransaction;
    public float debtRemaining;
}

