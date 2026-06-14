using UnityEngine;

public class FlockingAgent : MonoBehaviour
{
    private FlockingManager manager;
    private Vector3 velocity;

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
        Vector3 obstacleAvoidance = CalculateObstacleAvoidance();

        Vector3 finalDirection =
            separation * manager.SeparationWeight +
            cohesion * manager.CohesionWeight +
            alignment * manager.AlignmentWeight +
            obstacleAvoidance * manager.ObstacleAvoidanceWeight;

        finalDirection.y = 0f;

        if (finalDirection.sqrMagnitude > 0.001f)
        {
            velocity = Vector3.Lerp(
                velocity,
                finalDirection.normalized * manager.Speed,
                Time.deltaTime * manager.TurnSpeed
            );
        }

        MoveSafely();

        if (velocity.sqrMagnitude > 0.001f)
        {
            transform.forward = velocity.normalized;
        }
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

            if (slideDirection.sqrMagnitude > 0.001f)
            {
                velocity = slideDirection.normalized * manager.Speed;

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

    private Vector3 CalculateSeparation()
    {
        Vector3 direction = Vector3.zero;

        foreach (FlockingAgent agent in manager.Agents)
        {
            if (agent == this)
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
            if (agent == this)
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
            if (agent == this)
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

        Vector3 avoidDirection = Vector3.ProjectOnPlane(
            forwardDirection,
            hit.normal
        );

        avoidDirection.y = 0f;

        if (avoidDirection.sqrMagnitude <= 0.001f)
        {
            avoidDirection = Vector3.Cross(Vector3.up, hit.normal);
            avoidDirection.y = 0f;
        }

        return avoidDirection.normalized;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        manager.RemoveAgent(this);
        Destroy(gameObject);
    }

    private void OnDrawGizmosSelected()
    {
        if (manager == null)
            return;

        Gizmos.color = Color.red;

        Gizmos.DrawWireSphere(
            transform.position,
            manager.AgentRadius
        );

        Gizmos.DrawRay(
            transform.position,
            transform.forward * manager.ObstacleDetectionDistance
        );
    }
}