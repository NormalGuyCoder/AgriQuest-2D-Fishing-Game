using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class InfoCanvas : MonoBehaviour
{
    [Header("UI Elements")]
    public GameObject infoCanvas;       // The canvas to show/hide
    public TextMeshProUGUI textDisplay; // Typewriter text
    public Button continueButton;       // Button to enable after text

    [Header("Typewriter Settings")]
    [TextArea]
    public string fullText;             // The text to display
    public float typingSpeed = 0.05f;   // Delay per character

    private Coroutine typingCoroutine;

    private void Start()
    {
        // Ensure canvas is hidden at start
        infoCanvas.SetActive(false);
    }

    /// <summary>
    /// Call this from your main menu button OnClick
    /// </summary>
    public void ShowInfoCanvas()
    {
        infoCanvas.SetActive(true);      // Show the canvas
        textDisplay.text = "";           // Clear previous text
        continueButton.interactable = false; // Disable button
        typingCoroutine = StartCoroutine(TypeText());
    }

    private void Update()
    {
        // Skip typewriter if player clicks anywhere
        if (infoCanvas.activeSelf && typingCoroutine != null && Input.GetMouseButtonDown(0))
        {
            StopCoroutine(typingCoroutine);
            textDisplay.text = fullText;
            continueButton.interactable = true;
            typingCoroutine = null;
        }
    }

    private IEnumerator TypeText()
    {
        textDisplay.text = "";

        foreach (char letter in fullText.ToCharArray())
        {
            textDisplay.text += letter;
            yield return new WaitForSeconds(typingSpeed);
        }

        continueButton.interactable = true;
        typingCoroutine = null;
    }

    /// <summary>
    /// Optional: Call this from the continue button to close the canvas
    /// </summary>
    public void CloseInfoCanvas()
    {
        infoCanvas.SetActive(false);
    }
}