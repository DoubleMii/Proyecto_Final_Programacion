using System;

public static class EventManager
{
    public static event Action OnPlayerDeath;
    public static event Action OnVictory;
    public static event Action<bool> OnPlayerDetected;

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
        OnPlayerDetected?.Invoke(isDetected);
    }
}
