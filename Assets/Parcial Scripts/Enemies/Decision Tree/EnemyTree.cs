using System.Collections.Generic;
using UnityEngine;

public class EnemyTree : MonoBehaviour
{


    [Header("Stats")]
    [SerializeField] private float maxStamina = 100f;
    [SerializeField] private float stamina = 100f;
    [SerializeField] private float staminaRecoveryRate = 10f;

    [Header("References")]
    [SerializeField] private GameObject player;
    private LineOfSigth los;
    private PlayerState playerState;
    private EnemyController enemyController;

    [Header("Vision")]
    [SerializeField] private float losDistance;
    [SerializeField] private float losAngle;
    [SerializeField] private LayerMask losWalls;


    [Header("Chase")]
    [SerializeField] private float chasingSpeed = 6f;
    [SerializeField] private float attackRange = 1.5f;
    [SerializeField] private float predictionTime = 0.5f;


    private Vector3 lastPlayerPosition;
    private Vector3 playerVelocityEstimate;


    [Header("Patrol")]
    [SerializeField] private List<Transform> wayPoints = new List<Transform>();
    private int currentWaypointIndex = 0;
    [SerializeField] private float patrolSpeed = 3f;
    [SerializeField] private float minDistanceToWaypoint = 1f;

    [Header("RunAway")]
    [SerializeField]private float runAwaySpeed = 7f;

    [Header("Obstacle Avoidance")]
    [SerializeField] private LayerMask obstacleMask;
    [SerializeField] private float obstacleLookAhead = 2.5f;
    [SerializeField] private float obstacleSphereRadius = 0.4f;
    [SerializeField] private float obstacleAvoideForce = 12f;
    [SerializeField] private float obstacleWeight = 1.5f;   


    private string currentState = "";

    [SerializeField] private bool debugStates = true;



    private ITreeeNode root;

    private void Awake()
    {
        los = GetComponent<LineOfSigth>();
        enemyController = GetComponent<EnemyController>();

        if (player != null)
        {
            playerState = player.GetComponent<PlayerState>();
            lastPlayerPosition = player.transform.position;
        }
    }

    void Start()
    {
        ActionNode attack = new ActionNode(Attack);
        ActionNode chasing = new ActionNode(Chasing);
        ActionNode runAway = new ActionNode(RunAway);
        ActionNode patrol = new ActionNode(Patrol);
        ActionNode rest = new ActionNode(Rest);

        QuestionNode isInRange = new QuestionNode(IsInRange, attack, chasing);
        QuestionNode isInDisadvantage = new QuestionNode(IsInDisadvantage, runAway, isInRange);
        QuestionNode isSeeingPlayer = new QuestionNode(IsSeeingPlayer, isInDisadvantage, patrol);
        QuestionNode isPlayerPoweredUp = new QuestionNode(IsPlayerPoweredUp, runAway, isSeeingPlayer);
        QuestionNode hasStamina = new QuestionNode(HasStamina, isPlayerPoweredUp, rest);

        root = hasStamina;

    }

    private void UpdatePlayerVelocityEstimate()
    {
        if (player == null)
        {
            if (debugStates) Debug.Log("UpdatePlayerVelocityEstimate -> player null");
            return;
        }

        playerVelocityEstimate = (player.transform.position - lastPlayerPosition) / Time.deltaTime;
        playerVelocityEstimate.y = 0f;
        lastPlayerPosition = player.transform.position;

        if (debugStates) Debug.Log("PLAYER VELOCITY ESTIMATE -> " + playerVelocityEstimate);
    }


    void Update()
    {
        UpdatePlayerVelocityEstimate();

        if (root != null)
        {
            root.Execute();
        }
    }

    private bool IsPlayerPoweredUp()
    {
        if (playerState == null)
        {
            if (debugStates)
            {
                Debug.Log("Check isplayerpowererup = false (playerState is null)");
            }
            return false;
        }

        bool result = playerState.HasPowerUp;
        if (debugStates)
        {
            Debug.Log("Check isplayerpowererup = " + result);
        }
        return result;
    }

    private bool HasStamina()
    {
        bool result = stamina > 0f;
        if (debugStates) Debug.Log("CHECK HasStamina = " + result + " | stamina = " + stamina);
        return result;
    }

    private bool IsInDisadvantage()
    {
        bool result = false;
        if (debugStates) Debug.Log("CHECK IsInDisadvantage = " + result);
        return result;
    }

    private bool IsSeeingPlayer()
    {
        if (los == null || player == null)
        {
            if (debugStates) Debug.Log("CHECK IsSeeingPlayer = false (los o player null)");
            return false;
        }

        bool result = los.Sigth(player, losDistance, losAngle, losWalls);
        if (debugStates) Debug.Log("CHECK IsSeeingPlayer = " + result);
        return result;
    }

    private bool IsInRange()
    {
        if (player == null)
        {
            if (debugStates) Debug.Log("CHECK IsInRange = false (player null)");
            return false;
        }

        float distanceToPlayer = Vector3.Distance(transform.position, player.transform.position);
        bool result = distanceToPlayer <= attackRange;

        if (debugStates) Debug.Log("CHECK IsInRange = " + result + " | distance = " + distanceToPlayer);
        return result;
    }


    private void Attack()
    {
        SetDebugState("ATTACK");

        if (!IsSeeingPlayer())
        {
            if (debugStates) Debug.Log("ATTACK CANCELLED -> no ve al player");
            return;
        }

        if (!IsInRange())
        {
            if (debugStates) Debug.Log("ATTACK CANCELLED -> player fuera de rango");
            return;
        }

        if (debugStates) Debug.Log("ACTION -> Attacking");
    }


