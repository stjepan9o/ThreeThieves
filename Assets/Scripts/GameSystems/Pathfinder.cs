using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// A* pathfinding nad gridom iz GridManagera. Jedan zajednicki sustav koji koriste
/// i player i guardovi - svatko samo zove FindPath(start, target) i dobije listu
/// world-space waypointova koje UnitGridMovement onda prolazi.
///
/// Postavljanje u Unity: stavi ovu skriptu na isti GameObject kao GridManager (ili bilo koji
/// drugi u sceni) - postoji samo jedna instanca (singleton), kao i GridManager.
/// </summary>
public class Pathfinder : MonoBehaviour
{
    public static Pathfinder Instance { get; private set; }

    private GridManager gridManager;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void Start()
    {
        gridManager = GridManager.Instance;
    }

    /// <summary>
    /// Vraca listu world-pozicija (waypointova) od startPos do targetPos.
    /// Ako put ne postoji (cilj nedostupan/zid), vraca praznu listu - provjeri path.Count == 0.
    /// </summary>
    public List<Vector3> FindPath(Vector3 startPos, Vector3 targetPos)
    {
        if (gridManager == null)
            gridManager = GridManager.Instance;

        Node startNode = gridManager.NodeFromWorldPoint(startPos);
        Node targetNode = gridManager.NodeFromWorldPoint(targetPos);

        Debug.Log($"Start walkable: {startNode.walkable}, Target walkable: {targetNode.walkable}");

        if (!targetNode.walkable)
            return new List<Vector3>();

        // Bitno: Node objekti se reuse-aju izmedju poziva (grid se gradi samo jednom).
        // startNode mora poceti sa cistim costovima, inace ce ostati "memorija" iz proslog FindPath poziva.
        startNode.gCost = 0;
        startNode.hCost = GetDistance(startNode, targetNode);
        startNode.parent = null;

        List<Node> openSet = new List<Node>();
        HashSet<Node> closedSet = new HashSet<Node>();
        openSet.Add(startNode);

        while (openSet.Count > 0)
        {
            // Uzmi cvor s najmanjim fCost (kod jednakosti, manji hCost = blizi cilju)
            Node currentNode = openSet[0];
            for (int i = 1; i < openSet.Count; i++)
            {
                if (openSet[i].fCost < currentNode.fCost ||
                   (openSet[i].fCost == currentNode.fCost && openSet[i].hCost < currentNode.hCost))
                {
                    currentNode = openSet[i];
                }
            }

            openSet.Remove(currentNode);
            closedSet.Add(currentNode);

            if (currentNode == targetNode)
                return RetracePath(startNode, targetNode);

            foreach (Node neighbour in gridManager.GetNeighbours(currentNode))
            {
                if (!neighbour.walkable || closedSet.Contains(neighbour))
                    continue;

                int newCostToNeighbour = currentNode.gCost + GetDistance(currentNode, neighbour);
                bool inOpenSet = openSet.Contains(neighbour);

                if (newCostToNeighbour < neighbour.gCost || !inOpenSet)
                {
                    neighbour.gCost = newCostToNeighbour;
                    neighbour.hCost = GetDistance(neighbour, targetNode);
                    neighbour.parent = currentNode;

                    if (!inOpenSet)
                        openSet.Add(neighbour);
                }
            }
        }

        // openSet prazan, a cilj nije pronadjen -> nema puta
        return new List<Vector3>();
    }

    private List<Vector3> RetracePath(Node startNode, Node endNode)
    {
        List<Vector3> path = new List<Vector3>();
        Node currentNode = endNode;

        while (currentNode != startNode)
        {
            path.Add(currentNode.worldPosition);
            currentNode = currentNode.parent;
        }

        path.Reverse();
        return path;
    }

    /// <summary>
    /// Standardni A* "10/14" trik - cijeli brojevi umjesto floatova (10 = ravno, 14 = priblizno sqrt(2)*10 za dijagonalu).
    /// Brze je i izbjegava floating point usporedbe.
    /// </summary>
    private int GetDistance(Node a, Node b)
    {
        int dstX = Mathf.Abs(a.gridX - b.gridX);
        int dstY = Mathf.Abs(a.gridY - b.gridY);

        if (dstX > dstY)
            return 14 * dstY + 10 * (dstX - dstY);
        return 14 * dstX + 10 * (dstY - dstX);
    }
}
