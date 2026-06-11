// Clase base de todos los estados del enemigo.
public abstract class State
{
    // Referencia a la FSM para poder acceder al enemigo y cambiar estados.
    protected FSMClasses fsm;

    public State(FSMClasses fsm)
    {
        this.fsm = fsm;
    }

    // Se llama cuando entro al estado.
    public virtual void Enter()
    {

    }

    // Se llama cuando salgo del estado.
    public virtual void Exit()
    {

    }

    // Se llama mientras el estado está activo.
    public virtual void Update(bool canSeePlayer)
    {

    }
}