using UnityEngine;

public class EnemyTree : MonoBehaviour
{
    private LineOfSight los;
    private EnemyController controller;

    private ITreeeNode root;

    private void Awake()
    {
        los = GetComponent<LineOfSight>();
        controller = GetComponent<EnemyController>();
    }

    private void Start()
    {
        ActionNode attack = new ActionNode(Attack);
        ActionNode chasing = new ActionNode(Chasing);
        ActionNode runAway = new ActionNode(RunAway);
        ActionNode patrol = new ActionNode(Patrol);
        ActionNode rest = new ActionNode(Rest);

        // Armo el árbol de decisión desde las acciones finales hacia la raíz.
        QuestionNode isInRange = new QuestionNode(() => controller.IsInRange(), attack, chasing);
        QuestionNode isInDisadvantage = new QuestionNode(() => controller.IsInDisadvantage(), runAway, isInRange);
        QuestionNode isSeeingPlayer = new QuestionNode(() => controller.IsSeeingPlayer(), isInDisadvantage, patrol);
        QuestionNode hasStamina = new QuestionNode(() => controller.HasStamina(), isSeeingPlayer, rest);

        root = hasStamina;
    }

    private void Update()
    {
        if (root != null)
        {
            root.Execute();
        }
    }

    private void Attack()
    {
        controller.Attack();
    }

    private void Chasing()
    {
        controller.Seek();
    }

    private void Rest()
    {
        controller.Rest();
    }

    private void Patrol()
    {
        controller.PatrollingWaypoints();
    }

    private void RunAway()
    {
        controller.EvadePlayer();
    }
}