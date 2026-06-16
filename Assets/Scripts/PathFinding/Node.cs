using System.Collections.Generic;
using UnityEngine;

public class Node : MonoBehaviour
{

    [SerializeField] List<Node> neighbors = new(); //lista que guarda los nodos vecinos de este nodo
    [SerializeField] private int x, y;
    public List<Node> Neighbors => neighbors; 

    [Header("Cost")]
    [SerializeField] private float cost = 1f; //costo de moverse a este nodo, puede ser modificado para representar diferentes tipos de terreno o obstáculos

    [Header("Walkable")]
    [SerializeField] private bool isWalkable = true; //indica si el nodo es transitable o no, puede ser modificado para representar obstáculos o áreas inaccesibles

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
