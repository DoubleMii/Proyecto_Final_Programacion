using System;
using UnityEngine;

public static class EventManager
{
    // Ejemplos de eventos globales a los que otros scripts se pueden suscribir
    public static event Action OnPlayerDeath;
    public static event Action<int> OnScoreUpdated;

    public static void TriggerPlayerDeath()
    {
        OnPlayerDeath?.Invoke();
    }

    public static void TriggerScoreUpdate(int newScore)
    {
        OnScoreUpdated?.Invoke(newScore);
    }
}
