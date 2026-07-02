using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class RuntimeAudioFeedback : MonoBehaviour
{
    private static RuntimeAudioFeedback _instance;

    private AudioSource _uiSource;
    private AudioClip _clickClip;
    private AudioClip _openClip;
    private AudioClip _closeClip;
    private AudioClip _alertClip;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Install()
    {
        if (_instance != null) return;

        GameObject root = new GameObject("RuntimeAudioFeedback");
        DontDestroyOnLoad(root);
        _instance = root.AddComponent<RuntimeAudioFeedback>();
        _instance.Initialize();
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += HandleSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= HandleSceneLoaded;
    }

    public static void PlayClick()
    {
        EnsureInstance();
        _instance.Play(_instance._clickClip, 0.55f);
    }

    public static void PlayMenuOpen()
    {
        EnsureInstance();
        _instance.Play(_instance._openClip, 0.65f);
    }

    public static void PlayMenuClose()
    {
        EnsureInstance();
        _instance.Play(_instance._closeClip, 0.55f);
    }

    public static void PlayAlert()
    {
        EnsureInstance();
        _instance.Play(_instance._alertClip, 0.45f);
    }

    private static void EnsureInstance()
    {
        if (_instance != null) return;
        Install();
    }

    private void Initialize()
    {
        _uiSource = gameObject.AddComponent<AudioSource>();
        _uiSource.playOnAwake = false;
        _uiSource.spatialBlend = 0f;
        _uiSource.volume = 0.8f;

        _clickClip = CreateTone("UIClick", 760f, 0.055f, 0.22f);
        _openClip = CreateSweep("UIOpen", 420f, 820f, 0.11f, 0.18f);
        _closeClip = CreateSweep("UIClose", 620f, 280f, 0.09f, 0.18f);
        _alertClip = CreateSweep("Alert", 320f, 920f, 0.18f, 0.16f);

        WireButtons();
    }

    private void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        WireButtons();
    }

    private void WireButtons()
    {
        Button[] buttons = FindObjectsByType<Button>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (Button button in buttons)
        {
            if (button == null) continue;
            button.onClick.RemoveListener(PlayClick);
            button.onClick.AddListener(PlayClick);
        }
    }

    private void Play(AudioClip clip, float volume)
    {
        if (_uiSource == null || clip == null) return;
        _uiSource.pitch = 1f;
        _uiSource.PlayOneShot(clip, volume);
    }

    private AudioClip CreateTone(string name, float frequency, float duration, float volume)
    {
        int sampleRate = 44100;
        int samples = Mathf.RoundToInt(sampleRate * duration);
        AudioClip clip = AudioClip.Create(name, samples, 1, sampleRate, false);
        float[] data = new float[samples];

        for (int i = 0; i < samples; i++)
        {
            float t = i / (float)sampleRate;
            float envelope = 1f - (i / (float)samples);
            data[i] = Mathf.Sin(2f * Mathf.PI * frequency * t) * volume * envelope;
        }

        clip.SetData(data, 0);
        return clip;
    }

    private AudioClip CreateSweep(string name, float startFrequency, float endFrequency, float duration, float volume)
    {
        int sampleRate = 44100;
        int samples = Mathf.RoundToInt(sampleRate * duration);
        AudioClip clip = AudioClip.Create(name, samples, 1, sampleRate, false);
        float[] data = new float[samples];
        float phase = 0f;

        for (int i = 0; i < samples; i++)
        {
            float progress = i / (float)samples;
            float frequency = Mathf.Lerp(startFrequency, endFrequency, progress);
            phase += 2f * Mathf.PI * frequency / sampleRate;
            float envelope = Mathf.Sin(progress * Mathf.PI);
            data[i] = Mathf.Sin(phase) * volume * envelope;
        }

        clip.SetData(data, 0);
        return clip;
    }
}
