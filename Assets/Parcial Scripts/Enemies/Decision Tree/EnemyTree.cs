using NUnit.Framework;
using System.Collections.Generic;
using System.ComponentModel.Design.Serialization;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class EnemyTree : MonoBehaviour
{
    private LineOfSight los;
    private EnemyController controller;

    [Header("Chase")]
    [SerializeField] private GameObject player;
    [SerializeField] private float losDistance;
    [SerializeField] private float losAngle;
    [SerializeField] private LayerMask losWalls;
    [SerializeField] private float chasingSpeed = 6f;


    private ITreeeNode root;

    private void Awake()
    {
        los = GetComponent<LineOfSight>();
        controller = GetComponent<EnemyController>();
    }

    void Start()
    {
        ActionNode attack = new ActionNode(Attack);//falta
        ActionNode chasing = new ActionNode(Chasing);//esta
        ActionNode runAway = new ActionNode(RunAway);//esta
        ActionNode patrol = new ActionNode(Patrol);//esta
        ActionNode rest = new ActionNode(Rest);//esta

        QuestionNode isInRange = new QuestionNode(()=>controller.IsInRange(), attack, chasing);//falta
        QuestionNode isInDisadvantage = new QuestionNode(()=>controller.IsInDisadvantage(), runAway, isInRange);//esta mal
        QuestionNode isSeeingPlayer = new QuestionNode(() => controller.IsSeeingPlayer(), isInDisadvantage, patrol);//esta
        QuestionNode hasStamina = new QuestionNode(() => controller.HasStamina(), isSeeingPlayer, rest);//esta

        root = hasStamina;
        
    }

    void Update()
    {
        if (root != null)
        {
            root.Execute();
        }
    }


    /*private bool IsInDisadvantage()
    {
        
    }*/

   /* private bool IsSeeingPlayer()
    {
        if (los.Sigth(player, losDistance, losAngle, losWalls) == true)
        {

            return true;
        }
        else 
        { 

            return false; 
        }
    }*/
    
    //private bool IsInRange()
    //{
    //    Debug.Log("is in range");
    //    return true;
    //}


    private void Attack() 
    {
        controller.Attack();
    }
    private void Chasing()
    {
        controller.Seek();
    }
    private void Rest()
    {
        controller.Rest();
    }

    private void Patrol()
    {
        controller.PatrollingWaypoints();
    }
    private void RunAway()
    {
        controller.EvadePlayer();
        Debug.Log("corre");
    }
}
