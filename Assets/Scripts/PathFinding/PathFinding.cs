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


    // =========================================================
    // A*
    // =========================================================

    public static List<Node> AStar(Node start, Node end) //encontrar el camino mas conveniente desde un nodo inicial a un nodo final
    {
        if (start == null || end == null) //comprueba que exista nodo inicial y final
        {
            Debug.LogWarning("A*: el nodo inicial o final es null.");
            return new List<Node>();
        }

        PriorityQueue<Node> frontier = new PriorityQueue<Node>(); //lista de nodos que todavia quedan por explorar, es una PriorityQueue porque se ordena por el costo conveniente a seguir buscando
        frontier.Enqueue(start, 0f);//metes el nodo inicial con prioridad 0 porque es el punto de partida

        Dictionary<Node, Node> cameFrom = new Dictionary<Node, Node>(); //guarda un diccionario de donde vino cada nodo, para luego reconstruir el camino final
        Dictionary<Node, float> costSoFar = new Dictionary<Node, float>(); //guarda costo acumulado para llegar a cada nodo, para comparar si encontramos un camino mas barato

        cameFrom[start] = null;//como el nodo inicial no viene de ningun lado, se asigna null
        costSoFar[start] = 0f;//el costo para llegar al nodo inicial es 0

        int visitedNodes = 0;//contador de nodos visitados para debug

        while (!frontier.IsEmpty) //mientras haya nodos por explorar, mientras frontier no este vacia el algoritmo sigue buscando
        {
            Node current = frontier.Dequeue(); //saca de la priority queue el nodo mas prometedor para seguir buscando
            visitedNodes++;

            if (current == end) //comprueba si llego al objetivo
            {
                Debug.Log(
                    "A* encontró el objetivo. Nodos visitados: " +
                    visitedNodes
                );

                break;
            }

            foreach (Node next in current.Neighbors) //si todavia no llego al objetivo revisa todos los vecinos del nodo actual
            {
                if (next == null || !next.IsWalkable) //ignorar nodos invalidos a los que no se puede caminar
                    continue;

                float newCost = //calcula cuanto costaria llegar al vecino next
                    costSoFar[current] +
                    next.Cost;

                bool hasNoCost = !costSoFar.ContainsKey(next); //ver si el vecino next no tiene un costo registrado, lo que significa que no se ha visitado antes
                bool foundCheaperPath = //ya conocia este nodo, pero ahora encontro una forma mas barata de llegar?
                    !hasNoCost &&
                    newCost < costSoFar[next];

                if (hasNoCost || foundCheaperPath) //si es la primera vez que ve el nodo, o encontro un camino mas barato actualiza los diccionarios
                {
                    costSoFar[next] = newCost;//guarda el nuevo costo acumulado para llegar a ese vecino
                    cameFrom[next] = current; //guarda que para llegar a next vino desde current

                    float priority = //elije el nodo usando priority queue combinado con una estimacion de distancia al objetivo para que sea mas eficiente
                        newCost +
                        Heuristic(next, end);

                    frontier.Enqueue(next, priority); //agrega al vecino a frontier, osea el que parezca mas conveniente
                }
            }
        }

        if (!cameFrom.ContainsKey(end)) //si despues de explorar todo no se encontro un camino al objetivo, devuelve una lista vacia
        {
            Debug.LogWarning("A* no encontró un camino.");
            return new List<Node>();
        }

        Debug.Log( //muestra de costo total del camino encontrado por A*
            "Costo total de A*: " +
            costSoFar[end]
        );

        return ReconstructPath(end, cameFrom); //arma la lista final
    }

    private static float Heuristic(Node current, Node end) //que tan lejos esta el nodo del objetivo usando Manhattan distance estimando distancia en un grid sin diagonales
    {
        return Mathf.Abs(current.X - end.X) +
               Mathf.Abs(current.Y - end.Y);
    }

    private static List<Node> ReconstructPath(Node end, Dictionary<Node, Node> cameFrom) //arma el camino final
    {
        Node current = end; //empieza desde el nodo final
        List<Node> path = new List<Node>();

        while (current != null) //mientras no llegue al nodo inicial, va agregando el nodo actual a la lista del camino y retrocediendo al nodo de donde vino
        {
            path.Add(current);
            current = cameFrom[current];
        }

        path.Reverse(); //como se agrego de final a inicio, se invierte la lista para que quede en orden correcto de inicio a final

        Debug.Log("Largo del camino: " + path.Count);

        return path; //devuelve la lista del camino final
    }



}
