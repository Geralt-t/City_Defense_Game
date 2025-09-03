using UnityEngine;
using UnityEngine.UI;
using static AllyPoolManager;

public class SelectableButtonUI : MonoBehaviour
{
    public enum AllyTypeEnum
    {
        Meele,
        Range,
        Barrier
    }

    public AllyTypeEnum allyType;
    public Image backgroundImage;
    public Image imageUnit;
    public Sprite normalSprite;
    public Sprite selectedSprite;
    public Sprite outofEnergy;

    public Meele_AllyConfigLevel meeleConfig;
    public Range_AllyConfigLevel rangeConfig;
    public Barrier_AllyConfigLevel barrierConfig;

    private ArmyButtonManager buttonManager;
    private bool isSelected = false;
    private int tier;
    private int sub;
  
    private void OnEnable()
    {
        buttonManager = GetComponentInParent<ArmyButtonManager>();
       
        if (AllyLevelManager.Instance == null)
        {
            return;
        }
        (tier, sub) = AllyLevelManager.Instance.GetCurrentLevel((AllyPoolManager.AllyType)allyType);
        // ?? L?y stats t??ng ?ng
        ScriptableObject stats = null;
        switch ((AllyPoolManager.AllyType)allyType)
        {
            case AllyType.Meele:
                stats = meeleConfig.GetStats(tier, sub);
                break;
            case AllyType.Range:
                stats = rangeConfig.GetStats(tier, sub);
                break;
            case AllyType.Barrier:
                stats = barrierConfig.GetStats(tier, sub);
                break;
        }
        if (stats is AllyMeleeStats meleeStats)
        {
            imageUnit.sprite = meleeStats.icon;
        }
        else if (stats is AllyRangeStats rangeStats)
        {
            imageUnit.sprite = rangeStats.icon;
        }
        else if (stats is AllyBarrierStats barrierStats)
        {
            imageUnit.sprite = barrierStats.icon;
        }
    }

    private void Update()
    {
        float cost = GetAllyCost(allyType);
        float currentEnergy = EnergyInGame.Instance.currentEnergy;

        if (currentEnergy < cost && !isSelected)
        {
            backgroundImage.sprite = outofEnergy;
        }
        else if (!isSelected) // ch? ??i v? n?u ch?a ???c ch?n
        {
            backgroundImage.sprite = normalSprite;
        }
    }

    public void OnClickSelect()
    {
        float cost = GetAllyCost(allyType);
        float currentEnergy = EnergyInGame.Instance.currentEnergy;

        if (currentEnergy < cost)
        {
            // Không ???c ch?n
            return;
        }

        AllyPoolManager.Instance.SetSelectedAllyType((AllyPoolManager.AllyType)allyType);

        if (buttonManager != null)
        {
            buttonManager.SelectButton(this);
        }
    }

    public void SetSelected(bool selected)
    {
        isSelected = selected;
        backgroundImage.sprite = selected ? selectedSprite : normalSprite;
    }

    private float GetAllyCost(AllyTypeEnum type)
    {
        switch (type)
        {
            case AllyTypeEnum.Meele:
                return AllyPoolManager.Instance.allyCostDictionary[AllyPoolManager.AllyType.Meele];
            case AllyTypeEnum.Range:
                return AllyPoolManager.Instance.allyCostDictionary[AllyPoolManager.AllyType.Range];
            case AllyTypeEnum.Barrier:
                return AllyPoolManager.Instance.allyCostDictionary[AllyPoolManager.AllyType.Barrier];
            default:
                return 0f;
        }
    }
}
