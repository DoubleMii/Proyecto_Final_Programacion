using UnityEngine;
using UnityEngine.InputSystem;

/// UI de prueba
/// Muestra botones de Guardar/Cargar/Nueva Partida y el estado actual.

public class PersistenceUI : MonoBehaviour
{
    [Header("Referencia al jugador")]
    [Tooltip("Si se asigna, se actualiza la posición del jugador al guardar")]
    public Transform playerTransform;

    // Estado de feedback en pantalla
    private string _lastAction  = "—";
    private float  _feedbackTimer = 0f;
    private const float FEEDBACK_DURATION = 3f;

    private BuildStabilityChecker _stabilityChecker;

    private void Start()
    {
        _stabilityChecker = FindAnyObjectByType<BuildStabilityChecker>();
    }

    private void Update()
    {
        if (Keyboard.current != null)
        {
            // Atajos de teclado rápidos
            if (Keyboard.current.f5Key.wasPressedThisFrame) QuickSave();
            if (Keyboard.current.f9Key.wasPressedThisFrame) QuickLoad();
        }

        if (_feedbackTimer > 0f)
            _feedbackTimer -= Time.deltaTime;
    }

    // UI EN PANTALLA
    private void OnGUI()
    {
        if (PersistenceManager.Instance == null) return;

        GUILayout.BeginArea(new Rect(10, 10, 280, 400));

        // Título 
        GUI.color = Color.cyan;
        GUILayout.Label("═══ SISTEMA DE PERSISTENCIA ═══");
        GUI.color = Color.white;

        GUILayout.Space(5);

        // Selector de slot 
        GUILayout.Label($"Slot activo: {PersistenceManager.Instance.activeSlot}");
        GUILayout.BeginHorizontal();
        for (int i = 0; i < 3; i++)
        {
            bool exists = SaveSystem.SaveExists(i);
            string label = exists ? $"Slot {i} ✓" : $"Slot {i}";
            if (GUILayout.Button(label, GUILayout.Width(80)))
                PersistenceManager.Instance.activeSlot = i;
        }
        GUILayout.EndHorizontal();

        GUILayout.Space(8);

        // ── Botones principales ──
        if (GUILayout.Button(" GUARDAR  [F5]"))  QuickSave();
        if (GUILayout.Button(" CARGAR   [F9]"))  QuickLoad();
        if (GUILayout.Button(" NUEVA PARTIDA"))  NewGame();
        if (GUILayout.Button(" BORRAR SLOT"))    DeleteSlot();

        GUILayout.Space(8);

        //  Datos actuales 
        GameData d = PersistenceManager.Instance.CurrentData;
        if (d != null)
        {
            GUI.color = Color.yellow;
            GUILayout.Label("── Datos en memoria ──");
            GUI.color = Color.white;

            GUILayout.Label($"Vida:       {d.player.health}/{d.player.maxHealth}");
            GUILayout.Label($"Muertes:    {d.stats.deaths}");

            float mins = d.stats.totalPlayTimeSeconds / 60f;
            GUILayout.Label($"Tiempo:     {mins:F1} min");
            GUILayout.Label($"Guardados:  {d.stats.saveCount}");
        }

        GUILayout.Space(5);

        //  Feedback
        if (_feedbackTimer > 0f)
        {
            GUI.color = Color.green;
            GUILayout.Label($"✔ {_lastAction}");
            GUI.color = Color.white;
        }

        // ── Estabilidad ──
        if (_stabilityChecker != null)
        {
            GUI.color = _stabilityChecker.IsStable ? Color.green : Color.red;
            GUILayout.Label($"Build: {(_stabilityChecker.IsStable ? "ESTABLE" : "INESTABLE")} | FPS: {_stabilityChecker.CurrentFPS:F0} | Errores: {_stabilityChecker.ErrorCount}");
            GUI.color = Color.white;
        }

        GUILayout.EndArea();
    }

   
    // ACCIONES
    
    private void QuickSave()
    {
        if (playerTransform != null)
            PersistenceManager.Instance.UpdatePlayerPosition(playerTransform);

        PersistenceManager.Instance.Save();
        ShowFeedback("Partida guardada");
    }

    private void QuickLoad()
    {
        PersistenceManager.Instance.Load();
        ShowFeedback("Partida cargada");
    }

    private void NewGame()
    {
        PersistenceManager.Instance.NewGame();
        ShowFeedback("Nueva partida iniciada");
    }

    private void DeleteSlot()
    {
        int slot = PersistenceManager.Instance.activeSlot;
        PersistenceManager.Instance.DeleteActiveSlot();
        ShowFeedback($"Slot {slot} borrado");
    }

    private void ShowFeedback(string message)
    {
        _lastAction    = message;
        _feedbackTimer = FEEDBACK_DURATION;
    }
}
