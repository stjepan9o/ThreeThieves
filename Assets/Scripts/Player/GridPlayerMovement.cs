using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;

public class GridPlayerController : MonoBehaviour
{
    public float moveSpeed = 5f;

    public int maxTilesPerTurn = 5;

    public int maxAP = 5;
    public int currentAP;
    public int moveCost = 2;
    public LayerMask wallLayer; 
    public TMP_Text apText;

    private bool isMoving = false;
    private bool isOnCooldown = false;

    private List<Vector3> currentPath;
    private int pathIndex = 0;

    // pocetni broj ap
    void Start()
    {
        currentAP = maxAP;
    }

    // provjerava movement i input
    void Update()
    {
        if (isMoving)
        {
            MoveAlongPath();
        }

        if (Input.GetMouseButtonDown(0) && !isMoving && !isOnCooldown)
        {
            TryMove();
        }

        if (apText != null)
        {
            apText.text = "AP: " + currentAP;
        }
    }

    // rovjerava klik za movement/interakciju
    void TryMove()
    {
        if (currentAP < moveCost)
            return;

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (!Physics.Raycast(ray, out RaycastHit hit))
            return;

        // interaction
        InteractableObject interactable =
            hit.collider.GetComponentInParent<InteractableObject>();

        if (interactable != null)
        {
            interactable.Interact();

            currentAP -= moveCost;

            StartCoroutine(NextTurn());

            return;
        }

        // movement
        Vector3 target = Snap(hit.point);
        Vector3 start = Snap(transform.position);

        List<Vector3> path = FindSimplePath(start, target);

        if (path == null || path.Count == 0)
            return;

        if (path.Count > maxTilesPerTurn)
        {
            path = path.GetRange(0, maxTilesPerTurn);
        }

        currentPath = path;
        pathIndex = 0;
        isMoving = true;

        currentAP -= moveCost;
    }

    // movement igraca po pathu
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

    // delay 1s nakon turna
    IEnumerator NextTurn()
    {
        isOnCooldown = true;

        yield return new WaitForSeconds(1f);

        currentAP = maxAP;

        isOnCooldown = false;
    }

    // grid path
    List<Vector3> FindSimplePath(Vector3 start, Vector3 target)
{
    List<Vector3> path = new List<Vector3>();

    Vector3 current = start;

    int safety = 0;

    // X movement
    while (current.x != target.x)
    {
        safety++;

        if (safety > 100)
            return path;

        Vector3 next = current;
        next.x += Mathf.Sign(target.x - current.x);

        // wall detection
        if (Physics.CheckSphere(next, 0.2f, wallLayer))
            return path;

        current = next;

        path.Add(new Vector3(
            Mathf.Round(current.x),
            transform.position.y,
            Mathf.Round(current.z)
        ));
    }

    // Z movement
    while (current.z != target.z)
    {
        safety++;

        if (safety > 100)
            return path;

        Vector3 next = current;
        next.z += Mathf.Sign(target.z - current.z);

        // wall detection
        if (Physics.CheckSphere(next, 0.2f, wallLayer))
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

// world position u grid position
Vector3 Snap(Vector3 pos)
{
    return new Vector3(
        Mathf.Round(pos.x),
        transform.position.y,
        Mathf.Round(pos.z)
    );
}
}