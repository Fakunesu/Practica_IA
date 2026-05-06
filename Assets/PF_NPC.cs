using UnityEngine;
using System.Collections.Generic;

public class PF_NPC : MonoBehaviour
{

    public Node start, end;
    public float speed = 2f;

    private List<Node> path = new List<Node>(); 
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (path.Count > 0) 
        {
            var dir = path[0].transform.position - transform.position;
            transform.position += dir.normalized * Time.deltaTime * speed;
            if (dir.magnitude < 0.3f)
                path.RemoveAt(0);
        } 

        if (Input.GetKeyDown(KeyCode.P))
        {
            path = PathFinding.BFS(start, end);
        }
    }
}
