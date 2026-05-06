using NUnit.Framework;
using Unity.VisualScripting;
using System.Collections.Generic;
using UnityEngine;

public class Node : MonoBehaviour
{

    [SerializeField] List<Node> neighbors = new( );
    [SerializeField] private int x, y;
    [SerializeField] float cost = 1f;



    public List<Node> Neighbors => neighbors;

    public float Cost => cost;
    public int X => x;
    public int Y => y;

    public void SetIndexes(int w, int h)
    {
        x = w;
        y = h;
    }

    public void SetNeighbors(List<Node> neighs)
    {
        neighbors = neighs;
    }
}
