using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;

public class DialogUIController : MonoBehaviour
{
    [Header("UI References - Assign from Scene")]
    public GameObject dialogCanvas;
    public TMP_Text npcNameText;
    public TMP_Text dialogText;
    public GameObject choicesSlots;

    [Header("Choice Buttons - Assign from Scene")]
    public Button[] choiceButtons = new Button[3];
    public TMP_Text[] choiceTexts = new TMP_Text[3];

    [Header("Settings")]
    public float textSpeed = 0.05f; // Speed of typewriter effect
    public bool autoAdvance = false;
    public KeyCode advanceKey = KeyCode.Space;
    public KeyCode skipKey = KeyCode.Return;
    public KeyCode closeKey = KeyCode.Escape;

    [Header("Visual Novel Style")]
    public Color actionTextColor = Color.yellow;
    public Color normalTextColor = Color.white;

    private bool isTyping = false;
    private bool showingChoices = false;
    private string fullText = "";
    private float typeTimer = 0f;
    private int currentCharIndex = 0;
    private List<DialogChoice> currentChoices = new List<DialogChoice>();

    void Start()
    {
        if (dialogCanvas != null)
            dialogCanvas.SetActive(false);

        for (int i = 0; i < choiceButtons.Length; i++)
        {
            if (choiceButtons[i] != null)
                choiceButtons[i].gameObject.SetActive(false);
        }

        if (choicesSlots != null)
            choicesSlots.SetActive(false);

        // Subscribe to DialogSystem events
        if (DialogSystem.Instance != null)
        {
            DialogSystem.Instance.OnDialogLineReady += HandleDialogLine;
            DialogSystem.Instance.OnChoicesReady += HandleChoices;
            DialogSystem.Instance.OnDialogStarted += HandleDialogStarted;
            DialogSystem.Instance.OnDialogEnded += HandleDialogEnded;
        }
    }

    void Update()
    {
        if (!dialogCanvas.activeSelf || !DialogSystem.Instance.IsDialogActive())
            return;

        // Handle typing effect
        if (isTyping)
        {
            HandleTyping();
        }
        else if (showingChoices)
        {
            // Player is viewing choices, wait for selection
            // No input handling needed here as buttons handle clicks
        }
        else
        {
            // NOT typing and NOT showing choices
            // This is where we can advance to next dialogue with Space
            if (Input.GetKeyDown(advanceKey))
            {
                // If we have choices available, show them
                if (DialogSystem.Instance.HasAvailableChoices() && !showingChoices)
                {
                    DialogSystem.Instance.ShowAvailableChoices();
                }
                else if (!DialogSystem.Instance.HasAvailableChoices())
                {
                    // If no choices, advance to next dialog
                    DialogSystem.Instance.AdvanceDialog();
                }
            }
        }

        // Always allow closing with Escape
        if (Input.GetKeyDown(closeKey))
        {
            DialogSystem.Instance.EndDialog();
        }
    }

    void HandleTyping()
    {
        // If Space or Return is pressed, skip to end of current line
        if (Input.GetKeyDown(skipKey) || Input.GetKeyDown(advanceKey))
        {
            dialogText.text = fullText;
            FinishTyping();
            return;
        }

        // Normal typewriter effect
        if (typeTimer <= 0)
        {
            if (currentCharIndex < fullText.Length)
            {
                dialogText.text = fullText.Substring(0, currentCharIndex + 1);
                currentCharIndex++;
                typeTimer = textSpeed;
            }
            else
            {
                FinishTyping();
            }
        }
        else
        {
            typeTimer -= Time.deltaTime;
        }
    }

    void FinishTyping()
    {
        isTyping = false;
        typeTimer = 0f;

        // Check if there are choices available
        if (DialogSystem.Instance != null && DialogSystem.Instance.HasAvailableChoices())
        {
            // Don't auto-show choices, wait for player to press Space
            Debug.Log("Dialogue finished. Choices available. Press Space to see choices.");
        }
        else if (autoAdvance)
        {
            // Auto-advance after a delay
            StartCoroutine(AutoAdvanceAfterDelay(2f));
        }
        else
        {
            // No choices, just wait for Space to advance
            Debug.Log("Dialogue finished. Press Space to continue.");
        }
    }

