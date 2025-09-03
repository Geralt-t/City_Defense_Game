using UnityEngine;

[CreateAssetMenu(fileName = "AllyMeleeStats", menuName = "Data/AllyData/AllyMeleeStats")]
public class AllyMeleeStats : ScriptableObject
{
    public float moveSpeed = 2.5f;
    public float detectionRange = 8f;
    public float health = 50f;
    public float attackRange = 1.5f;
    public float cost = 8;
    public float damage = 20;

    public int tier;
    public int sublevel;
    public int goldCost;
    public int diamondCost;
    public GameObject appearance;
    public Sprite icon;
}
