using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;

public class GridPlayerController : MonoBehaviour
{
    public float moveSpeed = 5f;
    public LayerMask wallLayer;
    public int maxTilesPerTurn = 5;
    public int maxAP = 5;
    public int currentAP;
    public TMP_Text apText;
    public int moveCost = 2;
    private bool isOnCooldown = false;
    private bool isMoving = false;
    private List<Vector3> currentPath;
    private int pathIndex = 0;

    void Start()
    {
        currentAP = maxAP;

        if (currentAP < moveCost || isOnCooldown) return;

        currentAP -= moveCost;
    }

    void Update()
    {
        if (isMoving) MoveAlongPath();

        if (Input.GetMouseButtonDown(0) && !isMoving && !isOnCooldown)
        {
            TryMove();
        }
        apText.text = "AP: " + currentAP;
    }

    void TryMove()
    {
        if (currentAP < moveCost) return;

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (!Physics.Raycast(ray, out RaycastHit hit)) return;

        Vector3 target = Snap(hit.point);
        Vector3 start = Snap(transform.position);

        List<Vector3> path = FindPath(start, target);

        if (path == null || path.Count == 0) return;

        // ograniči na max tileova po turnu
        if (path.Count > maxTilesPerTurn)
            path = path.GetRange(0, maxTilesPerTurn);

        currentPath = path;
        pathIndex = 0;
        isMoving = true;

        currentAP -= moveCost;
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

        transform.position = Vector3.MoveTowards(transform.position, target, moveSpeed * Time.deltaTime);

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

        currentAP = maxAP;
        isOnCooldown = false;
    }

    float GetWallPenalty(Vector3 pos)
    {
        float penalty = 0f;

        float checkDistance = 1f; // koliko daleko gledamo

        // provjeri 4 smjera
        if (Physics.CheckSphere(pos + Vector3.forward, 0.4f, wallLayer)) penalty += 3;
        if (Physics.CheckSphere(pos + Vector3.back, 0.4f, wallLayer)) penalty += 3;
        if (Physics.CheckSphere(pos + Vector3.left, 0.4f, wallLayer)) penalty += 3;
        if (Physics.CheckSphere(pos + Vector3.right, 0.4f, wallLayer)) penalty += 3;

    return penalty;
    }
    // ---------- A* PATHFINDING ----------

    List<Vector3> FindPath(Vector3 start, Vector3 target)
    {
        List<Node> open = new List<Node>();
        HashSet<Vector3> closed = new HashSet<Vector3>();

        Node startNode = new Node(start, null, 0, Heuristic(start, target));
        open.Add(startNode);

        while (open.Count > 0)
        {
            Node current = open[0];

            foreach (var n in open)
                if (n.F < current.F)
                    current = n;

            open.Remove(current);
            closed.Add(current.pos);

            if (current.pos == target)
                return BuildPath(current);

            foreach (Vector3 dir in Directions())
            {
                Vector3 next = current.pos + dir;

                if (closed.Contains(next)) continue;
                if (IsWall(next)) continue;

                float g = current.G + 1 + GetWallPenalty(next);

                Node existing = open.Find(n => n.pos == next);

                if (existing == null)
                {
                    open.Add(new Node(next, current, g, Heuristic(next, target)));
                }
                else if (g < existing.G)
                {
                    existing.G = g;
                    existing.parent = current;
                }
            }
        }

        return null;
    }

    List<Vector3> BuildPath(Node node)
    {
        List<Vector3> path = new List<Vector3>();

        while (node != null)
        {
            path.Add(node.pos);
            node = node.parent;
        }

        path.Reverse();
        path.RemoveAt(0); // makni start tile
        return path;
    }

    float Heuristic(Vector3 a, Vector3 b)
    {
        return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.z - b.z);
    }

    bool IsWall(Vector3 pos)
    {
        return Physics.CheckSphere(pos, 0.4f, wallLayer);
    }

    Vector3 Snap(Vector3 pos)
    {
        return new Vector3(
            Mathf.Round(pos.x),
            transform.position.y,
            Mathf.Round(pos.z)
        );
    }

    List<Vector3> Directions()
    {
        return new List<Vector3>
        {
            Vector3.forward,
            Vector3.back,
            Vector3.left,
            Vector3.right
        };
    }

    class Node
    {
        public Vector3 pos;
        public Node parent;
        public float G;
        public float H;
        public float F => G + H;

        public Node(Vector3 p, Node par, float g, float h)
        {
            pos = p;
            parent = par;
            G = g;
            H = h;
        }
    }
}
