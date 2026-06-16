using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

[ExecuteInEditMode]
public class GridGenerator : MonoBehaviour
{
    [Header("Grid")]
    [SerializeField] private Node prefab; //prefab del nodo que se instanciará para crear la cuadrícula, debe ser asignado en el inspector
    [SerializeField] private Node[] nodeGrid; //arreglo que almacenará los nodos generados, se llena automáticamente al crear la cuadrícula
    [SerializeField] private int width = 10; //ancho de la cuadrícula, determina cuántos nodos se crearán horizontalmente
    [SerializeField] private int height = 10; //alto de la cuadrícula, determina cuántos nodos se crearán verticalmente
    [SerializeField] private float distance = 1f; //distancia entre los nodos, determina el espacio entre cada nodo en la cuadrícula, afectando la escala del mundo y la precisión del pathfinding

    [Header("Cost")]
    [SerializeField] private bool useCosts; //indica si se deben usar costos personalizados para los nodos

    [Header("Obstacles")]
    [SerializeField] private LayerMask obstacleMask; //máscara de capa que define qué objetos se consideran obstáculos al generar la cuadrícula

    [Tooltip("Mitad del tamaño de la caja que revisa obstáculos.")]
    [SerializeField]
    private Vector3 nodeCheckSize = //tamaño de la caja utilizada para verificar si un nodo está bloqueado por un obstáculo
        new Vector3(0.4f, 1f, 0.4f);

    public Node[] NodeGrid => nodeGrid;

    [ContextMenu("SetNodeGrid")]
    public void SetNodeGrid() //método que genera la cuadrícula de nodos, se puede llamar desde el inspector para crear o actualizar la cuadrícula
    {
        ClearGrid(); //limpia cualquier nodo existente antes de generar la nueva cuadrícula

        if (prefab == null) //verifica que el prefab del nodo esté asignado antes de intentar generar la cuadrícula, si no lo está, muestra un error y detiene el proceso
        {
            Debug.LogError("GridGenerator: falta asignar el prefab de Node.");
            return;
        }

        if (width <= 0 || height <= 0) //verifica que el ancho y alto de la cuadrícula sean mayores a 0 antes de generar la cuadrícula, si no lo son, muestra un error y detiene el proceso
        {
            Debug.LogError("GridGenerator: width y height deben ser mayores a 0.");
            return;
        }

        nodeGrid = new Node[width * height];

        CreateNodes();
        SetNodeNeighbors();
    }

    private void CreateNodes() //método que crea los nodos de la cuadrícula
    {
        for (int h = 0; h < height; h++) //recorre toda la grillla en ambos ejes (horizontal y vertical) 
        {
            for (int w = 0; w < width; w++)
            {
                Vector3 nodePosition = //calcula la posicion del nodo actual
                    transform.position +
                    new Vector3(w * distance, 0f, h * distance);

                Node newNode = Instantiate( //instancia un nuevo nodo
                    prefab,
                    nodePosition,
                    transform.rotation,
                    transform
                );

                newNode.name = $"Node {w}, {h}"; //asigna un nombre al nodo para facilitar su identificación en la jerarquía de objetos
                newNode.SetIndexes(w, h);

                bool isBlocked = Physics.CheckBox( //verifica si el nodo está bloqueado por un obstáculo utilizando una caja de colisión
                    nodePosition + Vector3.up * nodeCheckSize.y,
                    nodeCheckSize,
                    transform.rotation,
                    obstacleMask,
                    QueryTriggerInteraction.Ignore
                );

                newNode.SetWalkable(!isBlocked); //establece si el nodo es transitable o no según si está bloqueado por un obstáculo

                if (isBlocked) //si el nodo está bloqueado, se le asigna un costo muy alto para que el pathfinding lo evite
                {
                    newNode.SetCost(float.MaxValue);
                }
                else if (useCosts)
                {
                    SetCostForNode(newNode, w, h); //si se están usando costos personalizados, se llama a un método para establecer el costo del nodo
                }
                else
                {
                    newNode.SetCost(1f); //si no se están usando costos personalizados, se asigna un costo estándar de 1 para los nodos transitable
                }

                nodeGrid[w + h * width] = newNode; //guarda nodo en el array
            }
        }
    }

