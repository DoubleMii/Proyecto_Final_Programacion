using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;
using UnityEngine.SceneManagement;


public class AudioManager : MonoBehaviour, IDataPersistence
{
    public static AudioManager instance;

    private const string MasterVolumeKey = "GlobalMasterVolume";
    private const string MusicVolumeKey = "GlobalMusicVolume";
    private const string SfxVolumeKey = "GlobalSfxVolume";

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

    private void OnEnable()
    {
        SceneManager.sceneLoaded += HandleSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= HandleSceneLoaded;
    }


    void Start()
    {
        BindSliders(true);
        LoadGlobalAudioSettings();
        ApplyCurrentVolumes();
        SyncSliderVisuals();

    }

    private void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        BindSliders(true);
        ApplyCurrentVolumes();
        SyncSliderVisuals();
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

    public void RestartMusic()
    {
        if (audioSource != null && audioSource.resource != null)
        {
            audioSource.Stop();
            audioSource.time = 0f;
            audioSource.Play();
        }

        AdaptiveMusic adaptiveMusic = FindAnyObjectByType<AdaptiveMusic>();
        if (adaptiveMusic != null)
            adaptiveMusic.RestartMusic();

        ApplyDirectSourceVolumes();
    }

    public void ChangeMasterVolume(float volume)
    {
        _masterVolume = NormalizeVolumeForSlider(masterSlider, volume);
        ApplyCurrentVolumes();
        SaveGlobalAudioSettings();
    }

    public void ChangeMusicVolume(float volume)
    {
        _musicVolume = NormalizeVolumeForSlider(musicSlider, volume);
        ApplyCurrentVolumes();
        SaveGlobalAudioSettings();
    }

    public void ChangeSFXVolume(float volume)
    {
        _sfxVolume = NormalizeVolumeForSlider(sfxSlider, volume);
        ApplyCurrentVolumes();
        SaveGlobalAudioSettings();
    }


    void Update()
    {
        
    }

    public void LoadData(GameData data)
    {
        BindSliders(false);
        LoadGlobalAudioSettings();
        ApplyCurrentVolumes();
        SyncSliderVisuals();
    }

    public void SaveData(GameData data)
    {
        SaveGlobalAudioSettings();
    }

    private void BindSliders(bool forceRefresh)
    {
        if (forceRefresh)
        {
            masterSlider = null;
            musicSlider = null;
            sfxSlider = null;
        }

        if (masterSlider != null && musicSlider != null && sfxSlider != null)
            return;

        Slider[] sliders = FindObjectsByType<Slider>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (Slider slider in sliders)
        {
            string sliderName = slider.name.ToLowerInvariant();
            if (masterSlider == null && sliderName.Contains("master")) masterSlider = slider;
            else if (musicSlider == null && (sliderName.Contains("music") || sliderName.Contains("musica"))) musicSlider = slider;
            else if (sfxSlider == null && (sliderName.Contains("sfx") || sliderName.Contains("efecto"))) sfxSlider = slider;
        }

        if (musicSlider != null)
        {
            musicSlider.onValueChanged.RemoveListener(ChangeMusicVolume);
            musicSlider.onValueChanged.AddListener(ChangeMusicVolume);
        }

        if (masterSlider != null)
        {
            masterSlider.onValueChanged.RemoveListener(ChangeMasterVolume);
            masterSlider.onValueChanged.AddListener(ChangeMasterVolume);
        }

        if (sfxSlider != null)
        {
            sfxSlider.onValueChanged.RemoveListener(ChangeSFXVolume);
            sfxSlider.onValueChanged.AddListener(ChangeSFXVolume);
        }
    }

    private void SyncSliderVisuals()
    {
        SetSliderFromNormalized(masterSlider, _masterVolume);
        SetSliderFromNormalized(musicSlider, _musicVolume);
        SetSliderFromNormalized(sfxSlider, _sfxVolume);
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

    private void LoadGlobalAudioSettings()
    {
        _masterVolume = Mathf.Clamp01(PlayerPrefs.GetFloat(MasterVolumeKey, _masterVolume));
        _musicVolume = Mathf.Clamp01(PlayerPrefs.GetFloat(MusicVolumeKey, _musicVolume));
        _sfxVolume = Mathf.Clamp01(PlayerPrefs.GetFloat(SfxVolumeKey, _sfxVolume));
    }

    private void SaveGlobalAudioSettings()
    {
        PlayerPrefs.SetFloat(MasterVolumeKey, _masterVolume);
        PlayerPrefs.SetFloat(MusicVolumeKey, _musicVolume);
        PlayerPrefs.SetFloat(SfxVolumeKey, _sfxVolume);
        PlayerPrefs.Save();
    }

    private void ApplyCurrentVolumes()
    {
        AudioListener.volume = _masterVolume;

        if (audioMixer != null)
        {
            audioMixer.SetFloat("MasterVolume", ToDecibels(_masterVolume));
            audioMixer.SetFloat("MusicVolume", ToDecibels(_musicVolume));
            audioMixer.SetFloat("SfxVolume", ToDecibels(_sfxVolume));
        }

        ApplyDirectSourceVolumes();
    }

    private void ApplyDirectSourceVolumes()
    {
        if (audioSource != null)
        {
            audioSource.volume = _musicVolume;
            audioSource.mute = _musicVolume <= 0.001f;
        }

        if (sfxSource != null)
        {
            sfxSource.volume = _sfxVolume;
            sfxSource.mute = _sfxVolume <= 0.001f;
        }

        AdaptiveMusic adaptiveMusic = FindAnyObjectByType<AdaptiveMusic>();
        if (adaptiveMusic != null)
        {
            AudioSource adaptiveSource = adaptiveMusic.GetComponent<AudioSource>();
            if (adaptiveSource != null)
            {
                adaptiveSource.volume = _musicVolume;
                adaptiveSource.mute = _musicVolume <= 0.001f;
            }
        }
    }
}
