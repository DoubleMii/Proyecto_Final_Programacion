using UnityEngine;
using UnityEngine.InputSystem;

// Componente que va en el GameObject del jugador.
// Se encarga de cargar la posición/estado al iniciar
// y de actualizar los datos antes de cada guardado.
[RequireComponent(typeof(CharacterController))]
public class PlayerPersistence : MonoBehaviour, IDataPersistence
{
    [Header("Configuración")]
    [Tooltip("Si true, al iniciar la escena el jugador se teletransporta a la posición guardada")]
    public bool loadPositionOnStart = true;

    
    // VARIABLES SIMULADAS (reemplazar con las reales del juego)
    [Header("Estado del Jugador (demo)")]
    public float health     = 100f;
    public float maxHealth  = 100f;
    public float stamina    = 100f;
    public float maxStamina = 100f;
    public int   level      = 1;
    public int   gold       = 0;

    private CharacterController _cc;

    
    // INICIO
    
    private void Awake()
    {
        _cc = GetComponent<CharacterController>();
    }

    private void Start()
    {
        LoadFromPersistence();
    }

    public void LoadData(GameData data)
    {
        if (data == null) return;

        health     = data.player.health;
        maxHealth  = data.player.maxHealth;
        stamina    = data.player.stamina;
        maxStamina = data.player.maxStamina;
        level      = data.player.level;
        gold       = data.player.gold;

        if (loadPositionOnStart)
        {
            Vector3 savedPos = data.player.Position;
            if (savedPos != Vector3.zero)
            {
                _cc.enabled = false;
                transform.position = savedPos;
                transform.rotation = Quaternion.Euler(0f, data.player.rotY, 0f);
                _cc.enabled = true;

                Debug.Log($"[PlayerPersistence] Posición cargada: {savedPos}");
            }
        }
    }

    public void SaveData(GameData data)
    {
        if (data == null) return;

        data.player.Position = transform.position;
        data.player.rotY = transform.eulerAngles.y;

        data.player.health     = health;
        data.player.maxHealth  = maxHealth;
        data.player.stamina    = stamina;
        data.player.maxStamina = maxStamina;
        data.player.level      = level;
        data.player.gold       = gold;
    }

    public void LoadFromPersistence()
    {
        if (PersistenceManager.Instance != null)
            LoadData(PersistenceManager.Instance.CurrentData);
    }

    public void PushDataToPersistence()
    {
        if (PersistenceManager.Instance != null)
            SaveData(PersistenceManager.Instance.CurrentData);
    }

    
    // ATAJOS PARA DEMO
        private void Update()
    {
        if (Keyboard.current == null) return;

        // Simular recoger oro con G
        if (Keyboard.current.gKey.wasPressedThisFrame)
        {
            gold += 10;
            if (PersistenceManager.Instance?.CurrentData != null)
                PersistenceManager.Instance.CurrentData.player.gold = gold;
            Debug.Log($"[PlayerPersistence] Oro: {gold}");
        }

        // Simular recibir daño con H
        if (Keyboard.current.hKey.wasPressedThisFrame)
        {
            health = Mathf.Max(0f, health - 10f);
            PersistenceManager.Instance?.UpdatePlayerHealth(health, maxHealth);
            Debug.Log($"[PlayerPersistence] Vida: {health}/{maxHealth}");
        }

        // Simular matar enemigo con K
        if (Keyboard.current.kKey.wasPressedThisFrame)
        {
            PersistenceManager.Instance?.RegisterKill();
            Debug.Log("[PlayerPersistence] Enemigo eliminado registrado.");
        }
    }
}
