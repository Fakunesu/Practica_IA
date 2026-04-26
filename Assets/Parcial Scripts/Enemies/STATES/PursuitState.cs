using UnityEngine;

public class PursuitState : State
{
    public PursuitState(FSMClasses fsm) : base(fsm)
    {

    }

    public override void Enter()
    {
        Debug.Log("Entering Pursuit State");
    }

    public override void Update(bool canSeePlayer)
    {
        fsm.enemy.Seek();

        float distance = Vector3.Distance(fsm.enemy.transform.position, fsm.enemy.player.position);

        if (!canSeePlayer)
        {
            fsm.ToPatrol();
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
}