    IEnumerator AutoAdvanceAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        DialogSystem.Instance.AdvanceDialog();
    }

    void HandleDialogLine(string npcName, DialogLine dialogLine)
    {
        dialogCanvas.SetActive(true);
        
        // Set NPC name (use dialog line's speaker name if available, otherwise use NPC name)
        if (!string.IsNullOrEmpty(dialogLine.speakerName))
            npcNameText.text = dialogLine.speakerName;
        else
            npcNameText.text = npcName;

        // FIRST: Hide choices panel when starting new dialogue
        if (choicesSlots != null)
            choicesSlots.SetActive(false);
            
        // Ensure dialog text is active and visible
        if (dialogText != null)
        {
            dialogText.gameObject.SetActive(true);
            
            // Apply visual novel style for actions
            if (dialogLine.showAsAction)
            {
                dialogText.color = actionTextColor;
                fullText = $"* {dialogLine.text} *";
            }
            else
            {
                dialogText.color = normalTextColor;
                fullText = dialogLine.text;
            }
        }

        // Start typewriter effect immediately
        dialogText.text = "";
        currentCharIndex = 0;
        typeTimer = textSpeed;
        isTyping = true;
        showingChoices = false;

        // Clear current choices when starting new dialog line
        currentChoices.Clear();
        ClearChoiceButtons();

        // Play audio clip if available
        if (!string.IsNullOrEmpty(dialogLine.audioClipName))
        {
            // Implement your audio playing logic here
            Debug.Log($"Would play audio clip: {dialogLine.audioClipName}");
        }
    }

    void HandleChoices(List<DialogChoice> choices)
    {
        // Store a copy of the choices
        currentChoices = new List<DialogChoice>(choices);
        
        // Show choices immediately
        ShowChoices();
    }

    void ShowChoices()
    {
        if (currentChoices == null || currentChoices.Count == 0)
        {
            Debug.LogWarning("No choices to show!");
            if (choicesSlots != null)
                choicesSlots.SetActive(false);
            return;
        }

        showingChoices = true;

        // IMPORTANT: Hide the NPC dialogue text before showing choices
        if (dialogText != null)
            dialogText.gameObject.SetActive(false);

        // Show the choices slots
        if (choicesSlots != null)
            choicesSlots.SetActive(true);

        int choicesToShow = Mathf.Min(currentChoices.Count, choiceButtons.Length);

        for (int i = 0; i < choicesToShow; i++)
        {
            if (i < choiceButtons.Length && i < choiceTexts.Length &&
                choiceButtons[i] != null && choiceTexts[i] != null)
            {
                var choice = currentChoices[i];
                choiceTexts[i].text = choice.choiceText;
                choiceButtons[i].gameObject.SetActive(true);
                choiceButtons[i].interactable = true;

                // Clear previous listeners and add new one
                choiceButtons[i].onClick.RemoveAllListeners();
                int index = i;
                choiceButtons[i].onClick.AddListener(() => OnChoiceSelected(index));
            }
        }

        // Hide unused buttons
        for (int i = choicesToShow; i < choiceButtons.Length; i++)
        {
            if (choiceButtons[i] != null)
                choiceButtons[i].gameObject.SetActive(false);
        }
    }

    void OnChoiceSelected(int choiceIndex)
    {
        if (choiceIndex < 0 || choiceIndex >= currentChoices.Count)
        {
            Debug.LogWarning($"Invalid choice index: {choiceIndex}. Available choices: {currentChoices.Count}");
            return;
        }

        var selectedChoice = currentChoices[choiceIndex];
        Debug.Log($"Selected choice: {selectedChoice.choiceText}");

        ClearChoiceButtons();
        currentChoices.Clear();
        showingChoices = false;

        if (choicesSlots != null)
            choicesSlots.SetActive(false);

        DialogSystem.Instance.SelectChoice(choiceIndex);
    }

    void HandleDialogStarted(string npcName)
    {
        Debug.Log($"Dialog started with {npcName}");
    }

    void HandleDialogEnded(string npcName)
    {
        dialogCanvas.SetActive(false);
        ClearChoiceButtons();
        currentChoices.Clear();
        showingChoices = false;

        if (choicesSlots != null)
            choicesSlots.SetActive(false);

        Debug.Log($"Dialog ended with {npcName}");
    }

    void ClearChoiceButtons()
    {
        for (int i = 0; i < choiceButtons.Length; i++)
        {
            if (choiceButtons[i] != null)
            {
                choiceButtons[i].gameObject.SetActive(false);
                choiceButtons[i].onClick.RemoveAllListeners();
            }
        }
    }

    // Public method for UI button to advance dialog
    public void AdvanceDialogButton()
    {
        if (dialogCanvas.activeSelf && DialogSystem.Instance.IsDialogActive() && !showingChoices)
        {
            if (isTyping)
            {
                // If typing, skip to end
                dialogText.text = fullText;
                FinishTyping();
            }
            else
            {
                // If not typing and has choices, show choices
                if (DialogSystem.Instance.HasAvailableChoices() && !showingChoices)
                {
                    DialogSystem.Instance.ShowAvailableChoices();
                }
                else
                {
                    // If not typing and no choices, advance to next dialog
                    DialogSystem.Instance.AdvanceDialog();
                }
            }
        }
    }

    void OnDestroy()
    {
        // Unsubscribe from events
        if (DialogSystem.Instance != null)
        {
            DialogSystem.Instance.OnDialogLineReady -= HandleDialogLine;
            DialogSystem.Instance.OnChoicesReady -= HandleChoices;
            DialogSystem.Instance.OnDialogStarted -= HandleDialogStarted;
            DialogSystem.Instance.OnDialogEnded -= HandleDialogEnded;
        }
    }
}