    private void Chasing()
    {
        SetDebugState("CHASE");

        if (player == null || enemyController == null)
        {
            if (debugStates) Debug.Log("CHASE CANCELLED -> player o enemyController null");
            return;
        }

        Vector3 pursuitSteering = SteeringBehaviours.Pursuit(
            transform.position,
            enemyController.Velocity,
            player.transform.position,
            playerVelocityEstimate,
            enemyController.MaxSpeed,
            predictionTime
        );

        Vector3 avoidanceSteering = SteeringBehaviours.ObstacleAvoidance(
            transform,
            enemyController.Velocity,
            enemyController.MaxSpeed,
            obstacleLookAhead,
            obstacleSphereRadius,
            obstacleMask,
            obstacleAvoideForce
        );

        Vector3 finalSteering = pursuitSteering + avoidanceSteering * obstacleWeight;

        if (debugStates)
        {
            Debug.Log("CHASE DATA -> " +
                      " pursuit: " + pursuitSteering +
                      " | avoidance: " + avoidanceSteering +
                      " | final: " + finalSteering +
                      " | velocity: " + enemyController.Velocity);
        }

        enemyController.ApplySteering(finalSteering);

        stamina -= Time.deltaTime * 5f;
        stamina = Mathf.Clamp(stamina, 0f, maxStamina);

        Vector3 futureTarget = player.transform.position + playerVelocityEstimate * predictionTime;

        Debug.DrawLine(transform.position, player.transform.position, Color.blue);
        Debug.DrawLine(transform.position, futureTarget, Color.red);
        Debug.DrawLine(transform.position, transform.position + finalSteering, Color.green);

        Vector3 debugOrigin = transform.position + Vector3.up * 0.5f;
        Vector3 debugForward = enemyController.Velocity.sqrMagnitude > 0.0001f
            ? enemyController.Velocity.normalized
            : transform.forward;

        debugForward.y = 0f;
        Debug.DrawRay(debugOrigin, debugForward * obstacleLookAhead, Color.yellow);
    }


    private void Rest()
    {
        SetDebugState("REST");

        if (stamina < maxStamina)
        {
            stamina += Time.deltaTime * staminaRecoveryRate;
            stamina = Mathf.Clamp(stamina, 0f, maxStamina);

            if (debugStates) Debug.Log("ACTION -> Resting | stamina = " + stamina);
        }
    }


    private void Patrol()
    {
        SetDebugState("PATROL");

        if (wayPoints == null || wayPoints.Count == 0)
        {
            Debug.Log("No hay waypoints");
            return;
        }

        if (wayPoints[currentWaypointIndex] == null || enemyController == null)
        {
            return;
        }

        Transform currentWaypoint = wayPoints[currentWaypointIndex];

        Vector3 arrivalSteering = SteeringBehaviours.Arrival(
            transform.position,
            enemyController.Velocity,
            currentWaypoint.position,
            enemyController.MaxSpeed,
            1.5f
        );

        Vector3 avoidanceSteering = SteeringBehaviours.ObstacleAvoidance(
            transform,
            enemyController.Velocity,
            enemyController.MaxSpeed,
            obstacleLookAhead,
            obstacleSphereRadius,
            obstacleMask,
            obstacleAvoideForce
        );

        Vector3 finalSteering = arrivalSteering + avoidanceSteering * obstacleWeight;

        if (debugStates)
        {
            Debug.Log("PATROL DATA -> waypointIndex: " + currentWaypointIndex +
                      " | waypoint: " + currentWaypoint.name +
                      " | distance: " + Vector3.Distance(transform.position, currentWaypoint.position) +
                      " | arrival: " + arrivalSteering +
                      " | avoidance: " + avoidanceSteering +
                      " | final: " + finalSteering);
        }

        enemyController.ApplySteering(finalSteering);

        float distance = Vector3.Distance(transform.position, currentWaypoint.position);
        if (distance <= minDistanceToWaypoint)
        {
            currentWaypointIndex++;

            if (currentWaypointIndex >= wayPoints.Count)
            {
                currentWaypointIndex = 0;
            }

            Debug.Log("PATROL -> cambio al waypoint " + currentWaypointIndex);
        }
    }
    private void RunAway()
    {
        SetDebugState("RUN_AWAY");

        if (player == null || enemyController == null)
        {
            if (debugStates) Debug.Log("RUNAWAY CANCELLED -> player o enemyController null");
            return;
        }

        Vector3 evadeSteering = SteeringBehaviours.Evade(
            transform.position,
            enemyController.Velocity,
            player.transform.position,
            playerVelocityEstimate,
            enemyController.MaxSpeed,
            predictionTime
        );

        Vector3 avoidanceSteering = SteeringBehaviours.ObstacleAvoidance(
            transform,
            enemyController.Velocity,
            enemyController.MaxSpeed,
            obstacleLookAhead,
            obstacleSphereRadius,
            obstacleMask,
            obstacleAvoideForce
        );

        Vector3 finalSteering = evadeSteering + avoidanceSteering * obstacleWeight;

        if (debugStates)
        {
            Debug.Log("RUNAWAY DATA -> evade: " + evadeSteering +
                      " | avoidance: " + avoidanceSteering +
                      " | final: " + finalSteering +
                      " | velocity: " + enemyController.Velocity);
        }

        enemyController.ApplySteering(finalSteering);
    }

    private void SetDebugState(string newState)
    {
        if (!debugStates) return;

        if (currentState != newState)
        {
            currentState = newState;
            Debug.Log("ENEMY STATE -> " + currentState);

            if (enemyController != null)
            {
                enemyController.StopImmediately();
            }
        }
    }

}
