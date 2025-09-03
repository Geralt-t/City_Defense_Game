using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class MainMenuUI : MonoBehaviour
{
    public TextMeshProUGUI goldText;
    public TextMeshProUGUI diamondText;
    public GameObject Settings;
    public TextMeshProUGUI currentLevel;
    void Start()
    {
       
    }

    
    void Update()
    {
        UpdateGoldAndDiamond();
        UpdateLevel();
    }
    public void UpdateGoldAndDiamond()
    {
        goldText.text = ((int)Math.Round(DataManager.Instance.data.gold)).ToString();
        diamondText.text = ((int)Math.Round(DataManager.Instance.data.diamond)).ToString();
    }
    public void UpdateLevel()
    {
        currentLevel.text= "Level " + ((DataManager.Instance.data.currentLevel)).ToString();
    }
    public void OnClickPlay()
    {
        UIManager.Instance.SetState(UIManager.GameState.InGame);

        AudioManager.Instance.Play("play");
    }
    public void OnClickOpenSetting()
    {
        Settings.SetActive(true);

        AudioManager.Instance.Play("click");
    }
    public void OnClickCloseSetting()
    {
        Settings.SetActive(false);
        AudioManager.Instance.Play("click");
    }
}
