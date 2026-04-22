using NUnit.Framework;
using System.Collections.Generic;
using System.ComponentModel.Design.Serialization;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class EnemyTree : MonoBehaviour
{

    private float maxStamina=100;
    [SerializeField] private float stamina;
    private LineOfSigth los;

    [Header("Chase")]
    [SerializeField] private GameObject player;
    [SerializeField] private float losDistance;
    [SerializeField] private float losAngle;
    [SerializeField] private LayerMask losWalls;
    [SerializeField] private float chasingSpeed = 6f;

    [Header("Patrol")]
    [SerializeField] private List<Transform> wayPoints = new List<Transform>();
    [SerializeField] private float patrolSpeed = 3f;
    [SerializeField] private float minDistanceToWaypoint = 0.2f;

    private int currentWaypointIndex = 0;


    private ITreeeNode root;

    private void Awake()
    {
        los = GetComponent<LineOfSigth>();
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
        QuestionNode hasStamina = new QuestionNode(HasStamina, isSeeingPlayer, rest);

        root = hasStamina;

    }

    // Update is called once per frame
    void Update()
    {
        if (root != null)
        {
            root.Execute();
        }
    }

    private bool HasStamina()
    {
        if (stamina <= 0)
        {
            return false;
        }
        else { return true; }
    }

    private bool IsInDisadvantage()
    {
        return false;
    }

    private bool IsSeeingPlayer()
    {
        if (los.Sigth(player, losDistance, losAngle, losWalls) == true)
        {

            return true;
        }
        else 
        { 

            return false; 
        }
    }
    
    private bool IsInRange()
    {
        { return false; }
    }


    private void Attack() 
    {

    }
    private void Chasing()
    {
        Vector3 dir = player.transform.position - transform.position;
        dir.y = 0f;

        if (dir.magnitude > 0.1f)
        {
            Vector3 moveDir = dir.normalized;

            transform.position += moveDir * chasingSpeed * Time.deltaTime;
            transform.forward = moveDir;
        }
    }
    private void Rest()
    {
        if (stamina < maxStamina)
        {
            stamina += Time.deltaTime * 2;
            Debug.Log("Healing");
        }
    }
    private void Patrol()
    {
        if (wayPoints == null || wayPoints.Count == 0) 
        {
            Debug.Log("No hay waypoints");
            return; 
        }
        if (wayPoints[currentWaypointIndex] == null) return;

        Transform currentWaypoint = wayPoints[currentWaypointIndex];

        Vector3 direction = currentWaypoint.position - transform.position;
        direction.y = 0f;

        if (direction.magnitude <= minDistanceToWaypoint)
        {
            currentWaypointIndex++;

            if (currentWaypointIndex >= wayPoints.Count)
            {
                currentWaypointIndex = 0;
            }

            return;
        }

        Vector3 moveDirection = direction.normalized;
        transform.position += moveDirection * patrolSpeed * Time.deltaTime;

        if (moveDirection != Vector3.zero)
        {
            transform.forward = moveDirection;
        }
    }
    private void RunAway()
    {

    }
}
