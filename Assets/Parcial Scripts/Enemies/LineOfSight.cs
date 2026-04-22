using UnityEngine;
using System.Collections.Generic;

public class LineOfSigth : MonoBehaviour
{


    public bool Sigth(GameObject gameObject, float distance, float angle, LayerMask walls)
    {
        float realAngle;
        realAngle = angle / 2;
        var dir = gameObject.transform.position - transform.position;
        if (dir.magnitude > distance)
        {
            Debug.Log("No lo ve: está lejos");
            return false; 
        }
        if (Vector3.Angle(transform.forward, dir) > realAngle)
        {
            Debug.Log("No lo ve: está fuera del ángulo");
            return false;
        }
        if (Physics.Raycast(transform.position, dir.normalized, dir.magnitude, walls))
        {
            Debug.Log("No lo ve: hay una pared en el medio");
            return false; 
        }
        else
        {
            Debug.Log("lo ve");
            return true;
        }
    }
}
