using UnityEngine;
using UnityEngine.AI;

public enum eEnemyState
{
    Idle,
    Patrol,
    Chase,
    Attack,
    Flee
}

public enum eEnemyType
{
    Guard,
    Pursuer,
    Roamer
}

[RequireComponent(typeof(NavMeshAgent))]
public class NPCController : MonoBehaviour
{
    [Header("Datos del enemigo")]
    [SerializeField] private EnemyData _enemyData; //ScriptableObject con las estadísticas del enemigo
    [SerializeField] private eEnemyType _enemyType; //Tipo de NPC que define su comportamiento en la FSM

    [Header("Patrulla")]
    [SerializeField] private Transform[] _waypoints; //Puntos de patrulla, solo usados por el Guard
    [Range(0f, 10f)]
    [SerializeField] private float _waypointWaitTime = 2f; //Tiempo de espera en cada punto de patrulla antes de continuar

    [Header("Detección")]
    [Range(1f, 30f)]
    [SerializeField] private float _detectionRange = 10f; //Rango máximo de detección del jugador
    [Range(10f, 360f)]
    [SerializeField] private float _fieldOfView = 120f; //Ángulo de visión en grados para el raycast
    [Range(1f, 10f)]
    [SerializeField] private float _proximityRange = 3f; //Radio de detección por proximidad sin necesidad de visión directa

    private NavMeshAgent _agent;
    private Transform _player;
    private eEnemyState _currentState;
    private int _currentWaypointIndex;
    private float _currentHealth;
    private float _waitTimer;
    private bool _isWaiting;

    private void Awake()
    {
        _agent = GetComponent<NavMeshAgent>();
        _player = GameObject.FindGameObjectWithTag("Player").transform;
        _currentHealth = _enemyData.maxHealth;
        _agent.autoTraverseOffMeshLink = true;
        //Iniciamos el agente y obtenemos referencias. autoTraverseOffMeshLink habilita saltos y escaleras automáticos
    }

    private void Start()
    {
        SetInitialState();
        //Asignamos el estado inicial según el tipo de enemigo
    }

    private void Update()
    {
        UpdateFSM();
        //Actualizamos la máquina de estados cada frame
    }

    private void SetInitialState()
    {
        switch (_enemyType)
        {
            case eEnemyType.Guard:
                ChangeState(eEnemyState.Patrol);
                break;
            case eEnemyType.Roamer:
                ChangeState(eEnemyState.Idle);
                break;
            case eEnemyType.Pursuer:
                ChangeState(eEnemyState.Chase);
                break;
        }
        //El Guard patrulla, el Roamer deambula en Idle, el Pursuer persigue desde el inicio
    }

    private void ChangeState(eEnemyState newState)
    {
        _currentState = newState;
        //Cambiamos el estado actual de la FSM
    }

    private void UpdateFSM()
    {
        switch (_currentState)
        {
            case eEnemyState.Idle:    HandleIdle();   break;
            case eEnemyState.Patrol:  HandlePatrol(); break;
            case eEnemyState.Chase:   HandleChase();  break;
            case eEnemyState.Attack:  HandleAttack(); break;
            case eEnemyState.Flee:    HandleFlee();   break;
        }
        //Ejecutamos la lógica del estado activo
    }

    // ──────────────────────────────────────────────
    // ESTADOS
    // ──────────────────────────────────────────────

    private void HandleIdle()
    {
        _agent.isStopped = true;

        if (DetectPlayer())
        {
            ChangeState(eEnemyState.Chase);
        }
        //El Roamer espera quieto; si detecta al jugador, pasa a Chase
    }

    private void HandlePatrol()
    {
        _agent.isStopped = false;
        _agent.speed = _enemyData.moveSpeed;

        if (DetectPlayer())
        {
            _isWaiting = false;
            _waitTimer = 0f;
            ChangeState(eEnemyState.Chase);
            return;
        }

        if (_isWaiting)
        {
            _waitTimer -= Time.deltaTime;
            if (_waitTimer <= 0f)
            {
                _isWaiting = false;
                _currentWaypointIndex = (_currentWaypointIndex + 1) % _waypoints.Length;
                _agent.SetDestination(_waypoints[_currentWaypointIndex].position);
            }
            return;
        }

        if (_waypoints.Length > 0 && !_agent.pathPending && _agent.remainingDistance < 0.5f)
        {
            _isWaiting = true;
            _waitTimer = _waypointWaitTime;
            _agent.isStopped = true;
        }
        //El Guard llega a un waypoint, espera X segundos, y luego va al siguiente en loop
    }

