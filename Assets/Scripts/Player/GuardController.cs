using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Kontrolira AI kretanje guarda po predefiniranoj patrol ruti.
/// Reagira na TurnManager.OnGuardTurnStart - kad player zavrsi potez,
/// guard se pomice do tilesPerTurn tile-ova prema sljedecem patrol pointu.
///
/// Postavljanje u Unity:
/// 1) Dodaj ovu skriptu na Guard GameObject.
///    UnitGridMovement ce se automatski zahtijevati.
/// 2) U sceni kreiraj prazne GameObjecte kao waypointe
///    (npr. "Guard1_Point1", "Guard1_Point2"...) i postavi ih na prohodni pod.
/// 3) Povuci waypointe u Patrol Points array u Inspectoru, redom kojim
///    zelis da guard patrolira. Guard ce rute prolaziti u krug.
/// 4) Tiles Per Turn = koliko tile-ova guard prelazi po potezu (preporuka: 3-4).
/// </summary>
[RequireComponent(typeof(UnitGridMovement))]
public class GuardController : MonoBehaviour
{
    [Header("Patrol Settings")]
    public Transform[] patrolPoints;
    public int tilesPerTurn = 3;

    private UnitGridMovement movement;
    private int currentPatrolIndex = 0;
    private bool isTakingTurn = false;

    void Awake()
    {
        movement = GetComponent<UnitGridMovement>();
    }

    void Start()
    {
        TurnManager.OnGuardTurnStart += OnGuardTurnStart;
        movement.OnPathComplete += OnMovementComplete;
    }

    void OnDestroy()
    {
        TurnManager.OnGuardTurnStart -= OnGuardTurnStart;
        if (movement != null)
            movement.OnPathComplete -= OnMovementComplete;
    }

    void OnGuardTurnStart()
    {
        if (isTakingTurn || movement.IsMoving) return;
        if (patrolPoints == null || patrolPoints.Length == 0) return;

        isTakingTurn = true;
        MoveTowardsCurrentPatrolPoint();
    }

    void MoveTowardsCurrentPatrolPoint()
    {
        Transform target = patrolPoints[currentPatrolIndex];
        Debug.Log($"Guard ide prema: {target.name} na poziciji {target.position}");

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

        Transform target = patrolPoints[currentPatrolIndex];
        if (target != null)
        {
            float dist = Vector2.Distance(
                new Vector2(transform.position.x, transform.position.z),
                new Vector2(target.position.x, target.position.z)
            );

            if (dist <= 1.5f)
                AdvancePatrolIndex();
        }

        isTakingTurn = false;
    }

    void AdvancePatrolIndex()
    {
        if (patrolPoints.Length == 0) return;
        currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length;
    }

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
