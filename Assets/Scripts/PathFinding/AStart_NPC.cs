using UnityEngine;
using System.Collections.Generic;

public class AStar_NPC : MonoBehaviour
{
    public Node start;
    public Node end;
    public float speed = 2f;

    private List<Node> path = new List<Node>();

    private void Start()
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

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.A))
        {
            path = PathFinding.AStar(start, end);
        }

        MoveThroughPath();
    }

    private void MoveThroughPath()
    {
        if (path.Count <= 0)
            return;

        Vector3 targetPosition = path[0].transform.position;
        targetPosition.y = transform.position.y;

        transform.position = Vector3.MoveTowards(
            transform.position,
            targetPosition,
            speed * Time.deltaTime
        );

        float distance = Vector3.Distance(
            transform.position,
            targetPosition
        );

        if (distance < 0.05f)
        {
            path.RemoveAt(0);
        }
    }
}