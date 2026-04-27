using UnityEngine;
using System.Collections.Generic;

public class LineOfSight : MonoBehaviour
{
    [SerializeField] private int distance = 13;
    [SerializeField] private float angle = 13;
    [SerializeField] private LayerMask obs;
    private GameObject player;

    [SerializeField] private float distanceForAttack=2.3f;
    private void Start()
    {
        player = GameObject.Find("Player");
    }

    public bool IsRange(Transform self, Transform target)
    {
        return Vector3.Distance(self.position, target.position) < distance;
    }

    public bool IsRangeAttack(Transform self, Transform target)
    {
        return Vector3.Distance(self.position, target.position) < distanceForAttack;
    }

    public bool IsAngle(Transform self, Transform target)
    {
        Vector3 dir = target.position - self.position;

        return Vector3.Angle(self.forward, dir) < angle / 2;
    }

    public bool IsObstacle(Transform self, Transform target)
    {
        Vector3 dir = target.position - self.position;

        return Physics.Raycast(self.position, dir.normalized, dir.magnitude, obs);
    }
}
