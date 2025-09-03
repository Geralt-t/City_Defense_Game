using UnityEngine;

[CreateAssetMenu(fileName = "AllySpecialUnitStats", menuName = "Data/AllyData/AllySpecialUnitStats")]
public class AllySpecialUnitStats : ScriptableObject
{
    public float moveSpeed = 2.5f;
    public float detectionRange = 8f;
    public float health = 300f;
    public float damage = 25f;
    public float attackRange = 1.5f;
}
