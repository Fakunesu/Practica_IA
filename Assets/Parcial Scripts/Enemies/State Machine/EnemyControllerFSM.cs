using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EnemyControllerFSM : MonoBehaviour
{
    public Transform player;

    private Rigidbody rb;
    private PlayerMovementController playerStats;
    private LineOfSight los;
    private FSMClasses fsm;

    [Header("Movement")]
    [SerializeField] private float speed = 3f;
    [SerializeField] private float rotationSpeed = 33f;

    [Header("Patrol")]
    [SerializeField] private Transform[] wayPoints;
    [SerializeField] private float waypointThreshold = 0.5f;
    private int currentWaypointIndex = 0;

    [Header("A* Patrol")]
    [SerializeField] private GridGenerator grid;
    [SerializeField] private float repathTime = 0.5f;

    private List<Node> currentPath = new List<Node>();
    private int currentPathIndex = 0;
    private float repathTimer = 0f;

    private Vector3 dir;
    private bool isAttacking = false;
    private Coroutine freezeRoutine;

    [Header("Evade")]
    [SerializeField] private float evadeDistance = 6f;

    [Header("Stamina")]
    [SerializeField] private float currentStamina = 100f;
    [SerializeField] private float maxStamina = 100f;
    [SerializeField] private float staminaRegenRate = 300f;
    [SerializeField] private float staminaDepletionRate = 30f;

    [Header("Obstacle Avoidance")]
    [SerializeField] private float obstacleDetectionDistance = 5f;
    [SerializeField] private LayerMask obstacleMask;


    public bool HasStamina => currentStamina > 0f;
    public bool IsStaminaFull => currentStamina >= maxStamina;

    private void Awake()
    {
        fsm = GetComponent<FSMClasses>();
        los = GetComponent<LineOfSight>();
    }

    protected virtual void Start()
    {
        GameObject playerObject = GameObject.Find("Player");

        if (playerObject != null)
        {
            player = playerObject.transform;
            playerStats = player.GetComponent<PlayerMovementController>();
            rb = player.GetComponent<Rigidbody>();
        }
        else
        {
            Debug.LogWarning("EnemyControllerFSM: no se encontró un GameObject llamado Player.");
        }
    }

    private void Update()
    {
        if (player == null || los == null || fsm == null)
            return;

        bool canSeePlayer =
            los.IsRange(transform, player) &&
            los.IsAngle(transform, player) &&
            !los.IsObstacle(transform, player);

        fsm.UpdateState(canSeePlayer);

        if (isAttacking)
        {
            LookAtPlayer();
        }

        Move(dir);
    }

    public bool IsInDisadvantage()
    {
        if (playerStats == null)
            return false;

        return playerStats.IsPowerUpped;
    }

    public bool ShouldEvade()
    {
        if (player == null || playerStats == null)
            return false;

        return IsPlayerCloseForEvade() && IsInDisadvantage();
    }

    public bool IsPlayerCloseForEvade()
    {
        if (player == null)
            return false;

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        return distanceToPlayer <= evadeDistance;
    }

    public void StopAttack()
    {
        isAttacking = false;
    }

    public void Attack()
    {
        StopMoving();
        LookAtPlayer();
        RestartScene();
    }

    // =========================================================
    // PATRULLA CON A*
    // =========================================================

    public bool HasActivePath
    {
        get
        {
            return currentPath != null &&
                   currentPath.Count > 0 &&
                   currentPathIndex < currentPath.Count;
        }
    }


    public virtual void StartPatrolPath()
    {
        // Si ya tenía un path activo, no recalculo.
        // Esto evita que al volver de Rest busque el nodo más cercano y vuelva para atrás.
        if (HasActivePath)
        {
            return;
        }

        currentPath.Clear();
        currentPathIndex = 0;

        CalculatePathToCurrentWaypoint();
    }

    public virtual void PatrolWaypoints()
    {
        if (wayPoints == null || wayPoints.Length == 0)
        {
            StopMoving();
            return;
        }

        Transform currentWaypoint = wayPoints[currentWaypointIndex];

        if (currentWaypoint == null)
        {
            StopMoving();
            return;
        }

        bool reachedPathEnd = MoveThroughCurrentPath();

        float distanceToWaypoint = Vector3.Distance(
            transform.position,
            currentWaypoint.position
        );

        if (reachedPathEnd || distanceToWaypoint <= waypointThreshold)
        {
            GoToNextWaypoint();
            CalculatePathToCurrentWaypoint();
        }
    }

    public void CalculatePathTo(Vector3 targetPosition)
    {
        if (grid == null)
        {
            Debug.LogWarning("EnemyControllerFSM: falta asignar GridGenerator.");
            StopMoving();
            return;
        }

        Node startNode = grid.GetClosestWalkableNode(transform.position);
        Node endNode = grid.GetClosestWalkableNode(targetPosition);

        if (startNode == null || endNode == null)
        {
            Debug.LogWarning("EnemyControllerFSM: no se encontró nodo inicial o final para A*.");
            StopMoving();
            return;
        }

        currentPath = PathFinding.AStar(startNode, endNode);
        currentPathIndex = 0;

        SkipCloseNodes();
    }

    private void CalculatePathToCurrentWaypoint()
    {
        if (wayPoints == null || wayPoints.Length == 0)
        {
            StopMoving();
            return;
        }

        Transform currentWaypoint = wayPoints[currentWaypointIndex];

        if (currentWaypoint == null)
        {
            StopMoving();
            return;
        }

        CalculatePathTo(currentWaypoint.position);
    }

    public bool MoveThroughCurrentPath()
    {
        if (currentPath == null || currentPath.Count == 0)
        {
            StopMoving();
            return true;
        }

        SkipCloseNodes();

        if (currentPathIndex >= currentPath.Count)
        {
            StopMoving();
            return true;
        }

        Node currentNode = currentPath[currentPathIndex];

        if (currentNode == null)
        {
            currentPathIndex++;
            return false;
        }

        Vector3 targetPosition = currentNode.transform.position;
        targetPosition.y = transform.position.y;

        Vector3 pathDirection = targetPosition - transform.position;
        pathDirection.y = 0f;

        if (pathDirection.magnitude <= 0.45f)
        {
            currentPathIndex++;
            return false;
        }

        SetDirection(pathDirection.normalized);
        return false;
    }

    private void SkipCloseNodes()
    {
        if (currentPath == null)
            return;

        while (currentPathIndex < currentPath.Count)
        {
            Node node = currentPath[currentPathIndex];

            if (node == null)
            {
                currentPathIndex++;
                continue;
            }

            Vector3 nodePosition = node.transform.position;
            nodePosition.y = transform.position.y;

            float distance = Vector3.Distance(
                transform.position,
                nodePosition
            );

            if (distance > 0.45f)
                break;

            currentPathIndex++;
        }
    }

    private void GoToNextWaypoint()
    {
        currentWaypointIndex++;

        if (currentWaypointIndex >= wayPoints.Length)
        {
            currentWaypointIndex = 0;
        }

        currentPath.Clear();
        currentPathIndex = 0;
    }

    // =========================================================
    // STEERING / CHASE / EVADE
    // =========================================================

    public void Seek()
    {
        if (player == null)
        {
            StopMoving();
            return;
        }

        Vector3 seekDir = SteeringBehaviour.Seek(transform, player.position);

        dir = SteeringBehaviour.ObstacleAvoidance(
            transform,
            seekDir,
            obstacleDetectionDistance,
            obstacleMask
        );
    }

    public void Flee()
    {
        if (player == null)
        {
            StopMoving();
            return;
        }

        dir = SteeringBehaviour.Flee(transform, player.position);
    }

    public void FreezePlayer(float duration)
    {
        if (freezeRoutine != null)
        {
            StopCoroutine(freezeRoutine);
        }

        freezeRoutine = StartCoroutine(FreezeCoroutine(duration));
    }

    private IEnumerator FreezeCoroutine(float duration)
    {
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.isKinematic = true;
        }

        yield return new WaitForSeconds(duration);

        if (rb != null)
        {
            rb.isKinematic = false;
        }
    }

    private void Move(Vector3 moveDir)
    {
        transform.position += moveDir * speed * Time.deltaTime;

        if (moveDir != Vector3.zero)
        {
            transform.forward = Vector3.Lerp(
                transform.forward,
                moveDir,
                Time.deltaTime * rotationSpeed
            );
        }
    }

    public void DrainStamina()
    {
        currentStamina -= staminaDepletionRate * Time.deltaTime;
        currentStamina = Mathf.Clamp(currentStamina, 0f, maxStamina);
    }

    public void RegenerateStamina()
    {
        currentStamina += staminaRegenRate * Time.deltaTime;
        currentStamina = Mathf.Clamp(currentStamina, 0f, maxStamina);
    }

    public void SetDirection(Vector3 newDir)
    {
        dir = newDir;
    }

    public void StopMoving()
    {
        dir = Vector3.zero;
    }

    public void LookAtPlayer()
    {
        if (player == null)
            return;

        Vector3 lookDir = player.position - transform.position;
        lookDir.y = 0f;

        if (lookDir.sqrMagnitude <= 0.001f)
            return;

        Quaternion targetRotation = Quaternion.LookRotation(lookDir);

        transform.rotation = Quaternion.Lerp(
            transform.rotation,
            targetRotation,
            Time.deltaTime * rotationSpeed
        );
    }

    public void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Attack();
        }
    }

    public void RestartScene()
    {
        Scene currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currentScene.name);
    }
}