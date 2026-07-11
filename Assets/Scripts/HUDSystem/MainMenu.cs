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

    // PlayButton → onClick
    public void PlayGame()
    {
        SceneManager.LoadScene(1);
    }

    // TipsButton → onClick
    public void OpenTips()
    {
        if (tipsPanel != null)
            tipsPanel.SetActive(true);
    }

    // "Back" gumb unutar Tips panela → onClick
    public void CloseTips()
    {
        if (tipsPanel != null)
            tipsPanel.SetActive(false);
    }

    // QuitButton → onClick
    public void QuitGame()
    {
        Debug.Log("Izlaz iz igre.");
        Application.Quit();
    }
}
