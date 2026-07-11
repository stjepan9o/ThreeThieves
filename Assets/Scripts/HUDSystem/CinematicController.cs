using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class CinematicController : MonoBehaviour
{
    [Header("Slides")]
    public Sprite[] slides;

    [Header("Settings")]
    public float holdDuration = 3f;
    public float fadeDuration = 0.8f;
    public float zoomAmount = 1.12f;

    [Header("References")]
    public Image slideImage;
    public CanvasGroup blackOverlay;

    void Start()
    {
        blackOverlay.alpha = 1f;
        StartCoroutine(PlayCinematic());
    }

    IEnumerator PlayCinematic()
    {
        for (int i = 0; i < slides.Length; i++)
        {
            slideImage.sprite = slides[i];
            slideImage.transform.localScale = Vector3.one;

            yield return StartCoroutine(FadeOverlay(1f, 0f, fadeDuration));

            float elapsed = 0f;
            while (elapsed < holdDuration)
            {
                elapsed += Time.deltaTime;
                float scale = Mathf.Lerp(1f, zoomAmount, elapsed / holdDuration);
                slideImage.transform.localScale = new Vector3(scale, scale, 1f);
                yield return null;
            }

            yield return StartCoroutine(FadeOverlay(0f, 1f, fadeDuration));
        }

        SceneManager.LoadScene(3);
    }

    IEnumerator FadeOverlay(float from, float to, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            blackOverlay.alpha = Mathf.Lerp(from, to, elapsed / duration);
            yield return null;
        }
        blackOverlay.alpha = to;
    }
}