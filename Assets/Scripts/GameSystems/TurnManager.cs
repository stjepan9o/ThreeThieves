using UnityEngine;
using System;
using System.Collections;

public enum TurnState
{
    PlayerTurn,
    GuardTurn
}

/// <summary>
/// Upravlja izmjenom poteza player/guard.
///
/// Tok:
/// 1) Player koristi AP (ili pritisne E)
/// 2) EndPlayerTurn() -> stanje = GuardTurn -> OnGuardTurnStart event
/// 3) Guardovi se pocnu micati po patrol ruti
/// 4) Nakon guardTurnDuration sekundi -> stanje = PlayerTurn -> OnPlayerTurnStart event
/// 5) ActionPointManager resetira AP, player moze ponovo igrati
///
/// Postavljanje: dodaj na GameManager objekt.
/// </summary>
public class TurnManager : MonoBehaviour
{
    public static TurnManager Instance { get; private set; }

    [Header("Turn Settings")]
    [Tooltip("Koliko sekundi traje guard potez (koliko player ceka).")]
    public float guardTurnDuration = 3f;

    public TurnState CurrentState { get; private set; } = TurnState.PlayerTurn;
    public bool IsPlayerTurn => CurrentState == TurnState.PlayerTurn;

    /// <summary>Okida se kad pocinje player potez - ActionPointManager resetira AP.</summary>
    public static event Action OnPlayerTurnStart;

    /// <summary>Okida se kad pocinje guard potez - GuardController pocinje kretanje.</summary>
    public static event Action OnGuardTurnStart;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    /// <summary>
    /// Poziva ActionPointManager kad AP padne na 0 ili player pritisne E.
    /// Ignorira poziv ako je vec guard potez u tijeku.
    /// </summary>
    public void EndPlayerTurn()
    {
        if (CurrentState != TurnState.PlayerTurn) return;

        CurrentState = TurnState.GuardTurn;
        Debug.Log("--- GUARD POTEZ POCINJE ---");
        OnGuardTurnStart?.Invoke();
        StartCoroutine(GuardTurnRoutine());
    }

    IEnumerator GuardTurnRoutine()
    {
        yield return new WaitForSeconds(guardTurnDuration);

        CurrentState = TurnState.PlayerTurn;
        Debug.Log("--- PLAYER POTEZ POCINJE ---");
        OnPlayerTurnStart?.Invoke();
    }
}
