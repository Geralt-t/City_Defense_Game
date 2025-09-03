using UnityEngine;

public class CurrencyManager : MonoBehaviour
{
    public static CurrencyManager Instance;

   
    private void Awake()
    {
        Instance = this;
    }
    public double Gold
    {
        get => DataManager.Instance.data.gold;
        set
        {
            DataManager.Instance.data.gold = value;
           
        }
    }
    public double Diamond
    {
        get => DataManager.Instance.data.diamond;
        set
        {
            DataManager.Instance.data.diamond = value;
            
        }
    }
    public void AddGold(double amount)
    {
        Gold += amount;
    }
    public void AddDiamond(double amount)
    {
        Diamond += amount;
    }
   
}
