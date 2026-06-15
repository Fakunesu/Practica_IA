using UnityEngine;
using System.Collections.Generic;

public class PF_NPC : MonoBehaviour
{

    public Node start, end;
    public float speed = 2f;
    private List<Node> path = new List<Node>(); 

    void Start()
    {
        if (start != null)
        {
            Vector3 startPos = start.transform.position;

            transform.position = new Vector3(
                startPos.x,
                transform.position.y,
                startPos.z
            );
        }
    }

    void Update()
    {
        if (path.Count > 0) 
        {
            var dir = path[0].transform.position - transform.position;
            dir.y = 0f;

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
