using UnityEngine;

public class ReturnToPatrolState : State
{
    public ReturnToPatrolState(FSMClasses fsm) : base(fsm)
    {
    }

    public override void Enter()
    {
        fsm.enemy.SetColor(Color.green);

        fsm.enemy.UseReturnToPatrolSpeed();


        fsm.enemy.CalculatePathToCurrentPatrolWaypoint();
    }

    public override void Update(bool canSeePlayer)
    {

        if (canSeePlayer)
        {
            fsm.ToPursuit();
            return;
        }

        bool reachedDestination = fsm.enemy.MoveThroughCurrentPath();

        if (reachedDestination)
        {
            fsm.ToPatrol();
        }
    }

    public override void Exit()
    {

        fsm.enemy.ResetSpeed();

        fsm.enemy.SetColor(Color.white);
        fsm.enemy.StopMoving();
    }
}