using UnityEngine;

public class PatrolState : State
{
    public PatrolState(FSMClasses fsm) : base(fsm)
    {

    }

    public override void Enter()
    {
        Debug.Log("Entering Patrol State");
    }

    public override void Update (bool canSeePlayer)
    {
        fsm.enemy.Wander();
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
