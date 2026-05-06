using NUnit.Framework;
using Unity.VisualScripting;
using System.Collections.Generic;
using UnityEngine;

public class GridGenerator : MonoBehaviour
{
    [SerializeField] public Node prefab;

    public Node[] nodeGrid;
    public int width;
    public int height;

    public float distance;

    public void SetNodeGrid()
    {
        nodeGrid = new Node[width * height];

        for (int h = 0; h < height; h++)
        {
            for (int w = 0; w < width; w++)
            {
                Vector3 pos = transform.position + new Vector3(w * distance, 0, h * distance);
                Node newNode = Instantiate(prefab, pos, transform.rotation);

                newNode.SetIndexes(w, h);
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
}
