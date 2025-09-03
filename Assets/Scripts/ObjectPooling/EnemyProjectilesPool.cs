using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyProjectilesPool : MonoBehaviour
{
    public static EnemyProjectilesPool Instance { get; private set; }
    public GameObject projectilePrefab;
    [SerializeField] private int poolSize = 100;

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
    public void Start()
    {
      


        InitializePool();
    }
    private void InitializePool()
    {
        for (int i = 0; i < poolSize; i++)
        {
            GameObject projectile = Instantiate(projectilePrefab);
            projectile.SetActive(false); // Deactivate immediately
            availableProjectiles.Add(projectile);
        }
    }


    public GameObject GetProjectile()
    {
        if (availableProjectiles.Count > 0)
        {
            GameObject projectile = availableProjectiles[0]; // Get the first available projectile
            availableProjectiles.RemoveAt(0); // Remove it from the list
                                              // Activate before returning
            return projectile;
        }
        else
        {
            // If the pool is empty, create a new projectile
            GameObject newProjectile = Instantiate(projectilePrefab);
            Debug.LogWarning("Projectiles pool exhausted! Instantiating new projectile. Consider increasing pool size.");
            return newProjectile;
        }
    }

    public void ReturnToPool(GameObject projectile)
    {
        projectile.SetActive(false); // Deactivate before returning to pool
        projectile.transform.SetParent(null); // Optional: Detach from any parent for cleanliness
        availableProjectiles.Add(projectile);
    }
}
