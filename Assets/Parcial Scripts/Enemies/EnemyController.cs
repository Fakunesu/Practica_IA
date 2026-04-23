using UnityEngine;

public class EnemyController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float mass = 1f;
    [SerializeField] private float maxSpeed = 5f;
    [SerializeField] private float maxForce = 15f;
    [SerializeField] private float rotationSpeed = 10f;


    [SerializeField] private float drag = 3f;

    private Vector3 velocity;

    public Vector3 Velocity => velocity;
    public float MaxSpeed => maxSpeed;
    public float MaxForce => maxForce;
    public float Mass => mass;

    public void ApplySteering(Vector3 steeringForce)
    {
        steeringForce.y = 0f;
        steeringForce = Vector3.ClampMagnitude(steeringForce, maxForce);

        Vector3 acceleration = steeringForce / Mathf.Max(mass, 0.0001f);
        velocity += acceleration * Time.deltaTime;
        velocity *= 1f / (1f + drag * Time.deltaTime);
        velocity = Vector3.ClampMagnitude(velocity, maxSpeed);

        transform.position += velocity * Time.deltaTime;

        RotateToVelocity();
    }

    public void StopSmoothly(float brakeStrength = 8f)
    {
        velocity = Vector3.Lerp(velocity, Vector3.zero, Time.deltaTime * brakeStrength);
        transform.position += velocity * Time.deltaTime;
        RotateToVelocity();
    }

    public void StopImmediately()
    {
        velocity = Vector3.zero;
    }

    private void RotateToVelocity()
    {
        Vector3 flatVelocity = velocity;
        flatVelocity.y = 0f;

        if (flatVelocity.sqrMagnitude <= 0.0001f)
        {
            return;
        }

        Quaternion targetRotation = Quaternion.LookRotation(flatVelocity.normalized);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
    }
}