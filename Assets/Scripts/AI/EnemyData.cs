using UnityEngine;

[CreateAssetMenu(fileName = "NewEnemyData", menuName = "Proyecto Final/Enemy Data")]
public class EnemyData : ScriptableObject
{
    public string enemyName;
    public float maxHealth;
    public float moveSpeed;
    public float chaseSpeed;
    public float attackRange;
    public float attackDamage;
    //Añadir más estadísticas relevantes para la IA
}
