using UnityEngine;

// Maneja qué estado está activo en el enemigo.
public class FSMClasses : MonoBehaviour
{
    private State currentState;

    private PatrolState patrolState;
    private PursuitState pursuitState;
    private FreezeState freezeState;
    private AttackState attackState;
    private EvadeState evadeState;
    private RestState restState;

    // Controlador del enemigo. También funciona con RouletteEnemyController porque hereda de EnemyControllerFSM.
    public EnemyControllerFSM enemy;

    private void Awake()
    {
        enemy = GetComponent<EnemyControllerFSM>();

        // Creo todos los estados y les paso esta FSM.
        patrolState = new PatrolState(this);
        pursuitState = new PursuitState(this);
        freezeState = new FreezeState(this);
        attackState = new AttackState(this);
        evadeState = new EvadeState(this);
        restState = new RestState(this);

        currentState = patrolState;
        currentState.Enter();
    }

    public void UpdateState(bool canSeePlayer)
    {
        // Si el player tiene power up y está cerca, fuerza evasión.
        if (enemy.ShouldEvade())
        {
            ChangeState(evadeState);
        }

        currentState.Update(canSeePlayer);
    }

    public void ChangeState(State newState)
    {
        if (currentState == newState)
        {
            return;
        }

        currentState.Exit();
        currentState = newState;
        currentState.Enter();
    }

    public void ToPatrol()
    {
        ChangeState(patrolState);
    }

    public void ToPursuit()
    {
        ChangeState(pursuitState);
    }

    public void ToFreeze()
    {
        ChangeState(freezeState);
    }

    public void ToAttack()
    {
        ChangeState(attackState);
    }

    public void ToEvade()
    {
        ChangeState(evadeState);
    }

    public void ToRest()
    {
        ChangeState(restState);
    }
}