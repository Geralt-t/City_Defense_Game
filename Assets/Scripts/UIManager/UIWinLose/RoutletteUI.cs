using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

[System.Serializable]
public class RewardSlot
{
    public double label;       
    public float startAngle;   
    public float endAngle;     
}
public class RoutletteUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI goldText;
    [SerializeField] private TextMeshProUGUI diamondText;
    [SerializeField] private Image gold;
    [SerializeField] private Image diamond;
    public float spinSpeed = 360f;
    public float deceleration = 500f;
    public Button claimButton;
    
    public List<RewardSlot> rewardSlots;

    private bool isSpinning = true;
    private float currentSpeed;
    private bool hasStopped = false;
    private double res;
    private bool isSpinningSoundPlaying = false;

    void Start()
    {
        isSpinning = true;
    currentSpeed = spinSpeed;
        claimButton.onClick.AddListener(StopSpinning);
        UpdateRewardDisplay(LevelManager.Instance.goldReceivedinWheel, LevelManager.Instance.diamondReceivedinWheel);

    }
    private void OnDisable()
    {
        ResetRoulette();
    
    }
    private void OnEnable()
    {
        UpdateRewardDisplay(DataManager.Instance.data.GoldReceivedAfterLevel, DataManager.Instance.data.DiamondReceivedAfterLevel);
    }
    void Update()
{
    if (!hasStopped)
    {
        transform.Rotate(Vector3.forward, -currentSpeed * Time.deltaTime);

        if (isSpinning && !isSpinningSoundPlaying)
        {
            AudioManager.Instance.PlayLoopingSFX("spinsound"); // gi? s? có clip loop tên là "spin_loop"
            isSpinningSoundPlaying = true;
        }

        if (!isSpinning)
        {
            currentSpeed -= Random.Range(200, 500) * Time.deltaTime;
            currentSpeed = Mathf.Max(currentSpeed, 0);

            if (currentSpeed == 0)
            {
                hasStopped = true;

                // D?NG ÂM THANH XOAY
                AudioManager.Instance.StopLoopingSFX("spinsound");
                isSpinningSoundPlaying = false;

                EvaluateReward();
            }
        }
    }
}

    void UpdateRewardDisplay(double goldReceived, double diamondReceived)
    {
       
        Debug.Log("gold "+goldReceived +" diamond "+ diamondReceived);
        if (goldReceived > 0)
        {
            goldText.text = "+" + Mathf.RoundToInt((float)goldReceived).ToString();
            gold.gameObject.SetActive(true);
        }
        if (diamondReceived > 0)
        {
            diamondText.text = "+" + Mathf.RoundToInt((float)diamondReceived).ToString();
            diamond.gameObject.SetActive(true);
        }
    }

    public void StopSpinning()
    {
        AudioManager.Instance.Play("click");
        isSpinning = false;
    }

    void EvaluateReward()
    {
        float angle = transform.eulerAngles.z;
        angle = NormalizeAngle(angle);

        foreach (var slot in rewardSlots)
        {
            if (IsAngleInRange(angle, slot.startAngle, slot.endAngle))
            {
                res = slot.label;
                Debug.Log("res" + res);
                break;
            }
        }
        double goldReceived=LevelManager.Instance.goldReceivedinWheel;
        double diamondReceived = LevelManager.Instance.diamondReceivedinWheel;
        goldReceived *= res;
        diamondReceived *= res;
        if (UIManager.Instance.CurrentState == UIManager.GameState.Win)
        {     
            LevelManager.Instance.OnLevelCompleteWin(goldReceived, diamondReceived);
    }
        else if(UIManager.Instance.CurrentState == UIManager.GameState.Lose) 
        { 
            LevelManager.Instance.OnLevelCompleteLoose(goldReceived, diamondReceived);
        }
        UpdateRewardDisplay(goldReceived, diamondReceived);
    }

    float NormalizeAngle(float angle)
    {
        return (angle + 360f) % 360f;
    }

    bool IsAngleInRange(float angle, float start, float end)
    {
        // Handle wraparound (ex: 350° -> 10°)
        if (start < end)
            return angle >= start && angle < end;
        else
            return angle >= start || angle < end;
    }
    public void ResetRoulette()
    {
        
        isSpinning = true;
        hasStopped = false;
        currentSpeed = spinSpeed;
        res = 1;

        // Reset hi?n th? ph?n th??ng
        gold.gameObject.SetActive(false);
        diamond.gameObject.SetActive(false);
        goldText.text = "";
        diamondText.text = "";

  
    }

}
