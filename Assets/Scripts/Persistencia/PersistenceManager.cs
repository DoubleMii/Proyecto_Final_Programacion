using System.Collections.Generic;
using UnityEngine;

public class PersistenceManager : MonoBehaviour
{
    public static PersistenceManager Instance { get; private set; }

    public GameData CurrentData { get; private set; }

    public event System.Action OnDataLoaded;

    [Header("Configuración")]
    [Range(0, 2)]
    public int activeSlot = 0;

    public bool clearSlotsOnStartup = true;
    public float autoSaveInterval = 60f;
    public bool saveOnSceneChange = true;

    private float _sessionStartTime;
    private float _autoSaveTimer;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (clearSlotsOnStartup)
            ClearAllSaveSlots();

        CurrentData = new GameData();

        _sessionStartTime = Time.time;
        _autoSaveTimer = autoSaveInterval;

        Debug.Log("[PersistenceManager] Inicializado con partida nueva. Usa Cargar para recuperar un slot guardado.");

        OnDataLoaded?.Invoke();
    }

    private void ClearAllSaveSlots()
    {
        for (int i = 0; i < 3; i++)
            SaveSystem.DeleteSave(i);
    }

    private void Update()
    {
        if (CurrentData == null)
        {
            CurrentData = new GameData();
        }

        CurrentData.stats.totalPlayTimeSeconds += Time.deltaTime;

        if (autoSaveInterval > 0f)
        {
            _autoSaveTimer -= Time.deltaTime;

            if (_autoSaveTimer <= 0f)
            {
                _autoSaveTimer = autoSaveInterval;
                TrySave(false);
            }
        }
    }

    public void Save()
    {
        TrySave();
    }

    public bool TrySave()
    {
        return TrySave(true);
    }

    public bool TrySave(bool manualSave)
    {
        if (CurrentData == null)
        {
            CurrentData = new GameData();
        }

        var targets = FindAllPersistenceObjects();
        foreach (var t in targets)
            t.SaveData(CurrentData);

        if (!manualSave)
            return true;

        return SaveSystem.Save(CurrentData, activeSlot, manualSave);
    }

    public void SaveToSlot(int slot)
    {
        TrySaveToSlot(slot);
    }

    public bool TrySaveToSlot(int slot)
    {
        activeSlot = Mathf.Clamp(slot, 0, 2);

        if (CurrentData == null)
        {
            CurrentData = new GameData();
        }

        var targets = FindAllPersistenceObjects();
        foreach (var t in targets)
            t.SaveData(CurrentData);

        return SaveSystem.Save(CurrentData, activeSlot, true);
    }

    public void Load()
    {
        TryLoad();
    }

    public bool TryLoad()
    {
        GameData loaded = SaveSystem.LoadManualSave(activeSlot);

        if (loaded != null)
        {
            CurrentData = loaded;

            var targets = FindAllPersistenceObjects();
            foreach (var t in targets)
                t.LoadData(CurrentData);

            OnDataLoaded?.Invoke();

            return true;
        }

        Debug.LogWarning("[PersistenceManager] No se encontró guardado. Se mantienen los datos actuales.");
        return false;
    }

    public void LoadFromSlot(int slot)
    {
        TryLoadFromSlot(slot);
    }

    public bool TryLoadFromSlot(int slot)
    {
        slot = Mathf.Clamp(slot, 0, 2);

        GameData loaded = SaveSystem.LoadManualSave(slot);

        if (loaded != null)
        {
            activeSlot = slot;
            CurrentData = loaded;

            var targets = FindAllPersistenceObjects();
            foreach (var t in targets)
                t.LoadData(CurrentData);

            OnDataLoaded?.Invoke();

            return true;
        }

        return false;
    }

    public void NewGame()
    {
        CurrentData = new GameData();

        var targets = FindAllPersistenceObjects();
        foreach (var t in targets)
            t.LoadData(CurrentData);

        if (AudioManager.instance != null)
            AudioManager.instance.LoadData(CurrentData);

        OnDataLoaded?.Invoke();
        RestartMusic();

        Debug.Log("[PersistenceManager] Nueva partida iniciada.");
    }

    private void RestartMusic()
    {
        AudioManager audioManager = AudioManager.instance;
        if (audioManager != null)
        {
            audioManager.RestartMusic();
            return;
        }

        AdaptiveMusic adaptiveMusic = FindAnyObjectByType<AdaptiveMusic>();
        if (adaptiveMusic != null)
            adaptiveMusic.RestartMusic();
    }

    public void ResetActiveSlot()
    {
        NewGame();
        Debug.Log("[PersistenceManager] Partida reiniciada sin borrar slots.");
    }

    public void DeleteActiveSlot()
    {
        int slotToDelete = activeSlot;
        SaveSystem.DeleteSave(slotToDelete);

        if (CurrentData != null && CurrentData.saveSlot == slotToDelete)
            NewGame();

        Debug.Log("[PersistenceManager] Slot " + slotToDelete + " borrado.");
    }

    public void UpdatePlayerPosition(Transform playerTransform)
    {
        if (CurrentData == null) return;

        CurrentData.player.posX = playerTransform.position.x;
        CurrentData.player.posY = playerTransform.position.y;
        CurrentData.player.posZ = playerTransform.position.z;
        CurrentData.player.rotY = playerTransform.eulerAngles.y;
    }

    public Vector3 GetPlayerPosition()
    {
        if (CurrentData == null) return Vector3.zero;

        return new Vector3(
            CurrentData.player.posX,
            CurrentData.player.posY,
            CurrentData.player.posZ
        );
    }

    public void UpdatePlayerHealth(float current, float max)
    {
        if (CurrentData == null) return;

        CurrentData.player.health = current;
        CurrentData.player.maxHealth = max;
    }

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

    private void OnApplicationQuit()
    {
        if (CurrentData != null)
        {
            TrySave(false);
        }

        Debug.Log("[PersistenceManager] Datos actualizados en memoria al salir.");
    }

    private void OnApplicationPause(bool paused)
    {
        if (paused && CurrentData != null) TrySave(false);
    }

    private List<IDataPersistence> FindAllPersistenceObjects()
    {
        MonoBehaviour[] monoBehaviours =
            FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include, FindObjectsSortMode.None);

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
