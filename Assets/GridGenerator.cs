using System.CodeDom.Compiler;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class GridGenerator : MonoBehaviour
{
    public Node prefab;
    public Node[] nodeGrid;
    public int width;
    public int height;
    public float distance;

    [Header("Cost")]
    public bool useCosts;


    [ContextMenu("SetNodeGrid")]
    public void SetNodeGrid()
    {

        ClearGrid();

        nodeGrid = new Node[width * height];


        for (int h = 0; h < height; h++)
        {
            for (int w = 0; w < width; w++)
            {
                Node newNode = Instantiate(prefab, transform.position
                    + new Vector3(w * distance, 0, h * distance), transform.rotation, transform);

                newNode.name = $"Node {w}, {h}";
                newNode.SetIndexes(w, h);

                if (useCosts)
                {
                    SetCostForNode(newNode, w, h);
                }
                else
                {
                    newNode.SetCost(1f);
                    newNode.SetColor(Color.white);
                }

                    nodeGrid[w + h * width] = newNode;
            }
        }

        for (int h = 0; h < height; h++)
        {
            for (int w = 0; w < width; w++)
            {
                List<Node> neighs = new();
                if (w > 0) neighs.Add(nodeGrid[w - 1 + h * width]);
                if (w < width - 1) neighs.Add(nodeGrid[w + 1 + h * width]);
                if (h > 0) neighs.Add(nodeGrid[w + (h - 1) * width]);
                if (h < height - 1) neighs.Add(nodeGrid[w + (h + 1) * width]);
                nodeGrid[w + h * width].SetNeighbors(neighs);
            }
        }
    }

    private void SetCostForNode(Node node, int w, int h)
    {
        if (w > 15 && w < 35 && h > 20 && h < 25)
        {
            node.SetCost(1f);
            node.SetColor(Color.gray);
        }
        else
        {
            node.SetCost(1f);
            node.SetColor(Color.white);
        }
    } 

    [ContextMenu("ClearGrid")]
    public void ClearGrid()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            DestroyImmediate(transform.GetChild(i).gameObject);
        }
    }
}