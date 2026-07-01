using UnityEngine;

public class FlockingAgent : MonoBehaviour
{
    private FlockingManager manager;
    private Vector3 velocity;
    private bool rescued;

    public void Initialize(FlockingManager newManager)
    {
        manager = newManager;

        Vector3 randomDirection = new Vector3(
            Random.Range(-1f, 1f),
            0f,
            Random.Range(-1f, 1f)
        ).normalized;

        if (randomDirection == Vector3.zero)
        {
            randomDirection = transform.forward;
        }

        velocity = randomDirection * manager.Speed;
        transform.forward = randomDirection;
    }

    private void Update()
    {
        if (manager == null)
            return;

        Vector3 separation = CalculateSeparation();
        Vector3 cohesion = CalculateCohesion();
        Vector3 alignment = CalculateAlignment();
        Vector3 targetDirection = CalculateTargetDirection();
        Vector3 obstacleAvoidance = CalculateObstacleAvoidance();
        Vector3 enemyFlee = CalculateEnemyFlee();

        Vector3 finalDirection;
        float currentSpeed;

        if (enemyFlee != Vector3.zero)
        {
            // Cuando hay enemigo cerca, escapar domina al resto.
            finalDirection =
                enemyFlee * 8f +
                separation * manager.SeparationWeight +
                obstacleAvoidance * manager.ObstacleAvoidanceWeight;

            currentSpeed = manager.Speed * manager.FleeSpeedMultiplier;
        }
        else
        {
            finalDirection =
                separation * manager.SeparationWeight +
                cohesion * manager.CohesionWeight +
                alignment * manager.AlignmentWeight +
                targetDirection * manager.TargetWeight +
                obstacleAvoidance * manager.ObstacleAvoidanceWeight;

            currentSpeed = manager.Speed;
        }

        finalDirection.y = 0f;

        if (finalDirection.sqrMagnitude > 0.001f)
        {
            float currentTurnSpeed = enemyFlee != Vector3.zero
                ? manager.TurnSpeed * 2f
                : manager.TurnSpeed;

            velocity = Vector3.Lerp(
                velocity,
                finalDirection.normalized * currentSpeed,
                Time.deltaTime * currentTurnSpeed
            );
        }

        MoveSafely();

        if (velocity.sqrMagnitude > 0.001f)
        {
            transform.forward = velocity.normalized;
        }
    }

    private Vector3 CalculateEnemyFlee()
    {
        if (manager.CurrentEnemyThreat == null)
            return Vector3.zero;

        Vector3 directionAway =
            transform.position - manager.CurrentEnemyThreat.position;

        directionAway.y = 0f;

        float distance = directionAway.magnitude;

        if (distance > manager.EnemyDetectDistance)
            return Vector3.zero;

        if (distance <= 0.001f)
        {
            directionAway = Random.insideUnitSphere;
            directionAway.y = 0f;
        }

        return directionAway.normalized;
    }

    private Vector3 CalculateTargetDirection()
    {
        Vector3 direction =
            manager.CurrentTargetPosition - transform.position;

        direction.y = 0f;

        if (direction.sqrMagnitude <= 0.001f)
            return Vector3.zero;

        return direction.normalized;
    }

    private Vector3 CalculateSeparation()
    {
        Vector3 direction = Vector3.zero;

        foreach (FlockingAgent agent in manager.Agents)
        {
            if (agent == this || agent == null)
                continue;

            float distance = Vector3.Distance(
                transform.position,
                agent.transform.position
            );

            if (distance < manager.SeparationRadius)
            {
                Vector3 awayDirection =
                    transform.position - agent.transform.position;

                awayDirection.y = 0f;

                if (distance > 0.001f)
                {
                    direction += awayDirection.normalized / distance;
                }
            }
        }

        return direction;
    }

    private Vector3 CalculateCohesion()
    {
        Vector3 center = Vector3.zero;
        int count = 0;

        foreach (FlockingAgent agent in manager.Agents)
        {
            if (agent == this || agent == null)
                continue;

            float distance = Vector3.Distance(
                transform.position,
                agent.transform.position
            );

            if (distance < manager.NeighborRadius)
            {
                center += agent.transform.position;
                count++;
            }
        }

        if (count == 0)
            return Vector3.zero;

        center /= count;

        Vector3 directionToCenter = center - transform.position;
        directionToCenter.y = 0f;

        return directionToCenter.normalized;
    }

    private Vector3 CalculateAlignment()
    {
        Vector3 averageForward = Vector3.zero;
        int count = 0;

        foreach (FlockingAgent agent in manager.Agents)
        {
            if (agent == this || agent == null)
                continue;

            float distance = Vector3.Distance(
                transform.position,
                agent.transform.position
            );

            if (distance < manager.NeighborRadius)
            {
                averageForward += agent.transform.forward;
                count++;
            }
        }

        if (count == 0)
            return Vector3.zero;

        averageForward /= count;
        averageForward.y = 0f;

        return averageForward.normalized;
    }

    private Vector3 CalculateObstacleAvoidance()
    {
        Vector3 forwardDirection = velocity.normalized;

        if (forwardDirection.sqrMagnitude <= 0.001f)
        {
            forwardDirection = transform.forward;
        }

        bool detectedObstacle = Physics.SphereCast(
            transform.position,
            manager.AgentRadius,
            forwardDirection,
            out RaycastHit hit,
            manager.ObstacleDetectionDistance,
            manager.ObstacleMask,
            QueryTriggerInteraction.Ignore
        );

        if (!detectedObstacle)
            return Vector3.zero;

        // hit.normal apunta hacia afuera de la superficie:
        // sirve como dirección para alejarse de la pared.
        Vector3 avoidDirection = hit.normal;
        avoidDirection.y = 0f;

        // Si pega muy de frente o la normal no sirve en X/Z,
        // elegimos un costado para rodear el obstáculo.
        if (avoidDirection.sqrMagnitude <= 0.001f)
        {
            avoidDirection = Vector3.Cross(Vector3.up, forwardDirection);
            avoidDirection.y = 0f;
        }

        Debug.DrawRay(
        transform.position,
        forwardDirection * manager.ObstacleDetectionDistance,
        Color.red
);

        return avoidDirection.normalized;
    }

    private void MoveSafely()
    {
        Vector3 moveDirection = velocity.normalized;

        if (moveDirection.sqrMagnitude <= 0.001f)
            return;

        float moveDistance = velocity.magnitude * Time.deltaTime;

        bool willHitObstacle = Physics.SphereCast(
            transform.position,
            manager.AgentRadius,
            moveDirection,
            out RaycastHit hit,
            moveDistance + 0.05f,
            manager.ObstacleMask,
            QueryTriggerInteraction.Ignore
        );

        if (willHitObstacle)
        {
            Vector3 slideDirection = Vector3.ProjectOnPlane(
                moveDirection,
                hit.normal
            );

            slideDirection.y = 0f;

            if (slideDirection.sqrMagnitude <= 0.001f)
            {
                slideDirection = Vector3.Cross(Vector3.up, hit.normal);
                slideDirection.y = 0f;
            }

            if (slideDirection.sqrMagnitude > 0.001f)
            {
                velocity = slideDirection.normalized * velocity.magnitude;

                transform.position +=
                    slideDirection.normalized *
                    moveDistance *
                    0.5f;
            }
            else
            {
                velocity = Vector3.zero;
            }

            return;
        }

        transform.position += velocity * Time.deltaTime;
    }

    public void Rescue()
    {
        if (rescued)
            return;

        rescued = true;

        if (manager != null)
        {
            manager.RemoveAgent(this);
        }

        Destroy(gameObject);
    }
}