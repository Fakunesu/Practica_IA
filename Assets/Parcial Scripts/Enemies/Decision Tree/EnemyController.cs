using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EnemyController : MonoBehaviour
{
    [Header("Player")]
    [SerializeField] private GameObject player;
    private Rigidbody playerRB;

    //"Scripts"
    private LineOfSight los;
    private EnemyTree desitionTree;
    private EnemyContext context;
    private PlayerMovementController playerStats;

    [Header("Enemy Stats")]
    private float maxStamina = 100;
    [SerializeField] private float stamina;
    [SerializeField] private float speed = 5;
    [SerializeField] private float chasingSpeed = 10f;

    [Header("SteeringBehaviour")]
    private Vector3 dir;
    [SerializeField] private float rotationSpeed = 33;
    [SerializeField] private float patrolRotationSpeed = 33;
    private Material defaultMaterial;
    private Vector3 wanderDirection;
    private float wanderTime;
    [SerializeField] private float WanderchangeInterval = 1.5f;
    //private bool isAttacking = false;
    [SerializeField] private float maxPredictionTime = 2f;
    [SerializeField] private float arriveRadius = 3f;


    private void Awake()
    {
        los = GetComponent<LineOfSight>();
        desitionTree = GetComponent<EnemyTree>();
        playerStats = player.GetComponent<PlayerMovementController>();
        wanderDirection = transform.forward;
        context = new EnemyContext { self = transform, player = player.transform, los = los };
    }

    private void Start()
    {
        
    }

    public void Update()
    {
        context.player = player.transform;
        Move(dir);
    }

    public bool HasStamina()
    {
        if (stamina <= 0)
        {
            return false;
        }
        else { return true; }
    }
    public bool IsSeeingPlayer()
    {
        if (los.IsRange(transform, player.transform))
        {

            return true;
        }
        else
        {
            return false;
        }
        
    }

    public bool IsInDisadvantage()
    {
        if (playerStats.isPowerUpped == true)
        {
            return true;
        }
        else
        {
            return false;
        }
        
    }

    public bool IsInRange()
    {
        Debug.Log("pregunta si ataca");
        if (los.IsRangeAttack(transform, player.transform))
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public void FleePlayer()
    {
        dir = SteeringBehaviour.Flee(transform, player.transform.position);
    }

    public void EvadePlayer()
    {
        dir = SteeringBehaviour.Evade(transform, player.transform, playerRB, maxPredictionTime);
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

        transform.position += moveDirection * speed * Time.deltaTime;

        transform.forward = Vector3.Lerp(transform.forward, moveDirection, Time.deltaTime * rotationSpeed);
    }

    public void Patrol()
    {
        transform.Rotate(0, rotationSpeed * Time.deltaTime, 0);
    }

    public void Rest()
    {
        if (stamina < maxStamina)
        {
            stamina += Time.deltaTime * 2;
            Debug.Log("Healing");
        }
    }


    public void Attack()
    {
        Debug.Log("Ataco");
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

        Debug.Log("random");
    }
    public void Seek()
    {
        dir = SteeringBehaviour.Seek(transform, player.transform.position);
    }

    private void Move(Vector3 dir)
    {
        transform.position += dir * speed * Time.deltaTime;

        if (dir != Vector3.zero)
        {
            transform.forward = Vector3.Lerp(transform.forward, dir, Time.deltaTime * rotationSpeed);
        }
    }

}