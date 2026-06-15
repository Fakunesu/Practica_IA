using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class GridGenerator : MonoBehaviour
{
    [Header("Grid")]
    [SerializeField] private Node prefab;
    [SerializeField] private Node[] nodeGrid;
    [SerializeField] private int width = 10;
    [SerializeField] private int height = 10;
    [SerializeField] private float distance = 1f;

    [Header("Cost")]
    [SerializeField] private bool useCosts;

    [Header("Obstacles")]
    [SerializeField] private LayerMask obstacleMask;

    [Tooltip("Mitad del tamaño de la caja que revisa obstáculos.")]
    [SerializeField]
    private Vector3 nodeCheckSize =
        new Vector3(0.4f, 1f, 0.4f);

    public Node[] NodeGrid => nodeGrid;

    [ContextMenu("SetNodeGrid")]
    public void SetNodeGrid()
    {
        ClearGrid();

        if (prefab == null)
        {
            Debug.LogError("GridGenerator: falta asignar el prefab de Node.");
            return;
        }

        if (width <= 0 || height <= 0)
        {
            Debug.LogError("GridGenerator: width y height deben ser mayores a 0.");
            return;
        }

        nodeGrid = new Node[width * height];

        CreateNodes();
        SetNodeNeighbors();
    }

    private void CreateNodes()
    {
        for (int h = 0; h < height; h++)
        {
            for (int w = 0; w < width; w++)
            {
                Vector3 nodePosition =
                    transform.position +
                    new Vector3(w * distance, 0f, h * distance);

                Node newNode = Instantiate(
                    prefab,
                    nodePosition,
                    transform.rotation,
                    transform
                );

                newNode.name = $"Node {w}, {h}";
                newNode.SetIndexes(w, h);

                bool isBlocked = Physics.CheckBox(
                    nodePosition + Vector3.up * nodeCheckSize.y,
                    nodeCheckSize,
                    transform.rotation,
                    obstacleMask,
                    QueryTriggerInteraction.Ignore
                );

                newNode.SetWalkable(!isBlocked);

                if (isBlocked)
                {
                    newNode.SetCost(float.MaxValue);
                }
                else if (useCosts)
                {
                    SetCostForNode(newNode, w, h);
                }
                else
                {
                    newNode.SetCost(1f);
                }

                nodeGrid[w + h * width] = newNode;
            }
        }
    }

    private void SetNodeNeighbors()
    {
        for (int h = 0; h < height; h++)
        {
            for (int w = 0; w < width; w++)
            {
                Node currentNode = nodeGrid[w + h * width];

                List<Node> neighbors = new List<Node>();

                if (currentNode == null || !currentNode.IsWalkable)
                {
                    currentNode?.SetNeighbors(neighbors);
                    continue;
                }

                if (w > 0)
                {
                    TryAddNeighbor(
                        neighbors,
                        nodeGrid[w - 1 + h * width]
                    );
                }

                if (w < width - 1)
                {
                    TryAddNeighbor(
                        neighbors,
                        nodeGrid[w + 1 + h * width]
                    );
                }

                if (h > 0)
                {
                    TryAddNeighbor(
                        neighbors,
                        nodeGrid[w + (h - 1) * width]
                    );
                }

                if (h < height - 1)
                {
                    TryAddNeighbor(
                        neighbors,
                        nodeGrid[w + (h + 1) * width]
                    );
                }

                currentNode.SetNeighbors(neighbors);
            }
        }
    }

    private void TryAddNeighbor(
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

    private void SetCostForNode(Node node, int w, int h)
    {
        // Franja roja en el medio: transitable, pero cara
        if (w >= 4 && w <= 7 && h >= 2 && h <= 4)
        {
            node.SetCost(10f);
        }
        else
        {
            node.SetCost(1f);
        }
    }

    public Node GetClosestWalkableNode(Vector3 worldPosition)
    {
        if (nodeGrid == null || nodeGrid.Length == 0)
            return null;

        Node closestNode = null;
        float closestDistance = Mathf.Infinity;

        foreach (Node node in nodeGrid)
        {
            if (node == null || !node.IsWalkable)
                continue;

            float distanceToNode = Vector3.SqrMagnitude(
                node.transform.position - worldPosition
            );

            if (distanceToNode < closestDistance)
            {
                closestDistance = distanceToNode;
                closestNode = node;
            }
        }

        return closestNode;
    }

    [ContextMenu("ClearGrid")]
    public void ClearGrid()
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