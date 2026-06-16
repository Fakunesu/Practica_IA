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
    [SerializeField] private float neighborRadius = 5f; //radio para detectar vecinos cercanos
    [SerializeField] private float separationRadius = 0.5f;//radio para detectar vecinos demasiado cerca y evitar colisiones

    [Header("Weights")]
    [SerializeField] private float separationWeight = 0.6f;//cuanto se alejan entre si
    [SerializeField] private float cohesionWeight = 3f;//cuanto intentan mantenerse juntos, dispersasion
    [SerializeField] private float alignmentWeight = 1f;//cuanto intentan alinearse con la dirección del grupo
    [SerializeField] private float targetWeight = 2.5f;//cuanto priorizan ir al target (jugador)

    [Header("Follow Player")]
    [SerializeField] private float playerDetectDistance = 5f;//distancia a la que el grupo detecta al jugador y comienza a seguirlo
    [SerializeField] private float playerAbandonDistance = 8f;//distancia a la que el grupo abandona al jugador y vuelve a deambular

    [Header("Wander Waypoints")]
    [SerializeField] private Transform[] wanderWaypoints;//puntos de interés para que el grupo deambule cuando no sigue al jugador
    [SerializeField] private float waypointReachDistance = 2f;//distancia a la que el grupo considera que ha llegado a un waypoint y pasa al siguiente

    [Header("Obstacle Avoidance")]
    [SerializeField] private LayerMask obstacleMask;//capa que representa los obstáculos en el entorno
    [SerializeField] private float obstacleDetectionDistance = 2.5f;//distancia a la que los agentes detectan obstáculos y comienzan a evitarlos
    [SerializeField] private float obstacleAvoidanceWeight = 5f;//cuanto priorizan evitar obstáculos en su movimiento
    [SerializeField] private float agentRadius = 0.3f;//radio que representa el tamaño del agente para evitar colisiones con obstáculos y otros agentes

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

    public Vector3 CurrentTargetPosition //posición objetivo actual, ya sea el jugador o el waypoint de deambular
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

    private Vector3 CurrentWanderWaypointPosition //posición del waypoint de deambular actual, si no hay waypoints válidos devuelve la posición del manager como fallback
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

        for (int i = 0; i < amount; i++) //genera la cantidad de agentes especificada en posiciones aleatorias dentro del radio de spawn
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

    private void UpdateMode() //verifica la distancia al jugador y cambia el modo de comportamiento del grupo entre deambular y seguir al jugador
    {
        if (player == null || agents.Count == 0)
            return;

        Vector3 groupCenter = GetGroupCenter();

        float distanceToPlayer = Vector3.Distance(
            groupCenter,
            player.position
        );

        if (currentMode == FlockMode.Wander) //si el grupo está deambulando y el jugador se acerca lo suficiente, cambia a modo seguir al jugador
        {
            if (distanceToPlayer <= playerDetectDistance)
            {
                currentMode = FlockMode.FollowPlayer;
                Debug.Log("Flock: siguiendo al jugador.");
            }
        }
        else if (currentMode == FlockMode.FollowPlayer) //si el grupo está siguiendo al jugador y este se aleja lo suficiente, cambia a modo deambular
        {
            if (distanceToPlayer >= playerAbandonDistance)
            {
                currentMode = FlockMode.Wander;
                Debug.Log("Flock: el jugador se alejó, vuelven a deambular.");
            }
        }
    }

    private void UpdateWanderWaypoint() //si el grupo está deambulando, verifica la distancia al waypoint actual y si lo ha alcanzado, pasa al siguiente waypoint
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

    private void GoToNextWaypoint() //incrementa el índice del waypoint actual para que el grupo se dirija al siguiente waypoint, si se alcanza el final de la lista vuelve al primer waypoint
    {
        currentWaypointIndex++;

        if (currentWaypointIndex >= wanderWaypoints.Length)
        {
            currentWaypointIndex = 0;
        }
    }

    private Vector3 GetGroupCenter() //calcula la posición central del grupo promediando las posiciones de todos los agentes
    {
        if (agents == null || agents.Count == 0)
            return transform.position;

        Vector3 center = Vector3.zero;
        int count = 0;

        foreach (FlockingAgent agent in agents) //suma la posicion de todos los agentes para luego dividir por la cantidad y obtener el centro del grupo
        {
            if (agent == null)
                continue;

            center += agent.transform.position;
            count++;
        }

        if (count == 0)
            return transform.position;

        return center / count; //devuelve la posición central del grupo
    }

    public void RemoveAgent(FlockingAgent agent) //elimina un agente de la lista de agentes del manager, se llama cuando un agente es rescatado o destruido
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


