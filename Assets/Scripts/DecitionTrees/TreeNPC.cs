using System.Drawing;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class TreeNPC : MonoBehaviour
{
   // public LineOfSight _los;
    public Transform target;
    public Transform safePlace;

    public float stamina;
    public float speed;
    private bool recharging;
    public Transform[] wayPoints;
    private int currentWP = 0;
    private bool isNight;
    private bool isFullRested;
    private bool isInRangeOfWood;


    private ITreeeNode root;
    void Start()
    {
        ActionNode idle = new ActionNode(Idle);
        ActionNode patrol = new ActionNode(Patrol);
        ActionNode follow = new ActionNode(Follow);
        ActionNode rest = new ActionNode(Rest);
        ActionNode locateWood = new ActionNode(LocateWood);
        ActionNode collectWood = new ActionNode(CollectWood);

        QuestionNode hasStamina = new QuestionNode(HasStamina, patrol, rest);
        QuestionNode isNight = new QuestionNode(IsNight, rest, patrol);
        QuestionNode isFullyRested = new QuestionNode(IsFullyRested, patrol, rest);
        QuestionNode isInRangeOfWood = new QuestionNode(IsInRangeOfWood, collectWood, locateWood);

        root = hasStamina;
    }

   
    void Update()
    {
        root.Execute();
        ManageStamina();
        if (recharging)
        {
            Rest();
            
        }
        else
        {
            Patrol();
        }

    }

   // private bool IsInLos() => _los.CheckRange(target) && _los.CheckAngle(target) && _los.CheckView(target);

    private bool HasStamina() => recharging;

    private bool IsNight() => isNight;

    private bool IsFullyRested() => isFullRested;

    
    private void ManageStamina()
    {
        if(stamina < 0 && !recharging)
            recharging = true;
        else if(stamina >= 10 && recharging)
            recharging = false;
    }

    private void Idle()
    {
        stamina += Time.deltaTime;
    }
    private void Follow()
    {
        var dir = target.position - transform.position;
        transform.position += dir.normalized * speed * Time.deltaTime;
        stamina -= Time.deltaTime;

    }
    private void Patrol()
    {
        if (Vector3.Distance(transform.position, wayPoints[currentWP].position) <= 0.5f)
        {
            currentWP = (currentWP + 1) % wayPoints.Length;
        }
        var dir = wayPoints[currentWP].position - transform.position;
        transform.position += dir.normalized * speed * Time.deltaTime;
        stamina -= Time.deltaTime;

    }

    private void CollectWood()
    {
        if (target == null) return;
    }

    private bool IsInRangeOfWood()
    {
        if (target == null) return false;
        else
        {
            float dist = Vector3.Distance(transform.position, target.position);

            if (dist >= 2) return false;
            else return true;

        }


    }

    

    private void LocateWood()
    {
        Vector3 dir = target.position - transform.position;
        transform.position += dir.normalized*speed*Time.deltaTime;
    }

    private void Rest()
    {
        var dirToWaypoint = safePlace.position - transform.position;
        if (safePlace.position != transform.position) transform.position += dirToWaypoint.normalized * speed * Time.deltaTime;
        else Idle();
        
    }

}
