using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    private bool isPlay = false;
    [SerializeField]
    private GameObject allyPoolManager;
    [SerializeField]
    private GameObject projectilePoolManager;
    void Start()
    {
        
        
    }

    // Update is called once per frame
    void Update()
    {
        DataManager.Instance.LoadData();
        if (UIManager.Instance.CurrentState == UIManager.GameState.InGame&& isPlay==false)
        {
            StartGame();
            isPlay = true;
        }
        if(UIManager.Instance.CurrentState == UIManager.GameState.MainMenu && isPlay == true)
        {
            isPlay = false;
        }
    }
    private void StartGame()
    {
        LevelManager.Instance.LoadLevelData(DataManager.Instance.data.currentLevel);
        LevelManager.Instance.Spawn();
        EnergyInGame.Instance.ResetEnergy();
        allyPoolManager.SetActive(true);
        projectilePoolManager.SetActive(true);
    }
}
