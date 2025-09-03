using UnityEngine;

public class AllyLevelManager : MonoBehaviour
{
    public static AllyLevelManager Instance;
    public Meele_AllyConfigLevel meeleConfigLevel;
    public Range_AllyConfigLevel rangeConfigLevel;
    public Barrier_AllyConfigLevel barrierConfigLevel;
    [Header("Sublevel")]
    public int maxSublevelPerTier = 3;
    private int goldCost;
    private int diamondCost;
    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    /// <summary>
    /// T?o key l?u trong PlayerPrefs ho?c PlayerData.otherPrefs
    /// </summary>
    private string GetLevelKey(AllyPoolManager.AllyType type)
    {
        return $"Ally{type}_Level";
    }

    /// <summary>
    /// L?y c?p hi?n t?i c?a ally
    /// </summary>
    public (int tier, int sublevel) GetCurrentLevel(AllyPoolManager.AllyType type)
    {
        string key = GetLevelKey(type);

        if (DataManager.Instance.data.otherPrefs.TryGetValue(key, out string levelStr))
        {
            var parts = levelStr.Split('-');
            if (parts.Length == 2 &&
                int.TryParse(parts[0], out int tier) &&
                int.TryParse(parts[1], out int sub))
            {
                return (tier, sub);
            }
        }

        // M?c ??nh
        return (1, 1);
    }

    /// <summary>
    /// L?u c?p ?? ally
    /// </summary>
    public void SetLevel(AllyPoolManager.AllyType type, int tier, int sublevel)
    {
        string key = GetLevelKey(type);
        DataManager.Instance.data.otherPrefs[key] = $"{tier}-{sublevel}";
        DataManager.Instance.SaveData();
    }

    /// <summary>
    /// Nâng c?p c?p ?? ally: t?ng sublevel ho?c tier
    /// </summary>
    public bool UpgradeAlly(AllyPoolManager.AllyType type)
    {

        var (tier, sub) = GetCurrentLevel(type);

     
        if (type == AllyPoolManager.AllyType.Meele)
        {
            goldCost = meeleConfigLevel.GetStats(tier, sub).goldCost;
            diamondCost = meeleConfigLevel.GetStats(tier, sub).diamondCost;
        }
        else if (type == AllyPoolManager.AllyType.Range)
        {
            goldCost = rangeConfigLevel.GetStats(tier, sub).goldCost;
            diamondCost = rangeConfigLevel.GetStats(tier, sub).diamondCost;
        }
        else if (type == AllyPoolManager.AllyType.Barrier)
        {
            goldCost = barrierConfigLevel.GetStats(tier, sub).goldCost;
            diamondCost = barrierConfigLevel.GetStats(tier, sub).diamondCost;
        }
        if (DataManager.Instance.data.gold >= goldCost && DataManager.Instance.data.diamond >= diamondCost)
        {
            if (sub < maxSublevelPerTier)
            {
                sub++;
            }
            else
            {
                tier++;
                sub = 1;

                if (!HasStatsFor(type, tier, sub))
                {
                    return false;
                }
            }

            SetLevel(type, tier, sub);
            DataManager.Instance.data.gold -= goldCost;
            DataManager.Instance.data.diamond -= diamondCost;
            DataManager.Instance.SaveData();
            return true;
        }
        else return false;
    }
    private bool HasStatsFor(AllyPoolManager.AllyType type, int tier, int sub)
    {
        switch (type)
        {
            case AllyPoolManager.AllyType.Meele:
                return meeleConfigLevel.GetStats(tier, sub) != null;
            case AllyPoolManager.AllyType.Range:
                return rangeConfigLevel.GetStats(tier, sub) != null;
            case AllyPoolManager.AllyType.Barrier:
                return barrierConfigLevel.GetStats(tier, sub) != null;
        }
        return false;
    }
    /// <summary>
    /// Reset ally v? tier 1, sublevel 1
    /// </summary>
    public void ResetAllyLevel(AllyPoolManager.AllyType type)
    {
        SetLevel(type, 1, 1);
        Debug.Log($"?ã reset {type} v? c?p 1-1");
    }

    /// <summary>
    /// In t?t c? thông tin level ra log
    /// </summary>
    public void DebugPrintAllLevels()
    {
        foreach (AllyPoolManager.AllyType type in System.Enum.GetValues(typeof(AllyPoolManager.AllyType)))
        {
            var (tier, sub) = GetCurrentLevel(type);
            Debug.Log($"{type}: Tier {tier} - Sub {sub}");
        }
    }
}