    private void SetNodeNeighbors() //conecta los nodos entre si para saber a que nodos puede moverse
    {
        for (int h = 0; h < height; h++)
        {
            for (int w = 0; w < width; w++)
            {
                Node currentNode = nodeGrid[w + h * width]; //obtiene el nodo actual 

                List<Node> neighbors = new List<Node>(); //crea una lista vacia de vecinos

                if (currentNode == null || !currentNode.IsWalkable) //Si el nodo no existe o no es caminable, le asigna una lista vacía y pasa al siguiente.
                {
                    currentNode?.SetNeighbors(neighbors);
                    continue;
                }

                if (w > 0) //Si w > 0, significa que no estamos en el borde izquierdo entonces intenta agregar el nodo de la izquierda.
                {
                    TryAddNeighbor(
                        neighbors,
                        nodeGrid[w - 1 + h * width]
                    );
                }

                if (w < width - 1) //Si w < width - 1, significa que no estamos en el borde derecho entonces intenta agregar el nodo de la derecha.
                {
                    TryAddNeighbor(
                        neighbors,
                        nodeGrid[w + 1 + h * width]
                    );
                }

                if (h > 0) //Si h > 0, significa que no estamos en el borde inferior entonces intenta agregar el nodo de abajo.
                {
                    TryAddNeighbor(
                        neighbors,
                        nodeGrid[w + (h - 1) * width]
                    );
                }

                if (h < height - 1) //Si h < height - 1, significa que no estamos en el borde superior entonces intenta agregar el nodo de arriba.
                {
                    TryAddNeighbor(
                        neighbors,
                        nodeGrid[w + (h + 1) * width]
                    );
                }

                currentNode.SetNeighbors(neighbors); //asigna la lista de vecinos al nodo actual, lo que permitirá al sistema de pathfinding saber a qué nodos puede moverse
            }
        }
    }

    private void TryAddNeighbor( //método auxiliar que intenta agregar un nodo vecino a la lista de vecinos, verificando que el nodo no sea nulo y sea transitable antes de agregarlo a la lista
        List<Node> neighbors,
        Node possibleNeighbor
    )
    {
        if (possibleNeighbor == null)
            return;

        if (!possibleNeighbor.IsWalkable)
            return;

        neighbors.Add(possibleNeighbor);
    }

    private void SetCostForNode(Node node, int w, int h) //método que asigna un costo personalizado a un nodo específico basado en su posición en la cuadrícula
    {
        if (w >= 4 && w <= 7 && h >= 2 && h <= 4)
        {
            node.SetCost(10f);
        }
        else
        {
            node.SetCost(1f);
        }
    }

    public Node GetClosestWalkableNode(Vector3 worldPosition) //método que encuentra el nodo caminable más cercano a una posición en el mundo
    {
        if (nodeGrid == null || nodeGrid.Length == 0) //verifica que la grilla de nodos exista
            return null;

        Node closestNode = null; //variable para almacenar el nodo más cercano encontrado durante la búsqueda
        float closestDistance = Mathf.Infinity; //variable para almacenar la distancia más corta encontrada durante la búsqueda

        foreach (Node node in nodeGrid) //recorre todos los nodos en la cuadrícula para encontrar el más cercano a la posición dada
        {
            if (node == null || !node.IsWalkable) //si el nodo es nulo o no es caminable, se salta a la siguiente iteración del bucle
                continue;

            float distanceToNode = Vector3.SqrMagnitude( //calcula la distancia al nodo utilizando la magnitud al cuadrado para evitar la operación de raíz cuadrada, lo que mejora el rendimiento
                node.transform.position - worldPosition
            );
            //si la distancia al nodo actual es menor que la distancia más corta encontrada hasta ahora, se actualiza el nodo más cercano y la distancia más corta
            if (distanceToNode < closestDistance) 
            {
                closestDistance = distanceToNode;
                closestNode = node;
            }
        }

        return closestNode; //devuelve el nodo caminable más cercano a la posición dada, o null si no se encontró ningún nodo caminable
    }

    [ContextMenu("ClearGrid")]
    public void ClearGrid() //método que elimina todos los nodos existentes en la cuadrícula, se puede llamar desde el inspector para limpiar la cuadrícula antes de generar una nueva
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            if (Application.isPlaying)
            {
                Destroy(transform.GetChild(i).gameObject);
            }
            else
            {
                DestroyImmediate(transform.GetChild(i).gameObject);
            }
        }

        nodeGrid = null;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;

        for (int h = 0; h < height; h++)
        {
            for (int w = 0; w < width; w++)
            {
                Vector3 nodePosition =
                    transform.position +
                    new Vector3(w * distance, 0f, h * distance);

                Vector3 checkCenter =
                    nodePosition + Vector3.up * nodeCheckSize.y;

                Gizmos.DrawWireCube(
                    checkCenter,
                    nodeCheckSize * 2f
                );
            }
        }
    }
}