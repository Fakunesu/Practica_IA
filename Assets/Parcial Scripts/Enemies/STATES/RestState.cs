using UnityEngine;

public class RestState : State
{
    public RestState(FSMClasses fsm) : base(fsm) 
    {

    }

    public override void Enter()
    {
        Debug.Log("Entering Rest State");
        fsm.enemy.StopMoving();
    }

    public override void Update(bool canSeePlayer)
    {
        fsm.enemy.StopMoving();
        fsm.enemy.RegenerateStamina();

        if(fsm.enemy.IsStaminaFull)
        {
            fsm.ToPatrol();
        }
    }
}
