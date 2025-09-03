using System.Collections.Generic;
using UnityEngine;

public class EnemyPoolManager : MonoBehaviour
{
    public static EnemyPoolManager Instance;

  
    private LevelDataSO levelData;
    private int activeEnemyCount = 0;

    private class EnemyPool
    {
        public GameObject prefab;
        public Queue<GameObject> pooledEnemies = new Queue<GameObject>();
        public int poolSize;
    }

    private Dictionary<GameObject, EnemyPool> prefabToPool = new Dictionary<GameObject, EnemyPool>();

    void Awake()
    {
        Instance = this;

        if (LevelManager.Instance == null || LevelManager.Instance.currentLevelData == null)
        {
            Debug.LogError("[EnemyPoolManager] Không tìm th?y LevelData t? LevelManager.");
            return;
        }

        levelData = LevelManager.Instance.currentLevelData;

        InitializePoolFromWaveData();
        activeEnemyCount = levelData.waveEnemyStats.GetTotalEnemyCount();
    }


    void InitializePoolFromWaveData()
    {
        Dictionary<GameObject, int> prefabCountMap = new Dictionary<GameObject, int>();

        foreach (var spawnPointData in levelData.waveEnemyStats.spawnPointsWithWaves)
        {
            Wave wave = spawnPointData.wave;

            // Meele
            if (wave.enemyMeelePrefab != null)
            {
                if (!prefabCountMap.ContainsKey(wave.enemyMeelePrefab))
                    prefabCountMap[wave.enemyMeelePrefab] = 0;

                prefabCountMap[wave.enemyMeelePrefab] += wave.enemyMeeleCount;
            }

            // Range
            if (wave.enemyRangePrefab != null)
            {
                if (!prefabCountMap.ContainsKey(wave.enemyRangePrefab))
                    prefabCountMap[wave.enemyRangePrefab] = 0;

                prefabCountMap[wave.enemyRangePrefab] += wave.enemyRangeCount;
            }
        }

        // T?o pool
        foreach (var kvp in prefabCountMap)
        {
            GameObject prefab = kvp.Key;
            int requiredCount = kvp.Value;

            if (!prefabToPool.ContainsKey(prefab))
            {
                prefabToPool[prefab] = new EnemyPool
                {
                    prefab = prefab,
                    poolSize = 0
                };
            }

            var pool = prefabToPool[prefab];
            int toAdd = requiredCount - pool.poolSize;

            if (toAdd > 0)
            {
                for (int i = 0; i < toAdd; i++)
                {
                    GameObject obj = Instantiate(prefab);
                    obj.SetActive(false);
                    pool.pooledEnemies.Enqueue(obj);
                }

                pool.poolSize += toAdd;
            }
        }


    }


    public GameObject GetEnemyFromPool(GameObject prefab, Vector3 position, Quaternion rotation)
    {
        if (!prefabToPool.ContainsKey(prefab))
        {
            Debug.LogWarning($"[EnemyPoolManager] No pool found for prefab: {prefab.name}");
            return null;
        }

        var pool = prefabToPool[prefab];
        int originalCount = pool.pooledEnemies.Count;
        
        for (int i = 0; i < originalCount; i++)
        {
            GameObject obj = pool.pooledEnemies.Dequeue();
            
            if (!obj.activeInHierarchy)
            {
                obj.transform.position = position;
                obj.transform.rotation = rotation;
                

               
                pool.pooledEnemies.Enqueue(obj);
                


              
                return obj;
            }
            pool.pooledEnemies.Enqueue(obj);
        }
        // N?u không còn enemy r?nh, t?o m?i
        GameObject newEnemy = Instantiate(prefab, position, rotation);
        newEnemy.SetActive(true);
        pool.pooledEnemies.Enqueue(newEnemy);
        pool.poolSize++;
        return newEnemy;
    }

    public void ReturnToPool(GameObject enemy)
    {
        enemy.SetActive(false);
        activeEnemyCount--;
        if (activeEnemyCount <= 0&& UIManager.Instance.CurrentState!=UIManager.GameState.Lose)
        {
            UIManager.Instance.SetWin(true);
        }
        Debug.Log(activeEnemyCount);
    }
    public void ResetAllEnemies()
    {
        foreach (var pool in prefabToPool.Values)
        {
            foreach (var enemy in pool.pooledEnemies)
            {
                if (enemy.activeInHierarchy)
                {
                    enemy.SetActive(false);
                }
            }
        }

        activeEnemyCount = levelData.waveEnemyStats.GetTotalEnemyCount();
    }
    public void ReloadPool(LevelDataSO newLevelData)
    {
        levelData = newLevelData;
        InitializePoolFromWaveData();

        activeEnemyCount = levelData.waveEnemyStats.GetTotalEnemyCount();
    }

}
