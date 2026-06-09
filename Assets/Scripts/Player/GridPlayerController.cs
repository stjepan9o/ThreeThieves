using UnityEngine;
using System.Collections;
using TMPro;
using System.Collections.Generic;

/// <summary>
/// Refaktorirana verzija tvog GridPlayerControllera. Razlike u odnosu na original:
///
/// 1) Pathfinding vise NIJE rucni "prvo X pa Z" hod uz Physics.CheckSphere po koraku -
///    sada se trazi pravi A* put preko Pathfinder.Instance.FindPath(). To znaci da igrac
///    moze obici prepreke, a ne samo da se kretanje "zaglavi" cim naleti na zid.
/// 2) Samo izvrsavanje kretanja je izdvojeno u UnitGridMovement - identicna komponenta
///    koju ce koristiti i guardovi. Ovaj skript sad SAMO odlucuje KAMO ici (na temelju
///    mis klika) i koliko AP to kosta - ne zna nista o tome KAKO se hoda.
/// 3) "NextTurn" je preimenovan u "Cooldown" da se ne pomijesa sa stvarnim turn-based
///    sustavom (player turn / AI turn) koji ce vjerojatno biti zaseban manager.
///
/// Setup u Unity:
/// - Stavi UnitGridMovement komponentu na isti GameObject (RequireComponent to osigurava).
/// - GridManager i Pathfinder moraju postojati negdje u sceni (singletoni).
/// - moveSpeed/wallLayer su se preselili - moveSpeed je sad na UnitGridMovement,
///   wallLayer (sada "unwalkableMask") je na GridManageru.
/// </summary>
[RequireComponent(typeof(UnitGridMovement))]
public class GridPlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    public int maxTilesPerTurn = 5;
    public int moveCost = 2;

    [Header("References")]
    public ActionPointManager apManager; // Dijeljeni AP za cijeli tim
    public TMP_Text apText;

    [Header("Character Settings")]
    public bool canMove = true; // Hacker ima false, ostali true

    private UnitGridMovement movement;
    private bool isOnCooldown = false;

    void Awake()
    {
        movement = GetComponent<UnitGridMovement>();
    }

    void Start()
    {
        if (apManager == null)
            apManager = FindObjectOfType<ActionPointManager>();

        movement.OnPathComplete += HandlePathComplete;
    }

    void OnDestroy()
    {
        if (movement != null)
            movement.OnPathComplete -= HandlePathComplete;
    }

    void Update()
    {
        if (!enabled) return;

        if (Input.GetMouseButtonDown(0) && !movement.IsMoving && !isOnCooldown)
            TryMove();

        if (apText != null && apManager != null)
            apText.text = "AP: " + apManager.CurrentAP;
    }

    void TryMove()
    {
        if (!apManager.HasEnoughAP(moveCost))
        {
            Debug.Log("Nema dovoljno AP za kretanje!");
            return;
        }

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (!Physics.Raycast(ray, out RaycastHit hit))
            return;

        // INTERAKCIJA - klik na InteractableObject
        InteractableObject interactable = hit.collider.GetComponent<InteractableObject>();
        if (interactable == null)
            interactable = hit.collider.GetComponentInParent<InteractableObject>();

        if (interactable != null)
        {
            interactable.Interact(); // InteractableObject sam trosi svoj apCost
            StartCoroutine(Cooldown());
            return;
        }

        // KRETANJE - klik na pod
        if (!canMove)
        {
            Debug.Log("Ovaj lik ne moze hodati po mapi!");
            return;
        }

        List<Vector3> path = Pathfinder.Instance.FindPath(transform.position, hit.point);

        if (path == null || path.Count == 0)
            return; // cilj nedostupan ili nema puta - ne trosimo AP

        movement.SetPath(path, maxTilesPerTurn);
        apManager.SpendAP(moveCost);
    }

    private void HandlePathComplete()
    {
        StartCoroutine(Cooldown());
    }

    private IEnumerator Cooldown()
    {
        isOnCooldown = true;
        yield return new WaitForSeconds(1f);
        isOnCooldown = false;
    }
}
