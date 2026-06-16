using System.Collections.Generic;
using UnityEngine;

public class RouletteEnemyController : EnemyControllerFSM
{
    [Header("Roulette Waypoints")]
    [SerializeField] private Transform[] rouletteWaypoints;
    [SerializeField] private float rouletteWaypointThreshold = 1f;

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

        if (HasValidWaypoints())
        {
            ChooseRandomWaypoint();
            CalculatePathTo(currentWaypoint.position);
        }
    }

    public override void StartPatrolPath()
    {
        if (!HasValidWaypoints())
        {
            StopMoving();
            return;
        }

        if (currentWaypoint == null)
        {
            ChooseRandomWaypoint();
        }

        CalculatePathTo(currentWaypoint.position);
    }

    public override void PatrolWaypoints()
    {
        if (!HasValidWaypoints())
        {
            StopMoving();
            return;
        }

        if (currentWaypoint == null)
        {
            ChooseRandomWaypoint();
            CalculatePathTo(currentWaypoint.position);
            return;
        }

        MoveThroughCurrentPath();

        float distanceToWaypoint = Vector3.Distance(
            transform.position,
            currentWaypoint.position
        );

        if (distanceToWaypoint <= rouletteWaypointThreshold)
        {
            StopMoving();

            ChooseRandomWaypoint();
            CalculatePathTo(currentWaypoint.position);
        }
    }

    private bool HasValidWaypoints()
    {
        if (rouletteWaypoints == null || rouletteWaypoints.Length < 4)
            return false;

        for (int i = 0; i < 4; i++)
        {
            if (rouletteWaypoints[i] == null)
                return false;
        }

        return true;
    }

    private void ChooseRandomWaypoint()
    {
        Dictionary<RouletteEnemyActions, float> waypointChances = new()
        {
            { RouletteEnemyActions.Waypoint1, waypoint1Chance },
            { RouletteEnemyActions.Waypoint2, waypoint2Chance },
            { RouletteEnemyActions.Waypoint3, waypoint3Chance },
            { RouletteEnemyActions.Waypoint4, waypoint4Chance }
        };

        currentWaypointAction = MyRandom.RouletteWheelSelection(waypointChances);
        currentWaypoint = GetWaypointFromAction(currentWaypointAction);

        UpdateChancesAfterChoosing(currentWaypointAction);

        Debug.Log("Nuevo waypoint roulette elegido: " + currentWaypointAction);
    }

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

    private void UpdateChancesAfterChoosing(RouletteEnemyActions action)
    {
        switch (action)
        {
            case RouletteEnemyActions.Waypoint1:
                waypoint1Chance = 0f;
                waypoint2Chance = 40f;
                waypoint3Chance = 20f;
                waypoint4Chance = 30f;
                break;

            case RouletteEnemyActions.Waypoint2:
                waypoint1Chance = 40f;
                waypoint2Chance = 0f;
                waypoint3Chance = 20f;
                waypoint4Chance = 30f;
                break;

            case RouletteEnemyActions.Waypoint3:
                waypoint1Chance = 30f;
                waypoint2Chance = 40f;
                waypoint3Chance = 0f;
                waypoint4Chance = 30f;
                break;

            case RouletteEnemyActions.Waypoint4:
                waypoint1Chance = 30f;
                waypoint2Chance = 10f;
                waypoint3Chance = 40f;
                waypoint4Chance = 0f;
                break;
        }
    }
}