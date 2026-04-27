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
        fsm.enemy.Wander();

        if (canSeePlayer)
        {
            fsm.ToPursuit();
        }
    }   
}
