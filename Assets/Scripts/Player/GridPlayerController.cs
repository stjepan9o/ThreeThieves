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

        if (TurnManager.Instance != null && !TurnManager.Instance.IsPlayerTurn) return;

        if (Input.GetMouseButtonDown(0) && !movement.IsMoving && !isOnCooldown)
            TryMove();

        if (apText != null && apManager != null)
            apText.text = "AP: " + apManager.CurrentAP;
    }

    void TryMove()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (!Physics.Raycast(ray, out RaycastHit hit))
            return;

        InteractableObject interactable = hit.collider.GetComponent<InteractableObject>();
        if (interactable == null)
            interactable = hit.collider.GetComponentInParent<InteractableObject>();

        if (interactable != null)
        {
            interactable.Interact();
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

        List<Vector3> path = Pathfinder.Instance.FindPath(transform.position, hit.point);

        if (path == null || path.Count == 0)
            return;

        int steps = Mathf.Min(path.Count, tilesRemainingThisTurn);
        movement.SetPath(path, steps);
        tilesRemainingThisTurn -= steps;
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
