using UnityEngine;

public class SafeZoneFlocking : MonoBehaviour
{
    [SerializeField] private Vector3 boxSize = new Vector3(5f, 2f, 5f);
    [SerializeField] private LayerMask agentMask;
    [SerializeField] private WinCondition levelManager;


    private void Update()
    {
        Collider[] colliders = Physics.OverlapBox(
            transform.position,
            boxSize * 0.5f,
            transform.rotation,
            agentMask,
            QueryTriggerInteraction.Collide
        );

        foreach (Collider col in colliders)
        {
            FlockingAgent agent = col.GetComponentInParent<FlockingAgent>();

            if (agent == null)
                continue;

            levelManager.addSavedNPC();
            agent.Rescue();
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;

        Matrix4x4 oldMatrix = Gizmos.matrix;

        Gizmos.matrix = Matrix4x4.TRS(
            transform.position,
            transform.rotation,
            Vector3.one
        );

        Gizmos.DrawWireCube(Vector3.zero, boxSize);

        Gizmos.matrix = oldMatrix;
    }
}