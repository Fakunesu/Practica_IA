using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EnemyController : MonoBehaviour
{
    private LineOfSight los;
    private EnemyTree desitionTree;
    private EnemyContext context;
    private Rigidbody rb;

    [Header("Flocking Target")]
    [SerializeField] private FlockingManager flockingManager;
    private FlockingAgent currentTarget;

    [Header("Enemy Stats")]
    private float maxStamina = 15f;
    [SerializeField] private float stamina;
    [SerializeField] private float speed = 5f;
    [SerializeField] private float chasingSpeed = 10f;
    [SerializeField] private float attackDistance = 1.5f;

    [Header("Steering Behaviour")]
    private Vector3 dir;
    [SerializeField] private float rotationSpeed = 33f;
    [SerializeField] private float patrolRotationSpeed = 33f;
    private Vector3 wanderDirection;
    private float wanderTime;
    [SerializeField] private float WanderchangeInterval = 1.5f;
    [SerializeField] private float arriveRadius = 3f;

    [Header("Patrol")]
    [SerializeField] private List<Transform> wayPoints = new List<Transform>();
    [SerializeField] private float patrolSpeed = 10f;
    [SerializeField] private float minDistanceToWaypoint = 0.2f;
    private int currentWaypointIndex;
    private bool rightPatrol;

    [Header("Rest")]
    [SerializeField] private float timer = 5f;
    [SerializeField] private float counter;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        los = GetComponent<LineOfSight>();
        desitionTree = GetComponent<EnemyTree>();

        if (flockingManager == null)
        {
            flockingManager = FindFirstObjectByType<FlockingManager>();
        }

        wanderDirection = transform.forward;
    }

    private void Update()
    {
        FindVisibleFlockingAgent();
    }

    private void FixedUpdate()
    {
        Move(dir);
    }

    private void FindVisibleFlockingAgent()
    {
        if (flockingManager == null || flockingManager.Agents.Count == 0)
        {
            currentTarget = null;
            flockingManager?.ClearEnemyThreat(transform);
            return;
        }

        FlockingAgent closestAgent = null;
        float closestDistanceSqr = Mathf.Infinity;

        foreach (FlockingAgent agent in flockingManager.Agents)
        {
            if (agent == null)
                continue;

            if (!los.IsRange(transform, agent.transform))
                continue;

            Vector3 distance =
                agent.transform.position - transform.position;

            distance.y = 0f;

            float distanceSqr = distance.sqrMagnitude;

            if (distanceSqr < closestDistanceSqr)
            {
                closestDistanceSqr = distanceSqr;
                closestAgent = agent;
            }
        }

        currentTarget = closestAgent;

        if (currentTarget != null)
        {
            flockingManager.SetEnemyThreat(transform);
        }
        else
        {
            flockingManager.ClearEnemyThreat(transform);
        }
    }

    public bool HasStamina()
    {
        return stamina > 0f;
    }

    // El árbol puede seguir usando este nombre.
    public bool IsSeeingPlayer()
    {
        return currentTarget != null;
    }

    public bool IsInDisadvantage()
    {
        return false;
    }

    public bool IsInRange()
    {
        if (currentTarget == null || los == null)
            return false;

        return los.IsRangeAttack(transform, currentTarget.transform);
    }

    public void PatrollingWaypoints()
    {
        if (wayPoints == null || wayPoints.Count == 0)
            return;

        if (wayPoints[currentWaypointIndex] == null)
            return;

        Transform currentWaypoint = wayPoints[currentWaypointIndex];

        dir = SteeringBehaviour.Seek(transform, currentWaypoint.position);

        if ((currentWaypoint.position - transform.position).magnitude < minDistanceToWaypoint)
        {
            if (currentWaypointIndex == 0)
            {
                rightPatrol = true;
            }
            else if (currentWaypointIndex == wayPoints.Count - 1)
            {
                rightPatrol = false;
            }

            currentWaypointIndex += rightPatrol ? 1 : -1;
        }

        if (dir != Vector3.zero)
        {
            transform.forward = dir;
            stamina -= Time.deltaTime;
        }
    }

    public void FleePlayer()
    {
        if (currentTarget == null)
        {
            dir = Vector3.zero;
            return;
        }

        dir = SteeringBehaviour.Flee(
            transform,
            currentTarget.transform.position
        );
    }

    public void EvadePlayer()
    {
        FleePlayer();
    }

    public void ArriveToPlayer()
    {
        if (currentTarget == null)
        {
            dir = Vector3.zero;
            return;
        }

        dir = SteeringBehaviour.Arrive(
            transform,
            currentTarget.transform.position,
            arriveRadius
        );
    }

    public void Pursue()
    {
        if (currentTarget == null)
        {
            dir = Vector3.zero;
            return;
        }

        Vector3 direction =
            currentTarget.transform.position - transform.position;

        direction.y = 0f;

        dir = direction.normalized;
    }

    public void Patrol()
    {
        transform.Rotate(0f, patrolRotationSpeed * Time.deltaTime, 0f);
        dir = Vector3.zero;
    }

    public void Rest()
    {
        counter += Time.deltaTime;
        dir = Vector3.zero;

        if (currentTarget != null)
        {
            Seek();
        }

        if (counter > timer)
        {
            stamina = maxStamina;
            counter = 0f;
        }
    }

    public void Attack()
    {
        if (currentTarget == null)
            return;

        currentTarget.Rescue();

        currentTarget = null;
        dir = Vector3.zero;
    }

    public void Wander()
    {
        wanderTime -= Time.deltaTime;

        if (wanderTime <= 0f)
        {
            wanderDirection = SteeringBehaviour.Wander(
                wanderDirection,
                180f
            );

            wanderTime = WanderchangeInterval;
        }

        dir = wanderDirection;
    }

    public void Seek()
    {
        if (currentTarget == null)
        {
            dir = Vector3.zero;
            return;
        }

        dir = SteeringBehaviour.Seek(
            transform,
            currentTarget.transform.position
        );
    }

    private void Move(Vector3 direction)
    {
        if (rb == null)
            return;

        float currentSpeed =
            currentTarget != null
                ? chasingSpeed
                : speed;

        rb.linearVelocity = direction.normalized * currentSpeed;

        if (direction != Vector3.zero)
        {
            transform.forward = Vector3.Lerp(
                transform.forward,
                direction,
                Time.deltaTime * rotationSpeed
            );
        }
    }

    public void RestartScene()
    {
        Scene currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currentScene.name);
    }
}