using UnityEngine;
using TMPro;
using System.Collections;

public class APDisplay : MonoBehaviour
{
    public static APDisplay Instance { get; private set; }

    [SerializeField] private TMP_Text apText;
    [SerializeField] private TMP_Text statusText;
    [SerializeField] private ActionPointManager apManager;

    [Header("Messages")]
    [SerializeField] private float messageDuration = 2f;

    private Coroutine messageRoutine;

    void Awake()
    {
        Instance = this;
    }

    public void ShowMessage(string message)
    {
        if (statusText == null) return;

        if (messageRoutine != null) StopCoroutine(messageRoutine);
        messageRoutine = StartCoroutine(TemporaryMessage(message));
    }

    void Start()
    {
        apManager.OnAPChanged += UpdateText;
        apManager.OnNotEnoughAP += ShowNotEnoughAP;
        TurnManager.OnGuardTurnStart += ShowEnemyTurn;
        TurnManager.OnPlayerTurnStart += ClearStatus;

        UpdateText(apManager.CurrentAP);
        ClearStatus();
    }

    void OnDestroy()
    {
        if (apManager != null)
        {
            apManager.OnAPChanged -= UpdateText;
            apManager.OnNotEnoughAP -= ShowNotEnoughAP;
        }
        TurnManager.OnGuardTurnStart -= ShowEnemyTurn;
        TurnManager.OnPlayerTurnStart -= ClearStatus;
    }

    void UpdateText(int ap)
    {
        apText.text = $"Current AP: {ap}/{apManager.maxAP}";
    }

    void ShowNotEnoughAP()
    {
        if (statusText == null) return;

        if (messageRoutine != null) StopCoroutine(messageRoutine);
        messageRoutine = StartCoroutine(TemporaryMessage("Not enough action points!"));
    }

    void ShowEnemyTurn()
    {
        if (statusText == null) return;


        if (messageRoutine != null) StopCoroutine(messageRoutine);
        messageRoutine = null;
        statusText.text = "Enemy turn in progress...";
    }

    void ClearStatus()
    {
        if (statusText == null) return;

        if (messageRoutine != null) StopCoroutine(messageRoutine);
        messageRoutine = null;
        statusText.text = "";
    }

    IEnumerator TemporaryMessage(string message)
    {
        statusText.text = message;
        yield return new WaitForSeconds(messageDuration);
        statusText.text = "";
        messageRoutine = null;
    }
}
