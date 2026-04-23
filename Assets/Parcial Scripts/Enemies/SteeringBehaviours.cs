using UnityEngine;

public static class SteeringBehaviours
{
    public static Vector3 Seek(Vector3 currentPosition, Vector3 currentVelocity, Vector3 targetPosition, float maxSpeed)
    {
        Vector3 desiredVelocity = (targetPosition - currentPosition);
        desiredVelocity.y = 0f;

        if (desiredVelocity.sqrMagnitude <= 0.0001f)
        {
            return Vector3.zero;
        }

        desiredVelocity = desiredVelocity.normalized * maxSpeed;
        Vector3 steering = desiredVelocity - currentVelocity;
        steering.y = 0f;
        return steering;

    }
    public static Vector3 Flee(Vector3 currentPosition, Vector3 currentVelocity, Vector3 targetPosition, float maxSpeed)
    {
        Vector3 desiredVelocity = (currentPosition - targetPosition);
        desiredVelocity.y = 0f;

        if (desiredVelocity.sqrMagnitude <= 0.0001f)
        {
            return Vector3.zero;
        }

        desiredVelocity = desiredVelocity.normalized * maxSpeed;
        Vector3 steering = desiredVelocity - currentVelocity;
        steering.y = 0f;
        return steering;
    }




    public static Vector3 Arrival
        (Vector3 currentPosition,
        Vector3 currentVelocity,
        Vector3 targetPosition,
        float maxSpeed,
        float slowingRadius
        )
    {
        Vector3 toTarget = targetPosition - currentPosition;
        toTarget.y = 0f;

        float distance = toTarget.magnitude;
        if (distance <= 0.0001f)
        {
            return -currentVelocity;
        }

        float targetSpeed = maxSpeed;

        if (distance < slowingRadius)
        {
            targetSpeed = maxSpeed * (distance / slowingRadius);
        }

        Vector3 desiredVelocity = toTarget.normalized * targetSpeed;
        Vector3 steering = desiredVelocity - currentVelocity;
        steering.y = 0f;
        return steering;
    }

    public static Vector3 Pursuit(
        Vector3 currentPosition,
        Vector3 currentVelocity,
        Vector3 targetPosition,
        Vector3 targetVelocity,
        float maxSpeed,
        float predictionTime
        )
    {
        Vector3 futureTarget = targetPosition + targetVelocity * predictionTime;
        return Seek(currentPosition, currentVelocity, futureTarget, maxSpeed);
    }

    public static Vector3 Evade(
        Vector3 currentPosition,
        Vector3 currentVelocity,
        Vector3 targetPosition,
        Vector3 targetVelocity,
        float maxSpeed,
        float predictionTime
        )
    {
        Vector3 futureTarget = targetPosition + targetVelocity * predictionTime;
        return Flee(currentPosition, currentVelocity, futureTarget, maxSpeed);
    }

    public static Vector3 ObstacleAvoidance(
        Transform agent,
        Vector3 currentVelocity,
        float maxSpeed,
        float lookAheadDistance,
        float sphereRadius,
        LayerMask obstacleMask,
        float avoidForce
        )
    {
        Vector3 origin = agent.position + Vector3.up * 0.5f;

        Vector3 forward;
        if (currentVelocity.sqrMagnitude > 0.0001f)
        {
            forward = currentVelocity.normalized;
        }
        else
        {
            forward = agent.forward;
        }

        forward.y = 0f;

        if (Physics.SphereCast(origin, sphereRadius, forward, out RaycastHit hit, lookAheadDistance, obstacleMask))
        {
            Vector3 awayFromObstacle = hit.normal;
            awayFromObstacle.y = 0f;

            Vector3 desiredVelocity = awayFromObstacle.normalized * maxSpeed;
            Vector3 steering = desiredVelocity - currentVelocity;
            steering.y = 0f;

            return steering.normalized * avoidForce;
        }
        return Vector3.zero;

    }


}