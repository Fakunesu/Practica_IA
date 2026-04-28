using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EnemyControllerFSM : MonoBehaviour
{
    public Transform player;

    private Rigidbody rb;
    private PlayerMovementController playerStats;
    private LineOfSight los;
    private FSMClasses fsm;

    [SerializeField] private float speed = 3f;
    [SerializeField] private float rotationSpeed = 33f;

    [SerializeField] private Transform[] wayPoints;
    [SerializeField] private float waypointThreshold = 0.5f;
    private int currentWaypointIndex = 0;

    private Vector3 dir;
    private bool isAttacking = false;
    private Coroutine freezeRoutine;

    [SerializeField] private float evadeDistance = 6f;

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
        player = GameObject.Find("Player").transform;

        playerStats = player.GetComponent<PlayerMovementController>();
        rb = player.GetComponent<Rigidbody>();
    }

    private void Update()
    {
        bool canSeePlayer = los.IsRange(transform, player) && !los.IsObstacle(transform, player);

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
        {
            return false;
        }

        return playerStats.IsPowerUpped;
    }

    public bool ShouldEvade()
    {
        if (player == null || playerStats == null)
        {
            return false;
        }

        return IsPlayerCloseForEvade() && IsInDisadvantage();
    }

    public bool IsPlayerCloseForEvade()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        return distanceToPlayer <= evadeDistance;
    }

    public void StopAttack()
    {
        isAttacking = false;
    }

    public void Attack()
    {
        isAttacking = true;
        dir = Vector3.zero;
    }

    // Patrulla por waypoints en orden.
    public virtual void PatrolWaypoints()
    {
        if (wayPoints == null || wayPoints.Length == 0)
        {
            dir = Vector3.zero;
            return;
        }

        Transform currentWaypoint = wayPoints[currentWaypointIndex];

        if (currentWaypoint == null)
        {
            dir = Vector3.zero;
            return;
        }

        dir = SteeringBehaviour.Seek(transform, currentWaypoint.position);

        float distance = Vector3.Distance(transform.position, currentWaypoint.position);

        if (distance <= waypointThreshold)
        {
            currentWaypointIndex++;

            if (currentWaypointIndex >= wayPoints.Length)
            {
                currentWaypointIndex = 0;
            }
        }
    }

    public void Seek()
    {
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

    // Aplica el movimiento final del enemigo.
    private void Move(Vector3 dir)
    {
        transform.position += dir * speed * Time.deltaTime;

        if (dir != Vector3.zero)
        {
            transform.forward = Vector3.Lerp(
                transform.forward,
                dir,
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

    // Hace que el enemigo mire al player mientras ataca.
    public void LookAtPlayer()
    {
        Vector3 lookDir = player.position - transform.position;
        lookDir.y = 0f;

        if (lookDir.sqrMagnitude <= 0.001f)
        {
            return;
        }

        Quaternion targetRotation = Quaternion.LookRotation(lookDir);

        transform.rotation = Quaternion.Lerp(
            transform.rotation,
            targetRotation,
            Time.deltaTime * rotationSpeed
        );
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            RestartScene();
        }
    }

    public void RestartScene()
    {
        Scene currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currentScene.name);
    }
}