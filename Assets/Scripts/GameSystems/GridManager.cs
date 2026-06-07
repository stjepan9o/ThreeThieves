using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Jedan cvor (tile) grida. Cuva walkable status, world poziciju te A* cost vrijednosti.
/// Reuse-a se izmedju razlicitih poziva FindPath (i za playera i za guardove).
/// </summary>
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

/// <summary>
/// Generira i drzi grid podataka za cijelu mapu (jednom, na pocetku).
/// Pathfinder ovaj grid koristi za A*, a sve jedinice (player + guardovi) dijele isti sustav -
/// nema vise rucnog Physics.CheckSphere po koraku kretanja.
///
/// Postavljanje u Unity:
/// 1) Stavi prazan GameObject "GridManager" na sredinu mape.
/// 2) Postavi Grid World Size tako da pokrije cijelu mapu (X = sirina, Y = "dubina"/Z os).
/// 3) Node Radius = pola velicine jedne tile-a (npr. 0.5 za tile od 1x1).
/// 4) Unwalkable Mask = layer na kojem su zidovi/prepreke (isti layer kao stari wallLayer).
/// </summary>
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

        // Donji-lijevi kut grida - grid se siri simetricno oko transform.position GridManagera
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

                bool walkable = !Physics.CheckSphere(worldPoint, nodeRadius * 0.9f, unwalkableMask);
                grid[x, y] = new Node(walkable, worldPoint, x, y);
            }
        }
    }

    /// <summary>Pretvara world poziciju (npr. raycast hit.point ili transform.position) u Node na gridu.</summary>
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

    /// <summary>Vraca susjedne cvorove (4 ili 8 smjerova, ovisno o allowDiagonalMovement).</summary>
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
                    // Sprijeci "rezanje uglova" - dijagonala je dozvoljena samo ako su obje susjedne strane prohodne
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

    public int MaxSize => gridSizeX * gridSizeY;

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
