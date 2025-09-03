using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class WaveEnemyController : MonoBehaviour
{
    private LevelDataSO levelData;

    void Start()
    {
        levelData = LevelManager.Instance?.currentLevelData;

        if (levelData == null)
        {
            Debug.LogError("[WaveEnemyController] Không tìm th?y LevelData t? LevelManager!");
            return;
        }
    }

    Vector3 GetRandomOffset(float radius)
    {
        Vector2 random2D = Random.insideUnitCircle * radius;
        return new Vector3(random2D.x, 0f, random2D.y);
    }

    public void SpawnEnemy()
    {
        levelData = LevelManager.Instance?.currentLevelData;
        if (levelData == null)
        {
            Debug.LogError("[WaveEnemyController] Không tìm th?y LevelData sau reset!");
            return;
        }
        StartCoroutine(SpawnAllSpawnPoints());
    }

    IEnumerator SpawnAllSpawnPoints()
    {
        yield return new WaitForSeconds(levelData.waveEnemyStats.delayTimeStartWave);

        foreach (var spawnPointData in levelData.waveEnemyStats.spawnPointsWithWaves)
        {
            StartCoroutine(SpawnWaveAtPoint(spawnPointData));
            yield return new WaitForSeconds(levelData.waveEnemyStats.delayBetweenWave);
        }
    }

    IEnumerator SpawnWaveAtPoint(SpawnPointWithWave spawnData)
    {
        Vector3 spawnPoint = spawnData.spawnPosition;
        Wave wave = spawnData.wave;

        // Spawn Melee Enemies
        for (int i = 0; i < wave.enemyMeeleCount; i++)
        {
            yield return StartCoroutine(SpawnIndividualEnemy(wave.enemyMeelePrefab, spawnPoint, 0.2f));
            yield return new WaitForSeconds(0.1f);
        }

        // Spawn Ranged Enemies
        for (int i = 0; i < wave.enemyRangeCount; i++)
        {
            yield return StartCoroutine(SpawnIndividualEnemy(wave.enemyRangePrefab, spawnPoint, 0.2f));
            yield return new WaitForSeconds(0.1f);
        }
    }

    IEnumerator SpawnIndividualEnemy(GameObject enemyPrefab, Vector3 spawnPoint, float offsetRadius)
    {
        Vector3 offset = GetRandomOffset(offsetRadius);
        Vector3 desiredPos = spawnPoint + offset;

        // 1. Ki?m tra v? trí trên NavMesh tr??c
        if (NavMesh.SamplePosition(desiredPos, out NavMeshHit hit, 1f, NavMesh.AllAreas))
        {
            GameObject enemyGameObject = EnemyPoolManager.Instance.GetEnemyFromPool(enemyPrefab, Vector3.zero, Quaternion.identity);
            enemyGameObject.SetActive(false);

            Entity entity = enemyGameObject.GetComponentInChildren<Entity>();

            if (entity == null || entity.agent == null)
            {
                Debug.LogError($"[WaveEnemyController] L?i: Entity ho?c NavMeshAgent là null cho k? ??ch {enemyGameObject.name}. Tr? v? pool.");
                EnemyPoolManager.Instance.ReturnToPool(enemyGameObject);
                yield break;
            }

            // ??m b?o agent b? vô hi?u hóa TR??C KHI Warp
            entity.agent.enabled = false;

            // S? d?ng Warp ?? ??t v? trí an toàn cho NavMeshAgent
            entity.agent.Warp(hit.position);

            enemyGameObject.SetActive(true);

            // V?n ??i FixedUpdate ?? agent ?n ??nh
            yield return new WaitForFixedUpdate();
            yield return new WaitForFixedUpdate(); // Có th? c?n 2 frames ?? ch?c ch?n

            if (entity.agent.isOnNavMesh)
            {
                entity.agent.enabled = true;
                entity.ResetEntity();
            }
            else
            {
             
                EnemyPoolManager.Instance.ReturnToPool(enemyGameObject);
            }
        }
       
    }

    public void ResetEnemy()
    {
        StopAllCoroutines();
        EnemyPoolManager.Instance.ResetAllEnemies();

        
    }
}