using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;

public class CreditSceneScript : MonoBehaviour
{
     public ScrollRect scrollRect;
    public float scrollSpeed = 20f;
    public float fastForwardMultiplier = 4f;
    public string nextSceneName = "MainMenu";
    public float fadeDuration = 1.5f;
    public float waitBeforeSwitch = 2f;
    public Image fadeImage;

    private float contentHeight;
    private bool isFinished = false;
    private bool isSkipping = false;

    void Start()
    {
        // Reset scroll to bottom (credit starts off screen)
        scrollRect.verticalNormalizedPosition = 0f;

        // Dynamic height check (auto adjusts to added text)
        LayoutRebuilder.ForceRebuildLayoutImmediate(scrollRect.content);

        contentHeight = scrollRect.content.rect.height - scrollRect.viewport.rect.height;

        if (fadeImage != null) fadeImage.gameObject.SetActive(true);
        StartCoroutine(FadeIn());
    }

    void Update()
    {
        if (isFinished) return;

        float speed = scrollSpeed;

        // Fast forward if button held
        if (Input.GetKey(KeyCode.Space))
            speed *= fastForwardMultiplier;

        // Scroll automatically
        scrollRect.content.anchoredPosition += Vector2.up * speed * Time.deltaTime;

        // When reached top (fully scrolled)
        if (scrollRect.content.anchoredPosition.y >= contentHeight)
        {
            StartCoroutine(EndCredits());
            isFinished = true;
        }

        // Skip button (like Enter or Esc)
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Escape))
        {
            isSkipping = true;
            StartCoroutine(EndCredits());
        }
    }

    IEnumerator EndCredits()
    {
        yield return new WaitForSeconds(isSkipping ? 0f : waitBeforeSwitch);
        yield return StartCoroutine(FadeOut());
        SceneManager.LoadScene(nextSceneName);
    }

    IEnumerator FadeIn()
    {
        float t = 0f;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            fadeImage.color = new Color(0, 0, 0, 1 - t / fadeDuration);
            yield return null;
        }
    }

    IEnumerator FadeOut()
    {
        float t = 0f;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            fadeImage.color = new Color(0, 0, 0, t / fadeDuration);
            yield return null;
        }
    }
}