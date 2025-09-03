using System.Collections;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance;

    [Header("Database")]
    public LevelDatabase levelDatabase;

    [Header("Level Info")]
    public LevelDataSO currentLevelData;
    public GameObject DefenseLine;
    public GameObject Camera;

    public double goldReceivedinWheel = 0;
    public double diamondReceivedinWheel = 0;

    private void Awake()
    {
        Instance = this;
    }

    void Start()
    {
       
    }
    public void LoadLevelData(int levelIndex)
    {
        currentLevelData = levelDatabase.GetLevel(levelIndex - 1);
        if (currentLevelData == null)
        {
            Debug.LogError($"Không tìm th?y Level {levelIndex} trong LevelDatabase");
            return;
        }

        SetupLevel(currentLevelData);

        EnemyPoolManager.Instance.ReloadPool(currentLevelData);
    }
    private IEnumerator DelayedToShowRoulleteWin()
    {
        yield return new WaitForSeconds(2f);
        UIManager.Instance.SetState(UIManager.GameState.MainMenu);
        LoadLevelData(DataManager.Instance.data.currentLevel);
        AllyPoolManager.Instance.ReturnAllToPools();
        AllyPoolManager.Instance.DestroyAllFromPools();
        AllyPoolManager.Instance.transform.gameObject.SetActive(false);
        EnergyInGame.Instance.ResetEnergy();
        ProjectilesPoolManager.Instance.DestroyAll();
        ProjectilesPoolManager.Instance.gameObject.SetActive(false);
        ResetAllWave();
    }
    private IEnumerator DelayedToShowRoulleteLose()
    {
        yield return new WaitForSeconds(2f);
        UIManager.Instance.SetState(UIManager.GameState.MainMenu);
        LoadLevelData(DataManager.Instance.data.currentLevel);

        EnergyInGame.Instance.ResetEnergy();
        AllyPoolManager.Instance.ReturnAllToPools();
        AllyPoolManager.Instance.DestroyAllFromPools();
        AllyPoolManager.Instance.transform.gameObject.SetActive(false);
        ProjectilesPoolManager.Instance.DestroyAll();
        ProjectilesPoolManager.Instance.gameObject.SetActive(false);
        ResetAllWave();
    }

    public void SetupLevel(LevelDataSO levelData)
    {
        DefenseLine.transform.position = levelData.defenseLinePosition;

        FloatingCamera floatingCam = Camera.GetComponent<FloatingCamera>();
        if (floatingCam != null)
        {
            floatingCam.SetBasePosition(levelData.cameraPosition);
        }
        else
        {
            Camera.transform.position = levelData.cameraPosition;
        }
        
        // N?u b?n có spawner khác, g?i nó t?i ?ây
        // FindObjectOfType<WaveEnemyController>()?.LoadWave(levelData);
    }
  
   public void ResetAllWave()
    {
        var waveSpawner = FindObjectOfType<WaveEnemyController>();
        if (waveSpawner != null)
        {
            waveSpawner.ResetEnemy();

        }
    }
    public void Spawn()
    {
        var waveSpawner = FindObjectOfType<WaveEnemyController>();
        if (waveSpawner != null)
        {
            waveSpawner.SpawnEnemy();

        }
    }
    public void OnLevelCompleteWin(double goldReceived, double diamondReceived)
    {
        goldReceivedinWheel= goldReceived;
        diamondReceivedinWheel = diamondReceived;
        CurrencyManager.Instance.AddGold(goldReceivedinWheel);
        CurrencyManager.Instance.AddDiamond(diamondReceivedinWheel);

        DataManager.Instance.data.currentLevel++;

        DataManager.Instance.data.DiamondReceivedAfterLevel += 4;
        DataManager.Instance.data.GoldReceivedAfterLevel *= 1.5;
        DataManager.Instance.SaveData();
        StartCoroutine(DelayedToShowRoulleteWin());
        
       
       
    }

    public void OnLevelCompleteLoose(double goldReceived, double diamondReceived)
    {
        goldReceivedinWheel = goldReceived;
        diamondReceivedinWheel =diamondReceived;
        CurrencyManager.Instance.AddGold(goldReceivedinWheel);
        CurrencyManager.Instance.AddDiamond(diamondReceivedinWheel);
        DataManager.Instance.SaveData();

        StartCoroutine(DelayedToShowRoulleteLose());
        

    }

    public void UpdateGoldAndDiamondAfterWin()
    {
        goldReceivedinWheel = DataManager.Instance.data.GoldReceivedAfterLevel;
        diamondReceivedinWheel = DataManager.Instance.data.currentLevel % 3 == 0
            ? DataManager.Instance.data.DiamondReceivedAfterLevel
            : 0;
    }

    public void UpdateGoldAndDiamondAfterLoose()
    {
        goldReceivedinWheel = DataManager.Instance.data.GoldReceivedAfterLevel;
        diamondReceivedinWheel = 0;
    }
   
  
}
