using System.Collections.Generic;
using UnityEngine;

public class FlockingManager : MonoBehaviour
{
    public enum FlockMode
    {
        Wander,
        FollowPlayer
    }

    [Header("References")]
    [SerializeField] private Transform player;
    [SerializeField] private FlockingAgent agentPrefab;

    [Header("Spawn")]
    [SerializeField] private int amount = 10;
    [SerializeField] private float spawnRadius = 5f;

    [Header("Movement")]
    [SerializeField] private float speed = 1.5f;
    [SerializeField] private float turnSpeed = 4f;

    [Header("Flocking Radius")]
    [SerializeField] private float neighborRadius = 5f;
    [SerializeField] private float separationRadius = 0.5f;

    [Header("Weights")]
    [SerializeField] private float separationWeight = 0.6f;
    [SerializeField] private float cohesionWeight = 3f;
    [SerializeField] private float alignmentWeight = 1f;
    [SerializeField] private float targetWeight = 2.5f;

    [Header("Follow Player")]
    [SerializeField] private float playerDetectDistance = 5f;
    [SerializeField] private float playerAbandonDistance = 8f;

    [Header("Wander Waypoints")]
    [SerializeField] private Transform[] wanderWaypoints;
    [SerializeField] private float waypointReachDistance = 2f;

    [Header("Obstacle Avoidance")]
    [SerializeField] private LayerMask obstacleMask;
    [SerializeField] private float obstacleDetectionDistance = 2.5f;
    [SerializeField] private float obstacleAvoidanceWeight = 5f;
    [SerializeField] private float agentRadius = 0.3f;

    private int currentWaypointIndex;
    private FlockMode currentMode = FlockMode.Wander;

    private List<FlockingAgent> agents = new List<FlockingAgent>();

    public List<FlockingAgent> Agents => agents;

    public float Speed => speed;
    public float TurnSpeed => turnSpeed;

    public float NeighborRadius => neighborRadius;
    public float SeparationRadius => separationRadius;

    public float SeparationWeight => separationWeight;
    public float CohesionWeight => cohesionWeight;
    public float AlignmentWeight => alignmentWeight;
    public float TargetWeight => targetWeight;

    public LayerMask ObstacleMask => obstacleMask;
    public float ObstacleDetectionDistance => obstacleDetectionDistance;
    public float ObstacleAvoidanceWeight => obstacleAvoidanceWeight;
    public float AgentRadius => agentRadius;

    public FlockMode CurrentMode => currentMode;

    public Vector3 CurrentTargetPosition
    {
        get
        {
            if (currentMode == FlockMode.FollowPlayer && player != null)
            {
                return player.position;
            }

            return CurrentWanderWaypointPosition;
        }
    }

    private Vector3 CurrentWanderWaypointPosition
    {
        get
        {
            if (
                wanderWaypoints == null ||
                wanderWaypoints.Length == 0 ||
                wanderWaypoints[currentWaypointIndex] == null
            )
            {
                return transform.position;
            }

            return wanderWaypoints[currentWaypointIndex].position;
        }
    }

    private void Start()
    {
        if (player == null)
        {
            GameObject playerObject = GameObject.Find("Player");

            if (playerObject != null)
            {
                player = playerObject.transform;
            }
        }

        SpawnAgents();
    }

    private void Update()
    {
        UpdateMode();
        UpdateWanderWaypoint();
    }

    private void SpawnAgents()
    {
        if (agentPrefab == null)
        {
            Debug.LogWarning("FlockingManager: falta asignar el prefab del agente.");
            return;
        }

        for (int i = 0; i < amount; i++)
        {
            Vector3 randomPosition =
                transform.position +
                new Vector3(
                    Random.Range(-spawnRadius, spawnRadius),
                    0f,
                    Random.Range(-spawnRadius, spawnRadius)
                );

            Quaternion randomRotation =
                Quaternion.Euler(
                    0f,
                    Random.Range(0f, 360f),
                    0f
                );

            FlockingAgent newAgent = Instantiate(
                agentPrefab,
                randomPosition,
                randomRotation
            );

            newAgent.Initialize(this);
            agents.Add(newAgent);
        }
    }

    private void UpdateMode()
    {
        if (player == null || agents.Count == 0)
            return;

        Vector3 groupCenter = GetGroupCenter();

        float distanceToPlayer = Vector3.Distance(
            groupCenter,
            player.position
        );

        if (currentMode == FlockMode.Wander)
        {
            if (distanceToPlayer <= playerDetectDistance)
            {
                currentMode = FlockMode.FollowPlayer;
                Debug.Log("Flock: siguiendo al jugador.");
            }
        }
        else if (currentMode == FlockMode.FollowPlayer)
        {
            if (distanceToPlayer >= playerAbandonDistance)
            {
                currentMode = FlockMode.Wander;
                Debug.Log("Flock: el jugador se alejó, vuelven a deambular.");
            }
        }
    }

