using UnityEngine;
using UnityEngine.AI;

public class RuntimeCharacterAnimation : MonoBehaviour
{
    [SerializeField] private Transform visualRoot;
    [SerializeField] private float idleBobAmount = 0.025f;
    [SerializeField] private float moveBobAmount = 0.08f;
    [SerializeField] private float tiltAmount = 4f;

    private Animator _animator;
    private NavMeshAgent _agent;
    private CharacterController _characterController;
    private Vector3 _lastPosition;
    private Vector3 _baseLocalPosition;
    private Quaternion _baseLocalRotation;
    private bool _hasSpeed;
    private bool _hasMotionSpeed;
    private bool _hasGrounded;
    private bool _hasIsMoving;

    private void Awake()
    {
        if (visualRoot == null) visualRoot = transform;

        _animator = GetComponentInChildren<Animator>();
        _agent = GetComponent<NavMeshAgent>();
        _characterController = GetComponent<CharacterController>();
        _lastPosition = transform.position;
        _baseLocalPosition = visualRoot.localPosition;
        _baseLocalRotation = visualRoot.localRotation;

        CacheAnimatorParameters();
    }

    private void Update()
    {
        Vector3 velocity = GetVelocity();
        float speed = new Vector3(velocity.x, 0f, velocity.z).magnitude;

        UpdateAnimator(speed);
        UpdateProceduralMotion(speed, velocity);

        _lastPosition = transform.position;
    }

    private Vector3 GetVelocity()
    {
        if (_agent != null && _agent.enabled)
            return _agent.velocity;

        if (_characterController != null)
            return _characterController.velocity;

        return (transform.position - _lastPosition) / Mathf.Max(Time.deltaTime, 0.0001f);
    }

    private void UpdateAnimator(float speed)
    {
        if (_animator == null) return;

        if (_hasSpeed) _animator.SetFloat("Speed", speed, 0.1f, Time.deltaTime);
        if (_hasMotionSpeed) _animator.SetFloat("MotionSpeed", speed > 0.1f ? 1f : 0f);
        if (_hasGrounded) _animator.SetBool("Grounded", true);
        if (_hasIsMoving) _animator.SetBool("IsMoving", speed > 0.1f);
    }

    private void CacheAnimatorParameters()
    {
        if (_animator == null) return;

        foreach (AnimatorControllerParameter parameter in _animator.parameters)
        {
            if (parameter.name == "Speed") _hasSpeed = true;
            else if (parameter.name == "MotionSpeed") _hasMotionSpeed = true;
            else if (parameter.name == "Grounded") _hasGrounded = true;
            else if (parameter.name == "IsMoving") _hasIsMoving = true;
        }
    }

    private void UpdateProceduralMotion(float speed, Vector3 velocity)
    {
        if (_animator != null || visualRoot == null) return;

        float normalizedSpeed = Mathf.InverseLerp(0f, 5f, speed);
        float bobAmount = Mathf.Lerp(idleBobAmount, moveBobAmount, normalizedSpeed);
        float bob = Mathf.Sin(Time.time * Mathf.Lerp(2f, 8f, normalizedSpeed)) * bobAmount;

        Vector3 localPosition = _baseLocalPosition + Vector3.up * bob;
        visualRoot.localPosition = Vector3.Lerp(visualRoot.localPosition, localPosition, Time.deltaTime * 10f);

        Vector3 localVelocity = transform.InverseTransformDirection(velocity);
        float tilt = Mathf.Clamp(-localVelocity.x, -1f, 1f) * tiltAmount;
        Quaternion targetRotation = _baseLocalRotation * Quaternion.Euler(0f, 0f, tilt);
        visualRoot.localRotation = Quaternion.Slerp(visualRoot.localRotation, targetRotation, Time.deltaTime * 8f);
    }
}
