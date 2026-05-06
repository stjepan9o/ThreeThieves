using UnityEngine;
using System.Collections.Generic;

public class GridPathfinder : MonoBehaviour
{
    public LayerMask wallLayer;

    public List<Vector3> FindPath(Vector3 start, Vector3 target)
    {
        start = Snap(start);
        target = Snap(target);

        List<Node> open = new List<Node>();
        HashSet<Vector3> closed = new HashSet<Vector3>();

        Node startNode = new Node(start, null, 0, Heuristic(start, target));
        open.Add(startNode);

        while (open.Count > 0)
        {
            Node current = open[0];

            foreach (Node n in open)
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

                float g = current.G + 1;

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
        return path;
    }

    bool IsWall(Vector3 pos)
    {
        return Physics.CheckSphere(pos, 0.4f, wallLayer);
    }

    float Heuristic(Vector3 a, Vector3 b)
    {
        return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.z - b.z);
    }

    Vector3 Snap(Vector3 pos)
    {
        return new Vector3(
            Mathf.Round(pos.x),
            1,
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