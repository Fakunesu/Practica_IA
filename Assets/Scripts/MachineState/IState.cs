using UnityEngine;

public interface IState 
{
    public void Awake();
    public void Execute();
    public void Sleep();
}
