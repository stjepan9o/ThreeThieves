using UnityEngine;
using System;

/// <summary>
/// Centralni manager za izmjenu poteza player/AI.
/// GridPlayerController poziva EndPlayerTurn() kad zavrsi akciju,
/// a GuardController (i eventualni ostali AI entiteti) slusaju OnPlayerTurnEnd event.
///
/// Postavljanje u Unity: dodaj na GameManager objekt (uz ostale managere).
/// Folder: Game Systems
/// </summary>
public class TurnManager : MonoBehaviour
{
    public static TurnManager Instance { get; private set; }

    /// <summary>
    /// Okida se kada igrac zavrsi svoju akciju (kretanje ili interakcija).
    /// Svi GuardControlleri u sceni ce reagirati na ovaj event i napraviti svoj potez.
    /// </summary>
    public static event Action OnPlayerTurnEnd;

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
    /// Poziva GridPlayerController po zavrsetku svake akcije.
    /// Ne zovi vise puta po potezu - svaki poziv ce pokrenuti AI potez svih guardova.
    /// </summary>
    public void EndPlayerTurn()
    {
        Debug.Log("EndPlayerTurn pozvan, broj pretplatnika: " + (OnPlayerTurnEnd?.GetInvocationList().Length ?? 0));
        OnPlayerTurnEnd?.Invoke();
    }
}
