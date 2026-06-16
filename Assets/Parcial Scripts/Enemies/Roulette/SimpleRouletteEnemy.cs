using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SimpleRouletteEnemy : MonoBehaviour
{
    [Header("Waypoints")]
    [SerializeField] private Transform[] waypoints = new Transform[4];
    [SerializeField] private float waypointReachDistance = 0.5f;

    [Header("Movement")]
    [SerializeField] private float speed = 3f;
    [SerializeField] private float rotationSpeed = 10f;

    [Header("Initial Weights")]
    [SerializeField] private float waypoint1Weight = 40f;
    [SerializeField] private float waypoint2Weight = 30f;
    [SerializeField] private float waypoint3Weight = 20f;
    [SerializeField] private float waypoint4Weight = 10f;

    private int currentWaypointIndex = -1;
    private Transform currentWaypoint;

    private void Start()
    {
        if (!HasValidWaypoints())
        {
            Debug.LogWarning("SimpleRouletteEnemy: faltan waypoints.");
            return;
        }

        ChooseNextWaypoint();
    }

    private void Update()
    {
        if (currentWaypoint == null)
            return;

        MoveToCurrentWaypoint();

        Vector3 enemyPosition = transform.position;
        Vector3 waypointPosition = currentWaypoint.position;

        enemyPosition.y = 0f;
        waypointPosition.y = 0f;

        float distanceToWaypoint = Vector3.Distance(
            enemyPosition,
            waypointPosition
        );

        if (distanceToWaypoint <= waypointReachDistance)
        {
            ChooseNextWaypoint();
        }
    }

    private void MoveToCurrentWaypoint()
    {
        Vector3 targetPosition = currentWaypoint.position;
        targetPosition.y = transform.position.y;

        Vector3 direction = targetPosition - transform.position;

        if (direction.sqrMagnitude <= 0.001f)
            return;

        Vector3 moveDirection = direction.normalized;

        transform.position += moveDirection * speed * Time.deltaTime;

        if (moveDirection.sqrMagnitude > 0.001f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);

            transform.rotation = Quaternion.Lerp(
                transform.rotation,
                targetRotation,
                rotationSpeed * Time.deltaTime
            );
        }
    }

    private void ChooseNextWaypoint()
    {
        Dictionary<int, float> waypointWeights = new Dictionary<int, float>()
        {
            { 0, waypoint1Weight },
            { 1, waypoint2Weight },
            { 2, waypoint3Weight },
            { 3, waypoint4Weight }
        };

        int selectedWaypointIndex = RouletteWheelSelection(waypointWeights);

        currentWaypointIndex = selectedWaypointIndex;
        currentWaypoint = waypoints[currentWaypointIndex];

        UpdateDynamicWeights(currentWaypointIndex);

        Debug.Log(
            "Nuevo waypoint elegido: " +
            (currentWaypointIndex + 1) +
            " | Pesos: " +
            waypoint1Weight + ", " +
            waypoint2Weight + ", " +
            waypoint3Weight + ", " +
            waypoint4Weight
        );
    }

    private int RouletteWheelSelection(Dictionary<int, float> weights)
    {
        float totalWeight = 0f;

        foreach (float weight in weights.Values)
        {
            totalWeight += weight;
        }

        float randomValue = Random.Range(0f, totalWeight);
        float currentSum = 0f;

        foreach (KeyValuePair<int, float> item in weights)
        {
            currentSum += item.Value;

            if (randomValue <= currentSum)
            {
                return item.Key;
            }
        }

        return 0;
    }

    private void UpdateDynamicWeights(int selectedIndex)
    {
        switch (selectedIndex)
        {
            case 0:
                waypoint1Weight = 0f;
                waypoint2Weight = 40f;
                waypoint3Weight = 30f;
                waypoint4Weight = 30f;
                break;

            case 1:
                waypoint1Weight = 35f;
                waypoint2Weight = 0f;
                waypoint3Weight = 35f;
                waypoint4Weight = 30f;
                break;

            case 2:
                waypoint1Weight = 25f;
                waypoint2Weight = 45f;
                waypoint3Weight = 0f;
                waypoint4Weight = 30f;
                break;

            case 3:
                waypoint1Weight = 40f;
                waypoint2Weight = 30f;
                waypoint3Weight = 30f;
                waypoint4Weight = 0f;
                break;
        }
    }

    private bool HasValidWaypoints()
    {
        if (waypoints == null || waypoints.Length < 4)
            return false;

        for (int i = 0; i < 4; i++)
        {
            if (waypoints[i] == null)
                return false;
        }

        return true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            RestartScene();
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            RestartScene();
        }
    }

    private void RestartScene()
    {
        Scene currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currentScene.name);
    }
}