using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(fileName = "Barrier_AllyConfigLevel", menuName = "Data/AllyData/ModelPrefabConfig/Barrier_AllyConfigLevel")]
public class Barrier_AllyConfigLevel : ScriptableObject
{
    public List<AllyBarrierStats> upgradeLevels;
    public AllyBarrierStats GetStats(int tier, int sublevel)
    {
        return upgradeLevels.Find(stat => stat.tier == tier && stat.sublevel == sublevel);
    }
}
