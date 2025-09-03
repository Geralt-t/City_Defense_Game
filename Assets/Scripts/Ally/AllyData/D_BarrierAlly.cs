using UnityEngine;

[CreateAssetMenu(fileName = "AllyBarrierStats", menuName = "Data/AllyData/AllyBarrierStats")]
public class AllyBarrierStats : ScriptableObject
{
    public float health= 100f;
    public float cost = 3;

    public int tier;
    public int sublevel;
    public int goldCost;
    public int diamondCost;
    public GameObject appearance;
    public Sprite icon;
}
