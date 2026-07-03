using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.AI;
using System.Collections;

[RequireComponent(typeof(CharacterController))]
public class PlayerPersistence : MonoBehaviour, IDataPersistence
{
    [Header("Configuración")]
    public bool loadPositionOnStart = true;

    [Header("Estado del jugador")]
    public float health = 100f;
    public float maxHealth = 100f;

    private CharacterController _cc;
    private Vector3 _spawnPosition;
    private float _spawnRotationY;
    private bool _hasLoaded;

    private void Awake()
    {
        _cc = GetComponent<CharacterController>();
        _spawnPosition = transform.position;
        _spawnRotationY = transform.eulerAngles.y;

        if (PersistenceManager.Instance != null)
        {
            PersistenceManager.Instance.OnDataLoaded += HandleDataLoaded;
        }
    }

    private void Start()
    {
        StartCoroutine(InitialLoad());
    }

    private void OnDestroy()
    {
        if (PersistenceManager.Instance != null)
        {
            PersistenceManager.Instance.OnDataLoaded -= HandleDataLoaded;
        }
    }

    private IEnumerator InitialLoad()
    {
        yield return null;

        LoadFromPersistence();
    }

    private void HandleDataLoaded()
    {
        LoadFromPersistence();
    }

    public void LoadData(GameData data)
    {
        if (data == null) return;

        health = data.player.health;
        maxHealth = data.player.maxHealth;

        Vector3 targetPosition = data.player.hasSavedPosition
            ? new Vector3(data.player.posX, data.player.posY, data.player.posZ)
            : _spawnPosition;

        float targetRotationY = data.player.hasSavedPosition ? data.player.rotY : _spawnRotationY;

        if (loadPositionOnStart || data.player.hasSavedPosition)
            StartCoroutine(ApplyPosition(targetPosition, targetRotationY));

        _hasLoaded = true;
    }

    private IEnumerator ApplyPosition(Vector3 pos, float rotY)
    {
        yield return null;

        if (NavMesh.SamplePosition(pos, out NavMeshHit hit, 4f, NavMesh.AllAreas))
            pos = hit.position;

        _cc.enabled = false;
        transform.position = pos;
        transform.rotation = Quaternion.Euler(0f, rotY, 0f);
        _cc.enabled = true;
    }

    public void SaveData(GameData data)
    {
        if (data == null) return;

        data.player.posX = transform.position.x;
        data.player.posY = transform.position.y;
        data.player.posZ = transform.position.z;
        data.player.rotY = transform.eulerAngles.y;
        data.player.hasSavedPosition = true;

        data.player.health = health;
        data.player.maxHealth = maxHealth;
    }

    public void LoadFromPersistence()
    {
        if (PersistenceManager.Instance == null) return;
        if (PersistenceManager.Instance.CurrentData == null) return;

        LoadData(PersistenceManager.Instance.CurrentData);
    }

    public void PushDataToPersistence()
    {
        if (PersistenceManager.Instance == null) return;

        SaveData(PersistenceManager.Instance.CurrentData);
    }

    private void Update()
    {
        if (Keyboard.current == null) return;

        if (Keyboard.current.hKey.wasPressedThisFrame)
        {
            health = Mathf.Max(0f, health - 10f);
            PersistenceManager.Instance?.UpdatePlayerHealth(health, maxHealth);
        }

        if (Keyboard.current.kKey.wasPressedThisFrame)
        {
            PersistenceManager.Instance?.RegisterKill();
        }

        if (!_hasLoaded) return;
    }
}
