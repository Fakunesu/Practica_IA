using System.Collections.Generic;
using UnityEngine;

// Este enemigo hereda de EnemyControllerFSM.
// Eso significa que mantiene toda la lógica del enemigo base:
// perseguir, escapar, atacar, usar la FSM, etc.
// Lo único que cambio en este script es cómo patrulla.
public class RouletteEnemyController : EnemyControllerFSM
{
    [Header("Roulette Waypoints")]

    // Array donde guardo los 4 waypoints posibles.
    // Estos puntos los asigno desde el Inspector.
    [SerializeField] private Transform[] rouletteWaypoints;

    // Distancia mínima para considerar que el enemigo llegó al waypoint.
    [SerializeField] private float rouletteWaypointThreshold = 0.5f;

    [Header("Waypoint Chances")]

    // Probabilidades de cada waypoint.
    // No son todas iguales, por eso funciona como Roulette Wheel Selection.
    [SerializeField] private float waypoint1Chance = 40f;
    [SerializeField] private float waypoint2Chance = 30f;
    [SerializeField] private float waypoint3Chance = 20f;
    [SerializeField] private float waypoint4Chance = 10f;

    // Waypoint al que el enemigo está yendo actualmente.
    private Transform currentWaypoint;

    // Guarda qué resultado salió en la ruleta.
    private RouletteEnemyActions currentWaypointAction;

    // Uso override porque el EnemyControllerFSM tiene un Start virtual.
    // Primero llamo al Start del padre para que se inicialicen las referencias del enemigo base.
    protected override void Start()
    {
        base.Start();

        // Al empezar, elijo el primer waypoint usando la ruleta.
        ChooseRandomWaypoint();
    }

    // Sobrescribo la patrulla del enemigo base.
    // La FSM va a llamar a PatrolWaypoints igual que siempre,
    // pero en este enemigo se ejecuta esta versión con ruleta.
    public override void PatrolWaypoints()
    {
        // Si no tengo 4 waypoints asignados, freno al enemigo para evitar errores.
        if (rouletteWaypoints == null || rouletteWaypoints.Length < 4)
        {
            StopMoving();
            return;
        }

        // Si todavía no hay un waypoint elegido, elijo uno con la ruleta.
        if (currentWaypoint == null)
        {
            ChooseRandomWaypoint();
        }

        // Calculo la dirección hacia el waypoint actual usando el mismo steering del enemigo base.
        Vector3 direction = SteeringBehaviour.Seek(transform, currentWaypoint.position);

        // Como la variable dir está en el EnemyControllerFSM,
        // uso este método para pasarle la dirección nueva.
        SetDirection(direction);

        // Calculo la distancia entre el enemigo y el waypoint actual.
        float distance = Vector3.Distance(transform.position, currentWaypoint.position);

        // Si ya llegué al waypoint, vuelvo a tirar la ruleta
        // para elegir el próximo punto al que va a ir.
        if (distance <= rouletteWaypointThreshold)
        {
            ChooseRandomWaypoint();
        }
    }

    // Este método se encarga de armar la ruleta y elegir un waypoint.
    private void ChooseRandomWaypoint()
    {
        // Creo un diccionario donde cada opción tiene una probabilidad/peso.
        Dictionary<RouletteEnemyActions, float> waypointChances = new Dictionary<RouletteEnemyActions, float>();

        // Agrego los 4 posibles resultados con sus chances.
        // Ejemplo: Waypoint1 tiene más chances que Waypoint4.
        waypointChances.Add(RouletteEnemyActions.Waypoint1, waypoint1Chance);
        waypointChances.Add(RouletteEnemyActions.Waypoint2, waypoint2Chance);
        waypointChances.Add(RouletteEnemyActions.Waypoint3, waypoint3Chance);
        waypointChances.Add(RouletteEnemyActions.Waypoint4, waypoint4Chance);

        // Uso la función de Roulette Wheel Selection para elegir una opción
        // según las probabilidades que cargué en el diccionario.
        currentWaypointAction = MyRandom.RouletteWheelSelection(waypointChances);

        // Convierto el resultado de la ruleta en un Transform real del array.
        currentWaypoint = GetWaypointFromAction(currentWaypointAction);

        // Debug para ver en consola qué waypoint eligió.
        Debug.Log("Roulette waypoint selected: " + currentWaypointAction);
    }

    // Este método transforma el resultado de la ruleta en un waypoint concreto.
    private Transform GetWaypointFromAction(RouletteEnemyActions action)
    {
        // Según qué acción salió, devuelvo un elemento distinto del array.
        switch (action)
        {
            case RouletteEnemyActions.Waypoint1:
                return rouletteWaypoints[0];

            case RouletteEnemyActions.Waypoint2:
                return rouletteWaypoints[1];

            case RouletteEnemyActions.Waypoint3:
                return rouletteWaypoints[2];

            case RouletteEnemyActions.Waypoint4:
                return rouletteWaypoints[3];

            // Si por alguna razón no coincide con ninguno,
            // uso el primer waypoint como valor por defecto.
            default:
                return rouletteWaypoints[0];
        }
    }
}