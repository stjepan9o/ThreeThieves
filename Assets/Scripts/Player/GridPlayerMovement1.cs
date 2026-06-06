using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;

public class GridPlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public int maxTilesPerTurn = 5;
    public int moveCost = 2;
    public LayerMask wallLayer;

    [Header("References")]
    public ActionPointManager apManager; // Dijeljeni AP za cijeli tim
    public TMP_Text apText;

    [Header("Character Settings")]
    public bool canMove = true; // Hacker ima false, ostali true

    private bool isMoving = false;
    private bool isOnCooldown = false;
    private List<Vector3> currentPath;
    private int pathIndex = 0;

    void Start()
    {
        // Auto-pronadji ActionPointManager ako nije assignan
        if (apManager == null)
            apManager = FindObjectOfType<ActionPointManager>();
    }

    void Update()
    {
        // Ako ovaj lik nije aktivan, ne slusaj input
        if (!enabled) return;

        if (isMoving)
            MoveAlongPath();

        if (Input.GetMouseButtonDown(0) && !isMoving && !isOnCooldown)
            TryMove();

        // Prikazi AP
        if (apText != null && apManager != null)
            apText.text = "AP: " + apManager.CurrentAP;
    }

    void TryMove()
    {
        // Provjeri ima li dovoljno AP
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
            // InteractableObject koristi svoj apCost
            interactable.Interact();
            StartCoroutine(NextTurn());
            return;
        }

        // KRETANJE - klik na pod
        if (!canMove)
        {
            Debug.Log("Ovaj lik ne moze hodati po mapi!");
            return;
        }

        Vector3 target = Snap(hit.point);
        Vector3 start = Snap(transform.position);

        List<Vector3> path = FindSimplePath(start, target);

        if (path == null || path.Count == 0)
            return;

        if (path.Count > maxTilesPerTurn)
            path = path.GetRange(0, maxTilesPerTurn);

        currentPath = path;
        pathIndex = 0;
        isMoving = true;

        apManager.SpendAP(moveCost);
    }

    void MoveAlongPath()
    {
        if (pathIndex >= currentPath.Count)
        {
            isMoving = false;
            StartCoroutine(NextTurn());
            return;
        }

        Vector3 target = currentPath[pathIndex];

        transform.position = Vector3.MoveTowards(
            transform.position,
            target,
            moveSpeed * Time.deltaTime
        );

        if (Vector3.Distance(transform.position, target) < 0.05f)
        {
            transform.position = target;
            pathIndex++;
        }
    }

    IEnumerator NextTurn()
    {
        isOnCooldown = true;
        yield return new WaitForSeconds(1f);
        isOnCooldown = false;
    }

    List<Vector3> FindSimplePath(Vector3 start, Vector3 target)
    {
        List<Vector3> path = new List<Vector3>();
        Vector3 current = start;
        int safety = 0;

        // Kretanje po X osi
        while (current.x != target.x)
        {
            safety++;
            if (safety > 100) return path;

            Vector3 next = current;
            next.x += Mathf.Sign(target.x - current.x);

           if (Physics.CheckSphere(next, 0.6f, wallLayer))
                return path;

            current = next;
            path.Add(new Vector3(
                Mathf.Round(current.x),
                transform.position.y,
                Mathf.Round(current.z)
            ));
        }

        // Kretanje po Z osi
        while (current.z != target.z)
        {
            safety++;
            if (safety > 100) return path;

            Vector3 next = current;
            next.z += Mathf.Sign(target.z - current.z);

            if (Physics.CheckSphere(next, 0.6f, wallLayer))
                return path;

            current = next;
            path.Add(new Vector3(
                Mathf.Round(current.x),
                transform.position.y,
                Mathf.Round(current.z)
            ));
        }

        return path;
    }

    Vector3 Snap(Vector3 pos)
    {
        return new Vector3(
            Mathf.Round(pos.x),
            transform.position.y,
            Mathf.Round(pos.z)
        );
    }
}
