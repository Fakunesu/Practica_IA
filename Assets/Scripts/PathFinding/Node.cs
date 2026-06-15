using System.Collections.Generic;
using UnityEngine;

public class Node : MonoBehaviour
{

    [SerializeField] List<Node> neighbors = new();
    [SerializeField] private int x, y;
    public List<Node> Neighbors => neighbors;

    [Header("Cost")]
    [SerializeField] private float cost = 1f;

    [Header("Walkable")]
    [SerializeField] private bool isWalkable = true;

    public int X => x;
    public int Y => y;

    public float Cost => cost;
    public bool IsWalkable => isWalkable;

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

    public void SetWalkable(bool value)
    {
        isWalkable = value;
    }

}
