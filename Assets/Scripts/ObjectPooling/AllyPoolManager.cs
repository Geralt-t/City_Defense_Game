using System.Collections.Generic;
using UnityEngine;

public class AllyPoolManager : MonoBehaviour
{
    public static AllyPoolManager Instance;
    public EnergyInGame EnergyInGame;
    [System.Serializable]
    public enum AllyType
    {
        Meele,
        Range,
        Barrier
    }

    [System.Serializable]
    public class AllyPool
    {
        public AllyType type;
        public int size;
    }
    public Meele_AllyConfigLevel meleeConfig;
    public Range_AllyConfigLevel rangeConfig;
    public Barrier_AllyConfigLevel barrierConfig;

    public List<AllyPool> pools;
    private Dictionary<AllyType, Queue<GameObject>> poolDictionary;
    private AllyType? currentSelectedType = null;  // nullable ?? ki?m tra n?u ch?a ch?n
    public Dictionary<AllyType, float> allyCostDictionary;


    private void Awake()
    {
        Instance = this;
    }

    void OnEnable()
    {
        allyCostDictionary = new Dictionary<AllyType, float>();
        poolDictionary = new Dictionary<AllyType, Queue<GameObject>>();

        foreach (var pool in pools)
        {
            Queue<GameObject> objectPool = new Queue<GameObject>();

            // ?? L?y level hi?n t?i
            var (tier, sub) = AllyLevelManager.Instance.GetCurrentLevel(pool.type);

            GameObject prefab = GetPrefabByLevel(pool.type, tier, sub);
            float cost = GetCostByLevel(pool.type, tier, sub);

            if (prefab == null)
            {
                Debug.LogError($"Không tìm th?y prefab cho {pool.type} Tier {tier} - Sub {sub}");
                continue;
            }

            for (int i = 0; i < pool.size; i++)
            {
                GameObject obj = Instantiate(prefab);
                obj.transform.SetParent(this.transform);
                obj.SetActive(false);
                objectPool.Enqueue(obj);
            }

            poolDictionary.Add(pool.type, objectPool);
            allyCostDictionary[pool.type] = cost;
        }

    }
    private GameObject GetPrefabByLevel(AllyType type, int tier, int sub)
    {
        switch (type)
        {
            case AllyType.Meele:
                var meleeStats = meleeConfig.GetStats(tier, sub);
                return meleeStats != null ? meleeStats.appearance : null;

            case AllyType.Range:
                var rangeStats = rangeConfig.GetStats(tier, sub);
                return rangeStats != null ? rangeStats.appearance : null;

            case AllyType.Barrier:
                var barrierStats = barrierConfig.GetStats(tier, sub);
                return barrierStats != null ? barrierStats.appearance : null;

            default:
                return null;
        }
    }

    private float GetCostByLevel(AllyType type, int tier, int sub)
    {
        switch (type)
        {
            case AllyType.Meele:
                var meleeStats = meleeConfig.GetStats(tier, sub);
                return meleeStats != null ? meleeStats.cost : 0;

            case AllyType.Range:
                var rangeStats = rangeConfig.GetStats(tier, sub);
                return rangeStats != null ? rangeStats.cost : 0;

            case AllyType.Barrier:
                var barrierStats = barrierConfig.GetStats(tier, sub);
                return barrierStats != null ? barrierStats.cost : 0;

            default:
                return 0;
        }
    }

    public AllyType? GetSelectedAllyType()
    {
        return currentSelectedType;
    }
    public void SetSelectedAllyType(AllyType type)
    {
        if (poolDictionary.ContainsKey(type))
            currentSelectedType = type;
        else
            Debug.LogWarning("AllyType không t?n t?i: " + type);
    }

    public GameObject GetFromPool(Vector3 position, Quaternion rotation)
    {
        if (currentSelectedType == null || !poolDictionary.ContainsKey(currentSelectedType.Value))
            return null;

        float cost;
        if (!allyCostDictionary.TryGetValue(currentSelectedType.Value, out cost))
            return null;

        if (EnergyInGame != null && !EnergyInGame.CanAfford(cost))
        {
            EnergyInGame.CantAffordAnim();
            return null;
        }

        EnergyInGame.SpendEnergy(cost);

        Queue<GameObject> pool = poolDictionary[currentSelectedType.Value];

        if (pool.Count == 0)
            return null;

        GameObject obj = pool.Dequeue();
        obj.SetActive(true);
        obj.transform.position = position;
        obj.transform.rotation = rotation;

        // Gán obj là con c?a AllyPoolManager
        obj.transform.SetParent(this.transform); // ho?c: obj.transform.parent = this.transform;

        return obj;
    }



    public void ReturnToPool(AllyType type, GameObject obj)
    {
        
       
        if (type == AllyType.Barrier)
        {
            obj.GetComponentInChildren<Barrier>().ResetForm();
        }
        else if (type == AllyType.Range)
        {
            obj.GetComponentInChildren<AllyRange>().ResetForm();
        }
        else if(type == AllyType.Meele)
        {
            obj.GetComponentInChildren<AllyMeele>().ResetForm();
        }
        obj.SetActive(false);

        if (poolDictionary.ContainsKey(type))
        {
            poolDictionary[type].Enqueue(obj);
        }
    }
    public void ReturnAllToPools()
    {
        
        foreach (Transform child in transform)
        {
            GameObject obj = child.gameObject;

            if (obj.activeSelf)
            {
                // Xác ??nh lo?i Ally nào
                if (obj.GetComponentInChildren<AllyMeele>() != null)
                {
                    ReturnToPool(AllyType.Meele, obj);
                   
                }

                else if (obj.GetComponentInChildren<AllyRange>() != null)
                {
                    ReturnToPool(AllyType.Range, obj);
                    
                }
                else if (obj.GetComponentInChildren<Barrier>() != null)
                {
                    ReturnToPool(AllyType.Barrier, obj);
                    
                }
            }
        }
    }
    public void DestroyAllFromPools()
    {
        if (poolDictionary == null) return;

        foreach (var kvp in poolDictionary)
        {
            Queue<GameObject> pool = kvp.Value;

            while (pool.Count > 0)
            {
                GameObject obj = pool.Dequeue();
                if (obj != null)
                {
                    Destroy(obj);
                }
            }
        }

        poolDictionary.Clear(); 
    }
}