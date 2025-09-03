using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(fileName = "Range_AllyConfigLevel", menuName = "Data/AllyData/ModelPrefabConfig/Range_AllyConfigLevel")]
public class Range_AllyConfigLevel : ScriptableObject
{
    public List<AllyRangeStats> upgradeLevels;
    public AllyRangeStats GetStats(int tier, int sublevel)
    {
        return upgradeLevels.Find(stat => stat.tier == tier && stat.sublevel == sublevel);
    }
}