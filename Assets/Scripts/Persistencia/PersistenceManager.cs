using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manager central de persistencia. Singleton que vive entre escenas.
/// Coordina el guardado/carga y expone los datos al resto del juego.
/// Añadir a un GameObject vacío llamado "PersistenceManager" en la escena inicial.
/// </summary>
public class PersistenceManager : MonoBehaviour
{
    // ─────────────────────────────────────────
    // SINGLETON
    // ─────────────────────────────────────────
    public static PersistenceManager Instance { get; private set; }

    // ─────────────────────────────────────────
    // DATOS EN MEMORIA
    // ─────────────────────────────────────────

    /// <summary>Datos actualmente cargados en memoria.</summary>
    public GameData CurrentData { get; private set; }

    // ─────────────────────────────────────────
    // CONFIGURACIÓN INSPECTOR
    // ─────────────────────────────────────────
    [Header("Configuración")]
    [Tooltip("Slot de guardado activo (0, 1 o 2)")]
    [Range(0, 2)]
    public int activeSlot = 0;

    [Tooltip("Guardar automáticamente cada X segundos (0 = desactivado)")]
    public float autoSaveInterval = 60f;

    [Tooltip("Guardar al cambiar de escena")]
    public bool saveOnSceneChange = true;

    // ─────────────────────────────────────────
    // TIEMPO DE JUEGO
    // ─────────────────────────────────────────
    private float _sessionStartTime;
    private float _autoSaveTimer;

    // ─────────────────────────────────────────
    // AWAKE / SINGLETON SETUP
    // ─────────────────────────────────────────
    private void Awake()
    {
        // Patrón Singleton con DontDestroyOnLoad
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Intentar cargar datos existentes, si no, crear nueva partida
        CurrentData = SaveSystem.Load(activeSlot) ?? new GameData();

        _sessionStartTime = Time.time;
        _autoSaveTimer    = autoSaveInterval;

        Debug.Log("[PersistenceManager] Inicializado. Datos cargados del slot " + activeSlot);
    }

    // ─────────────────────────────────────────
    // UPDATE — AUTOSAVE Y TIEMPO DE JUEGO
    // ─────────────────────────────────────────
    private void Update()
    {
        // Acumular tiempo de sesión en los datos
        CurrentData.stats.totalPlayTimeSeconds += Time.deltaTime;

        // Auto-guardado por intervalo
        if (autoSaveInterval > 0f)
        {
            _autoSaveTimer -= Time.deltaTime;
            if (_autoSaveTimer <= 0f)
            {
                _autoSaveTimer = autoSaveInterval;
                Save();
                Debug.Log("[PersistenceManager] Auto-guardado ejecutado.");
            }
        }
    }

    // ─────────────────────────────────────────
    // GUARDAR
    // ─────────────────────────────────────────

    /// <summary>Guarda los datos actuales en el slot activo.</summary>
    public void Save()
    {
        var targets = FindAllPersistenceObjects();
        foreach (var t in targets)
            t.SaveData(CurrentData);

        SaveSystem.Save(CurrentData, activeSlot);
    }

    /// <summary>Guarda en un slot específico.</summary>
    public void SaveToSlot(int slot)
    {
        var targets = FindAllPersistenceObjects();
        foreach (var t in targets)
            t.SaveData(CurrentData);

        SaveSystem.Save(CurrentData, slot);
    }

    // ─────────────────────────────────────────
    // CARGAR
    // ─────────────────────────────────────────

    /// <summary>Carga datos desde el slot activo.</summary>
    public void Load()
    {
        GameData loaded = SaveSystem.Load(activeSlot);
        if (loaded != null)
        {
            CurrentData = loaded;

            var targets = FindAllPersistenceObjects();
            foreach (var t in targets)
                t.LoadData(CurrentData);
        }
        else
        {
            Debug.LogWarning("[PersistenceManager] No se encontró guardado. Se mantienen los datos actuales.");
        }
    }

    /// <summary>Carga desde un slot específico.</summary>
    public void LoadFromSlot(int slot)
    {
        GameData loaded = SaveSystem.Load(slot);
        if (loaded != null)
        {
            activeSlot  = slot;
            CurrentData = loaded;

            var targets = FindAllPersistenceObjects();
            foreach (var t in targets)
                t.LoadData(CurrentData);
        }
    }

    // ─────────────────────────────────────────
    // NUEVA PARTIDA
    // ─────────────────────────────────────────

    /// <summary>Reinicia los datos a valores por defecto (nueva partida).</summary>
    public void NewGame()
    {
        CurrentData = new GameData();

        var targets = FindAllPersistenceObjects();
        foreach (var t in targets)
            t.LoadData(CurrentData);

        Debug.Log("[PersistenceManager] Nueva partida iniciada.");
    }

    // ─────────────────────────────────────────
    // ACTUALIZAR POSICIÓN DEL JUGADOR
    // ─────────────────────────────────────────

    /// <summary>
    /// Llamar desde el script del jugador para actualizar su posición antes de guardar.
    /// Ejemplo: PersistenceManager.Instance.UpdatePlayerPosition(transform);
    /// </summary>
    public void UpdatePlayerPosition(Transform playerTransform)
    {
        if (CurrentData == null) return;

        CurrentData.player.posX = playerTransform.position.x;
        CurrentData.player.posY = playerTransform.position.y;
        CurrentData.player.posZ = playerTransform.position.z;
        CurrentData.player.rotY = playerTransform.eulerAngles.y;
    }

    /// <summary>Devuelve la posición guardada del jugador como Vector3.</summary>
    public Vector3 GetPlayerPosition()
    {
        if (CurrentData == null) return Vector3.zero;

        return new Vector3(
            CurrentData.player.posX,
            CurrentData.player.posY,
            CurrentData.player.posZ
        );
    }

    // ─────────────────────────────────────────
    // ACTUALIZAR VIDA
    // ─────────────────────────────────────────

    public void UpdatePlayerHealth(float current, float max)
    {
        if (CurrentData == null) return;
        CurrentData.player.health    = current;
        CurrentData.player.maxHealth = max;
    }

    // ─────────────────────────────────────────
    // INVENTARIO
    // ─────────────────────────────────────────

    public void AddItem(string itemName)
    {
        if (CurrentData == null) return;
        CurrentData.player.inventory.Add(itemName);
        CurrentData.stats.itemsCollected++;
    }

    public void RemoveItem(string itemName)
    {
        CurrentData?.player.inventory.Remove(itemName);
    }

    public bool HasItem(string itemName)
    {
        return CurrentData?.player.inventory.Contains(itemName) ?? false;
    }

    // ─────────────────────────────────────────
    // ESTADÍSTICAS
    // ─────────────────────────────────────────

    public void RegisterKill()
    {
        if (CurrentData != null)
            CurrentData.stats.enemiesKilled++;
    }

    public void RegisterDeath()
    {
        if (CurrentData != null)
            CurrentData.stats.deaths++;
    }

    // ─────────────────────────────────────────
    // AL SALIR
    // ─────────────────────────────────────────
    private void OnApplicationQuit()
    {
        Save();
        Debug.Log("[PersistenceManager] Guardado automático al salir.");
    }

    private void OnApplicationPause(bool paused)
    {
        if (paused) Save();
    }

    private List<IDataPersistence> FindAllPersistenceObjects()
    {
        MonoBehaviour[] monoBehaviours = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None);
        List<IDataPersistence> list = new List<IDataPersistence>();
        foreach (MonoBehaviour mb in monoBehaviours)
        {
            if (mb is IDataPersistence persistenceObj)
            {
                list.Add(persistenceObj);
            }
        }
        return list;
    }
}
