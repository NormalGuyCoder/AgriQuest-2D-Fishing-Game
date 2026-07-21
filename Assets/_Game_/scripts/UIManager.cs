using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    private static UIManager instance;
    public static UIManager Instance
    {
        get { return instance; }
    }

    [Header("UI Panels")]
    [SerializeField] private GameObject pauseMenu;
    [SerializeField] private GameObject inventoryPanel;
    [SerializeField] private GameObject toolbarPanel;

    [Header("Game Info")]
    [SerializeField] private TextMeshProUGUI timeText;
    [SerializeField] private TextMeshProUGUI dateText;
    [SerializeField] private TextMeshProUGUI moneyText;
    [SerializeField] private TextMeshProUGUI energyText;

    [Header("Toolbar")]
    [SerializeField] private Image selectedToolIcon;
    [SerializeField] private Image selectedItemIcon;

    [Header("Messages")]
    [SerializeField] private GameObject messagePanel;
    [SerializeField] private TextMeshProUGUI messageText;
    [SerializeField] private float messageDuration = 2f;

    private bool isPaused = false;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        // Initialize UI
        if (pauseMenu != null) pauseMenu.SetActive(false);
        if (inventoryPanel != null) inventoryPanel.SetActive(false);
        if (messagePanel != null) messagePanel.SetActive(false);
    }

    private void Update()
    {
        // Handle pause menu
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePauseMenu();
        }

        // Handle inventory
        if (Input.GetKeyDown(KeyCode.I))
        {
            ToggleInventory();
        }
    }

    public void TogglePauseMenu()
    {
        isPaused = !isPaused;
        if (pauseMenu != null)
        {
            pauseMenu.SetActive(isPaused);
            Time.timeScale = isPaused ? 0f : 1f;
        }
    }

    public void ToggleInventory()
    {
        if (inventoryPanel != null)
        {
            bool isActive = !inventoryPanel.activeSelf;
            inventoryPanel.SetActive(isActive);
            Time.timeScale = isActive ? 0f : 1f;
        }
    }

    public void UpdateMoney(int amount)
    {
        if (moneyText != null)
        {
            moneyText.text = $"${amount:N0}";
        }
    }

    public void UpdateEnergy(float currentEnergy, float maxEnergy)
    {
        if (energyText != null)
        {
            energyText.text = $"Energy: {currentEnergy:N0}/{maxEnergy:N0}";
        }
    }

    public void UpdateTime(string time)
    {
        if (timeText != null)
        {
            timeText.text = time;
        }
    }

    public void UpdateDate(string date)
    {
        if (dateText != null)
        {
            dateText.text = date;
        }
    }

    public void ShowMessage(string message)
    {
        if (messagePanel != null && messageText != null)
        {
            messageText.text = message;
            messagePanel.SetActive(true);
            Invoke(nameof(HideMessage), messageDuration);
        }
    }

    private void HideMessage()
    {
        if (messagePanel != null)
        {
            messagePanel.SetActive(false);
        }
    }

    public void UpdateSelectedTool(Sprite toolSprite)
    {
        if (selectedToolIcon != null)
        {
            selectedToolIcon.sprite = toolSprite;
            selectedToolIcon.enabled = toolSprite != null;
        }
    }

    public void UpdateSelectedItem(Sprite itemSprite)
    {
        if (selectedItemIcon != null)
        {
            selectedItemIcon.sprite = itemSprite;
            selectedItemIcon.enabled = itemSprite != null;
        }
    }

    public void OnQuitButton()
    {
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }

    public void OnResumeButton()
    {
        TogglePauseMenu();
    }
} 