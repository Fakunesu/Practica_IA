using UnityEngine;

public class PatrolState : State
{
    public PatrolState(FSMClasses fsm) : base(fsm)
    {

    }

    public override void Enter()
    {

    }

    public override void Update (bool canSeePlayer)
    {
        fsm.enemy.PatrolWaypoints();
        fsm.enemy.DrainStamina();

        if (!fsm.enemy.HasStamina)
        {
            fsm.ToRest();
            return;
        }

        if (canSeePlayer)
        {
            fsm.ToPursuit();
        }
    }   
}
