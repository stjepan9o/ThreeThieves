using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;

/// <summary>
/// Kontrolira player input za kretanje i interakciju jednog lika-lopova.
/// Blokira input za vrijeme guard poteza (TurnManager.IsPlayerTurn = false).
/// </summary>
[RequireComponent(typeof(UnitGridMovement))]
public class GridPlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    public int maxTilesPerTurn = 5;
    public int moveCost = 2;

    [Header("References")]
    public ActionPointManager apManager;
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

        // Blokiraj input za vrijeme guard poteza
        if (TurnManager.Instance != null && !TurnManager.Instance.IsPlayerTurn) return;

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

        // INTERAKCIJA
        InteractableObject interactable = hit.collider.GetComponent<InteractableObject>();
        if (interactable == null)
            interactable = hit.collider.GetComponentInParent<InteractableObject>();

        if (interactable != null)
        {
            interactable.Interact();
            StartCoroutine(Cooldown());
            return;
        }

        // KRETANJE
        if (!canMove)
        {
            Debug.Log("Ovaj lik ne moze hodati po mapi!");
            return;
        }

        List<Vector3> path = Pathfinder.Instance.FindPath(transform.position, hit.point);

        if (path == null || path.Count == 0)
            return;

        movement.SetPath(path, maxTilesPerTurn);
        apManager.SpendAP(moveCost);
    }

    void HandlePathComplete()
    {
        StartCoroutine(Cooldown());
    }

    IEnumerator Cooldown()
    {
        isOnCooldown = true;
        yield return new WaitForSeconds(0.5f);
        isOnCooldown = false;
    }
}
