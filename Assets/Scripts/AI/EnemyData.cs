using UnityEngine;

[CreateAssetMenu(fileName = "NewEnemyData", menuName = "Proyecto Final/Enemy Data")]
public class EnemyData : ScriptableObject
{
    [Header("Movement")]
    public float moveSpeed;
    public float stopTime;

    [Header("Data")]
    public string enemyName;
    public float maxHealth;

    [Header("Attack")]
    public float chaseSpeed;
    public float attackRange;
    public float attackDamage;
}
