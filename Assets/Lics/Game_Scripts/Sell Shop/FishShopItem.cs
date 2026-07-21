using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class FishShopItem : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text descriptionText;
    [SerializeField] private Button sellButton;
    [SerializeField] private Button sellAllButton;

    private string fishId;
    private System.Action<string> sellCallback;
    private System.Action<string> sellAllCallback;

    private void Start()
    {
        if (sellButton != null)
        {
            sellButton.onClick.AddListener(OnSellClicked);
        }

        if (sellAllButton != null)
        {
            sellAllButton.onClick.AddListener(OnSellAllClicked);
        }
    }

    public void Setup(string id, string title, string description,
                     System.Action<string> onSell, System.Action<string> onSellAll = null)
    {
        fishId = id;
        sellCallback = onSell;
        sellAllCallback = onSellAll;

        if (titleText != null)
        {
            titleText.text = title;
        }

        if (descriptionText != null)
        {
            descriptionText.text = description;
        }

        if (sellAllButton != null && sellAllCallback == null)
        {
            sellAllButton.gameObject.SetActive(false);
        }
    }

    private void OnSellClicked()
    {
        sellCallback?.Invoke(fishId);
    }

    private void OnSellAllClicked()
    {
        sellAllCallback?.Invoke(fishId);
    }
}