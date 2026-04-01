using UnityEngine;

public class State<T> : IState
{

    protected StateMachine<T> _stateMachine;
    public State(StateMachine<T> stateMachine)
    {
        _stateMachine = stateMachine;
    }

    // Update is called once per frame
    public virtual void Awake()
    {
        Debug.Log("Entered " + GetType());
    }

    public virtual void Sleep()
    {
        Debug.Log("Exited " + GetType());
    }

    public virtual void Execute()
    {

    }
}
