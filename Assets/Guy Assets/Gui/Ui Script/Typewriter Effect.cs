using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class TypewriterEffect : MonoBehaviour
{
    public TextMeshProUGUI textComponent;
    public Button targetButton;

    [TextArea]
    public string fullText;

    public float delayBetweenWords = 0.3f;

    void Start()
    {
        targetButton.interactable = false;
        textComponent.text = "";
        StartCoroutine(TypeText());
    }

    IEnumerator TypeText()
    {
        string[] words = fullText.Split(' ');

        foreach (string word in words)
        {
            textComponent.text += word + " ";
            yield return new WaitForSeconds(delayBetweenWords);
        }

        // Enable button after text finishes
        targetButton.interactable = true;
    }
}