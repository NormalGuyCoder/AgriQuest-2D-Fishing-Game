using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class FishEntryUI : MonoBehaviour
{
    [SerializeField] private Image fishIcon;
    [SerializeField] private TextMeshProUGUI fishName;
    [SerializeField] private TextMeshProUGUI fishRarity;
    [SerializeField] private Button entryButton;

    private Action onClickCallback;

    private void Awake()
    {
        if (entryButton == null)
        {
            entryButton = GetComponent<Button>();
        }
        
        if (entryButton != null)
        {
            entryButton.onClick.AddListener(() => onClickCallback?.Invoke());
        }
    }

    public void Setup(FishData fish, Action onClick)
    {
        if (fish == null) return;

        onClickCallback = onClick;

        if (fishIcon != null && fish.fishIcon != null)
        {
            fishIcon.sprite = fish.fishIcon;
        }

        if (fishName != null)
        {
            fishName.text = fish.fishName;
        }

        if (fishRarity != null)
        {
            string rarityText = fish.rarity switch
            {
                var r when r < 0.3f => "Common",
                var r when r < 0.7f => "Rare",
                _ => "Legendary"
            };
            fishRarity.text = rarityText;
            fishRarity.color = fish.rarity switch
            {
                var r when r < 0.3f => Color.white,
                var r when r < 0.7f => Color.blue,
                _ => Color.yellow
            };
        }
    }
} 