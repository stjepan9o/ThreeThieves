using UnityEngine;
using System;
using System.Collections;

public enum TurnState
{
    PlayerTurn,
    GuardTurn
}

public class TurnManager : MonoBehaviour
{
    public static TurnManager Instance { get; private set; }

    [Header("Turn Settings")]
    [Tooltip("Koliko sekundi traje guard potez (koliko player ceka).")]
    public float guardTurnDuration = 3f;

    public TurnState CurrentState { get; private set; } = TurnState.PlayerTurn;
    public bool IsPlayerTurn => CurrentState == TurnState.PlayerTurn;

    public static event Action OnPlayerTurnStart;

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
