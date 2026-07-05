using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class ZombieChaseAudio : MonoBehaviour
{
    [SerializeField] private AudioClip chaseClip;
    [SerializeField, Range(0f, 1f)] private float volume = 0.75f;
    [SerializeField] private float repeatEverySeconds = 3f;

    private NPCController _controller;
    private AudioSource _source;
    private float _timer;

    private void Awake()
    {
        _source = GetComponent<AudioSource>();
        ConfigureSource();
        FindController();
    }

    private void OnEnable()
    {
        ConfigureSource();
        FindController();
        _timer = 0f;
    }

    private void Update()
    {
        if (chaseClip == null)
            return;

        if (_controller == null)
            FindController();
        if (_controller == null)
            return;

        bool isChasing = _controller.StateName == "Chase" || _controller.StateName == "Attack";
        if (!isChasing)
        {
            _timer = 0f;
            return;
        }

        _timer -= Time.deltaTime;
        if (_timer > 0f)
            return;

        float sfxVolume = AudioManager.instance != null ? AudioManager.instance.SfxVolume : 1f;
        if (sfxVolume > 0.001f)
            _source.PlayOneShot(chaseClip, volume * sfxVolume);

        _timer = Mathf.Max(0.2f, repeatEverySeconds);
    }

    private void FindController()
    {
        _controller = GetComponent<NPCController>();
        if (_controller == null)
            _controller = GetComponentInParent<NPCController>();
        if (_controller == null)
            _controller = GetComponentInChildren<NPCController>();
    }

    private void ConfigureSource()
    {
        if (_source == null)
            return;

        _source.playOnAwake = false;
        _source.mute = false;
        _source.volume = 1f;
        _source.spatialBlend = 0.6f;
        _source.minDistance = 3f;
        _source.maxDistance = 60f;
        _source.dopplerLevel = 0f;
        _source.rolloffMode = AudioRolloffMode.Linear;
    }
}
