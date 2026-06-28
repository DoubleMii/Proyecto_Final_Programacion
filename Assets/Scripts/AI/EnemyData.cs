using UnityEngine;

[CreateAssetMenu(fileName = "NewEnemyData", menuName = "Proyecto Final/Enemy Data")]
public class EnemyData : ScriptableObject
{
    [Header("Movement")]
    public float moveSpeed  = 3f;
    public float chaseSpeed = 5.5f;
    public float stopTime   = 1f;

    [Header("Data")]
    public string enemyName;
    public float  maxHealth;

    [Header("Attack / Captura")]
    public float attackRange    = 1.5f;
    public float attackCooldown = 1.2f; // Segundos de delay dramático antes de disparar la muerte

    [Header("Búsqueda (Search)")]
    public float searchTime        = 6f;  // Segundos buscando antes de rendirse
    public float maxChaseDistance  = 25f; // (Pursuer) Distancia máxima de persecución

    [Header("Roamer")]
    public float roamRadius   = 10f; // Radio de deambulación aleatoria
    public float roamWaitTime = 2f;  // Espera entre puntos de roam

    [Header("Detección Auditiva")]
    public float hearingRange = 5f; // Rango en el que el enemigo oye al jugador correr

    [Header("Sospecha (Peripheral Vision)")]
    public float suspicionSpeed = 1.5f; // Velocidad a la que aumenta la sospecha (unidades/segundo)

    [Header("Alerta Grupal")]
    public float alertRadius = 12f; // Radio en el que alerta a otros enemigos al ver al jugador
}
