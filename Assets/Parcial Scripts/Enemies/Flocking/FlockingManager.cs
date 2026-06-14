using System.Collections.Generic;
using UnityEngine;

public class FlockingManager : MonoBehaviour
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
}