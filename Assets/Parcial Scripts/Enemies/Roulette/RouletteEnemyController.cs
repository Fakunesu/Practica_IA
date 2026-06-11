using System.Collections.Generic;
using UnityEngine;

// Hereda del enemigo FSM normal, pero cambia la forma de patrullar.
public class RouletteEnemyController : EnemyControllerFSM
{
    [Header("Roulette Waypoints")]
    [SerializeField] private Transform[] rouletteWaypoints;
    [SerializeField] private float rouletteWaypointThreshold = 0.5f;

    [Header("Waypoint Chances")]
    [SerializeField] private float waypoint1Chance = 40f;
    [SerializeField] private float waypoint2Chance = 30f;
    [SerializeField] private float waypoint3Chance = 20f;
    [SerializeField] private float waypoint4Chance = 10f;

    private Transform currentWaypoint;
    private RouletteEnemyActions currentWaypointAction;

    protected override void Start()
    {
        base.Start();

        ChooseRandomWaypoint();
    }

    // Sobrescribe la patrulla del enemigo base para usar Roulette Wheel Selection.
    public override void PatrolWaypoints()
    {
        if (rouletteWaypoints == null || rouletteWaypoints.Length < 4)
        {
            StopMoving();
            return;
        }

        if (currentWaypoint == null)
        {
            ChooseRandomWaypoint();
        }

        Vector3 direction = SteeringBehaviour.Seek(transform, currentWaypoint.position);
        SetDirection(direction);

        float distance = Vector3.Distance(transform.position, currentWaypoint.position);

        // Cuando llega al waypoint, vuelve a elegir otro con la ruleta.
        if (distance <= rouletteWaypointThreshold)
        {
            ChooseRandomWaypoint();
        }
    }

    private void ChooseRandomWaypoint()
    {
        // Cada waypoint tiene un peso distinto.
        Dictionary<RouletteEnemyActions, float> waypointChances = new Dictionary<RouletteEnemyActions, float>();

        waypointChances.Add(RouletteEnemyActions.Waypoint1, waypoint1Chance);
        waypointChances.Add(RouletteEnemyActions.Waypoint2, waypoint2Chance);
        waypointChances.Add(RouletteEnemyActions.Waypoint3, waypoint3Chance);
        waypointChances.Add(RouletteEnemyActions.Waypoint4, waypoint4Chance);

        currentWaypointAction = MyRandom.RouletteWheelSelection(waypointChances);
        currentWaypoint = GetWaypointFromAction(currentWaypointAction);
    }

    // Convierte el resultado de la ruleta en un Transform del array.
    private Transform GetWaypointFromAction(RouletteEnemyActions action)
    {
        switch (action)
        {
            case RouletteEnemyActions.Waypoint1:
                return rouletteWaypoints[0];

            case RouletteEnemyActions.Waypoint2:
                return rouletteWaypoints[1];

            case RouletteEnemyActions.Waypoint3:
                return rouletteWaypoints[2];

            case RouletteEnemyActions.Waypoint4:
                return rouletteWaypoints[3];

            default:
                return rouletteWaypoints[0];
        }
    }
}