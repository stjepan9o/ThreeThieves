using UnityEngine;
using System;

// Upravlja Action Pointima tima
public class ActionPointManager : MonoBehaviour
{
    [Header("Action Points")]
    public int maxAP = 6;           // Maksimalni AP po potezu
    public int CurrentAP { get; private set; }

    public event System.Action<int> OnAPChanged;

    void Awake()
    {
        CurrentAP = maxAP;
    }

    void Start()
    {
        OnAPChanged?.Invoke(CurrentAP);
        Debug.Log($"Potez poceo! AP: {CurrentAP}/{maxAP}");

        // [DODANO] Kad TurnManager signalizira novi player potez, resetiraj AP
        TurnManager.OnPlayerTurnStart += ResetAP;
    }

    // [DODANO] Odjava od eventa kad se objekt unisti
    void OnDestroy()
    {
        TurnManager.OnPlayerTurnStart -= ResetAP;
    }

    public bool HasEnoughAP(int cost)
    {
        return CurrentAP >= cost;
    }

    public void SpendAP(int amount)
    {
        CurrentAP -= amount;
        CurrentAP = Mathf.Max(0, CurrentAP); // Ne moze ici ispod 0
        OnAPChanged?.Invoke(CurrentAP);
        Debug.Log($"Potroseno {amount} AP. Preostalo: {CurrentAP}/{maxAP}");

        // [DODANO] Kad AP padne na 0, automatski zavrsi player potez
        if (CurrentAP == 0)
            TurnManager.Instance?.EndPlayerTurn();
    }

    // Resetiraj AP na pocetku novog poteza
    public void ResetAP()
    {
        CurrentAP = maxAP;
        OnAPChanged?.Invoke(CurrentAP);
        Debug.Log($"Novi potez! AP resetiran na {maxAP}");
    }

    // [IZMIJENJENO] E vise ne resetira AP direktno - zavrsi potez, TurnManager ce resetirati AP
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
            TurnManager.Instance?.EndPlayerTurn();
    }
}