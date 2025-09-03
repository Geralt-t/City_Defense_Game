using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class UIInGame : MonoBehaviour
{

    public void OnClickButton()
    {
        UIManager.Instance.SetState(UIManager.GameState.MainMenu);
        AudioManager.Instance.Play("click");
        ResetEnemyAndAlly();
    }
    public void ResetEnemyAndAlly()
    {
        AllyPoolManager.Instance.ReturnAllToPools();
        LevelManager.Instance.ResetAllWave();
        AllyPoolManager.Instance.DestroyAllFromPools();
        AllyPoolManager.Instance.gameObject.SetActive(false);
        ProjectilesPoolManager.Instance.DestroyAll();
        ProjectilesPoolManager.Instance.gameObject.SetActive(false);
    }
}
