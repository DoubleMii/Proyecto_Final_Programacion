using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Estados de la FSM del enemigo.
/// Idle     - Espera estático (Pursuer, Guard sin waypoints).
/// Roam     - Deambula aleatoriamente (Roamer).
/// Patrol   - Patrulla waypoints (Guard).
/// Suspicious - Oyó o vio algo periférico; investiga.
/// Chase    - Ha visto directamente al jugador; persigue.
/// Search   - Perdió al jugador; busca en la última posición conocida.
/// Caught   - Alcanzó al jugador; delay dramático antes de activar la muerte.
/// </summary>
public enum eEnemyState
{
    Idle,
    Roam,
    Patrol,
    Suspicious,
    Chase,
    Search,
    Caught
}

public enum eEnemyType
{
    Guard,   // Azul   — Patrulla waypoints; busca al jugador si lo pierde antes de volver a ruta
    Pursuer, // Rojo   — Persigue desde su posición inicial; abandona si el jugador escapa muy lejos
    Roamer,  // Verde  — Deambula aleatoriamente; detección más fácil por ruido
    Watcher  // Naranja— Estático con FOV amplísimo; al detectar avisa a todos los cercanos
}

[RequireComponent(typeof(NavMeshAgent))]
public class NPCController : MonoBehaviour
{
    // ──────────────────────────────────────────────
    // INSPECTOR
    // ──────────────────────────────────────────────

    [Header("Datos del enemigo")]
    [SerializeField] private EnemyData _enemyData;
    [SerializeField] private eEnemyType _enemyType;

    [Header("Patrulla")]
    [SerializeField] private Transform[] _waypoints;
    [Range(0f, 10f)]
    [SerializeField] private float _waypointWaitTime = 2f;

    [Header("Detección")]
    [Range(1f, 30f)]
    [SerializeField] private float _detectionRange = 10f;
    [Range(10f, 360f)]
    [SerializeField] private float _fieldOfView = 120f;
    [Range(0.5f, 8f)]
    [SerializeField] private float _proximityRange = 2f;
    [Range(0.01f, 1f)]
    [SerializeField] private float _minDetectDistance = 0.5f;

    // ──────────────────────────────────────────────
    // REFERENCIAS
    // ──────────────────────────────────────────────

    private NavMeshAgent _agent;
    private Transform _player;
    private PlayerController _playerController;

    // ──────────────────────────────────────────────
    // VARIABLES DE ESTADO
    // ──────────────────────────────────────────────

    private eEnemyState _currentState;

    // --- Patrulla ---
    private int   _currentWaypointIndex;
    private bool  _isWaiting;
    private float _waitTimer;

    // --- Sospecha y detección gradual ---
    private float _suspicionLevel; // 0 = sin sospecha, 1 = alerta máxima → Chase

    // --- Última posición conocida del jugador ---
    private Vector3 _lastKnownPosition;
    private bool    _hasLastKnownPos;

    // --- Búsqueda (Search) ---
    private float _searchTimer;
    private bool  _searchingAtPoint; // true cuando ya llegó al LKP y está girando

    // --- Captura (Caught) ---
    private float _attackTimer;
    private bool  _caughtTriggered;

    // --- Roam ---
    private bool    _roamWaiting;
    private float   _roamTimer;
    private Vector3 _roamTarget;
    private bool    _hasRoamTarget;

    // --- Pursuer ---
    private Vector3 _initialPosition;

    // ──────────────────────────────────────────────
    // INICIALIZACIÓN
    // ──────────────────────────────────────────────

    private void Awake()
    {
        _agent = GetComponent<NavMeshAgent>();

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            _player = playerObj.transform;
            _playerController = playerObj.GetComponent<PlayerController>();
        }

