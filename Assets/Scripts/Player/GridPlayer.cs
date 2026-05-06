using UnityEngine;
using System.Collections.Generic;

public class GridPlayer : MonoBehaviour
{
    public float speed = 5f;
    public GridPathfinder pathfinder;
    public GameObject tilePrefab;

    private List<Vector3> currentPath;
    private int pathIndex = 0;
    private List<GameObject> previewTiles = new List<GameObject>();

    void Update()
    {
        ShowPreview();
        HandleClick();
        Move();
    }

    void ShowPreview()
    {
        ClearPreview();

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (!Physics.Raycast(ray, out RaycastHit hit)) return;

        List<Vector3> path = pathfinder.FindPath(transform.position, hit.point);
        if (path == null) return;

        foreach (var p in path)
        {
            GameObject tile = Instantiate(tilePrefab, p, Quaternion.identity);
            previewTiles.Add(tile);
        }
    }

    void HandleClick()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (!Physics.Raycast(ray, out RaycastHit hit)) return;

            currentPath = pathfinder.FindPath(transform.position, hit.point);
            pathIndex = 0;
        }
    }

    void Move()
    {
        if (currentPath == null || pathIndex >= currentPath.Count) return;

        Vector3 target = currentPath[pathIndex];

        transform.position = Vector3.MoveTowards(transform.position, target, speed * Time.deltaTime);

        if (Vector3.Distance(transform.position, target) < 0.05f)
        {
            transform.position = target;
            pathIndex++;
        }
    }

    void ClearPreview()
    {
        foreach (var t in previewTiles)
            Destroy(t);

        previewTiles.Clear();
    }
}