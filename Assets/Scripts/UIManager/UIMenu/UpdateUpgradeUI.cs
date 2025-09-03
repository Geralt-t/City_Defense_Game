using UnityEngine;
using UnityEngine.UI;
using TMPro;
using static AllyPoolManager;

public class UpdateUpgradeUI : MonoBehaviour
{
    public AllyType type; // lo?i Ally: Meele, Range, Barrier

    [Header("UI Elements")]
    public Image iconAlly;                      // Hình nhân v?t
    public Image[] sublevelProgress;            // 3 ô ti?n trình sublevel
    public TextMeshProUGUI goldCostText;        // Hi?n th? giá vàng
    public Image currencyImage;
    [Header("Config References")]
    public Meele_AllyConfigLevel meleeConfig;
    public Range_AllyConfigLevel rangeConfig;
    public Barrier_AllyConfigLevel barrierConfig;
    [Header("Progress Sprites")]
    public Sprite filledSprite;
    public Sprite emptySprite;
    [Header("Currency Sprites")]
    public Sprite goldSprite;
    public Sprite diamondSprite;
    private int tier;
    private int sub;
    [Header("Text level")]
    public TextMeshProUGUI lowLevel;
    public TextMeshProUGUI highLevel;
    void Start()
    {
        
        UpdateUI();
    }

    public void UpdateUI()
    {
        DataManager.Instance.LoadData();
        // ?? L?y level hi?n t?i
        (tier, sub) = AllyLevelManager.Instance.GetCurrentLevel(type);

        // ?? L?y stats t??ng ?ng
        ScriptableObject stats = null;
        switch (type)
        {
            case AllyType.Meele:
                stats = meleeConfig.GetStats(tier, sub);
                break;
            case AllyType.Range:
                stats = rangeConfig.GetStats(tier, sub);
                break;
            case AllyType.Barrier:
                stats = barrierConfig.GetStats(tier, sub);
                break;
        }

        if (stats == null)
        {
            Debug.LogError($"Không tìm th?y stats cho {type} Tier {tier} Sub {sub}");
            return;
        }

        // ?? Hi?n th? icon appearance
        Sprite icon = null;
        int goldCost = 0;
        int diamondCost = 0;
        if (stats is AllyMeleeStats meleeStats)
        {
            icon = meleeStats.icon;
            goldCost = meleeStats.goldCost;
            diamondCost = meleeStats.diamondCost;
        }
        else if (stats is AllyRangeStats rangeStats)
        {
            icon = rangeStats.icon;
            goldCost = rangeStats.goldCost;
            diamondCost = rangeStats.diamondCost;
        }
        else if (stats is AllyBarrierStats barrierStats)
        {
            icon = barrierStats.icon;
            goldCost = barrierStats.goldCost;
            diamondCost=barrierStats.diamondCost;
        }

        if (icon != null)
        {
            iconAlly.sprite = icon;
        }

        // ?? C?p nh?t s? ti?n
        if (sub < AllyLevelManager.Instance.maxSublevelPerTier)
        {
            goldCostText.text = FormatGold(goldCost);
            currencyImage.sprite=goldSprite;
        }
        if (sub == AllyLevelManager.Instance.maxSublevelPerTier)
        {
            goldCostText.text = FormatGold(diamondCost);
            currencyImage.sprite=diamondSprite;
        }
        // ?? C?p nh?t ti?n trình sublevel (3 ô t??ng ?ng)
        for (int i = 0; i < sublevelProgress.Length; i++)
        {
            if (i < sub )
                sublevelProgress[i].sprite = filledSprite;
            else
                sublevelProgress[i].sprite = emptySprite;
        }
        lowLevel.text = tier.ToString();
        highLevel.text = (tier+1).ToString();
    }
    
    private string FormatGold(int amount)
    {
        if (amount >= 1000000)
            return (amount / 1000000f).ToString("0.#") + "M";
        if (amount >= 1000)
            return (amount / 1000f).ToString("0.#") + "k";
        return amount.ToString();
    }
    public void OnClickUpgrade()
    {
        if (AllyLevelManager.Instance.UpgradeAlly(type) == true)
        {
            UpdateUI();
            AudioManager.Instance.PlaySequenceWithStaggeredDelay(0.3f,"upgrade", "upgrade2", "upgrade3", "upgrade4");
        }

        else return;
    }
}