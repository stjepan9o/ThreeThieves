using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Kontrolira AI kretanje guarda po predefiniranoj patrol ruti.
/// Na svaki "player turn end" event, guard se pomice do tilesPerTurn tile-ova
/// prema sljedecem patrol pointu koristeci isti A* Pathfinder kao i player.
///
/// Postavljanje u Unity:
/// 1) Dodaj ovu skriptu na Guard GameObject (UnitGridMovement ce se automatski zahtijevati).
/// 2) U sceni kreiraj prazne GameObjecte kao waypointe (npr. "PatrolPoint_1", "PatrolPoint_2"...)
///    i postavi ih na zelene lokacije na gridu (podi, hodnici).
/// 3) Povuci te waypointe u Patrol Points array u Inspectoru, redom kojim zelis da guard patrolira.
/// 4) Guard ce krizati rutu u krug (1 -> 2 -> 3 -> ... -> 1 -> 2 -> ...).
///
/// Folder: Game Systems (ili novi "Guards" folder)
/// </summary>
[RequireComponent(typeof(UnitGridMovement))]
public class GuardController : MonoBehaviour
{
    [Header("Patrol Settings")]
    [Tooltip("Prazni GameObjecti u sceni koji oznacavaju tocke patroliranja. " +
             "Guard ce ih obilaziti redom, u krug.")]
    public Transform[] patrolPoints;

    [Tooltip("Koliko tile-ova guard moze preci po jednom potezu (nakon sto player zavrsi akciju).")]
    public int tilesPerTurn = 3;

    private UnitGridMovement movement;
    private int currentPatrolIndex = 0;
    private bool isTakingTurn = false; // sprijecava pokretanje novog poteza dok stari nije gotov

    void Awake()
    {
        movement = GetComponent<UnitGridMovement>();
    }

    void Start()
    {
        TurnManager.OnPlayerTurnEnd += OnPlayerTurnEnd;
        movement.OnPathComplete += OnMovementComplete;
    }

    void OnDestroy()
    {
        TurnManager.OnPlayerTurnEnd -= OnPlayerTurnEnd;
        if (movement != null)
            movement.OnPathComplete -= OnMovementComplete;
    }

    void OnPlayerTurnEnd()
    {
        Debug.Log(gameObject.name + ": primio OnPlayerTurnEnd signal");
        // Ako guard jos hoda od proslog poteza (npr. player klikne prebrzo), preskoči ovaj potez
        if (isTakingTurn || movement.IsMoving)
            return;

        if (patrolPoints == null || patrolPoints.Length == 0)
            return;

        isTakingTurn = true;
        MoveTowardsCurrentPatrolPoint();
    }

    void MoveTowardsCurrentPatrolPoint()
    {
        Transform target = patrolPoints[currentPatrolIndex];

        if (target == null)
        {
            Debug.LogWarning($"{gameObject.name}: patrol point {currentPatrolIndex} nije assignan!");
            AdvancePatrolIndex();
            isTakingTurn = false;
            return;
        }

        List<Vector3> path = Pathfinder.Instance.FindPath(transform.position, target.position);

        if (path == null || path.Count == 0)
        {
            // Waypoint nedostupan (iza zida ili van grida) - preskoči na sljedeći
            Debug.LogWarning($"{gameObject.name}: ne mogu dosegnuti patrol point {currentPatrolIndex} ({target.name}), preskacam.");
            AdvancePatrolIndex();
            isTakingTurn = false;
            return;
        }

        movement.SetPath(path, tilesPerTurn);
    }

    void OnMovementComplete()
    {
        if (!isTakingTurn) return;

        // Provjeri je li guard dostigao trenutni patrol point
        // (koristi samo X/Z os jer Y moze varirati ovisno o terenu)
        Transform target = patrolPoints[currentPatrolIndex];
        if (target != null)
        {
            float dist = Vector2.Distance(
                new Vector2(transform.position.x, transform.position.z),
                new Vector2(target.position.x, target.position.z)
            );

            if (dist <= 1.5f)
            {
                // Stigao do waypointa - sljedeci potez ide prema iducem
                AdvancePatrolIndex();
            }
            // Ako nije stigao (zaustavljen zbog tilesPerTurn limita),
            // currentPatrolIndex ostaje isti - sljedeci potez nastavlja prema istom pointu
        }

        isTakingTurn = false;
    }

    void AdvancePatrolIndex()
    {
        if (patrolPoints.Length == 0) return;
        currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length;
    }

    // Iscrtava patrol rutu u editoru radi lakseg postavljanja
    void OnDrawGizmos()
    {
        if (patrolPoints == null || patrolPoints.Length < 2) return;

        Gizmos.color = Color.yellow;
        for (int i = 0; i < patrolPoints.Length; i++)
        {
            if (patrolPoints[i] == null) continue;
            Gizmos.DrawSphere(patrolPoints[i].position, 0.3f);

            int next = (i + 1) % patrolPoints.Length;
            if (patrolPoints[next] != null)
                Gizmos.DrawLine(patrolPoints[i].position, patrolPoints[next].position);
        }
    }
}
