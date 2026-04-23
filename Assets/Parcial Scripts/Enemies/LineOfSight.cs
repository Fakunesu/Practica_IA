using UnityEngine;
using System.Collections.Generic;
using System.IO;

public class LineOfSigth : MonoBehaviour
{
    [Header("Debug")]
    [SerializeField] private float debugDistance = 5f;
    [SerializeField] private float debugAngle = 90f;
    [SerializeField] private GameObject debugTarget;
    public bool Sigth(GameObject gameObject, float distance, float angle, LayerMask walls)
    {
        float realAngle;
        realAngle = angle / 2;
        var dir = gameObject.transform.position - transform.position;
        if (dir.magnitude > distance)
        {
            return false; 
        }
        if (Vector3.Angle(transform.forward, dir) > realAngle)
        {
            return false;
        }
        if (Physics.Raycast(transform.position, dir.normalized, dir.magnitude, walls))
        {
            return false; 
        }
        else
        {
            return true;
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, debugDistance);

        Vector3 leftBoundary = DirFromAngle(-debugAngle / 2f);
        Vector3 rightBoundary = DirFromAngle(debugAngle / 2f);

        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(transform.position, transform.position + leftBoundary * debugDistance);
        Gizmos.DrawLine(transform.position, transform.position + rightBoundary * debugDistance);

        Gizmos.color = Color.blue;
        Gizmos.DrawLine(transform.position, transform.position + transform.forward * debugDistance);

        if (debugTarget != null)
        {
            Vector3 dirToTarget = debugTarget.transform.position - transform.position;

            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, debugTarget.transform.position);
        }
    }

    private Vector3 DirFromAngle(float angleOffset)
    {
        float angle = transform.eulerAngles.y + angleOffset;
        float radians = angle * Mathf.Deg2Rad;

        return new Vector3(Mathf.Sin(radians), 0f, Mathf.Cos(radians));
    }
}
