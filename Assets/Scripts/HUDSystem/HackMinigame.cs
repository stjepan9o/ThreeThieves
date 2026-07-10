using UnityEngine;
using TMPro;
using System;

public class HackMinigame : MonoBehaviour
{
    public static HackMinigame Instance { get; private set; }

    [Header("UI")]
    public GameObject panel;
    public TextMeshProUGUI sequenceText;
    public TextMeshProUGUI timerText;

    [Header("Settings")]
    public int sequenceLength = 5;
    public float timeLimit = 5f;

    [Header("Timer Panic")]
    public float panicTime = 1.5f;
    public Color timerNormalColor = new Color(1f, 0.718f, 0.302f);
    public Color timerPanicColor = new Color(1f, 0.25f, 0.25f);

    public bool IsRunning { get; private set; }


    private static readonly KeyCode[] keyPool =
    {
        KeyCode.Q, KeyCode.W, KeyCode.R, KeyCode.T, KeyCode.F,
        KeyCode.Z, KeyCode.X, KeyCode.C, KeyCode.V, KeyCode.B
    };

    private KeyCode[] sequence;
    private int progress;
    private float timeLeft;
    private Action<bool> onFinished;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void Start()
    {
        if (panel != null)
            panel.SetActive(false);
    }

    public void StartGame(Action<bool> callback)
    {
        if (IsRunning) return;

        onFinished = callback;

        sequence = new KeyCode[sequenceLength];
        for (int i = 0; i < sequenceLength; i++)
            sequence[i] = keyPool[UnityEngine.Random.Range(0, keyPool.Length)];

        progress = 0;
        timeLeft = timeLimit;
        IsRunning = true;

        if (timerText != null)
        {
            timerText.color = timerNormalColor;
            timerText.transform.localScale = Vector3.one;
        }

        if (panel != null)
            panel.SetActive(true);

        UpdateSequenceText();
    }

    void Update()
    {
        if (!IsRunning) return;

        timeLeft -= Time.deltaTime;
        if (timerText != null)
        {
            timerText.text = timeLeft.ToString("0.0");

            if (timeLeft <= panicTime)
            {
                timerText.color = timerPanicColor;

                float pulse = 1f + 0.15f * (1f + Mathf.Sin(Time.time * 18f));
                timerText.transform.localScale = Vector3.one * pulse;
            }
        }

        if (timeLeft <= 0f)
        {
            Finish(false);
            return;
        }

        foreach (KeyCode key in keyPool)
        {
            if (!Input.GetKeyDown(key)) continue;

            if (key == sequence[progress])
            {
                progress++;
                UpdateSequenceText();
                if (progress >= sequence.Length)
                    Finish(true);
            }
            else
            {
                Finish(false);
            }
            return;
        }
    }

    private void UpdateSequenceText()
    {
        if (sequenceText == null) return;

        string text = "";
        for (int i = 0; i < sequence.Length; i++)
        {
            if (i < progress)
                text += $"<color=#4CAF50>{sequence[i]}</color> ";
            else
                text += sequence[i] + " ";
        }
        sequenceText.text = text.TrimEnd();
    }

    private void Finish(bool success)
    {
        IsRunning = false;
        if (panel != null)
            panel.SetActive(false);

        Debug.Log(success ? "Hack uspjesan!" : "Hack neuspjesan!");

        Action<bool> callback = onFinished;
        onFinished = null;
        callback?.Invoke(success);
    }
}