    private void HandleChase()
    {
        _agent.isStopped = false;
        _agent.speed = _enemyData.chaseSpeed;
        _agent.SetDestination(_player.position);

        float _distanceToPlayer = Vector3.Distance(transform.position, _player.position);

        if (_distanceToPlayer <= _enemyData.attackRange)
        {
            ChangeState(eEnemyState.Attack);
        }
        else if (!DetectPlayer() && _enemyType == eEnemyType.Guard)
        {
            ChangeState(eEnemyState.Patrol);
        }
        //Seguimos al jugador; atacamos si está en rango, o volvemos a Patrol si lo perdemos (solo Guard)
    }

    private void HandleAttack()
    {
        _agent.isStopped = true;
        transform.LookAt(_player);

        // Aquí conectarías la lógica de daño real con tu sistema de vida del jugador
        // _player.GetComponent<PlayerHealth>().TakeDamage(_enemyData.attackDamage);

        if (Vector3.Distance(transform.position, _player.position) > _enemyData.attackRange)
        {
            ChangeState(eEnemyState.Chase);
        }
        //Atacamos al jugador mientras está en rango; si se aleja, volvemos a Chase
    }

    private void HandleFlee()
    {
        _agent.isStopped = false;
        _agent.speed = _enemyData.chaseSpeed;

        Vector3 _fleeDirection = (transform.position - _player.position).normalized;
        Vector3 _fleeTarget = transform.position + _fleeDirection * _detectionRange;
        _agent.SetDestination(_fleeTarget);

        if (Vector3.Distance(transform.position, _player.position) > _detectionRange * 1.5f)
        {
            ChangeState(eEnemyState.Idle);
        }
        //Huimos del jugador en dirección contraria; volvemos a Idle al alcanzar distancia segura
    }

    // ──────────────────────────────────────────────
    // DETECCIÓN
    // ──────────────────────────────────────────────

    private bool DetectPlayer()
    {
        float _distanceToPlayer = Vector3.Distance(transform.position, _player.position);

        // Detección por proximidad: si el jugador está muy cerca, lo detectamos sin necesidad de visión
        if (_distanceToPlayer <= _proximityRange)
        {
            return true;
        }

        // Detección por raycast: comprobamos ángulo de visión y línea de visión directa
        if (_distanceToPlayer <= _detectionRange)
        {
            Vector3 _directionToPlayer = (_player.position - transform.position).normalized;
            float _angleToPlayer = Vector3.Angle(transform.forward, _directionToPlayer);

            if (_angleToPlayer <= _fieldOfView * 0.5f)
            {
                Vector3 _rayOrigin = transform.position + Vector3.up * 0.8f;
                RaycastHit _hit;

                if (Physics.Raycast(_rayOrigin, _directionToPlayer, out _hit, _detectionRange))
                {
                    return _hit.collider.CompareTag("Player");
                    //Si el primer objeto que toca el rayo es el Player, hay línea de visión directa
                }
            }
        }

        return false;
        //Devolvemos false si no hay detección por ninguno de los dos métodos
    }

    // ──────────────────────────────────────────────
    // VIDA
    // ──────────────────────────────────────────────

    public void TakeDamage(float damage)
    {
        _currentHealth -= damage;

        if (_currentHealth <= 0f)
        {
            Die();
        }
        //Restamos vida; si llega a 0 llamamos a Die()
    }

    private void Die()
    {
        Destroy(gameObject);
        //Destruimos el NPC al morir; aquí podrías añadir animación de muerte o drop de items
    }

    // ──────────────────────────────────────────────
    // GIZMOS (visibles en el editor de Unity)
    // ──────────────────────────────────────────────

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, _detectionRange);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, _proximityRange);
        //Dibujamos en el editor el rango de detección (amarillo) y proximidad (rojo) para ajustar visualmente
    }
}
