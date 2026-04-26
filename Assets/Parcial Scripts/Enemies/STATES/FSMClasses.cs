using UnityEngine;

public class FSMClasses : MonoBehaviour
{
    private State currentState;

    private PatrolState patrolState;
    private PursuitState pursuitState;
    private FreezeState freezeState;
    private AttackState attackState;

    public EnemyControllerFSM enemy;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Awake()
    {
        enemy = GetComponent<EnemyControllerFSM>();

        patrolState = new PatrolState(this);
        pursuitState = new PursuitState(this);
        freezeState = new FreezeState(this);
        attackState = new AttackState(this);

        currentState = patrolState;
        currentState.Enter();
    }

    // Update is called once per frame
    public void UpdateState(bool canSeePlayer)
    {
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


}