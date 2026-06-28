using UnityEngine;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(Animator))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;
    public float runSpeed = 8f;
    public float jumpHeight = 1.5f;
    public float gravity = -9.81f;

    [Header("Camera")]
    public float mouseSensitivity = 2f;
    public Transform playerCamera;
    private float xRot = 0f;

    private CharacterController controller;
    private Animator anim;
    private Vector3 velocity;

    /// <summary>
    /// True cuando el jugador está corriendo (Shift pulsado y moviéndose).
    /// Usado por la IA de los enemigos para detección auditiva.
    /// </summary>
    public bool IsRunning => Input.GetKey(KeyCode.LeftShift) && controller != null && controller.velocity.magnitude > 0.1f;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        anim = GetComponent<Animator>();
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void Update()
    {
        if (GameManager.Instance != null && GameManager.Instance.currentState != GameManager.GameState.Playing) return;
        HandleMovement();
    }

    private void HandleMovement()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        transform.Rotate(Vector3.up * mouseX);

        if (playerCamera != null)
        {
            xRot -= mouseY;
            xRot = Mathf.Clamp(xRot, -80f, 80f);
            playerCamera.localRotation = Quaternion.Euler(xRot, 0f, 0f);
        }

        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        Vector3 move = transform.right * x + transform.forward * z;
        // Si el vector tiene magnitud mayor a 1 (diagonales), lo normalizamos para no ir más rápido en diagonal
        if (move.magnitude > 1f) move.Normalize();

        float currentSpeed = Input.GetKey(KeyCode.LeftShift) ? runSpeed : moveSpeed;

        // SOLUCIÓN DEFINITIVA SALTO: Con el CharacterController, para que isGrounded devuelva True,
        // el personaje debe estar moviéndose constantemente contra el suelo.
        // Combinamos el movimiento horizontal y el salto en un único controller.Move()
        // porque hacer dos llamadas en el mismo frame resetea la propiedad isGrounded.
        bool isGrounded = controller.isGrounded;

        if (isGrounded)
        {
            velocity.y = -2f; // Fuerza descendente constante para mantener el contacto con el suelo

            if (Input.GetButtonDown("Jump") || Input.GetKeyDown(KeyCode.Space))
            {
                velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            }
        }
        else
        {
            // Caída rápida y pesada en el aire
            float currentGravity = (velocity.y < 0) ? gravity * 1.8f : gravity;
            velocity.y += currentGravity * Time.deltaTime;
        }

        // Combinamos movimiento horizontal + vertical en un solo vector final de desplazamiento
        Vector3 finalMovement = (move * currentSpeed) + velocity;
        controller.Move(finalMovement * Time.deltaTime);

        float speedPercent = move.magnitude * (Input.GetKey(KeyCode.LeftShift) ? 1f : 0.5f);
        if (anim != null) anim.SetFloat("Speed", speedPercent, 0.1f, Time.deltaTime);
    }

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (hit.gameObject.CompareTag("Enemy"))
        {
            EventManager.TriggerPlayerDeath();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("FinishLine"))
        {
            EventManager.TriggerVictory();
        }
    }
}
