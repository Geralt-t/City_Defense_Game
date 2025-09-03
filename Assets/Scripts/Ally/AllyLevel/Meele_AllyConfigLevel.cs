using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(fileName = "Meele_AllyConfigLevel", menuName = "Data/AllyData/ModelPrefabConfig/Meele_AllyConfigLevel")]
public class Meele_AllyConfigLevel : ScriptableObject
{
    public List<AllyMeleeStats> upgradeLevels;
    public AllyMeleeStats GetStats(int tier, int sublevel)
    {
        return upgradeLevels.Find(stat => stat.tier == tier && stat.sublevel == sublevel);
    }
}
