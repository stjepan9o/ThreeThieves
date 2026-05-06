using UnityEngine;
using System.Collections.Generic;

public class GridMovement : MonoBehaviour
{
    public float moveSpeed = 5f;

    private Queue<Vector3> path = new Queue<Vector3>();
    private bool isMoving = false;

    void Update()
    {
        HandleClick();
        MoveAlongPath();
    }

    void HandleClick()
    {
        if (Input.GetMouseButtonDown(0) && !isMoving)
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                Vector3 target = hit.point;

                target.x = Mathf.Round(target.x);
                target.z = Mathf.Round(target.z);
                target.y = transform.position.y;

                GeneratePath(target);
            }
        }
    }

    void GeneratePath(Vector3 target)
    {
        path.Clear();

        Vector3 current = new Vector3(
            Mathf.Round(transform.position.x),
            transform.position.y,
            Mathf.Round(transform.position.z)
        );

        // jednostavan path (bez obstacle)
        while (current != target)
        {
            if (Mathf.Abs(target.x - current.x) > 0)
            {
                current.x += Mathf.Sign(target.x - current.x);
            }
            else if (Mathf.Abs(target.z - current.z) > 0)
            {
                current.z += Mathf.Sign(target.z - current.z);
            }

            path.Enqueue(new Vector3(current.x, current.y, current.z));
        }

        isMoving = true;
    }

    void MoveAlongPath()
    {
        if (!isMoving || path.Count == 0) return;

        Vector3 nextTile = path.Peek();

        transform.position = Vector3.MoveTowards(
            transform.position,
            nextTile,
            moveSpeed * Time.deltaTime
        );

        if (Vector3.Distance(transform.position, nextTile) < 0.05f)
        {
            transform.position = nextTile;
            path.Dequeue();
        }

        if (path.Count == 0)
        {
            isMoving = false;
        }
    }
}