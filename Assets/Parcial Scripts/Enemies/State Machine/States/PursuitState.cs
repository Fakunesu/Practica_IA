using UnityEngine;

public class PursuitState : State
{
    public PursuitState(FSMClasses fsm) : base(fsm)
    {

    }

    public override void Enter()
    {
        fsm.enemy.SetColor(Color.red);
    }

    public override void Update(bool canSeePlayer)
    {
        fsm.enemy.Seek();

        float distance = Vector3.Distance(fsm.enemy.transform.position, fsm.enemy.player.position);

        if (!canSeePlayer)
        {
            fsm.ToReturnToPatrol();
        }
        else if (distance < 2f)
        {
            fsm.ToAttack();
        }
        else if(distance < 4f)
        {
            fsm.ToFreeze();
        }
    }
    public override void Exit()
    {
        fsm.enemy.SetColor(Color.white);
    }
}
