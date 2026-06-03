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


    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(instance);
        }
        
    }


    void Start()
    {
        musicSlider.onValueChanged.AddListener(ChangeMusicVolume);
        masterSlider.onValueChanged.AddListener(ChangeMasterVolume);
        sfxSlider.onValueChanged.AddListener(ChangeSFXVolume);

        if (PersistenceManager.Instance != null && PersistenceManager.Instance.CurrentData != null)
        {
            LoadData(PersistenceManager.Instance.CurrentData);
        }
        else
        {
            ChangeMusicVolume(musicSlider.value);
            ChangeMasterVolume(masterSlider.value);
            ChangeSFXVolume(sfxSlider.value);
        }
    }


    public void PlayMusic(AudioClip Song)
    {
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
       audioSource.Pause();
    }

    public void ChangeMasterVolume(float volume)
    {
        audioMixer.SetFloat("MasterVolume",
            Mathf.Log10(Mathf.Max(volume, 0.0001f)) * 20f);
    }

    public void ChangeMusicVolume(float volume)
    {
        audioMixer.SetFloat("MusicVolume",
            Mathf.Log10(Mathf.Max(volume, 0.0001f)) * 20f);
    }

    public void ChangeSFXVolume(float volume)
    {
        audioMixer.SetFloat("SfxVolume",
            Mathf.Log10(Mathf.Max(volume, 0.0001f)) * 20f);
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

        if (masterSlider != null) masterSlider.value = master;
        if (musicSlider != null) musicSlider.value = music;
        if (sfxSlider != null) sfxSlider.value = sfx;

        ChangeMasterVolume(master);
        ChangeMusicVolume(music);
        ChangeSFXVolume(sfx);
    }

    public void SaveData(GameData data)
    {
        if (data == null || data.settings == null) return;

        if (masterSlider != null) data.settings.masterVolume = masterSlider.value;
        if (musicSlider != null) data.settings.musicVolume = musicSlider.value;
        if (sfxSlider != null) data.settings.sfxVolume = sfxSlider.value;
    }
}