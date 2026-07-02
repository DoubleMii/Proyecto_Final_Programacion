using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;


public class AudioManager : MonoBehaviour, IDataPersistence
{
    public static AudioManager instance;


    public AudioSource audioSource;
    public AudioSource sfxSource;


    [SerializeField] private AudioMixer audioMixer;
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider masterSlider;
    [SerializeField] private Slider sfxSlider;

    private float _masterVolume = 1f;
    private float _musicVolume = 0.8f;
    private float _sfxVolume = 1f;


    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(instance);
            return;
        }

        Destroy(gameObject);
    }


    void Start()
    {
        AutoFindSliders();

        if (musicSlider != null) musicSlider.onValueChanged.AddListener(ChangeMusicVolume);
        if (masterSlider != null) masterSlider.onValueChanged.AddListener(ChangeMasterVolume);
        if (sfxSlider != null) sfxSlider.onValueChanged.AddListener(ChangeSFXVolume);

        if (PersistenceManager.Instance != null && PersistenceManager.Instance.CurrentData != null)
        {
            LoadData(PersistenceManager.Instance.CurrentData);
        }
        else
        {
            ChangeMusicVolume(musicSlider != null ? musicSlider.value : 0.8f);
            ChangeMasterVolume(masterSlider != null ? masterSlider.value : 1f);
            ChangeSFXVolume(sfxSlider != null ? sfxSlider.value : 1f);
        }
    }


    public void PlayMusic(AudioClip Song)
    {
        if (audioSource == null || Song == null) return;

        if (audioSource.resource == Song && audioSource.isPlaying) 
        {
            return;
        }
        else if (audioSource.resource == Song)
        {
            audioSource.UnPause();
        }
        else
        {
            audioSource.resource = Song;
            audioSource.Play();
        }
            
    }
    public void PlaySfx(AudioClip Sfx)
    {
        if (sfxSource == null || Sfx == null) return;

        sfxSource.resource = Sfx;
        sfxSource.Play();
    }

    public void PlaySfxAtSource(AudioSource source, AudioClip clip)
    {
        if (source == null || clip == null)
            return;

        source.pitch = Random.Range(0.92f, 1.08f);

        source.PlayOneShot(clip);
    }


    public void StopMusic()
    { 
       if (audioSource == null) return;

       audioSource.Pause();
    }

    public void ChangeMasterVolume(float volume)
    {
        _masterVolume = NormalizeVolumeForSlider(masterSlider, volume);
        AudioListener.volume = _masterVolume;

        if (audioMixer != null)
        {
            audioMixer.SetFloat("MasterVolume", ToDecibels(_masterVolume));
        }

        ApplyDirectSourceVolumes();
    }

    public void ChangeMusicVolume(float volume)
    {
        _musicVolume = NormalizeVolumeForSlider(musicSlider, volume);

        if (audioMixer != null)
        {
            audioMixer.SetFloat("MusicVolume", ToDecibels(_musicVolume));
        }

        ApplyDirectSourceVolumes();
    }

    public void ChangeSFXVolume(float volume)
    {
        _sfxVolume = NormalizeVolumeForSlider(sfxSlider, volume);

        if (audioMixer != null)
        {
            audioMixer.SetFloat("SfxVolume", ToDecibels(_sfxVolume));
        }

        ApplyDirectSourceVolumes();
    }


    void Update()
    {
        
    }

    public void LoadData(GameData data)
    {
        if (data == null || data.settings == null) return;

        float master = data.settings.masterVolume;
        float music = data.settings.musicVolume;
        float sfx = data.settings.sfxVolume;

        SetSliderFromNormalized(masterSlider, master);
        SetSliderFromNormalized(musicSlider, music);
        SetSliderFromNormalized(sfxSlider, sfx);

        ChangeMasterVolume(masterSlider != null ? masterSlider.value : master);
        ChangeMusicVolume(musicSlider != null ? musicSlider.value : music);
        ChangeSFXVolume(sfxSlider != null ? sfxSlider.value : sfx);
    }

    public void SaveData(GameData data)
    {
        if (data == null || data.settings == null) return;

        data.settings.masterVolume = masterSlider != null ? NormalizeVolumeForSlider(masterSlider, masterSlider.value) : _masterVolume;
        data.settings.musicVolume = musicSlider != null ? NormalizeVolumeForSlider(musicSlider, musicSlider.value) : _musicVolume;
        data.settings.sfxVolume = sfxSlider != null ? NormalizeVolumeForSlider(sfxSlider, sfxSlider.value) : _sfxVolume;
    }

    private void AutoFindSliders()
    {
        if (masterSlider != null && musicSlider != null && sfxSlider != null) return;

        Slider[] sliders = FindObjectsByType<Slider>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (Slider slider in sliders)
        {
            string sliderName = slider.name.ToLowerInvariant();
            if (masterSlider == null && sliderName.Contains("master")) masterSlider = slider;
            else if (musicSlider == null && (sliderName.Contains("music") || sliderName.Contains("musica"))) musicSlider = slider;
            else if (sfxSlider == null && (sliderName.Contains("sfx") || sliderName.Contains("efecto"))) sfxSlider = slider;
        }
    }

    private float NormalizeSliderVolume(float volume)
    {
        return Mathf.Clamp01(volume);
    }

    private float NormalizeVolumeForSlider(Slider slider, float volume)
    {
        if (slider != null && slider.minValue < 0f && slider.maxValue <= 0f)
        {
            return Mathf.InverseLerp(slider.minValue, slider.maxValue, volume);
        }

        if (slider != null && slider.maxValue > 1f)
        {
            return Mathf.InverseLerp(slider.minValue, slider.maxValue, volume);
        }

        return NormalizeSliderVolume(volume);
    }

    private void SetSliderFromNormalized(Slider slider, float normalizedVolume)
    {
        if (slider == null) return;

        float value = Mathf.Clamp01(normalizedVolume);
        if (slider.minValue < 0f && slider.maxValue <= 0f)
        {
            value = Mathf.Lerp(slider.minValue, slider.maxValue, value);
        }
        else if (slider.maxValue > 1f)
        {
            value = Mathf.Lerp(slider.minValue, slider.maxValue, value);
        }

        slider.SetValueWithoutNotify(value);
    }

    private float ToDecibels(float volume)
    {
        return Mathf.Log10(Mathf.Max(volume, 0.0001f)) * 20f;
    }

    private void ApplyDirectSourceVolumes()
    {
        if (audioSource != null) audioSource.volume = _musicVolume;
        if (sfxSource != null) sfxSource.volume = _sfxVolume;

        AdaptiveMusic adaptiveMusic = FindAnyObjectByType<AdaptiveMusic>();
        if (adaptiveMusic != null)
        {
            AudioSource adaptiveSource = adaptiveMusic.GetComponent<AudioSource>();
            if (adaptiveSource != null) adaptiveSource.volume = _musicVolume;
        }
    }
}
