using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverManager : MonoBehaviour
{
    public static GameOverManager Instance { get; private set; }

    [Header("Panels")]
    public GameObject gameOverPanel;
    public GameObject victoryPanel;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Start()
    {
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (victoryPanel != null) victoryPanel.SetActive(false);

        if (AlarmSystem.Instance != null)
            AlarmSystem.Instance.OnStateChanged += OnAlarmStateChanged;
    }

    void OnDestroy()
    {
        if (AlarmSystem.Instance != null)
            AlarmSystem.Instance.OnStateChanged -= OnAlarmStateChanged;
    }

    void OnAlarmStateChanged(AlarmSystem.AlarmState state)
    {
        if (state == AlarmSystem.AlarmState.Red)
            TriggerGameOver();
    }

    public void TriggerGameOver()
    {
        if (gameOverPanel != null) gameOverPanel.SetActive(true);
        Time.timeScale = 0f;
    }

    public void TriggerVictory()
    {
        if (victoryPanel != null) victoryPanel.SetActive(true);
        Time.timeScale = 0f;
    }

    // --- Game Over gumbi ---
    public void PlayAgain()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void GoToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(0);
    }
}
