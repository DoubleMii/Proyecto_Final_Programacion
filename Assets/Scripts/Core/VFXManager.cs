using UnityEngine;

public class VFXManager : MonoBehaviour
{
    public static VFXManager Instance;

    [SerializeField] private GameObject _dustParticles;
    [SerializeField] private GameObject _bloodParticles;
    [SerializeField] private GameObject _alertParticles; // El signo de exclamación

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    public void PlayAlert(Vector3 position)
    {
        if (_alertParticles) Instantiate(_alertParticles, position, Quaternion.identity);
    }
    
    // El profesor pide 3 sistemas de partículas
    public void PlayBlood(Vector3 position)
    {
        if (_bloodParticles) Instantiate(_bloodParticles, position, Quaternion.identity);
    }

    public void PlayDust(Vector3 position)
    {
        if (_dustParticles) Instantiate(_dustParticles, position, Quaternion.identity);
    }
}
