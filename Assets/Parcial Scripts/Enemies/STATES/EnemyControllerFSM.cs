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


    private Vector3 dir;
    private bool isAttacking = false;
    private Coroutine freezeRoutine;

    [SerializeField] private float currentStamina = 100f;
    [SerializeField] private float maxStamina = 100f;
    [SerializeField] private float staminaRegenRate = 300f;
    [SerializeField] private float staminaDepletionRate = 30f;

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

    public void Wander()
    {
        wanderTime -= Time.deltaTime;
        
        if (wanderTime <= 0f)
        {
            wanderDirection = SteeringBehaviour.Wander(wanderDirection, 180);
            wanderTime = wanderChangeInterval;
        }
        dir = wanderDirection;
    }

    public void Seek()
    {
        dir = SteeringBehaviour.Seek(transform, player.position);
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



