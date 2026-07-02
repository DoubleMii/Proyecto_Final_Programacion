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
    public float jumpCooldown = 0.2f;

    [Header("Camera")]
    public float mouseSensitivity = 2f;
    public Transform playerCamera;

    [Header("Visuals")]
    [SerializeField] private Animator visualAnimator;

    private float xRot;
    private CharacterController controller;
    private Animator anim;

    private Vector3 velocity;
    private float jumpCooldownTimer;
    private float jumpAnimationTimer;
    private bool hasSpeedParameter;
    private bool hasMotionSpeedParameter;
    private bool hasGroundedParameter;
    private bool hasFreeFallParameter;
    private bool hasJumpParameter;
    private AnimatorControllerParameterType jumpParameterType;

    public CharacterController Controller => controller;
    public Animator Animator => anim;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        anim = visualAnimator != null ? visualAnimator : GetComponentInChildren<Animator>();
        CacheAnimatorParameters();

        Cursor.lockState = CursorLockMode.Locked;
    }

    private void Update()
    {
        if (GameManager.Instance != null &&
            GameManager.Instance.currentState != GameManager.GameState.Playing)
            return;

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

        bool grounded = controller.isGrounded;

        if (grounded && velocity.y < 0f)
            velocity.y = -2f;

        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        Vector3 move = (transform.right * x + transform.forward * z);

        float speed = Input.GetKey(KeyCode.LeftShift) ? runSpeed : moveSpeed;

        controller.Move(move * speed * Time.deltaTime);

        jumpCooldownTimer -= Time.deltaTime;

        if ((Input.GetButtonDown("Jump") || Input.GetKeyDown(KeyCode.Space)) &&
            grounded &&
            jumpCooldownTimer <= 0f)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            jumpCooldownTimer = jumpCooldown;
            jumpAnimationTimer = 0.12f;

            if (hasJumpParameter)
                SetJumpParameter(true);
        }

        velocity.y += gravity * Time.deltaTime;
        controller.Move(Vector3.up * velocity.y * Time.deltaTime);

        float animationSpeed = move.magnitude > 0.1f ? speed : 0f;
        bool freeFall = !grounded && velocity.y < -0.1f;
        jumpAnimationTimer -= Time.deltaTime;

        if (hasJumpParameter && jumpAnimationTimer <= 0f)
            SetJumpParameter(false);

        if (hasSpeedParameter)
            anim.SetFloat("Speed", animationSpeed, 0.1f, Time.deltaTime);
        if (hasMotionSpeedParameter)
            anim.SetFloat("MotionSpeed", move.magnitude > 0.1f ? 1f : 0f);
        if (hasGroundedParameter)
            anim.SetBool("Grounded", grounded);
        if (hasFreeFallParameter)
            anim.SetBool("FreeFall", freeFall);
    }

    private void CacheAnimatorParameters()
    {
        if (anim == null) return;

        foreach (AnimatorControllerParameter parameter in anim.parameters)
        {
            if (parameter.name == "Speed") hasSpeedParameter = true;
            else if (parameter.name == "MotionSpeed") hasMotionSpeedParameter = true;
            else if (parameter.name == "Grounded") hasGroundedParameter = true;
            else if (parameter.name == "FreeFall") hasFreeFallParameter = true;
            else if (parameter.name == "Jump")
            {
                hasJumpParameter = true;
                jumpParameterType = parameter.type;
            }
        }
    }

    private void SetJumpParameter(bool value)
    {
        if (anim == null || !hasJumpParameter) return;

        if (jumpParameterType == AnimatorControllerParameterType.Trigger && value)
            anim.SetTrigger("Jump");
        else if (jumpParameterType == AnimatorControllerParameterType.Bool)
            anim.SetBool("Jump", value);
    }

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        // Los enemigos deciden si atacan. El jugador no muere por un roce accidental.
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("FinishLine"))
            EventManager.TriggerVictory();
    }
}
