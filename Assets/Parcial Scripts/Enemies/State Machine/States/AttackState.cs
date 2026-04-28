using Unity.VisualScripting.FullSerializer;
using UnityEngine;

public class AttackState : State
{
    public AttackState(FSMClasses fsm) : base(fsm)
    {

    }
    
    public override void Enter()
    {
        fsm.enemy.Attack();
    }

    public override void Exit() 
    {
        fsm.enemy.StopAttack();
    }
    
    public override void Update(bool canSeePlayer)
    {
        float distance = Vector3.Distance(fsm.enemy.transform.position, fsm.enemy.player.position);
        
        if (distance > 2f)
        {
            fsm.ToPursuit();
        }
    }
}
