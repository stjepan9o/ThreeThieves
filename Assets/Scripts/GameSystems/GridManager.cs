using UnityEngine;
using System.Collections.Generic;

public class Node
{
    public bool walkable;
    public Vector3 worldPosition;
    public int gridX;
    public int gridY;

    public int gCost;
    public int hCost;
    public Node parent;

    public Node(bool _walkable, Vector3 _worldPosition, int _gridX, int _gridY)
    {
        walkable = _walkable;
        worldPosition = _worldPosition;
        gridX = _gridX;
        gridY = _gridY;
    }

    public int fCost => gCost + hCost;
}

public class GridManager : MonoBehaviour
{
    public static GridManager Instance { get; private set; }

    [Header("Grid Settings")]
    public LayerMask unwalkableMask;
    public Vector2 gridWorldSize = new Vector2(50, 50);
    public float nodeRadius = 0.5f;
    public bool allowDiagonalMovement = false;

    [Header("Debug")]
    public bool showGizmos = true;

    private Node[,] grid;
    private float nodeDiameter;
    private int gridSizeX, gridSizeY;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        nodeDiameter = nodeRadius * 2f;
        gridSizeX = Mathf.RoundToInt(gridWorldSize.x / nodeDiameter);
        gridSizeY = Mathf.RoundToInt(gridWorldSize.y / nodeDiameter);

        CreateGrid();
    }

    void CreateGrid()
    {
        grid = new Node[gridSizeX, gridSizeY];

        Vector3 worldBottomLeft = transform.position
            - Vector3.right * (gridWorldSize.x / 2f)
            - Vector3.forward * (gridWorldSize.y / 2f);

        for (int x = 0; x < gridSizeX; x++)
        {
            for (int y = 0; y < gridSizeY; y++)
            {
                Vector3 worldPoint = worldBottomLeft
                    + Vector3.right * (x * nodeDiameter + nodeRadius)
                    + Vector3.forward * (y * nodeDiameter + nodeRadius);

                bool walkable = !Physics.CheckSphere(worldPoint, nodeRadius, unwalkableMask, QueryTriggerInteraction.Ignore);
                grid[x, y] = new Node(walkable, worldPoint, x, y);
            }
        }
    }

    public Node NodeFromWorldPoint(Vector3 worldPosition)
    {
        float percentX = (worldPosition.x - transform.position.x + gridWorldSize.x / 2f) / gridWorldSize.x;
        float percentY = (worldPosition.z - transform.position.z + gridWorldSize.y / 2f) / gridWorldSize.y;

        percentX = Mathf.Clamp01(percentX);
        percentY = Mathf.Clamp01(percentY);

        int x = Mathf.Clamp(Mathf.RoundToInt((gridSizeX - 1) * percentX), 0, gridSizeX - 1);
        int y = Mathf.Clamp(Mathf.RoundToInt((gridSizeY - 1) * percentY), 0, gridSizeY - 1);

        return grid[x, y];
    }

    public List<Node> GetNeighbours(Node node)
    {
        List<Node> neighbours = new List<Node>();

        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                if (x == 0 && y == 0)
                    continue;

                bool isDiagonal = (x != 0 && y != 0);
                if (isDiagonal && !allowDiagonalMovement)
                    continue;

                int checkX = node.gridX + x;
                int checkY = node.gridY + y;

                if (checkX < 0 || checkX >= gridSizeX || checkY < 0 || checkY >= gridSizeY)
                    continue;

                if (isDiagonal)
                {
                    bool sideAClear = grid[node.gridX + x, node.gridY].walkable;
                    bool sideBClear = grid[node.gridX, node.gridY + y].walkable;
                    if (!sideAClear || !sideBClear)
                        continue;
                }

                neighbours.Add(grid[checkX, checkY]);
            }
        }

        return neighbours;
    }

    /// <summary>
    /// Vrati prohodna (walkable) polja oko zadane svjetske pozicije, poredana od
    /// najblizeg prstena prema van (dakle od najblizeg objektu prema daljem).
    /// Korisno kad je sam objekt (vrata, sef, kartica) na neprohodnom polju, pa
    /// lik mora stati na susjedno polje da bi interaktirao s njim. Pozivatelj
    /// zatim moze provjeriti do kojeg od tih polja stvarno postoji put.
    /// </summary>
    public List<Node> GetWalkableNodesAround(Vector3 worldPosition, int maxRingSearch = 8)
    {
        List<Node> result = new List<Node>();
        Node origin = NodeFromWorldPoint(worldPosition);

        if (origin.walkable)
            result.Add(origin);

        for (int r = 1; r <= maxRingSearch; r++)
        {
            for (int x = -r; x <= r; x++)
            {
                for (int y = -r; y <= r; y++)
                {
                    // Samo rub prstena na udaljenosti r (unutrasnji prsteni su vec dodani).
                    if (Mathf.Max(Mathf.Abs(x), Mathf.Abs(y)) != r)
                        continue;

                    int cx = origin.gridX + x;
                    int cy = origin.gridY + y;
                    if (cx < 0 || cx >= gridSizeX || cy < 0 || cy >= gridSizeY)
                        continue;

                    Node n = grid[cx, cy];
                    if (n.walkable)
                        result.Add(n);
                }
            }
        }

        return result;
    }

    public int MaxSize => gridSizeX * gridSizeY;

    public void RebuildGrid()
    {
        Physics.SyncTransforms(); 
        Debug.Log("Grid se rebuilduje!");
        CreateGrid();
    }
    void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(transform.position, new Vector3(gridWorldSize.x, 0.1f, gridWorldSize.y));

        if (grid == null || !showGizmos)
            return;

        foreach (Node n in grid)
        {
            Gizmos.color = n.walkable ? new Color(1f, 1f, 1f, 0.2f) : new Color(1f, 0f, 0f, 0.4f);
            Gizmos.DrawCube(n.worldPosition, Vector3.one * (nodeDiameter - 0.05f));
        }
    }
}
