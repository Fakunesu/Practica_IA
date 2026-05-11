using NUnit.Framework;
using Unity.VisualScripting;
using System.Collections.Generic;
using UnityEngine;

public class Node : MonoBehaviour
{

    [SerializeField] List<Node> neighbors = new();
    [SerializeField] private int x, y;
    private Renderer rend;
    public List<Node> Neighbors => neighbors;

    [Header("Cost")]
    [SerializeField] private float cost = 1f;

    public int X => x;
    public int Y => y;

    public float Cost => cost;

    public void SetIndexes(int w, int h)
    {
        x = w;
        y = h;
    }

    public void SetNeighbors(List<Node> neighbors)
    {
        this.neighbors = neighbors;
    }

    public void SetCost(float newCost)
    {
        cost = newCost;
    }

    public void SetColor(Color color)
    {
        if (rend == null)
        {
            rend = GetComponent<Renderer>();
        }
        rend.material.color = color;
    }
}
