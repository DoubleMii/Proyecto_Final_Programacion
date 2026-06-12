using UnityEngine;

public class AdaptiveMusic : MonoBehaviour
{
    [SerializeField] private AudioSource _musicSource;
    [SerializeField] private AudioClip _stealthMusic;
    [SerializeField] private AudioClip _chaseMusic;

    private int _enemiesChasing = 0;

    private void OnEnable()
    {
        EventManager.OnPlayerDetected += OnDetectionChanged;
    }

    private void OnDisable()
    {
        EventManager.OnPlayerDetected -= OnDetectionChanged;
    }

    private void OnDetectionChanged(bool isDetected)
    {
        if (isDetected) _enemiesChasing++;
        else _enemiesChasing = Mathf.Max(0, _enemiesChasing - 1);

        AudioClip desiredClip = (_enemiesChasing > 0) ? _chaseMusic : _stealthMusic;

        if (_musicSource != null && desiredClip != null && _musicSource.clip != desiredClip)
        {
            _musicSource.clip = desiredClip;
            _musicSource.Play();
        }
    }
}
