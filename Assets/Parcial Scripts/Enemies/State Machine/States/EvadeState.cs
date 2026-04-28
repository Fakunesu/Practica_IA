using UnityEngine;

public class EvadeState : State
{
    public EvadeState (FSMClasses fsm) : base (fsm)
    {
        this.fsm = fsm;
    }

    public override void Enter()
    {

    }

    public override void Update(bool canSeePlayer)
    {
        fsm.enemy.Flee();

        if(!fsm.enemy.ShouldEvade())
        {
            if(canSeePlayer)
            {
                fsm.ToPursuit();
            }
            else
            {
                fsm.ToPatrol();
            }
        }

    }

    public override void Exit()
    {

    }
}
