using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "WaveEnemyStats", menuName = "Data/WaveEnemyData")]
public class WaveEnemyStats : ScriptableObject
{
    public float delayTimeStartWave;
    public float delayBetweenWave;

    [Header("M?i spawn point có 1 wave riêng")]
    public List<SpawnPointWithWave> spawnPointsWithWaves;

    public int GetTotalEnemyCount()
    {
        int total = 0;
        foreach (var point in spawnPointsWithWaves)
        {
            total += point.wave.enemyMeeleCount;
            total += point.wave.enemyRangeCount;
        }
        return total;
    }
}

[System.Serializable]
public class SpawnPointWithWave
{
    public Vector3 spawnPosition;
    public Wave wave;
}

[System.Serializable]
public class Wave
{
    [Header("Meele Enemy")]
    public GameObject enemyMeelePrefab;
    public int enemyMeeleCount;

    [Header("Range Enemy")]
    public GameObject enemyRangePrefab;
    public int enemyRangeCount;
}
