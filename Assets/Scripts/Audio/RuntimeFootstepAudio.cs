using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(AudioSource))]
public class RuntimeFootstepAudio : MonoBehaviour
{
    [SerializeField] private float walkStepInterval = 0.48f;
    [SerializeField] private float runStepInterval = 0.32f;
    [SerializeField] private float minSpeed = 0.2f;
    [SerializeField] private bool spawnDust = true;

    private static AudioClip _stepClip;

    private AudioSource _source;
    private NavMeshAgent _agent;
    private CharacterController _controller;
    private Vector3 _lastPosition;
    private float _timer;

    private void Awake()
    {
        _source = GetComponent<AudioSource>();
        _agent = GetComponent<NavMeshAgent>();
        _controller = GetComponent<CharacterController>();
        _lastPosition = transform.position;

        _source.playOnAwake = false;
        _source.spatialBlend = gameObject.CompareTag("Player") ? 0.15f : 1f;
        _source.volume = gameObject.CompareTag("Player") ? 0.45f : 0.7f;

        if (_stepClip == null)
            _stepClip = CreateStepClip();
    }

    private void Update()
    {
        Vector3 velocity = GetVelocity();
        float speed = new Vector3(velocity.x, 0f, velocity.z).magnitude;

        if (speed < minSpeed)
        {
            _timer = 0f;
            _lastPosition = transform.position;
            return;
        }

        float interval = Mathf.Lerp(walkStepInterval, runStepInterval, Mathf.InverseLerp(1f, 6f, speed));
        _timer -= Time.deltaTime;

        if (_timer <= 0f)
        {
            _timer = interval;
            PlayStep(speed);
        }

        _lastPosition = transform.position;
    }

    private Vector3 GetVelocity()
    {
        Vector3 positionVelocity = (transform.position - _lastPosition) / Mathf.Max(Time.deltaTime, 0.0001f);

        if (_agent != null && _agent.enabled)
            return _agent.velocity;

        if (_controller != null)
        {
            Vector3 controllerVelocity = _controller.velocity;
            Vector3 flatController = new Vector3(controllerVelocity.x, 0f, controllerVelocity.z);
            Vector3 flatPosition = new Vector3(positionVelocity.x, 0f, positionVelocity.z);

            return flatPosition.sqrMagnitude > flatController.sqrMagnitude ? positionVelocity : controllerVelocity;
        }

        return positionVelocity;
    }

    private void PlayStep(float speed)
    {
        if (_stepClip == null || _source == null) return;

        _source.pitch = Random.Range(0.85f, 1.12f) + Mathf.InverseLerp(1f, 7f, speed) * 0.08f;
        _source.PlayOneShot(_stepClip, Mathf.Clamp01(speed / 6f));

        if (spawnDust && VFXManager.Instance != null)
            VFXManager.Instance.PlayDust(GetFootstepPosition());
    }

    private Vector3 GetFootstepPosition()
    {
        Vector3 origin = transform.position + Vector3.up * 0.5f;

        if (Physics.Raycast(origin, Vector3.down, out RaycastHit hit, 2f, ~0, QueryTriggerInteraction.Ignore))
            return hit.point;

        return transform.position;
    }

    private AudioClip CreateStepClip()
    {
        int sampleRate = 44100;
        int samples = Mathf.RoundToInt(sampleRate * 0.09f);
        AudioClip clip = AudioClip.Create("RuntimeFootstep", samples, 1, sampleRate, false);
        float[] data = new float[samples];

        for (int i = 0; i < samples; i++)
        {
            float progress = i / (float)samples;
            float envelope = Mathf.Exp(-progress * 18f);
            float low = Mathf.Sin(2f * Mathf.PI * 95f * i / sampleRate);
            float noise = Random.Range(-1f, 1f) * 0.35f;
            data[i] = (low + noise) * 0.22f * envelope;
        }

        clip.SetData(data, 0);
        return clip;
    }
}
