using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovementController : MonoBehaviour
{
    [SerializeField] private float speed = 10f;
    [SerializeField] private float gravity = -9.81f;

    [SerializeField]public bool isPowerUpped;
    private CharacterController controller;
    private Vector2 moveInput;
    private Vector3 velocity;

    //Encapsulamiento
    public bool IsPowerUpped => IsPowerUpped;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();

    }

    private void Update()
    {
        Vector3 move = new Vector3(moveInput.x, 0f, moveInput.y);
        controller.Move(move * speed * Time.deltaTime);

        if (controller.isGrounded && velocity.y < 0f)
        {
            velocity.y = -2f;
        }

        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("PowerUp"))
        {
            isPowerUpped = true;
        }
    }
    public void OnMove1(InputValue value)
    {
        moveInput = value.Get<Vector2>();
    }
}