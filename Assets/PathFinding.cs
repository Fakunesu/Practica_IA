using UnityEngine;
using System.Collections.Generic;

public class PathFinding
{
    public static List<Node> BFS(Node start, Node end)
    {
        var frontier = new Queue<Node>();
        frontier.Enqueue(start);

        var cameFrom = new Dictionary<Node, Node>();
        cameFrom[start] = null;

        int visitedNodes = 0;

        while (frontier.Count > 0)
        {
            Node current = frontier.Dequeue();
            visitedNodes++;

            current.SetColor(Color.cyan);

            if (current == end)
            {
                Debug.Log("BFS encontró el objetivo. Nodos visitados: " + visitedNodes);
                break;
            }

            foreach (var next in current.Neighbors)
            {
                if (cameFrom.ContainsKey(next)) continue;

                frontier.Enqueue(next);
                cameFrom[next] = current;
            }
        }

        if (!cameFrom.ContainsKey(end))
        {
            Debug.LogWarning("BFS no encontró camino.");
            return new List<Node>();
        }

        return ReconstructPath(end, cameFrom);
    }

    public static List<Node> Dijkstra(Node start, Node end)
    {
        PriorityQueue<Node> frontier = new PriorityQueue<Node>();

        frontier.Enqueue(start, 0);

        Dictionary<Node, Node> cameFrom = new Dictionary<Node, Node>();
        Dictionary<Node, float> costSoFar = new Dictionary<Node, float>();

        cameFrom[start] = null;
        costSoFar[start] = 0;

        int visitedNodes = 0;

        while (!frontier.IsEmpty)
        {
            Node current = frontier.Dequeue();
            visitedNodes++;

            current.SetColor(Color.blue);

            if (current == end)
            {
                Debug.Log("Dijkstra encontró el objetivo. Nodos visitados: " + visitedNodes);
                break;
            }

            foreach (Node next in current.Neighbors)
            {
                float newCost = costSoFar[current] + next.Cost;

                if (!costSoFar.ContainsKey(next) || newCost < costSoFar[next])
                {
                    costSoFar[next] = newCost;
                    cameFrom[next] = current;

                    frontier.Enqueue(next, newCost);
                }
            }
        }

        if (!cameFrom.ContainsKey(end))
        {
            Debug.LogWarning("Dijkstra no encontró camino.");
            return new List<Node>();
        }

        Debug.Log("Costo total del camino: " + costSoFar[end]);

        return ReconstructPath(end, cameFrom);
    }

    private static List<Node> ReconstructPath(Node end, Dictionary<Node, Node> cameFrom)
    {
        Node current = end;
        List<Node> path = new List<Node>();

        while (current != null)
        {
            path.Add(current);
            current = cameFrom[current];
        }

        path.Reverse();

        Debug.Log("Largo del camino: " + path.Count);

        return path;
    }

}
