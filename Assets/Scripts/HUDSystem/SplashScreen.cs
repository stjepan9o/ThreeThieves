
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class SplashScreen : MonoBehaviour
{
    public CanvasGroup logoCanvasGroup;
    public float fadeInDuration = 1.5f;
    public float holdDuration = 2f;
    public float fadeOutDuration = 1f;

    void Start()
    {
        logoCanvasGroup.alpha = 0f;
        StartCoroutine(PlaySplash());
    }

    IEnumerator PlaySplash()
    {
        float t = 0f;
        while (t < fadeInDuration)
        {
            t += Time.deltaTime;
            logoCanvasGroup.alpha = Mathf.Clamp01(t / fadeInDuration);
            yield return null;
        }

        yield return new WaitForSeconds(holdDuration);

        t = 0f;
        while (t < fadeOutDuration)
        {
            t += Time.deltaTime;
            logoCanvasGroup.alpha = 1f - Mathf.Clamp01(t / fadeOutDuration);
            yield return null;
        }

        SceneManager.LoadScene(1);
    }
}