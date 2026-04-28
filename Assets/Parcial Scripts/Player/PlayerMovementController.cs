using UnityEngine;
using UnityEngine.InputSystem;

// Obligo a que este GameObject tenga un CharacterController.
// Si no lo tiene, Unity se lo agrega automáticamente.
[RequireComponent(typeof(CharacterController))]
public class PlayerMovementController : MonoBehaviour
{
    // Velocidad de movimiento del player.
    [SerializeField] private float speed = 10f;

    // Gravedad manual, porque CharacterController no usa física como un Rigidbody.
    [SerializeField] private float gravity = -9.81f;

    [Header("Power Up")]

    // Booleano que indica si el jugador está actualmente con el power up activo.
    [SerializeField] private bool isPowerUpped;

    // Duración total del power up en segundos.
    [SerializeField] private float powerUpDuration = 10f;

    // Timer interno que va bajando mientras el power up está activo.
    private float powerUpTimer;

    [Header("UI")]

    // Canvas que se activa mientras dura el power up.
    [SerializeField] private GameObject powerUpCanvas;

    // Referencia al CharacterController del player.
    private CharacterController controller;

    // Guarda el input de movimiento que viene del Input System.
    private Vector2 moveInput;

    // Velocidad vertical usada para aplicar gravedad.
    private Vector3 velocity;

    // Propiedad pública para que otros scripts puedan saber si el power up está activo,
    // pero sin poder modificar directamente la variable privada.
    public bool IsPowerUpped => isPowerUpped;

    private void Awake()
    {
        // Busco el CharacterController en este mismo GameObject.
        controller = GetComponent<CharacterController>();

        // Al iniciar, apago el canvas del power up para que no aparezca activo.
        if (powerUpCanvas != null)
        {
            powerUpCanvas.SetActive(false);
        }
    }

    private void Update()
    {
        // Muevo al jugador cada frame.
        MovePlayer();

        // Actualizo el timer del power up si está activo.
        UpdatePowerUpTimer();
    }

    private void MovePlayer()
    {
        // Convierto el input 2D en movimiento 3D.
        // X mueve horizontal y Y del input lo uso como Z en el mundo.
        Vector3 move = new Vector3(moveInput.x, 0f, moveInput.y);

        // Muevo al player usando CharacterController.
        controller.Move(move * speed * Time.deltaTime);

        // Si está tocando el piso y la velocidad vertical es negativa,
        // dejo un valor bajo para mantenerlo pegado al suelo.
        if (controller.isGrounded && velocity.y < 0f)
        {
            velocity.y = -2f;
        }

        // Aplico gravedad acumulando velocidad vertical.
        velocity.y += gravity * Time.deltaTime;

        // Aplico el movimiento vertical por gravedad.
        controller.Move(velocity * Time.deltaTime);
    }

    private void UpdatePowerUpTimer()
    {
        // Si el power up no está activo, no hago nada.
        if (!isPowerUpped)
        {
            return;
        }

        // Mientras esté activo, resto tiempo al timer.
        powerUpTimer -= Time.deltaTime;

        // Cuando el timer llega a 0, desactivo el power up.
        if (powerUpTimer <= 0f)
        {
            DeactivatePowerUp();
        }
    }

    public void ActivatePowerUp()
    {
        // Activo el estado de power up.
        isPowerUpped = true;

        // Reinicio el timer a la duración total.
        // Esto también sirve si agarro otro power up mientras ya estaba activo.
        powerUpTimer = powerUpDuration;

        // Activo el canvas para mostrar visualmente que tengo el power up.
        if (powerUpCanvas != null)
        {
            powerUpCanvas.SetActive(true);
        }
    }

    public void DeactivatePowerUp()
    {
        // Apago el estado de power up.
        isPowerUpped = false;

        // Reseteo el timer.
        powerUpTimer = 0f;

        // Apago el canvas del power up.
        if (powerUpCanvas != null)
        {
            powerUpCanvas.SetActive(false);
        }
    }

    public void OnMove1(InputValue value)
    {
        // Recibo el input del nuevo Input System y lo guardo.
        // Después se usa en MovePlayer().
        moveInput = value.Get<Vector2>();
    }
}