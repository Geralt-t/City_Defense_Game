using UnityEngine;
using System.Collections.Generic;

public class ProjectilesPoolManager : MonoBehaviour
{
    public static ProjectilesPoolManager Instance { get; private set; }

    [SerializeField] private Range_AllyConfigLevel configLevel;
    [SerializeField] private int poolSize = 100;

    private GameObject projectilePrefab;
    private List<GameObject> availableProjectiles = new List<GameObject>();

    private void Awake()
    {
        // Enforce Singleton pattern
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void OnEnable()
    {
        if(AllyLevelManager.Instance == null)
        {
            return;
        }
        var (tier, sub) = AllyLevelManager.Instance.GetCurrentLevel(AllyPoolManager.AllyType.Range);
        projectilePrefab = configLevel.GetStats(tier, sub).bullet;

        InitializePool();
    }

    private void InitializePool()
    {
        DestroyAll(); // D?n s?ch tr??c khi kh?i t?o l?i (tránh leak b? nh?)

        for (int i = 0; i < poolSize; i++)
        {
            GameObject projectile = Instantiate(projectilePrefab);
            projectile.SetActive(false);
            availableProjectiles.Add(projectile);
        }
    }

    public GameObject GetProjectile()
    {
        if (availableProjectiles.Count > 0)
        {
            GameObject projectile = availableProjectiles[0];
            availableProjectiles.RemoveAt(0);
            return projectile;
        }
        else
        {
            Debug.LogWarning("Projectiles pool exhausted! Instantiating new projectile. Consider increasing pool size.");
            return Instantiate(projectilePrefab);
        }
    }

    public void ReturnToPool(GameObject projectile)
    {
        projectile.SetActive(false);
        projectile.transform.SetParent(null);
        availableProjectiles.Add(projectile);
    }

    public void DestroyAll()
    {
        foreach (var projectile in availableProjectiles)
        {
            if (projectile != null)
                Destroy(projectile);
        }

        availableProjectiles.Clear();
    }
}