        _initialPosition = transform.position;
        _agent.autoTraverseOffMeshLink = true;
        // autoTraverseOffMeshLink habilita saltos y escaleras automáticos en el NavMesh
    }

    private void Start()
    {
        SetInitialState();
    }

    // ──────────────────────────────────────────────
    // UPDATE
    // ──────────────────────────────────────────────

    private void Update()
    {
        if (_player == null) return;

        // La sospecha decae pasivamente cuando no ve al jugador
        if (_currentState != eEnemyState.Chase && _currentState != eEnemyState.Caught)
        {
            if (!CanSeePlayerDirectly())
                _suspicionLevel = Mathf.Max(0f, _suspicionLevel - Time.deltaTime * 0.4f);
        }

        UpdateFSM();
    }

    // ──────────────────────────────────────────────
    // FSM - CONFIGURACIÓN INICIAL
    // ──────────────────────────────────────────────

    private void SetInitialState()
    {
        switch (_enemyType)
        {
            case eEnemyType.Guard:
                ChangeState(_waypoints.Length > 0 ? eEnemyState.Patrol : eEnemyState.Idle);
                break;
            case eEnemyType.Roamer:
                ChangeState(eEnemyState.Roam);
                break;
            case eEnemyType.Pursuer:
                ChangeState(eEnemyState.Idle);
                break;
            case eEnemyType.Watcher:
                _agent.isStopped = true; // El Watcher nunca se mueve por su cuenta
                ChangeState(eEnemyState.Patrol); // Reutilizamos Patrol como estado de "escaneo rotatorio"
                break;
        }
    }

    private void ChangeState(eEnemyState newState)
    {
        // ── Eventos de transición ──

        // Entrar a Chase: avisar al EventManager + VFX + alertar aliados
        if (_currentState != eEnemyState.Chase && newState == eEnemyState.Chase)
        {
            EventManager.TriggerPlayerDetected(true);
            if (VFXManager.Instance != null)
                VFXManager.Instance.PlayAlert(transform.position + Vector3.up * 2f);
            AlertNearbyEnemies();
        }
        // Salir de Chase: notificar que ya no perseguimos
        else if (_currentState == eEnemyState.Chase && newState != eEnemyState.Chase)
        {
            EventManager.TriggerPlayerDetected(false);
        }

        // ── Reset de variables al entrar a cada estado ──
        switch (newState)
        {
            case eEnemyState.Search:
                _searchTimer       = _enemyData.searchTime;
                _searchingAtPoint  = false;
                _agent.isStopped   = false;
                break;

            case eEnemyState.Caught:
                _attackTimer    = _enemyData.attackCooldown;
                _caughtTriggered = false;
                _agent.isStopped = true;
                break;

            case eEnemyState.Roam:
                _hasRoamTarget = false;
                _roamWaiting   = false;
                break;

            case eEnemyState.Suspicious:
                _agent.isStopped = false;
                break;
        }

        _currentState = newState;
    }

    private void UpdateFSM()
    {
        switch (_currentState)
        {
            case eEnemyState.Idle:       HandleIdle();       break;
            case eEnemyState.Roam:       HandleRoam();       break;
            case eEnemyState.Patrol:     HandlePatrol();     break;
            case eEnemyState.Suspicious: HandleSuspicious(); break;
            case eEnemyState.Chase:      HandleChase();      break;
            case eEnemyState.Search:     HandleSearch();     break;
            case eEnemyState.Caught:     HandleCaught();     break;
        }
    }

    // ──────────────────────────────────────────────
    // ESTADOS
    // ──────────────────────────────────────────────

    /// <summary>
    /// Idle: el enemigo espera estático (Pursuer en posición inicial, Guard sin waypoints).
    /// </summary>
    private void HandleIdle()
    {
        _agent.isStopped = true;

        eEnemyState? detected = CheckDetection();
        if (detected.HasValue)
            ChangeState(detected.Value);
    }

    /// <summary>
    /// Roam: el Roamer deambula aleatoriamente por el NavMesh dentro de su radio.
    /// </summary>
    private void HandleRoam()
    {
        _agent.speed = _enemyData.moveSpeed * 0.75f;

        // Siempre comprueba detección primero
        eEnemyState? detected = CheckDetection();
        if (detected.HasValue) { ChangeState(detected.Value); return; }

        if (_roamWaiting)
        {
            _agent.isStopped = true;
            _roamTimer -= Time.deltaTime;
            if (_roamTimer <= 0f)
            {
                _roamWaiting   = false;
                _hasRoamTarget = false;
            }
            return;
        }

        _agent.isStopped = false;

        if (!_hasRoamTarget)
        {
            if (TryGetRoamPoint(out Vector3 pt))
            {
                _roamTarget    = pt;
                _hasRoamTarget = true;
                _agent.SetDestination(_roamTarget);
            }
            return; // Si no encontró punto válido, reintentará el próximo frame
        }

        // Llegó al punto de roam: espera antes de elegir otro
        if (!_agent.pathPending && _agent.remainingDistance < _minDetectDistance)
        {
            _roamWaiting   = true;
            _roamTimer     = _enemyData.roamWaitTime;
            _hasRoamTarget = false;
        }
    }

    /// <summary>
    /// Patrol: el Guard sigue su ruta de waypoints y espera en cada punto.
    /// </summary>
    private void HandlePatrol()
    {
        // ── Watcher: escaneo rotatorio estático ──────────────────────────────
        if (_enemyType == eEnemyType.Watcher)
        {
            _agent.isStopped = true;
            // Gira lentamente sobre sí mismo para cubrir 360°
            transform.Rotate(Vector3.up, _enemyData.moveSpeed * 8f * Time.deltaTime);

            eEnemyState? watchDetect = CheckDetection();
            if (watchDetect.HasValue)
            {
                // El Watcher alerta PRIMERO a todos los aliados antes de perseguir
                AlertNearbyEnemies();
                ChangeState(watchDetect.Value);
            }
            return;
        }

        // ── Guard: patrulla de waypoints ─────────────────────────────────────
        if (_waypoints.Length == 0) { ChangeState(eEnemyState.Idle); return; }

        _agent.isStopped = false;
        _agent.speed     = _enemyData.moveSpeed;

        eEnemyState? detected = CheckDetection();
        if (detected.HasValue)
        {
            _isWaiting  = false;
            _waitTimer  = 0f;
            ChangeState(detected.Value);
            return;
        }

        if (_isWaiting)
        {
            _waitTimer -= Time.deltaTime;
            if (_waitTimer <= 0f)
            {
                _isWaiting            = false;
                _currentWaypointIndex = (_currentWaypointIndex + 1) % _waypoints.Length;
                _agent.SetDestination(_waypoints[_currentWaypointIndex].position);
                _agent.isStopped = false;
            }
            return;
        }

        // Llegó al waypoint: espera antes de continuar
        if (!_agent.pathPending && _agent.remainingDistance < _minDetectDistance)
        {
            _isWaiting       = true;
            _waitTimer       = _waypointWaitTime;
            _agent.isStopped = true;
        }
    }

    /// <summary>
    /// Suspicious: el enemigo oyó un ruido o vio algo de reojo; investiga la zona.
    /// La sospecha aumenta gradualmente — si llega al máximo, pasa a Chase.
    /// </summary>
    private void HandleSuspicious()
    {
        _agent.isStopped = false;
        _agent.speed     = _enemyData.moveSpeed * 1.15f;

        // Visión directa → Chase inmediato (sin necesidad de llenar el medidor)
        if (CanSeePlayerDirectly())
        {
            _lastKnownPosition = _player.position;
            _hasLastKnownPos   = true;
            _suspicionLevel    = 1f;
            ChangeState(eEnemyState.Chase);
            return;
        }

        // Visión periférica: sospecha sube lentamente
        if (CanSeePlayerPeripheral())
        {
            _suspicionLevel    = Mathf.Clamp01(_suspicionLevel + _enemyData.suspicionSpeed * Time.deltaTime);
            _lastKnownPosition = _player.position;
            _hasLastKnownPos   = true;
            if (_suspicionLevel >= 1f)
            {
                ChangeState(eEnemyState.Chase);
                return;
            }
        }

        // Detecta por proximidad
        if (DetectPlayerByProximity())
        {
            ChangeState(eEnemyState.Chase);
            return;
        }

        // Se mueve hacia la posición sospechosa
        if (_hasLastKnownPos)
            _agent.SetDestination(_lastKnownPosition);

        // Sospecha agotada o llegó al punto sin encontrar nada → vuelve a rutina
        if (_suspicionLevel <= 0f ||
            (_hasLastKnownPos && !_agent.pathPending && _agent.remainingDistance < _minDetectDistance * 2f))
        {
            _suspicionLevel = 0f;
            ReturnToRoutine();
        }
    }

    /// <summary>
    /// Chase: el enemigo persigue activamente al jugador.
    /// Actualiza la última posición conocida mientras mantiene contacto visual.
    /// Si pierde al jugador, pasa a Search.
    /// </summary>
    private void HandleChase()
    {
        _agent.isStopped = false;
        _agent.speed     = _enemyData.chaseSpeed;

        float distToPlayer = Vector3.Distance(transform.position, _player.position);

        // Mantener la última posición conocida actualizada mientras lo ve
        if (CanSeePlayerDirectly() || DetectPlayerByProximity())
        {
            _lastKnownPosition = _player.position;
            _hasLastKnownPos   = true;
        }

        _agent.SetDestination(_player.position);

        // Captura: el enemigo alcanzó al jugador
        if (distToPlayer <= _enemyData.attackRange)
        {
            ChangeState(eEnemyState.Caught);
            return;
        }

        // Pursuer: abandona si el jugador se aleja demasiado
        if (_enemyType == eEnemyType.Pursuer && distToPlayer > _enemyData.maxChaseDistance)
        {
            ChangeState(eEnemyState.Search);
            return;
        }

        // Perdió contacto visual y ya no está muy cerca → busca en LKP
        if (!CanSeePlayerDirectly() && distToPlayer > _proximityRange * 2f)
        {
            ChangeState(eEnemyState.Search);
        }
    }

    /// <summary>
    /// Search: el enemigo va a la última posición conocida del jugador y busca durante
    /// un tiempo determinado. Si lo reencuentra, vuelve a Chase. Si no, se rinde.
    /// </summary>
    private void HandleSearch()
    {
        _agent.isStopped = false;
        _agent.speed     = _enemyData.moveSpeed * 1.1f;

        _searchTimer -= Time.deltaTime;

        // Redetección durante la búsqueda
        if (CanSeePlayerDirectly() || DetectPlayerByProximity())
        {
            _lastKnownPosition = _player.position;
            _hasLastKnownPos   = true;
            _suspicionLevel    = 1f;
            ChangeState(eEnemyState.Chase);
            return;
        }

        if (_hasLastKnownPos && !_searchingAtPoint)
        {
            // Primera fase: ir al LKP
            _agent.SetDestination(_lastKnownPosition);

            if (!_agent.pathPending && _agent.remainingDistance < _minDetectDistance * 2f)
                _searchingAtPoint = true; // Llegó al LKP, ahora gira para buscar
        }
        else if (_searchingAtPoint)
        {
            // Segunda fase: girar en el sitio simulando búsqueda visual
            _agent.isStopped = true;
            transform.Rotate(Vector3.up, 55f * Time.deltaTime);
        }

        // Tiempo de búsqueda agotado → rendirse y volver a rutina
        if (_searchTimer <= 0f)
        {
            _suspicionLevel   = 0f;
            _hasLastKnownPos  = false;
            _searchingAtPoint = false;
            ReturnToRoutine();
        }
    }

    /// <summary>
    /// Caught: el enemigo ha alcanzado al jugador.
    /// Aplica un breve delay dramático antes de disparar la muerte.
    /// Si el jugador logra alejarse en ese intervalo, el enemigo retoma la persecución.
    /// </summary>
    private void HandleCaught()
    {
        _agent.isStopped = true;
        transform.LookAt(_player);

        _attackTimer -= Time.deltaTime;

        // Ventana de escape: si el jugador consigue alejarse antes del delay, sigue persiguiendo
        if (Vector3.Distance(transform.position, _player.position) > _enemyData.attackRange * 2.2f)
        {
            _caughtTriggered = false;
            ChangeState(eEnemyState.Chase);
            return;
        }

        // Delay cumplido → muerte
        if (_attackTimer <= 0f && !_caughtTriggered)
        {
            _caughtTriggered = true;
            if (VFXManager.Instance != null)
                VFXManager.Instance.PlayBlood(_player.position);
            EventManager.TriggerPlayerDeath();
        }
    }

    // ──────────────────────────────────────────────
    // SISTEMA DE DETECCIÓN
    // ──────────────────────────────────────────────

    /// <summary>
    /// Punto de entrada de detección unificado. Devuelve el estado al que transicionar
    /// o null si no hay detección.
    /// </summary>
    private eEnemyState? CheckDetection()
    {
        // 1. Proximidad absoluta (sin importar dirección ni línea de visión)
        if (DetectPlayerByProximity()) return eEnemyState.Chase;

        // 2. Visión directa dentro del FOV
        if (CanSeePlayerDirectly())
        {
            _lastKnownPosition = _player.position;
            _hasLastKnownPos   = true;
            _suspicionLevel    = 1f;
            return eEnemyState.Chase;
        }

        // 3. Visión periférica → aumenta sospecha gradualmente
        if (CanSeePlayerPeripheral())
        {
            _lastKnownPosition  = _player.position;
            _hasLastKnownPos    = true;
            _suspicionLevel     = Mathf.Clamp01(_suspicionLevel + _enemyData.suspicionSpeed * Time.deltaTime);
            if (_suspicionLevel >= 1f) return eEnemyState.Chase;
            return eEnemyState.Suspicious;
        }

        // 4. Detección auditiva (jugador corriendo)
        if (DetectPlayerBySound())
        {
            _lastKnownPosition = _player.position;
            _hasLastKnownPos   = true;
            return eEnemyState.Suspicious;
        }

        return null; // Sin detección
    }

    /// <summary>Detección por proximidad: demasiado cerca para no notarlo.</summary>
    private bool DetectPlayerByProximity()
    {
        return Vector3.Distance(transform.position, _player.position) <= _proximityRange;
    }

    /// <summary>
    /// Detección por visión directa: el jugador está dentro del FOV principal
    /// y no hay obstáculos entre ambos (raycast).
    /// </summary>
    private bool CanSeePlayerDirectly()
    {
        float dist = Vector3.Distance(transform.position, _player.position);
        if (dist > _detectionRange) return false;

        Vector3 dir   = (_player.position - transform.position).normalized;
        float   angle = Vector3.Angle(transform.forward, dir);
        if (angle > _fieldOfView * 0.5f) return false;

        Vector3 rayOrigin = transform.position + Vector3.up * 0.8f;
        if (Physics.Raycast(rayOrigin, dir, out RaycastHit hit, _detectionRange))
            return hit.collider.CompareTag("Player");

        return false;
    }

    /// <summary>
    /// Detección periférica: el jugador está entre el FOV directo y el área lateral.
    /// No requiere raycast (lo ve por el rabillo del ojo), pero solo a distancia reducida.
    /// </summary>
    private bool CanSeePlayerPeripheral()
    {
        float dist = Vector3.Distance(transform.position, _player.position);
        if (dist > _detectionRange * 0.55f) return false; // Solo en rango reducido

        Vector3 dir   = (_player.position - transform.position).normalized;
        float   angle = Vector3.Angle(transform.forward, dir);

        // Zona periférica: entre el límite del FOV directo y el doble periférico
        return angle > _fieldOfView * 0.5f && angle <= _fieldOfView * 0.78f;
    }

    /// <summary>
    /// Detección auditiva: el jugador está corriendo (Shift) dentro del rango auditivo.
    /// </summary>
    private bool DetectPlayerBySound()
    {
        if (_playerController == null) return false;
        float dist = Vector3.Distance(transform.position, _player.position);
        if (dist > _enemyData.hearingRange) return false;

        return _playerController.IsRunning;
        // IsRunning se expone como propiedad pública en PlayerController
    }

    // ──────────────────────────────────────────────
    // UTILIDADES
    // ──────────────────────────────────────────────

    /// <summary>
    /// Alerta a los enemigos cercanos en un radio definido en EnemyData.
    /// Útil para que el grupo reaccione cuando uno de ellos detecta al jugador.
    /// </summary>
    private void AlertNearbyEnemies()
    {
        Collider[] nearby = Physics.OverlapSphere(transform.position, _enemyData.alertRadius);
        foreach (Collider col in nearby)
        {
            if (col.gameObject == gameObject) continue;
            NPCController other = col.GetComponent<NPCController>();
            if (other != null)
                other.AlertEnemy(_player.position);
        }
    }

    /// <summary>
    /// Método público para que otros enemigos puedan alertar a este NPC.
    /// </summary>
    public void AlertEnemy(Vector3 lastKnownPlayerPosition)
    {
        if (_currentState == eEnemyState.Chase || _currentState == eEnemyState.Caught) return;

        _lastKnownPosition = lastKnownPlayerPosition;
        _hasLastKnownPos   = true;
        _suspicionLevel    = 1f;
        ChangeState(eEnemyState.Chase);

        if (VFXManager.Instance != null)
            VFXManager.Instance.PlayAlert(transform.position + Vector3.up * 2f);
    }

    /// <summary>
    /// Hace que el enemigo vuelva a su comportamiento de base según su tipo.
    /// </summary>
    private void ReturnToRoutine()
    {
        switch (_enemyType)
        {
            case eEnemyType.Guard:
                if (_waypoints.Length > 0)
                {
                    _agent.SetDestination(_waypoints[_currentWaypointIndex].position);
                    ChangeState(eEnemyState.Patrol);
                }
                else ChangeState(eEnemyState.Idle);
                break;

            case eEnemyType.Roamer:
                ChangeState(eEnemyState.Roam);
                break;

            case eEnemyType.Pursuer:
                // El Pursuer regresa a su posición inicial
                _agent.SetDestination(_initialPosition);
                ChangeState(eEnemyState.Idle);
                break;

            case eEnemyType.Watcher:
                // El Watcher vuelve a su posición original y retoma el escaneo rotatorio
                _agent.SetDestination(_initialPosition);
                ChangeState(eEnemyState.Patrol);
                break;
        }
    }

    /// <summary>
    /// Elige un punto aleatorio en el NavMesh dentro del radio de roam.
    /// Intenta hasta 5 veces antes de rendirse.
    /// </summary>
    private bool TryGetRoamPoint(out Vector3 result)
    {
        for (int i = 0; i < 5; i++)
        {
            Vector3 randomDir = Random.insideUnitSphere * _enemyData.roamRadius;
            randomDir += _initialPosition;

            if (NavMesh.SamplePosition(randomDir, out NavMeshHit hit, _enemyData.roamRadius, NavMesh.AllAreas))
            {
                result = hit.position;
                return true;
            }
        }
        result = Vector3.zero;
        return false;
    }

    // ──────────────────────────────────────────────
    // VIDA (sin uso en este juego — no se puede matar a los enemigos)
    // ──────────────────────────────────────────────

    public void TakeDamage(float damage)
    {
        // Reservado: en este juego el jugador no puede eliminar enemigos.
    }

    // ──────────────────────────────────────────────
    // GIZMOS (visibles en el editor de Unity)
    // ──────────────────────────────────────────────

    private void OnDrawGizmosSelected()
    {
        // Rango de detección visual (amarillo)
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, _detectionRange);

        // Rango de proximidad (rojo)
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, _proximityRange);

        if (_enemyData != null)
        {
            // Rango auditivo (azul)
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, _enemyData.hearingRange);

            // Radio de alerta grupal (magenta)
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(transform.position, _enemyData.alertRadius);
        }

        // Línea y punto de la última posición conocida (naranja)
        if (Application.isPlaying && _hasLastKnownPos)
        {
            Gizmos.color = new Color(1f, 0.5f, 0f);
            Gizmos.DrawSphere(_lastKnownPosition, 0.3f);
            Gizmos.DrawLine(transform.position, _lastKnownPosition);
        }

        // Cono de FOV (líneas del ángulo de visión)
        Gizmos.color = new Color(1f, 1f, 0f, 0.6f);
        Vector3 leftBound  = Quaternion.Euler(0, -_fieldOfView * 0.5f, 0) * transform.forward * _detectionRange;
        Vector3 rightBound = Quaternion.Euler(0,  _fieldOfView * 0.5f, 0) * transform.forward * _detectionRange;
        Gizmos.DrawLine(transform.position, transform.position + leftBound);
        Gizmos.DrawLine(transform.position, transform.position + rightBound);
    }
}
