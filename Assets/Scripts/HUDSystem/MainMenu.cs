using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [Header("Panels")]
    public GameObject tipsPanel;

    void Start()
    {
        if (tipsPanel != null)
            tipsPanel.SetActive(false);
    }

    public void PlayGame()
    {
        SceneManager.LoadScene(2); 
    }

    public void OpenTips()
    {
        if (tipsPanel != null)
            tipsPanel.SetActive(true);
    }

    public void CloseTips()
    {
        if (tipsPanel != null)
            tipsPanel.SetActive(false);
    }

    public void QuitGame()
    {
        Debug.Log("Izlaz iz igre.");
        Application.Quit();
    }
}
