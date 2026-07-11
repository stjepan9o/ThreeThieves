using UnityEngine;
using System.Collections;

/// <summary>
/// Fade-in animacija za Game Over panel.
/// Postavljanje:
/// 1) Dodaj na GameOverPanel objekt.
/// 2) GameOverPanel treba imati CanvasGroup komponentu (Add Component → CanvasGroup).
/// </summary>
[RequireComponent(typeof(CanvasGroup))]
public class GameOverPanel : MonoBehaviour
{
    [Header("Fade Settings")]
    public float fadeDuration = 1.5f;
    public float textDelay = 0.5f; // Koliko cekati prije nego tekst pocne pojavljivati

    private CanvasGroup canvasGroup;

    void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        canvasGroup.alpha = 0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
    }

    void OnEnable()
    {
        StartCoroutine(FadeIn());
    }

    IEnumerator FadeIn()
    {
        canvasGroup.alpha = 0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;

        // Cekaj malo prije fade-ina
        yield return new WaitForSecondsRealtime(textDelay);

        // Fade in
        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.unscaledDeltaTime; // unscaled jer je Time.timeScale = 0
            canvasGroup.alpha = Mathf.Clamp01(elapsed / fadeDuration);
            yield return null;
        }

        canvasGroup.alpha = 1f;
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;
    }
}
