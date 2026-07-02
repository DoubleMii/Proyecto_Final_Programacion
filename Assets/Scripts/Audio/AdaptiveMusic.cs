using UnityEngine;

public class AdaptiveMusic : MonoBehaviour
{
    [SerializeField] private AudioSource _musicSource;
    [SerializeField] private AudioClip _stealthMusic;
    [SerializeField] private AudioClip _chaseMusic;

    private void OnEnable()
    {
        EventManager.OnPlayerDetected += OnDetectionChanged;
    }

    private void Start()
    {
        EnsureAudioSetup();

        if (_musicSource != null && _stealthMusic != null)
        {
            _musicSource.clip = _stealthMusic;
            _musicSource.loop = true;
            _musicSource.Play();
        }
    }

    private void OnDisable()
    {
        EventManager.OnPlayerDetected -= OnDetectionChanged;
    }

    private void OnDetectionChanged(bool isDetected)
    {
        AudioClip desiredClip = isDetected ? _chaseMusic : _stealthMusic;

        if (_musicSource != null && desiredClip != null && _musicSource.clip != desiredClip)
        {
            _musicSource.clip = desiredClip;
            _musicSource.Play();
        }
    }

    public void Configure(AudioSource source, AudioClip stealthMusic, AudioClip chaseMusic)
    {
        _musicSource = source;
        _stealthMusic = stealthMusic;
        _chaseMusic = chaseMusic;
    }

    private void EnsureAudioSetup()
    {
        if (_musicSource == null)
        {
            _musicSource = GetComponent<AudioSource>();
            if (_musicSource == null) _musicSource = gameObject.AddComponent<AudioSource>();
        }

        _musicSource.loop = true;
        _musicSource.spatialBlend = 0f;
        _musicSource.playOnAwake = false;
        _musicSource.volume = Mathf.Max(_musicSource.volume, 0.35f);

        if (_stealthMusic == null)
            _stealthMusic = CreateToneClip("ExplorationTone", 196f, 0.12f);

        if (_chaseMusic == null)
            _chaseMusic = CreateToneClip("TensionTone", 330f, 0.18f);
    }

    private AudioClip CreateToneClip(string clipName, float frequency, float volume)
    {
        const int sampleRate = 44100;
        const float duration = 2f;
        int samples = Mathf.CeilToInt(sampleRate * duration);
        float[] data = new float[samples];

        for (int i = 0; i < samples; i++)
        {
            float t = i / (float)sampleRate;
            float pulse = Mathf.Sin(2f * Mathf.PI * frequency * t);
            float octave = Mathf.Sin(2f * Mathf.PI * frequency * 0.5f * t) * 0.45f;
            data[i] = (pulse + octave) * volume;
        }

        AudioClip clip = AudioClip.Create(clipName, samples, 1, sampleRate, false);
        clip.SetData(data, 0);
        return clip;
    }
}
