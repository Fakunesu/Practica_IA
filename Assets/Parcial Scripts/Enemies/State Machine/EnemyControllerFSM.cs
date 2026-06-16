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

    [Header("A* Patrol")] //referencia a la grilla y tiempo entre recalculos de path
    [SerializeField] private GridGenerator grid;
    [SerializeField] private float repathTime = 0.5f;

    private List<Node> currentPath = new List<Node>(); //lista de nodos que forman el camino actual
    private int currentPathIndex = 0; //índice del nodo actual en el camino
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
    // A*
    // =========================================================

    public bool HasActivePath //indica si el enemigo tiene un camino activo hacia el waypoint actual
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

        currentPath.Clear();//limpio el camino actual
        currentPathIndex = 0;//reinicio el índice del camino

        CalculatePathToCurrentWaypoint();//calculo el camino hacia el waypoint actual
    }

    public virtual void PatrolWaypoints()
    {
        if (wayPoints == null || wayPoints.Length == 0) //valida que existan waypoints asignados
        {
            StopMoving();
            return;
        }

        Transform currentWaypoint = wayPoints[currentWaypointIndex]; //toma la posicion del waypoint actual

        if (currentWaypoint == null)
        {
            StopMoving();
            return;
        }

        bool reachedPathEnd = MoveThroughCurrentPath(); //mueve al enemigo a lo largo del camino calculado hacia el waypoint actual, devuelve true si llegó al final del camino

        float distanceToWaypoint = Vector3.Distance( //mide distancia real al waypoint
            transform.position,
            currentWaypoint.position
        );

        if (reachedPathEnd || distanceToWaypoint <= waypointThreshold) //calcula un nuevo camino con A* hacia el siguiente waypoint si llegó al final del camino
        {
            GoToNextWaypoint();
            CalculatePathToCurrentWaypoint();
        }
    }

    public void CalculatePathTo(Vector3 targetPosition) //calcula el camino hacia una posición objetivo utilizando A*
    {
        if (grid == null) //Revisa que haya una grid asignada, si no la hay, detiene el movimiento y muestra una advertencia en la consola
        {
            Debug.LogWarning("EnemyControllerFSM: falta asignar GridGenerator.");
            StopMoving();
            return;
        }

        Node startNode = grid.GetClosestWalkableNode(transform.position); //convierte la posición actual del enemigo y la posición objetivo en nodos de la grilla con metodo GetClosestWalkableNode
        Node endNode = grid.GetClosestWalkableNode(targetPosition);

        if (startNode == null || endNode == null) //Revisa que existan nodos válidos para el inicio y el final del camino
        {
            Debug.LogWarning("EnemyControllerFSM: no se encontró nodo inicial o final para A*.");
            StopMoving();
            return;
        }

        currentPath = PathFinding.AStar(startNode, endNode); //llamamos al algoritmo A* para calcular el camino entre el nodo inicial y el nodo final, y guardamos el resultado en currentPath
        currentPathIndex = 0;//reinicia el indice del camino para empezar a moverse desde el primer nodo del camino

        SkipCloseNodes(); //saltear nodos que ya estan demasiado cerca del enemigo para evitar que se quede atascado en el camino
    }

    private void CalculatePathToCurrentWaypoint()
    {
        if (wayPoints == null || wayPoints.Length == 0) //valida que existan waypoints asignados
        {
            StopMoving();
            return;
        }

        Transform currentWaypoint = wayPoints[currentWaypointIndex];//toma la posicion del waypoint actual

        if (currentWaypoint == null)
        {
            StopMoving();
            return;
        }

        CalculatePathTo(currentWaypoint.position); //obtiene la posición del waypoint actual y llama a CalculatePathTo para calcular el camino hacia ese waypoint utilizando A*
    }

    public bool MoveThroughCurrentPath() //devuelve un bool si ya termino el camino o si todavia esta recorriendo el camino
    {
        if (currentPath == null || currentPath.Count == 0) //revisa si no hay camino
        {
            StopMoving();
            return true;
        }

        SkipCloseNodes(); //saltea nodos que esten demasiado cerca del enemigo para evitar que se quede atascado en el camino

        if (currentPathIndex >= currentPath.Count) //revisa si ya se paso del final del camino
        {
            StopMoving();
            return true;
        }

        Node currentNode = currentPath[currentPathIndex]; //si todavia hay camino agarra el nodo actual del camino usando currentPathIndex para seguir el camino paso a paso

        if (currentNode == null) //si el nodo es nulo pasa al siguiente
        {
            currentPathIndex++;
            return false;
        }

        Vector3 targetPosition = currentNode.transform.position; //obtiene la posicion del nodo
        targetPosition.y = transform.position.y;

        Vector3 pathDirection = targetPosition - transform.position;//calucla la dirección hacia el nodo actual restando la posición del enemigo a la posición del nodo
        pathDirection.y = 0f;

        if (pathDirection.magnitude <= 0.45f)
        {
            currentPathIndex++;
            return false;
        }

        SetDirection(pathDirection.normalized);
        return false;
    }

    private void SkipCloseNodes() //evita que el enemigo se quede enganchado intentando ir a nodos que ya tiene encima
    {
        if (currentPath == null)//hay path?
            return;

        while (currentPathIndex < currentPath.Count) //mientras haya nodos por recorrer  
        {
            Node node = currentPath[currentPathIndex]; //agarra el nodo actual

            if (node == null)  //si es nulo lo saltea
            {
                currentPathIndex++;
                continue;
            }

            Vector3 nodePosition = node.transform.position;
            nodePosition.y = transform.position.y;

            float distance = Vector3.Distance( //calcula distancia entre el enemigo y el nodo
                transform.position,
                nodePosition
            );

            if (distance > 0.45f) //el nodo todavia esta lejos y vale la pena ir hacia el
                break;

            currentPathIndex++; //si el nodo ya esta muy cerca, lo saltea y pasa al siguiente
        }
    }

    private void GoToNextWaypoint() //cambia el waypoint actual
    {
        currentWaypointIndex++; //aumenta el indice en el array de waypoints para pasar al siguiente

        if (currentWaypointIndex >= wayPoints.Length) //si llego al ultimo waypoint , vuelve al primero para hacer un ciclo
        {
            currentWaypointIndex = 0;
        }

        currentPath.Clear(); //limpia el camino actual para que se calcule uno nuevo hacia el nuevo waypoint
        currentPathIndex = 0; //reinicia el indice del camino para empezar a moverse desde el primer nodo del nuevo camino
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