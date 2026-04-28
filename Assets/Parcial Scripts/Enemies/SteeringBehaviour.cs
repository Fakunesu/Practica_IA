using UnityEngine;

public static class SteeringBehaviour
{
    // Va hacia un objetivo.
    public static Vector3 Seek(Transform self, Vector3 Target)
    {
        Vector3 dir = Target - self.position;
        dir.y = 0;
        return dir.normalized;
    }

    // Se aleja de un objetivo.
    public static Vector3 Flee(Transform self, Vector3 Target)
    {
        Vector3 dir = self.position - Target;
        dir.y = 0;
        return dir.normalized;
    }

    // Intenta esquivar obstáculos usando un raycast hacia adelante.
    public static Vector3 ObstacleAvoidance(Transform self, Vector3 currentDir, float detecionDistance, LayerMask obstacleMask)
    {
        currentDir.y = 0f;

        if (currentDir == Vector3.zero)
        {
            currentDir = self.forward;
        }

        currentDir.Normalize();

        if (Physics.Raycast(self.position, currentDir, out RaycastHit hit, detecionDistance, obstacleMask))
        {
            Vector3 avoidDir = Vector3.Cross(Vector3.up, hit.normal);
            avoidDir.y = 0f;

            if (Vector3.Dot(avoidDir, self.forward) < 0f)
            {
                avoidDir = -avoidDir;
            }

            return avoidDir.normalized;
        }

        return currentDir;
    }

    // Va hacia un objetivo, pero desacelera cuando se acerca.
    public static Vector3 Arrive(Transform self, Vector3 Target, float slowRadius)
    {
        Vector3 dir = Target - self.position;
        float distance = dir.magnitude;

        if (distance < 0.01f)
        {
            return Vector3.zero;
        }

        float speedFactor = Mathf.Clamp01(distance / slowRadius);
        return dir.normalized * speedFactor;
    }

    // Persigue la posición futura del objetivo.
    public static Vector3 Pursue(Transform self, Transform target, Rigidbody targetRB, float maxPredictionTime)
    {
        Vector3 futurePos = CalculateFuturePos(self, target, targetRB, maxPredictionTime);
        return Seek(self, futurePos);
    }

    // Calcula dónde podría estar el objetivo en unos segundos.
    public static Vector3 CalculateFuturePos(Transform self, Transform target, Rigidbody targetRB, float maxPredictionTime)
    {
        Vector3 targetVelocity = targetRB.linearVelocity;

        Vector3 toTarget = target.position - self.position;
        toTarget.y = 0;

        float distance = toTarget.magnitude;
        float predictionTime = Mathf.Clamp(distance / 5f, 0f, maxPredictionTime);

        return target.position + targetVelocity * predictionTime;
    }

    // Escapa de la posición futura del objetivo.
    public static Vector3 Evade(Transform self, Transform target, Rigidbody targetRB, float maxPredictionTime)
    {
        Vector3 futurePos = CalculateFuturePos(self, target, targetRB, maxPredictionTime);
        return Flee(self, futurePos);
    }

    // Cambia la dirección de forma aleatoria para movimiento errático.
    public static Vector3 Wander(Vector3 currentDirection, float maxAngleChange)
    {
        float randomAngle = Random.Range(-maxAngleChange, maxAngleChange);

        Quaternion rotation = Quaternion.Euler(0f, randomAngle, 0f);
        Vector3 newDirection = rotation * currentDirection;

        newDirection.y = 0f;

        return newDirection.normalized;
    }
}