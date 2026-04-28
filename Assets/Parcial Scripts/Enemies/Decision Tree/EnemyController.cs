using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EnemyController : MonoBehaviour
{
    [Header("Player")]
    [SerializeField] private GameObject player;
    private Rigidbody playerRB;

    private LineOfSight los;
    private EnemyTree desitionTree;
    private EnemyContext context;
    private PlayerMovementController playerStats;
    private Rigidbody rb;

    [Header("Enemy Stats")]
    private float maxStamina = 15;
    [SerializeField] private float stamina;
    [SerializeField] private float speed = 5;
    [SerializeField] private float chasingSpeed = 10f;
    [SerializeField] private float attackDistance = 1.5f;

    [Header("SteeringBehaviour")]
    private Vector3 dir;
    [SerializeField] private float rotationSpeed = 33;
    [SerializeField] private float patrolRotationSpeed = 33;
    private Material defaultMaterial;
    private Vector3 wanderDirection;
    private float wanderTime;
    [SerializeField] private float WanderchangeInterval = 1.5f;
    [SerializeField] private float maxPredictionTime = 2f;
    [SerializeField] private float arriveRadius = 3f;

    [Header("Patrol")]
    [SerializeField] private List<Transform> wayPoints = new List<Transform>();
    [SerializeField] private float patrolSpeed = 10f;
    [SerializeField] private float minDistanceToWaypoint = 0.2f;
    private int currentWaypointIndex = 0;
    private bool rightPatrol;

    [Header("Rest")]
    [SerializeField] private float timer = 5f;
    [SerializeField] float counter;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        los = GetComponent<LineOfSight>();
        desitionTree = GetComponent<EnemyTree>();

        if (player == null)
        {
            player = GameObject.FindGameObjectWithTag("Player");
        }

        if (player != null)
        {
            playerStats = player.GetComponent<PlayerMovementController>();
            playerRB = player.GetComponent<Rigidbody>();

            context = new EnemyContext
            {
                self = transform,
                player = player.transform,
                los = los
            };
        }

        wanderDirection = transform.forward;
    }

    private void Update()
    {
        if (context != null && player != null)
        {
            context.player = player.transform;
        }
    }

    private void FixedUpdate()
    {
        Move(dir);
    }

    // Preguntas que usa el Decision Tree para decidir qué acción ejecutar.
    public bool HasStamina()
    {
        return stamina > 0;
    }

    public bool IsSeeingPlayer()
    {
        if (player == null || los == null)
        {
            return false;
        }

        return los.IsRange(transform, player.transform);
    }

    public bool IsInDisadvantage()
    {
        if (playerStats == null)
        {
            return false;
        }

        return playerStats.IsPowerUpped;
    }

    public bool IsInRange()
    {
        if (player == null || los == null)
        {
            return false;
        }

        return los.IsRangeAttack(transform, player.transform);
    }

    // Patrulla entre waypoints, yendo y volviendo por la lista.
    public void PatrollingWaypoints()
    {
        if (wayPoints == null || wayPoints.Count == 0)
        {
            return;
        }

        if (wayPoints[currentWaypointIndex] == null)
        {
            return;
        }

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

            if (rightPatrol)
            {
                currentWaypointIndex++;
            }
            else
            {
                currentWaypointIndex--;
            }
        }

        if (dir != Vector3.zero)
        {
            transform.forward = dir;
            stamina -= Time.deltaTime;
        }
    }

    public void FleePlayer()
    {
        dir = SteeringBehaviour.Flee(transform, player.transform.position);
    }

    public void EvadePlayer()
    {
        dir = SteeringBehaviour.Flee(transform, player.transform.position);
    }

    public void ArriveToPlayer()
    {
        dir = SteeringBehaviour.Arrive(transform, player.transform.position, arriveRadius);
    }

    public void Pursue()
    {
        Vector3 direction = player.transform.position - transform.position;
        direction.y = 0;

        Vector3 moveDirection = direction.normalized;

        dir = moveDirection;

        transform.forward = Vector3.Lerp(
            transform.forward,
            moveDirection,
            Time.deltaTime * rotationSpeed
        );
    }

    public void Patrol()
    {
        transform.Rotate(0, rotationSpeed * Time.deltaTime, 0);
    }

    // Descansa hasta recuperar stamina.
    public void Rest()
    {
        counter += Time.deltaTime;
        dir = Vector3.zero;

        if (los.IsRange(transform, player.transform))
        {
            Seek();
        }

        if (counter > timer)
        {
            stamina = maxStamina;
            counter = 0;
            return;
        }
    }

    public void Attack()
    {
         RestartScene();    
    }

    public void Wander()
    {
        wanderTime -= Time.deltaTime;

        if (wanderTime <= 0f)
        {
            wanderDirection = SteeringBehaviour.Wander(wanderDirection, 180f);
            wanderTime = WanderchangeInterval;
        }

        dir = wanderDirection;
    }

    public void Seek()
    {
        dir = SteeringBehaviour.Seek(transform, player.transform.position);
    }

    // Aplica el movimiento final según la dirección que haya elegido el árbol.
    private void Move(Vector3 dir)
    {
        if (rb == null)
        {
            return;
        }

        rb.linearVelocity = dir.normalized * speed;

        if (dir != Vector3.zero)
        {
            transform.forward = Vector3.Lerp(
                transform.forward,
                dir,
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