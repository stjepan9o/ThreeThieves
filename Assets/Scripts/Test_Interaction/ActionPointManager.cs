using UnityEngine;
using System;

public class ActionPointManager : MonoBehaviour
{
    [Header("Action Points")]
    public int maxAP = 6;
    public int CurrentAP { get; private set; }

    public event System.Action<int> OnAPChanged;
    public event System.Action OnNotEnoughAP;

    void Awake()
    {
        CurrentAP = maxAP;
    }

    void Start()
    {
        OnAPChanged?.Invoke(CurrentAP);
        Debug.Log($"Potez poceo! AP: {CurrentAP}/{maxAP}");

        TurnManager.OnPlayerTurnStart += ResetAP;
    }

    void OnDestroy()
    {
        TurnManager.OnPlayerTurnStart -= ResetAP;
    }

    public bool HasEnoughAP(int cost)
    {
        return CurrentAP >= cost;
    }


    public void NotifyNotEnoughAP()
    {
        OnNotEnoughAP?.Invoke();
    }

    public void SpendAP(int amount)
    {
        CurrentAP -= amount;
        CurrentAP = Mathf.Max(0, CurrentAP);
        OnAPChanged?.Invoke(CurrentAP);
        Debug.Log($"Potroseno {amount} AP. Preostalo: {CurrentAP}/{maxAP}");

        if (CurrentAP == 0)
            TurnManager.Instance?.EndPlayerTurn();
    }

    public void ResetAP()
    {
        CurrentAP = maxAP;
        OnAPChanged?.Invoke(CurrentAP);
        Debug.Log($"Novi potez! AP resetiran na {maxAP}");
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
            TurnManager.Instance?.EndPlayerTurn();
    }
}