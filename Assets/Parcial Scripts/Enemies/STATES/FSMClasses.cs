using UnityEngine;

// Esta clase maneja la máquina de estados del enemigo.
// No mueve al enemigo directamente, sino que decide qué estado está activo.
public class FSMClasses : MonoBehaviour
{
    // Estado actual del enemigo.
    private State currentState;

    // Instancias de todos los estados posibles.
    private PatrolState patrolState;
    private PursuitState pursuitState;
    private FreezeState freezeState;
    private AttackState attackState;
    private EvadeState evadeState;
    private RestState restState;

    // Referencia al controlador del enemigo.
    // Como RouletteEnemyController hereda de EnemyControllerFSM,
    // también sirve para enemigos normales y enemigos roulette.
    public EnemyControllerFSM enemy;

    private void Awake()
    {
        // Busco el EnemyControllerFSM en este mismo GameObject.
        // Si el enemigo es RouletteEnemyController también funciona,
        // porque RouletteEnemyController hereda de EnemyControllerFSM.
        enemy = GetComponent<EnemyControllerFSM>();

        // Creo cada estado y le paso esta FSM como referencia.
        // Así cada estado puede pedir cambios, por ejemplo ToPatrol o ToAttack.
        patrolState = new PatrolState(this);
        pursuitState = new PursuitState(this);
        freezeState = new FreezeState(this);
        attackState = new AttackState(this);
        evadeState = new EvadeState(this);
        restState = new RestState(this);

        // El enemigo empieza patrullando.
        currentState = patrolState;

        // Llamo al Enter del estado inicial.
        currentState.Enter();
    }

    public void UpdateState(bool canSeePlayer)
    {
        // Antes de actualizar el estado actual, reviso si el enemigo debería evadir.
        // Esto pasa cuando el player está cerca y tiene el power up.
        if (enemy.ShouldEvade())
        {
            ChangeState(evadeState);
        }

        // Actualizo el estado actual.
        // Le paso si puede ver al player para que cada estado decida qué hacer.
        currentState.Update(canSeePlayer);
    }

    public void ChangeState(State newState)
    {
        // Si ya estoy en ese estado, no hago nada.
        // Esto evita salir y entrar al mismo estado todo el tiempo.
        if (currentState == newState)
        {
            return;
        }

        // Llamo al Exit del estado actual antes de cambiar.
        currentState.Exit();

        // Cambio la referencia al nuevo estado.
        currentState = newState;

        // Llamo al Enter del nuevo estado.
        currentState.Enter();
    }

    public void ToPatrol()
    {
        // Cambio al estado de patrulla.
        ChangeState(patrolState);
    }

    public void ToPursuit()
    {
        // Cambio al estado de persecución.
        ChangeState(pursuitState);
    }

    public void ToFreeze()
    {
        // Cambio al estado que congela al player.
        ChangeState(freezeState);
    }

    public void ToAttack()
    {
        // Cambio al estado de ataque.
        ChangeState(attackState);
    }

    public void ToEvade()
    {
        // Cambio al estado de evasión.
        ChangeState(evadeState);
    }

    public void ToRest()
    {
        // Cambio al estado de descanso.
        ChangeState(restState);
    }
}