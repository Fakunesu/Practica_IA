using UnityEngine;

public class PatrolState : State
{
    public PatrolState(FSMClasses fsm) : base(fsm)
    {

    }

    public override void Enter()
    {
        fsm.enemy.StartPatrolPath();
    }

    public override void Update(bool canSeePlayer)
    {
        if (!fsm.enemy.HasStamina)
        {
            fsm.ToRest();
            return;
        }

        fsm.enemy.PatrolWaypoints();
        fsm.enemy.DrainStamina();

        if (canSeePlayer)
        {
            fsm.ToPursuit();
        }
    }

    public override void Exit()
    {
        fsm.enemy.StopMoving();
    }
}