using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "EnemyMeeleStats", menuName = "Data/EnemyData/EnemyMeeleStats")]

public class EnemyMeeleStats : ScriptableObject
{
    public float attackRange = 0.25f;
    public float detectionRange = 0.65f;
    public float speed = 0.5f;
    public float attackCooldown = 1.0f;
    public float health=50f;
    public float damage = 10f;
    public float energyGets = 1;
}
