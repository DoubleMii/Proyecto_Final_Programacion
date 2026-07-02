using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class NPCController : MonoBehaviour
{
    public enum EnemyType
    {
        Guard,
        Hunter,
        Sentinel
    }

    private enum AiState
    {
        Patrol,
        Suspicious,
        Chase,
        Search,
        Attack,
        Return
    }

    [SerializeField] private EnemyData _enemyData;
    [SerializeField] private EnemyType _enemyType;
    [SerializeField] private Transform[] _waypoints;
    [SerializeField] private float _waypointWaitTime = 2f;

    [Header("Vision")]
    [SerializeField] private float _detectionRange = 14f;
    [SerializeField] private float _proximityRange = 4f;
    [SerializeField] private float _fieldOfView = 105f;
    [SerializeField] private LayerMask _lineOfSightMask = ~0;

    [Header("Detection")]
    [SerializeField] private float _suspicionSpeed = 4f;
    [SerializeField] private float _loseSightSpeed = 1.5f;
    [SerializeField] private float _chaseThreshold = 0.65f;
    [SerializeField] private float _minDetectDistance = 0.5f;
    [SerializeField] private float _timeToLook = 1f;
    [SerializeField] private float _timeToDetect = 1.25f;

    [Header("Search")]
    [SerializeField] private float _searchDuration = 5f;
    [SerializeField] private float _repathInterval = 0.25f;

    [Header("Combat")]
    [SerializeField] private float _attackCooldown = 1.5f;
    [SerializeField] private float _attackStopDistance = 1.25f;

    private NavMeshAgent _agent;
    private Transform _player;
    private Renderer[] _renderers;
    private AudioSource _audioSource;

    private AiState _state;
    private Vector3 _spawnPosition;
    private Quaternion _spawnRotation;
    private Vector3 _lastKnownPlayerPosition;
    private Vector3[] _fallbackWaypoints;
    private float _suspicion;
    private float _stateTimer;
    private float _waitTimer;
    private float _repathTimer;
    private float _attackTimer;
    private int _wpIndex;
    private bool _alertSent;

    public EnemyType Type => _enemyType;
    public string StateName => _state.ToString();
    public float Suspicion => _suspicion;

    private void Awake()
    {
        _agent = GetComponent<NavMeshAgent>();
        _renderers = GetComponentsInChildren<Renderer>();
        _audioSource = GetComponent<AudioSource>();
        if (_audioSource == null) _audioSource = gameObject.AddComponent<AudioSource>();

        _spawnPosition = transform.position;
        _spawnRotation = transform.rotation;
        EnsureEnemyTag();
        EnsureContactCollider();
        FindPlayer();
        ApplyRoleDefaults();
        ApplyRoleMaterial();
        EnsureFallbackWaypoints();
        ChangeState(AiState.Patrol);
    }

    private void OnEnable()
    {
        FindPlayer();
    }

    private void OnDisable()
    {
        SetAlert(false);
    }

    private void Update()
    {
        if (_player == null)
        {
            FindPlayer();
            if (_player == null) return;
        }

        bool canSeePlayer = CanSeePlayer();
        UpdateSuspicion(canSeePlayer);

        _stateTimer += Time.deltaTime;
        _attackTimer -= Time.deltaTime;
        _repathTimer -= Time.deltaTime;

        switch (_state)
        {
            case AiState.Patrol:
                UpdatePatrol(canSeePlayer);
                break;
            case AiState.Suspicious:
                UpdateSuspicious(canSeePlayer);
                break;
            case AiState.Chase:
                UpdateChase(canSeePlayer);
                break;
            case AiState.Search:
                UpdateSearch(canSeePlayer);
                break;
            case AiState.Attack:
                UpdateAttack(canSeePlayer);
                break;
            case AiState.Return:
                UpdateReturn(canSeePlayer);
                break;
        }
    }

    public void ConfigureRole(EnemyType type, EnemyData data = null)
    {
        _enemyType = type;
        if (data != null) _enemyData = data;

        if (_agent == null) _agent = GetComponent<NavMeshAgent>();

        ApplyRoleDefaults();
        ApplyRoleMaterial();
    }

    public void ResetToSpawnPosition()
    {
        if (_agent == null) _agent = GetComponent<NavMeshAgent>();

        _suspicion = 0f;
        _stateTimer = 0f;
        _waitTimer = 0f;
        _repathTimer = 0f;
        _attackTimer = 0f;
        _wpIndex = 0;
        _alertSent = false;
        _lastKnownPlayerPosition = _spawnPosition;

        if (_agent != null)
        {
            _agent.isStopped = false;

            if (NavMesh.SamplePosition(_spawnPosition, out NavMeshHit hit, 4f, NavMesh.AllAreas))
            {
                _agent.Warp(hit.position);
                if (_agent.isOnNavMesh)
                    _agent.ResetPath();
            }
            else
            {
                transform.position = _spawnPosition;
            }
        }
        else
        {
            transform.position = _spawnPosition;
        }

        transform.rotation = _spawnRotation;
        FindPlayer();
        ChangeState(AiState.Patrol);
    }

    private void FindPlayer()
    {
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) _player = p.transform;
    }

    private void EnsureEnemyTag()
    {
        try
        {
            gameObject.tag = "Enemy";
        }
        catch
        {
            // Si el tag no existe, el contacto directo se comprueba igualmente por componente.
        }
    }

    private void ApplyRoleDefaults()
    {
        float moveSpeed = _enemyData != null && _enemyData.moveSpeed > 0f ? _enemyData.moveSpeed : 2.5f;
        float chaseSpeed = _enemyData != null && _enemyData.chaseSpeed > 0f ? _enemyData.chaseSpeed : 4.5f;

        switch (_enemyType)
        {
            case EnemyType.Hunter:
                _detectionRange = Mathf.Max(_detectionRange, 18f);
                _proximityRange = Mathf.Max(_proximityRange, 5f);
                _fieldOfView = Mathf.Max(_fieldOfView, 120f);
                _searchDuration = Mathf.Max(_searchDuration, 7f);
                _agent.speed = Mathf.Max(chaseSpeed, 5.5f);
                _agent.acceleration = 12f;
                _agent.stoppingDistance = _attackStopDistance;
                break;
            case EnemyType.Sentinel:
                _detectionRange = Mathf.Max(_detectionRange, 24f);
                _proximityRange = Mathf.Max(_proximityRange, 3f);
                _fieldOfView = Mathf.Max(_fieldOfView, 75f);
                _searchDuration = Mathf.Max(_searchDuration, 4f);
                _agent.speed = Mathf.Max(moveSpeed, 1.6f);
                _agent.acceleration = 8f;
                _agent.stoppingDistance = _attackStopDistance;
                break;
            default:
                _detectionRange = Mathf.Max(_detectionRange, 14f);
                _proximityRange = Mathf.Max(_proximityRange, 4f);
                _fieldOfView = Mathf.Max(_fieldOfView, 100f);
                _agent.speed = Mathf.Max(moveSpeed, 2.5f);
                _agent.acceleration = 8f;
                _agent.stoppingDistance = _attackStopDistance;
                break;
        }

        _agent.angularSpeed = 360f;
        _agent.autoBraking = true;
    }

    private void EnsureContactCollider()
    {
        if (GetComponent<Collider>() != null)
            return;

        CapsuleCollider collider = gameObject.AddComponent<CapsuleCollider>();
        collider.center = new Vector3(0f, 1f, 0f);
        collider.height = 2f;
        collider.radius = 0.45f;
        collider.isTrigger = false;
    }

    private void ApplyRoleMaterial()
    {
        if (_renderers == null || _renderers.Length == 0)
            _renderers = GetComponentsInChildren<Renderer>();

        Color color = _enemyType switch
        {
            EnemyType.Hunter => new Color(0.95f, 0.18f, 0.12f),
            EnemyType.Sentinel => new Color(0.15f, 0.35f, 1f),
            _ => new Color(0.95f, 0.72f, 0.18f)
        };

        foreach (Renderer rend in _renderers)
        {
            if (rend == null) continue;
            rend.material.color = color;
        }
    }

    private void EnsureFallbackWaypoints()
    {
        if (HasValidWaypoints()) return;

        _fallbackWaypoints = new Vector3[4];
        float radius = _enemyType == EnemyType.Sentinel ? 4f : 8f;

        for (int i = 0; i < _fallbackWaypoints.Length; i++)
        {
            float angle = (360f / _fallbackWaypoints.Length) * i;
            Vector3 candidate = _spawnPosition + Quaternion.Euler(0f, angle, 0f) * Vector3.forward * radius;

            if (NavMesh.SamplePosition(candidate, out NavMeshHit hit, radius + 3f, NavMesh.AllAreas))
                _fallbackWaypoints[i] = hit.position;
            else
                _fallbackWaypoints[i] = _spawnPosition;
        }
    }

    private bool HasValidWaypoints()
    {
        if (_waypoints == null || _waypoints.Length == 0) return false;

        foreach (Transform waypoint in _waypoints)
        {
            if (waypoint != null) return true;
        }

        return false;
    }

    private void UpdateSuspicion(bool canSeePlayer)
    {
        if (canSeePlayer)
        {
            _lastKnownPlayerPosition = _player.position;
            float distance = Vector3.Distance(transform.position, _player.position);
            float distanceBonus = distance <= _proximityRange ? 2f : 1f;
            _suspicion += Time.deltaTime * _suspicionSpeed * distanceBonus / Mathf.Max(_timeToDetect, 0.1f);
        }
        else
        {
            float lossMultiplier = _state == AiState.Search ? 0.45f : 1f;
            _suspicion -= Time.deltaTime * _loseSightSpeed * lossMultiplier;
        }

        _suspicion = Mathf.Clamp01(_suspicion);
    }

    private bool CanSeePlayer()
    {
        Vector3 origin = transform.position + Vector3.up * 1.4f;
        Vector3 target = _player.position + Vector3.up * 1.1f;
        Vector3 toPlayer = target - origin;
        float distance = toPlayer.magnitude;

        if (distance <= _minDetectDistance || distance <= _proximityRange)
            return HasLineOfSight(origin, target, distance);

        if (distance > _detectionRange)
            return false;

        float angle = Vector3.Angle(transform.forward, toPlayer.normalized);
        if (angle > _fieldOfView * 0.5f)
            return false;

        return HasLineOfSight(origin, target, distance);
    }

    private bool HasLineOfSight(Vector3 origin, Vector3 target, float distance)
    {
        Vector3 direction = (target - origin).normalized;
        RaycastHit[] hits = Physics.RaycastAll(origin, direction, distance, _lineOfSightMask, QueryTriggerInteraction.Collide);

        if (hits == null || hits.Length == 0)
            return true;

        System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

        foreach (RaycastHit hit in hits)
        {
            if (hit.collider == null)
                continue;

            if (hit.transform == transform || hit.transform.IsChildOf(transform))
                continue;

            if (hit.transform == _player || hit.transform.IsChildOf(_player))
                return true;

            return false;
        }

        return false;
    }

    private void UpdatePatrol(bool canSeePlayer)
    {
        SetAlert(false);
        SetMoveSpeed(GetPatrolSpeed());

        if (canSeePlayer || _suspicion > 0.15f)
        {
            ChangeState(AiState.Suspicious);
            return;
        }

        if (_agent.pathPending) return;

        if (!_agent.hasPath || _agent.remainingDistance <= Mathf.Max(_agent.stoppingDistance + 0.2f, 0.6f))
        {
            _waitTimer -= Time.deltaTime;
            if (_waitTimer <= 0f)
            {
                MoveToNextWaypoint();
                _waitTimer = _waypointWaitTime;
            }
        }
    }

    private void UpdateSuspicious(bool canSeePlayer)
    {
        SetAlert(false);
        SetMoveSpeed(0f);
        LookAt(_player.position);

        if (_suspicion >= _chaseThreshold)
        {
            ChangeState(AiState.Chase);
            return;
        }

        if (!canSeePlayer && _stateTimer >= _timeToLook && _suspicion <= 0.05f)
        {
            ChangeState(AiState.Return);
        }
    }

    private void UpdateChase(bool canSeePlayer)
    {
        SetAlert(true);
        SetMoveSpeed(GetChaseSpeed());

        if (canSeePlayer)
            _lastKnownPlayerPosition = _player.position;

        float distance = Vector3.Distance(transform.position, _player.position);
        float attackRange = GetAttackRange();

        if (distance <= attackRange && canSeePlayer)
        {
            ChangeState(AiState.Attack);
            return;
        }

        if (_repathTimer <= 0f)
        {
            _agent.SetDestination(_lastKnownPlayerPosition);
            _repathTimer = _repathInterval;
        }

        if (!canSeePlayer && _suspicion <= 0.25f)
        {
            ChangeState(AiState.Search);
        }
    }

    private void UpdateSearch(bool canSeePlayer)
    {
        SetAlert(false);
        SetMoveSpeed(GetPatrolSpeed() * 0.8f);

        if (canSeePlayer)
        {
            ChangeState(AiState.Chase);
            return;
        }

        if (!_agent.hasPath && _repathTimer <= 0f)
        {
            Vector3 searchPoint = _lastKnownPlayerPosition + Random.insideUnitSphere * 4f;
            searchPoint.y = _lastKnownPlayerPosition.y;
            if (NavMesh.SamplePosition(searchPoint, out NavMeshHit hit, 5f, NavMesh.AllAreas))
                _agent.SetDestination(hit.position);

            _repathTimer = 1f;
        }

        if (_stateTimer >= _searchDuration || _suspicion <= 0.02f)
        {
            ChangeState(AiState.Return);
        }
    }

    private void UpdateAttack(bool canSeePlayer)
    {
        SetAlert(true);
        LookAt(_player.position);

        float distance = Vector3.Distance(transform.position, _player.position);
        if (!canSeePlayer || distance > GetAttackRange() + 0.8f)
        {
            ChangeState(AiState.Chase);
            return;
        }

        SetMoveSpeed(distance > _attackStopDistance ? GetChaseSpeed() : 0f);

        if (_repathTimer <= 0f)
        {
            _agent.SetDestination(_player.position);
            _repathTimer = _repathInterval;
        }

        if (_attackTimer <= 0f)
        {
            _attackTimer = _attackCooldown;
            PlayTone(880f, 0.08f);

            if (CanDamagePlayer())
                EventManager.TriggerPlayerDeath();
        }
    }

    private void UpdateReturn(bool canSeePlayer)
    {
        SetAlert(false);
        SetMoveSpeed(GetPatrolSpeed());

        if (canSeePlayer)
        {
            ChangeState(AiState.Suspicious);
            return;
        }

        if (!_agent.hasPath || _agent.remainingDistance <= Mathf.Max(_agent.stoppingDistance + 0.2f, 0.6f))
        {
            ChangeState(AiState.Patrol);
        }
    }

    private void ChangeState(AiState newState)
    {
        if (_state == newState && _stateTimer > 0f) return;

        _state = newState;
        _stateTimer = 0f;
        _repathTimer = 0f;

        switch (_state)
        {
            case AiState.Patrol:
                _waitTimer = 0f;
                break;
            case AiState.Search:
                _agent.SetDestination(_lastKnownPlayerPosition);
                break;
            case AiState.Return:
                MoveToClosestWaypointOrSpawn();
                break;
            case AiState.Chase:
            case AiState.Attack:
                SetAlert(true);
                PlayTone(_enemyType == EnemyType.Hunter ? 740f : 520f, 0.12f);
                break;
        }
    }

    private void MoveToNextWaypoint()
    {
        Vector3 target = GetWaypointPosition(_wpIndex);
        _wpIndex++;
        _agent.SetDestination(target);

        if (VFXManager.Instance != null && Random.value < 0.2f)
            VFXManager.Instance.PlayDust(transform.position);
    }

    private void MoveToClosestWaypointOrSpawn()
    {
        Vector3 target = _spawnPosition;
        float bestDistance = float.MaxValue;
        int count = GetWaypointCount();

        for (int i = 0; i < count; i++)
        {
            Vector3 candidate = GetWaypointPosition(i);
            float distance = Vector3.SqrMagnitude(transform.position - candidate);
            if (distance < bestDistance)
            {
                bestDistance = distance;
                target = candidate;
                _wpIndex = i;
            }
        }

        if (NavMesh.SamplePosition(target, out NavMeshHit hit, 6f, NavMesh.AllAreas))
            _agent.SetDestination(hit.position);
        else
            _agent.SetDestination(_spawnPosition);
    }

    private int GetWaypointCount()
    {
        if (HasValidWaypoints()) return _waypoints.Length;
        return _fallbackWaypoints != null ? _fallbackWaypoints.Length : 0;
    }

    private Vector3 GetWaypointPosition(int index)
    {
        if (HasValidWaypoints())
        {
            for (int tries = 0; tries < _waypoints.Length; tries++)
            {
                int wrapped = Mathf.Abs(index + tries) % _waypoints.Length;
                if (_waypoints[wrapped] != null)
                    return _waypoints[wrapped].position;
            }
        }

        if (_fallbackWaypoints == null || _fallbackWaypoints.Length == 0)
            return _spawnPosition;

        return _fallbackWaypoints[Mathf.Abs(index) % _fallbackWaypoints.Length];
    }

    private float GetPatrolSpeed()
    {
        float speed = _enemyData != null && _enemyData.moveSpeed > 0f ? _enemyData.moveSpeed : 2.5f;
        return _enemyType == EnemyType.Sentinel ? Mathf.Min(speed, 2f) : speed;
    }

    private float GetChaseSpeed()
    {
        float speed = _enemyData != null && _enemyData.chaseSpeed > 0f ? _enemyData.chaseSpeed : 4.5f;
        return _enemyType == EnemyType.Hunter ? Mathf.Max(speed, 5.5f) : speed;
    }

    private float GetAttackRange()
    {
        float range = _enemyData != null && _enemyData.attackRange > 0f ? _enemyData.attackRange : 1.5f;
        return Mathf.Clamp(range, 1.1f, 1.6f);
    }

    private bool CanDamagePlayer()
    {
        if (_player == null || _state != AiState.Attack)
            return false;

        PlayerController playerController = _player.GetComponentInParent<PlayerController>();
        if (playerController != null && playerController.Controller != null && !playerController.Controller.isGrounded)
            return false;

        Vector3 flatEnemy = new Vector3(transform.position.x, 0f, transform.position.z);
        Vector3 flatPlayer = new Vector3(_player.position.x, 0f, _player.position.z);
        float horizontalDistance = Vector3.Distance(flatEnemy, flatPlayer);
        float verticalDifference = Mathf.Abs(transform.position.y - _player.position.y);

        return horizontalDistance <= GetAttackRange() && verticalDifference <= 0.65f;
    }

    private void SetMoveSpeed(float speed)
    {
        if (speed <= 0.01f)
        {
            _agent.isStopped = true;
            _agent.velocity = Vector3.zero;
            return;
        }

        _agent.isStopped = false;
        _agent.speed = speed;
    }

    private void SetAlert(bool alert)
    {
        if (_alertSent == alert) return;

        _alertSent = alert;
        EventManager.TriggerPlayerDetected(alert);
    }

    private void LookAt(Vector3 target)
    {
        Vector3 direction = target - transform.position;
        direction.y = 0f;
        if (direction.sqrMagnitude < 0.001f) return;

        Quaternion targetRotation = Quaternion.LookRotation(direction.normalized);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, _agent.angularSpeed * Time.deltaTime);
    }

    private void PlayTone(float frequency, float duration)
    {
        if (_audioSource == null || !_audioSource.enabled) return;

        AudioClip clip = AudioClip.Create("NPCAlertTone", Mathf.RoundToInt(44100 * duration), 1, 44100, false);
        float[] samples = new float[clip.samples];
        for (int i = 0; i < samples.Length; i++)
        {
            samples[i] = Mathf.Sin(2f * Mathf.PI * frequency * i / 44100f) * 0.12f;
        }

        clip.SetData(samples, 0);
        _audioSource.spatialBlend = 1f;
        _audioSource.PlayOneShot(clip);
    }

    private void OnCollisionEnter(Collision collision)
    {
        TryCatchPlayer(collision.gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        TryCatchPlayer(other.gameObject);
    }

    private void TryCatchPlayer(GameObject other)
    {
        if (other == null)
            return;

        if ((other.CompareTag("Player") || other.GetComponentInParent<PlayerController>() != null) && CanDamagePlayer())
            EventManager.TriggerPlayerDeath();
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, _detectionRange);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, _proximityRange);

        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(transform.position + Vector3.up, _lastKnownPlayerPosition + Vector3.up);
    }
}