    private void UpdateWanderWaypoint()
    {
        if (currentMode != FlockMode.Wander)
            return;

        if (wanderWaypoints == null || wanderWaypoints.Length == 0)
            return;

        Vector3 groupCenter = GetGroupCenter();

        float distanceToWaypoint = Vector3.Distance(
            groupCenter,
            CurrentWanderWaypointPosition
        );

        if (distanceToWaypoint <= waypointReachDistance)
        {
            GoToNextWaypoint();
        }
    }

    private void GoToNextWaypoint()
    {
        currentWaypointIndex++;

        if (currentWaypointIndex >= wanderWaypoints.Length)
        {
            currentWaypointIndex = 0;
        }
    }

    private Vector3 GetGroupCenter()
    {
        if (agents == null || agents.Count == 0)
            return transform.position;

        Vector3 center = Vector3.zero;
        int count = 0;

        foreach (FlockingAgent agent in agents)
        {
            if (agent == null)
                continue;

            center += agent.transform.position;
            count++;
        }

        if (count == 0)
            return transform.position;

        return center / count;
    }

    public void RemoveAgent(FlockingAgent agent)
    {
        if (agents.Contains(agent))
        {
            agents.Remove(agent);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, playerDetectDistance);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, playerAbandonDistance);

        if (wanderWaypoints == null)
            return;

        Gizmos.color = Color.yellow;

        for (int i = 0; i < wanderWaypoints.Length; i++)
        {
            if (wanderWaypoints[i] == null)
                continue;

            Gizmos.DrawWireSphere(
                wanderWaypoints[i].position,
                waypointReachDistance
            );

            int nextIndex = i + 1;

            if (nextIndex >= wanderWaypoints.Length)
                nextIndex = 0;

            if (wanderWaypoints[nextIndex] != null)
            {
                Gizmos.DrawLine(
                    wanderWaypoints[i].position,
                    wanderWaypoints[nextIndex].position
                );
            }
        }
    }
}

/*public class FlockingManager : MonoBehaviour
{
    [Header("Agent")]
    [SerializeField] private FlockingAgent agentPrefab;
    [SerializeField] private int amount = 10;
    [SerializeField] private float spawnRadius = 5f;

    [Header("Movement")]
    [SerializeField] private float speed = 1.5f;
    [SerializeField] private float turnSpeed = 4f;

    [Header("Flocking Radius")]
    [SerializeField] private float neighborRadius = 5f;
    [SerializeField] private float separationRadius = 0.5f;

    [Header("Weights")]
    [SerializeField] private float separationWeight = 0.6f;
    [SerializeField] private float cohesionWeight = 3f;
    [SerializeField] private float alignmentWeight = 1f;

    [Header("Obstacle Avoidance")]
    [SerializeField] private LayerMask obstacleMask;
    [SerializeField] private float obstacleDetectionDistance = 2f;
    [SerializeField] private float obstacleAvoidanceWeight = 3f;
    [SerializeField] private float agentRadius = 0.3f;
    [SerializeField] private Transform globalTarget;
    [SerializeField]
    private Vector3 boundsExtents = new Vector3(20, 1, 20);
    [SerializeField] private float maxForce = 5f;
    public float MaxForce => maxForce;
    public Vector3 BoundsCenter => transform.position;
    public Vector3 BoundsExtents => boundsExtents;

    [SerializeField]
    private float boundsWeight = 2f;

    public float BoundsWeight => boundsWeight;
    public Transform GlobalTarget => globalTarget;

    [SerializeField] private float targetWeight = 0.6f;
    public float TargetWeight => targetWeight;
    public LayerMask ObstacleMask => obstacleMask;
    public float ObstacleDetectionDistance => obstacleDetectionDistance;
    public float ObstacleAvoidanceWeight => obstacleAvoidanceWeight;
    public float AgentRadius => agentRadius;

    private List<FlockingAgent> agents = new List<FlockingAgent>();

    public List<FlockingAgent> Agents => agents;

    public float Speed => speed;
    public float TurnSpeed => turnSpeed;

    public float NeighborRadius => neighborRadius;
    public float SeparationRadius => separationRadius;

    public float SeparationWeight => separationWeight;
    public float CohesionWeight => cohesionWeight;
    public float AlignmentWeight => alignmentWeight;

    private void Start()
    {
        SpawnAgents();
    }

    private void SpawnAgents()
    {
        if (agentPrefab == null)
        {
            Debug.LogWarning("FlockingManager: falta asignar el prefab del agente.");
            return;
        }

        for (int i = 0; i < amount; i++)
        {
            Vector3 randomPosition =
                transform.position +
                new Vector3(
                    Random.Range(-spawnRadius, spawnRadius),
                    0f,
                    Random.Range(-spawnRadius, spawnRadius)
                );

            Quaternion randomRotation =
                Quaternion.Euler(
                    0f,
                    Random.Range(0f, 360f),
                    0f
                );

            FlockingAgent newAgent = Instantiate(
                agentPrefab,
                randomPosition,
                randomRotation
            );

            newAgent.Initialize(this);
            agents.Add(newAgent);
        }
    }

    public void RemoveAgent(FlockingAgent agent)
    {
        if (agents.Contains(agent))
        {
            agents.Remove(agent);
        }
    }
}*/
