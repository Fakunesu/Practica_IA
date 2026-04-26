public abstract class State
{
    protected FSMClasses fsm;
    public State(FSMClasses fsm)
    {
        this.fsm = fsm;
    }
    public virtual void Enter()
    {
        
    }
    public virtual void Exit()
    {
        
    }
    public virtual void Update(bool canSeePlayer)
    {
        
    }
}