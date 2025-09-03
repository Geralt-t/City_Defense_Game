using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "EnemyRangeStats", menuName = "Data/EnemyData/EnemyRangeStats")]

public class EnemyRangeStats : ScriptableObject
{
    public float MeeleAttackRange = 0.25f;
    public float RangeAttackRange = 0.65f;
    public float speed = 0.5f;
    public float attackCooldown = 1.0f;
    public float health = 30f;
    public float damage = 25f;
    public float rangeDamage = 10f;
    public float energyGets = 2f;
}
