using System.Collections;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EnemyControllerFSM : MonoBehaviour
{
    public Transform player;
    private PlayerMovementController playerStats;
    private Rigidbody playerRB;

    private LineOfSight los;
    private FSMClasses fsm;

    [SerializeField] private float speed = 3f;
    [SerializeField] private float rotationSpeed = 33f;

    private Vector3 wanderDirection;
    private float wanderTime;
    [SerializeField] private float wanderChangeInterval = 1.5f;

    [SerializeField] private Transform[] wayPoints;
    [SerializeField] private float waypointThreshold = 0.5f;
    private int currentWaypointIndex = 0;


    private Vector3 dir;
    private bool isAttacking = false;
    private Coroutine freezeRoutine;

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

        wanderDirection = transform.forward;
    }

    private void Start()
    {
        player = GameObject.Find("Player").transform;
        playerStats = player.GetComponent<PlayerMovementController>();
        playerRB = player.GetComponent<Rigidbody>();
    }

    private void Update()
    {
        bool canSeePlayer = los.IsRange(transform, player) && !los.IsObstacle(transform, player);

        fsm.UpdateState(canSeePlayer);

        Move(dir);
    }

    public bool IsInDisadvantage()
    {
        if (playerStats.IsPowerUpped == true)
        {
            return true;
        }
        else
        {
            return false;
        }
    }


    public void StopAttack()
    {
        isAttacking = false;
    }

    public void Attack()
    {
        isAttacking = true;

        if(isAttacking)
        {
            dir = Vector3.zero;
            Debug.Log("Attacking Player");
        }
        Debug.Log("Deja de atacar Player");
    }

    public void PatrolWaypoints()
    {
        if(wayPoints == null || wayPoints.Length == 0)
        {
            dir = Vector3.zero;
            return;
        }

        Transform currentWaypoint = wayPoints[currentWaypointIndex];

        if(currentWaypoint == null)
        {
            dir = Vector3.zero;
            return;
        }

        dir = SteeringBehaviour.Seek(transform, currentWaypoint.position);

        float distance = Vector3.Distance(transform.position, currentWaypoint.position);

        if ( distance <= waypointThreshold)
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

        dir = SteeringBehaviour.ObstacleAvoidance(transform, seekDir, obstacleDetectionDistance, obstacleMask);
    }

    public void Flee()
    {
        dir = SteeringBehaviour.Flee(transform, player.transform.position);
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
        Rigidbody rb = player.GetComponent<Rigidbody>();

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

    private void Move(Vector3 dir)
    {
       transform.position += dir * speed * Time.deltaTime;

        if ( dir!= Vector3.zero)
        {
            transform.forward = Vector3.Lerp(transform.forward, dir, Time.deltaTime * rotationSpeed);
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

    public void StopMoving()
    {
        dir = Vector3.zero;
    }

}



