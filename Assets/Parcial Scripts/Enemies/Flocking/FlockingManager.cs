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

    [Header("Enemy Threat")]
    [SerializeField] private float enemyDetectDistance = 8f;
    [SerializeField] private float fleeSpeedMultiplier = 2.5f;

    private int currentWaypointIndex;
    private FlockMode currentMode = FlockMode.Wander;

    private Transform currentEnemyThreat;
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

    public Transform CurrentEnemyThreat => currentEnemyThreat;
    public float EnemyDetectDistance => enemyDetectDistance;
    public float FleeSpeedMultiplier => fleeSpeedMultiplier;

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
            GameObject playerObject = GameObject.FindGameObjectWithTag("Player");

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
            }
        }
        else if (currentMode == FlockMode.FollowPlayer)
        {
            if (distanceToPlayer >= playerAbandonDistance)
            {
                currentMode = FlockMode.Wander;
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

    public void SetEnemyThreat(Transform enemy)
    {
        currentEnemyThreat = enemy;
    }

    public void ClearEnemyThreat(Transform enemy)
    {
        if (currentEnemyThreat == enemy)
        {
            currentEnemyThreat = null;
        }
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
        // Centro del grupo que se usa para detectar al player.
        Vector3 groupCenter = Application.isPlaying
            ? GetGroupCenter()
            : transform.position;

        // Radio donde empiezan a seguir al player.
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(groupCenter, playerDetectDistance);

        // Radio donde abandonan al player y vuelven a Wander.
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(groupCenter, playerAbandonDistance);

        // Si hay una amenaza, muestra el radio de escape desde el enemigo.
        if (currentEnemyThreat != null)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(
                currentEnemyThreat.position,
                enemyDetectDistance
            );
        }

        // Líneas visuales desde el centro del flock hacia cada agente.
        if (Application.isPlaying && agents != null)
        {
            Gizmos.color = Color.cyan;

            foreach (FlockingAgent agent in agents)
            {
                if (agent != null)
                {
                    Gizmos.DrawLine(groupCenter, agent.transform.position);
                }
            }
        }
    }
}