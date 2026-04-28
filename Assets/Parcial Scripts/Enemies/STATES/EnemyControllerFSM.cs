using System.Collections;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.SceneManagement;

// Este script controla el comportamiento general del enemigo.
// Usa una FSM para decidir estados como patrullar, perseguir, atacar, evadir, descansar, etc.
public class EnemyControllerFSM : MonoBehaviour
{
    // Referencia al player para poder perseguirlo, escapar o mirarlo.
    public Transform player;

    // Referencia al script del player, para saber si está con power up.
    private PlayerMovementController playerStats;

    // Script encargado de calcular si el enemigo puede ver al player.
    private LineOfSight los;

    // Referencia a la máquina de estados.
    private FSMClasses fsm;

    // Velocidad de movimiento del enemigo.
    [SerializeField] private float speed = 3f;

    // Velocidad con la que rota el enemigo.
    [SerializeField] private float rotationSpeed = 33f;




    [SerializeField] private Transform[] wayPoints;

    // Distancia mínima para considerar que llegó al waypoint.
    [SerializeField] private float waypointThreshold = 0.5f;

    private int currentWaypointIndex = 0;

    // Dirección actual de movimiento del enemigo.
    private Vector3 dir;

    private bool isAttacking = false;

    // Guardo la coroutine de congelar para poder frenarla si ya había una activa.
    private Coroutine freezeRoutine;

    [SerializeField] private float evadeDistance = 6f;

    // Sistema de stamina del enemigo.
    [SerializeField] private float currentStamina = 100f;
    [SerializeField] private float maxStamina = 100f;
    [SerializeField] private float staminaRegenRate = 300f;
    [SerializeField] private float staminaDepletionRate = 30f;

    [Header("Obstacle Avoidance")]

    // Distancia a la que el enemigo detecta obstáculos.
    [SerializeField] private float obstacleDetectionDistance = 5f;

    // Layer de obstáculos que el enemigo tiene que evitar.
    [SerializeField] private LayerMask obstacleMask;

    // Propiedades públicas para consultar stamina desde otros scripts/estados.
    public bool HasStamina => currentStamina > 0f;
    public bool IsStaminaFull => currentStamina >= maxStamina;

    private void Awake()
    {
        // Busco los componentes necesarios en el mismo enemigo.
        fsm = GetComponent<FSMClasses>();
        los = GetComponent<LineOfSight>();
    }

    protected virtual void Start()
    {
        // Busco al player por nombre.
        player = GameObject.Find("Player").transform;

        // Guardo referencias del player para consultar estados o modificarlo.
        playerStats = player.GetComponent<PlayerMovementController>();
    }

    private void Update()
    {
        // Chequeo si el enemigo puede ver al player:
        // primero si está en rango/ángulo, y después si no hay obstáculos en el medio.
        bool canSeePlayer = los.IsRange(transform, player) && !los.IsObstacle(transform, player);

        // Actualizo la FSM y le paso si puede ver al player.
        fsm.UpdateState(canSeePlayer);

        // Si está atacando, hago que mire al player aunque no se esté moviendo.
        if (isAttacking)
        {
            LookAtPlayer();
        }

        // Finalmente muevo al enemigo según la dirección calculada por el estado actual.
        Move(dir);
    }

    public bool IsInDisadvantage()
    {
        // Si no encontró el script del player, devuelvo false para evitar errores.
        if (playerStats == null)
        {
            return false;
        }

        // El enemigo está en desventaja si el player tiene power up.
        return playerStats.IsPowerUpped;
    }

    public bool ShouldEvade()
    {
        // Seguridad por si todavía no se asignaron las referencias.
        if (player == null || playerStats == null)
        {
            return false;
        }

        // El enemigo debe evadir si el player está cerca y además tiene power up.
        return IsPlayerCloseForEvade() && IsInDisadvantage();
    }

    public bool IsPlayerCloseForEvade()
    {
        // Calculo la distancia entre el enemigo y el player.
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        // Devuelve true si el player está dentro de la distancia de evasión.
        return distanceToPlayer <= evadeDistance;
    }

    public void StopAttack()
    {
        // Desactivo el estado de ataque.
        isAttacking = false;
    }

    public void Attack()
    {
        // Marco que el enemigo está atacando.
        isAttacking = true;

        if (isAttacking)
        {
            // Mientras ataca, no se mueve.
            dir = Vector3.zero;


        }


    }

