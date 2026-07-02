using System;

public static class EventManager
{
    public static event Action OnPlayerDeath;
    public static event Action OnVictory;
    public static event Action<bool> OnPlayerDetected;

    private static int _playerDetectionCount;

    public static bool IsPlayerDetected => _playerDetectionCount > 0;

    [UnityEngine.RuntimeInitializeOnLoadMethod(UnityEngine.RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        OnPlayerDeath = null;
        OnVictory = null;
        OnPlayerDetected = null;
        _playerDetectionCount = 0;
    }

    public static void TriggerPlayerDeath()
    {
        OnPlayerDeath?.Invoke();
    }

    public static void TriggerVictory()
    {
        OnVictory?.Invoke();
    }

    public static void TriggerPlayerDetected(bool isDetected)
    {
        bool wasDetected = IsPlayerDetected;

        _playerDetectionCount += isDetected ? 1 : -1;
        if (_playerDetectionCount < 0) _playerDetectionCount = 0;

        if (wasDetected != IsPlayerDetected)
        {
            OnPlayerDetected?.Invoke(IsPlayerDetected);
        }
    }

    public static void ResetPlayerDetection()
    {
        bool wasDetected = IsPlayerDetected;
        _playerDetectionCount = 0;

        if (wasDetected)
        {
            OnPlayerDetected?.Invoke(false);
        }
    }
}
