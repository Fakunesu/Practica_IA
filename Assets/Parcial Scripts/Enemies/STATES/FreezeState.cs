using UnityEngine;

public class FreezeState : State
{
    private float freezeTime = 1f;
    private float timer;

    public FreezeState(FSMClasses fsm) : base(fsm)
    {

    }

    public override void Enter()
    {
        timer = freezeTime;
        fsm.enemy.FreezePlayer(freezeTime);
        Debug.Log("Entering Freeze State");
    }

    public override void Update(bool canSeePlayer)
    {
        timer -= Time.deltaTime;  

        if (timer <= 0f)
        {
            fsm.ToPursuit();
        }
    }   
}
