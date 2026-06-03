using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Verifica la estabilidad del build en tiempo de ejecución.
/// Detecta errores en consola, mide FPS y reporta el estado general.
/// Cubre el criterio "Estabilidad del Build" (10%) de la práctica.
/// Añadir al mismo GameObject que PersistenceManager.
/// </summary>
public class BuildStabilityChecker : MonoBehaviour
{
    // ─────────────────────────────────────────
    // CONFIGURACIÓN INSPECTOR
    // ─────────────────────────────────────────
    [Header("Monitoreo de FPS")]
    [Tooltip("FPS mínimo aceptable antes de lanzar advertencia")]
    public float minAcceptableFPS = 20f;

    [Tooltip("Intervalo de muestreo de FPS en segundos")]
    public float fpsSampleInterval = 1f;

    [Header("Reporte")]
    [Tooltip("Mostrar reporte de estabilidad en consola al iniciar")]
    public bool logOnStart = true;

    [Tooltip("Mostrar FPS en consola periódicamente")]
    public bool logFPS = false;

    // ─────────────────────────────────────────
    // ESTADO INTERNO
    // ─────────────────────────────────────────
    private int   _errorCount      = 0;
    private int   _warningCount    = 0;
    private float _currentFPS      = 0f;
    private float _lowestFPS       = float.MaxValue;
    private float _fpsTimer        = 0f;
    private int   _frameCount      = 0;
    private bool  _isStable        = true;

    // ─────────────────────────────────────────
    // PROPIEDADES PÚBLICAS
    // ─────────────────────────────────────────
    public float CurrentFPS  => _currentFPS;
    public float LowestFPS   => _lowestFPS;
    public int   ErrorCount  => _errorCount;
    public bool  IsStable    => _isStable;

    // ─────────────────────────────────────────
    // INICIO
    // ─────────────────────────────────────────
    private void OnEnable()
    {
        Application.logMessageReceived += OnLogReceived;
        SceneManager.sceneLoaded       += OnSceneLoaded;
    }

    private void OnDisable()
    {
        Application.logMessageReceived -= OnLogReceived;
        SceneManager.sceneLoaded       -= OnSceneLoaded;
    }

    private void Start()
    {
        if (logOnStart)
            LogStabilityReport();
    }

    // ─────────────────────────────────────────
    // UPDATE — MEDICIÓN DE FPS
    // ─────────────────────────────────────────
    private void Update()
    {
        _frameCount++;
        _fpsTimer += Time.unscaledDeltaTime;

        if (_fpsTimer >= fpsSampleInterval)
        {
            _currentFPS = _frameCount / _fpsTimer;
            _frameCount = 0;
            _fpsTimer   = 0f;

            if (_currentFPS < _lowestFPS)
                _lowestFPS = _currentFPS;

            if (_currentFPS < minAcceptableFPS)
            {
                _isStable = false;
                Debug.LogWarning($"[StabilityChecker] FPS bajo detectado: {_currentFPS:F1} FPS");
            }

            if (logFPS)
                Debug.Log($"[StabilityChecker] FPS actual: {_currentFPS:F1}");
        }
    }

    // ─────────────────────────────────────────
    // CAPTURA DE LOGS
    // ─────────────────────────────────────────

    /// <summary>Escucha todos los mensajes de consola en tiempo real.</summary>
    private void OnLogReceived(string condition, string stackTrace, LogType type)
    {
        if (type == LogType.Error || type == LogType.Exception)
        {
            _errorCount++;
            _isStable = false;
            // Usamos Log en lugar de LogError para evitar bucle infinito
            Debug.Log($"[StabilityChecker] ERROR #{_errorCount} detectado: {condition}");
        }
        else if (type == LogType.Warning)
        {
            _warningCount++;
        }
    }

    // ─────────────────────────────────────────
    // CAMBIO DE ESCENA
    // ─────────────────────────────────────────
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log($"[StabilityChecker] Escena cargada: '{scene.name}' — Errores acumulados: {_errorCount}");
    }

    // ─────────────────────────────────────────
    // REPORTE
    // ─────────────────────────────────────────

    /// <summary>Imprime un resumen del estado de estabilidad en consola.</summary>
    public void LogStabilityReport()
    {
        string status = _isStable ? "ESTABLE ✓" : "INESTABLE ✗";

        Debug.Log($"[StabilityChecker] ══════ REPORTE DE ESTABILIDAD ══════\n" +
                  $"  Estado:      {status}\n" +
                  $"  Errores:     {_errorCount}\n" +
                  $"  Advertencias:{_warningCount}\n" +
                  $"  FPS actual:  {_currentFPS:F1}\n" +
                  $"  FPS mínimo:  {_lowestFPS:F1}\n" +
                  $"  Escena:      {SceneManager.GetActiveScene().name}\n" +
                  $"══════════════════════════════════════");
    }

    /// <summary>
    /// Resetea los contadores (útil al cambiar de nivel o en pruebas).
    /// </summary>
    public void ResetCounters()
    {
        _errorCount   = 0;
        _warningCount = 0;
        _lowestFPS    = float.MaxValue;
        _isStable     = true;
        Debug.Log("[StabilityChecker] Contadores reseteados.");
    }
}
