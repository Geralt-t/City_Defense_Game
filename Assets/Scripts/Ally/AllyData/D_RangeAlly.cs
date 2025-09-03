using UnityEngine;

[CreateAssetMenu(fileName = "AllyRangeStats", menuName = "Data/AllyData/AllyRangeStats")]
public class AllyRangeStats : ScriptableObject
{
    public GameObject bullet;
    public float attackDamage = 20;
    public float attackRange=2f;
    public float health = 20f;
    public float cost = 32;

    public int tier;
    public int sublevel;
    public int goldCost;
    public int diamondCost;
    public GameObject appearance;
    public Sprite icon;
}