    public virtual void PatrolWaypoints()
    {
        // Si no hay waypoints asignados, no se mueve.
        if (wayPoints == null || wayPoints.Length == 0)
        {
            dir = Vector3.zero;
            return;
        }

        // Guardo el waypoint actual según el índice.
        Transform currentWaypoint = wayPoints[currentWaypointIndex];

        // Si el waypoint actual no existe, freno.
        if (currentWaypoint == null)
        {
            dir = Vector3.zero;
            return;
        }

        // Calculo la dirección hacia el waypoint usando steering Seek.
        dir = SteeringBehaviour.Seek(transform, currentWaypoint.position);

        // Calculo distancia al waypoint actual.
        float distance = Vector3.Distance(transform.position, currentWaypoint.position);

        // Si llegué al waypoint, paso al siguiente.
        if (distance <= waypointThreshold)
        {
            currentWaypointIndex++;

            // Si me pasé del último waypoint, vuelvo al primero.
            if (currentWaypointIndex >= wayPoints.Length)
            {
                currentWaypointIndex = 0;
            }
        }
    }

    public void Seek()
    {
        // Calculo la dirección hacia el player.
        Vector3 seekDir = SteeringBehaviour.Seek(transform, player.position);

        // Ajusto esa dirección usando obstacle avoidance,
        // para que el enemigo intente esquivar obstáculos mientras persigue.
        dir = SteeringBehaviour.ObstacleAvoidance(
            transform,
            seekDir,
            obstacleDetectionDistance,
            obstacleMask
        );
    }

    public void Flee()
    {
        // Calculo una dirección contraria al player para escapar.
        dir = SteeringBehaviour.Flee(transform, player.transform.position);
    }

    public void FreezePlayer(float duration)
    {
        // Si ya había una coroutine de freeze activa, la freno.
        if (freezeRoutine != null)
        {
            StopCoroutine(freezeRoutine);
        }

        // Empiezo una nueva coroutine para congelar al player.
        freezeRoutine = StartCoroutine(FreezeCoroutine(duration));
    }

    private IEnumerator FreezeCoroutine(float duration)
    {
        // Busco el Rigidbody del player.
        Rigidbody rb = player.GetComponent<Rigidbody>();

        if (rb != null)
        {
            // Freno el movimiento del player.
            rb.linearVelocity = Vector3.zero;

            // Lo hago kinematic para que no se mueva por física.
            rb.isKinematic = true;
        }

        // Espero la duración del freeze.
        yield return new WaitForSeconds(duration);

        if (rb != null)
        {
            // Devuelvo el Rigidbody a normal.
            rb.isKinematic = false;
        }
    }

    private void Move(Vector3 dir)
    {
        // Muevo al enemigo en la dirección actual.
        transform.position += dir * speed * Time.deltaTime;

        // Si hay una dirección válida, roto el enemigo hacia esa dirección.
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
        // Bajo stamina con el tiempo.
        currentStamina -= staminaDepletionRate * Time.deltaTime;

        // La limito entre 0 y el máximo.
        currentStamina = Mathf.Clamp(currentStamina, 0f, maxStamina);
    }

    public void RegenerateStamina()
    {
        // Regenero stamina con el tiempo.
        currentStamina += staminaRegenRate * Time.deltaTime;

        // La limito entre 0 y el máximo.
        currentStamina = Mathf.Clamp(currentStamina, 0f, maxStamina);
    }

    public void SetDirection(Vector3 newDir)
    {
        // Método público para cambiar la dirección desde scripts hijos,
        // como RouletteEnemyController.
        dir = newDir;
    }

    public void StopMoving()
    {
        // Freno al enemigo.
        dir = Vector3.zero;
    }

    public void LookAtPlayer()
    {
        // Calculo la dirección hacia el player.
        Vector3 lookDir = player.position - transform.position;

        // Ignoro la altura para que rote solo en horizontal.
        lookDir.y = 0f;

        // Si la dirección es casi cero, no hago nada.
        if (lookDir.sqrMagnitude <= 0.001f)
        {
            return;
        }

        // Calculo la rotación hacia el player.
        Quaternion targetRotation = Quaternion.LookRotation(lookDir);

        // Roto suavemente hacia el player.
        transform.rotation = Quaternion.Lerp(
            transform.rotation,
            targetRotation,
            Time.deltaTime * rotationSpeed
        );
    }

    private void OnTriggerEnter(Collider other)
    {
        // Si el enemigo toca al player, reinicio la escena.
        if (other.CompareTag("Player"))
        {
            RestartScene();
        }
    }

    public void RestartScene()
    {
        // Obtengo la escena actual.
        Scene currentScene = SceneManager.GetActiveScene();

        // La cargo de nuevo para reiniciar.
        SceneManager.LoadScene(currentScene.name);
    }
}