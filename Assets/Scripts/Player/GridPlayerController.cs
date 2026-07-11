using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;

[RequireComponent(typeof(UnitGridMovement))]
public class GridPlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    [Tooltip("Maksimalni broj tileova koji se mogu prijeći u jednom potezu.")]
    public int maxTilesPerTurn = 20;

    [Header("References")]
    public ActionPointManager apManager;
    public TMP_Text apText;

    [Header("Character Settings")]
    public bool canMove = true;

    private UnitGridMovement movement;
    private bool isOnCooldown = false;
    private int tilesRemainingThisTurn;

    private System.Action pendingInteraction;
    private Vector3 pendingTargetPos;
    private float pendingRange;

    void Awake()
    {
        movement = GetComponent<UnitGridMovement>();
    }

    void Start()
    {
        tilesRemainingThisTurn = maxTilesPerTurn;

        if (apManager == null)
            apManager = FindObjectOfType<ActionPointManager>();

        TurnManager.OnPlayerTurnStart += ResetTiles;
        movement.OnPathComplete += HandlePathComplete;
    }

    void OnDestroy()
    {
        TurnManager.OnPlayerTurnStart -= ResetTiles;
        if (movement != null)
            movement.OnPathComplete -= HandlePathComplete;
    }

    void ResetTiles()
    {
        tilesRemainingThisTurn = maxTilesPerTurn;
    }

    void Update()
    {
        if (!enabled) return;

        if (KeypadUI.IsKeypadOpen) return;
        if (WireTaskManager.IsMinigameOpen) return;

        if (TurnManager.Instance != null && !TurnManager.Instance.IsPlayerTurn) return;

        if (Input.GetMouseButtonDown(0) && !movement.IsMoving && !isOnCooldown)
            TryMove();


    }

    void TryMove()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (!Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore))
            return;

        InteractableObject interactable = hit.collider.GetComponent<InteractableObject>();
        if (interactable == null)
            interactable = hit.collider.GetComponentInParent<InteractableObject>();

        // --- PRIVREMENI DEBUG: makni nakon sto sredimo vrata ---
        string parentChain = hit.collider.name;
        Transform tp = hit.collider.transform.parent;
        while (tp != null) { parentChain += " <- " + tp.name; tp = tp.parent; }
        Debug.Log($"[CLICK] Pogodjeno: '{hit.collider.name}' (layer {LayerMask.LayerToName(hit.collider.gameObject.layer)}) | " +
                  $"InteractableObject u parentima: {(interactable == null ? "NEMA" : interactable.gameObject.name)} | " +
                  $"Lanac: {parentChain}");
        // --- KRAJ DEBUG ---

        if (interactable != null)
        {

            if ((interactable is Door || interactable is SciFiGate) &&
                CharacterSwitcher.Instance != null &&
                CharacterSwitcher.Instance.abilitiesHUD != null)
            {
                CharacterSwitcher.Instance.abilitiesHUD.FlashDoorAbilityIcon();
            }

            RequestInteraction(interactable.transform.position, interactable.interactionRange, interactable.Interact);
            StartCoroutine(Cooldown());
            return;
        }

        if (!canMove)
        {
            Debug.Log("Ovaj lik ne može hodati po mapi!");
            return;
        }

        if (tilesRemainingThisTurn <= 0)
        {
            Debug.Log("Nema više tileova za ovaj potez — pritisni E za skip.");
            return;
        }

        // Obican pokret po mapi ponistava eventualnu zakazanu interakciju.
        pendingInteraction = null;

        List<Vector3> path = Pathfinder.Instance.FindPath(transform.position, hit.point);

        if (path == null || path.Count == 0)
            return;

        int steps = Mathf.Min(path.Count, tilesRemainingThisTurn);
        movement.SetPath(path, steps);
        tilesRemainingThisTurn -= steps;
    }

    /// <summary>
    /// Zatrazi interakciju s objektom na zadanoj poziciji. Ako je aktivni lik vec
    /// dovoljno blizu - interakcija se izvrsi odmah. Inace lik automatski dohoda do
    /// najblizeg prohodnog polja uz objekt i interaktira cim stigne.
    /// </summary>
    public void RequestInteraction(Vector3 targetPosition, float range, System.Action interaction)
    {
        if (interaction == null) return;

        // Vec smo dovoljno blizu -> interaktiraj odmah.
        if (InteractionRange.IsActiveCharacterInRange(targetPosition, range))
        {
            interaction.Invoke();
            return;
        }

        if (!canMove)
        {
            Debug.Log("Ovaj lik ne moze prici objektu.");
            return;
        }

        List<Vector3> path = BuildPathToInteractable(targetPosition);
        if (path == null || path.Count == 0)
        {
            Debug.Log("Ne mogu pronaci put do objekta.");
            return;
        }

        pendingInteraction = interaction;
        pendingTargetPos = targetPosition;
        pendingRange = range;

        // Namjerno bez maxTilesPerTurn ogranicenja - zelimo da lik stvarno dodje do objekta.
        movement.SetPath(path);
    }

    private List<Vector3> BuildPathToInteractable(Vector3 targetPosition)
    {
        // Prvo probaj direktno - radi ako je polje objekta prohodno (npr. kartica na podu).
        List<Vector3> direct = Pathfinder.Instance.FindPath(transform.position, targetPosition);
        if (direct != null && direct.Count > 0)
            return direct;

        // Objekt je na neprohodnom polju (vrata, sef...). Prohodno polje koje je
        // geometrijski najblize NIJE nuzno i dohvatljivo (npr. polje s druge strane
        // zida/vrata). Zato probaj sva prohodna polja oko objekta, od najblizeg prema
        // daljem, i uzmi prvo do kojeg stvarno postoji put.
        if (GridManager.Instance == null)
            return null;

        List<Node> candidates = GridManager.Instance.GetWalkableNodesAround(targetPosition);
        foreach (Node n in candidates)
        {
            List<Vector3> path = Pathfinder.Instance.FindPath(transform.position, n.worldPosition);
            if (path != null && path.Count > 0)
                return path;
        }

        return null;
    }

    void HandlePathComplete()
    {
        StartCoroutine(Cooldown());

        if (pendingInteraction != null)
        {
            System.Action action = pendingInteraction;
            pendingInteraction = null;

            // Provjeri jesmo li stvarno stigli dovoljno blizu (put je mogao biti blokiran).
            if (InteractionRange.IsActiveCharacterInRange(pendingTargetPos, pendingRange))
                action.Invoke();
            else
                Debug.Log("Nisam uspio doci dovoljno blizu za interakciju.");
        }
    }

    IEnumerator Cooldown()
    {
        isOnCooldown = true;
        yield return new WaitForSeconds(0.5f);
        isOnCooldown = false;
    }
}
