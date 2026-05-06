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

        while (frontier.Count > 0)
        {
            Node current = frontier.Dequeue();
            foreach (var next in current.Neighbors)
            {
                if (cameFrom.ContainsKey(next)) continue;
                frontier.Enqueue(next);
                cameFrom[next] = current;
            }
        }

        Node newCurrent = end;
        var path = new List<Node>();
        while (newCurrent != null)
        {
            path.Add(newCurrent);
            newCurrent = cameFrom[newCurrent];
        }
        path.Reverse();
        return path;
    }
}
