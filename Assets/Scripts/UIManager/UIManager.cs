using UnityEngine;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    public GameObject UIIngame;
    public GameObject UIWin;
    public GameObject UILose;
    public GameObject UIMainMenu;
    public enum GameState
    {
        Loading,
        MainMenu,
        InGame,
        Win,
        Lose
    }
    [SerializeField]
    GameObject buttonNuke;
    [SerializeField]
    GameObject buttonSpecial;
    [SerializeField]
    GameObject playerController;
    public bool isWin { get; private set; }
    public bool isLose { get; private set; }
    public bool isInMenu { get; private set; }
    
    public void SetWin(bool value)
    {
        isWin = value;
        SetState(GameState.Win);
    }
    public void SetLose(bool value)
    {
        isLose = value;
        SetState(GameState.Lose);
    }
    public GameState CurrentState { get; private set; }

    public static UIManager Instance { get; private set; }

    

    void Awake()
    {

        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        SetState(GameState.MainMenu);
    }

    public void SetState(GameState newState)
    {
        
        CurrentState = newState;
        Debug.Log("Chuy?n tr?ng thái game sang: " + newState);

        switch (newState)
        {
            case GameState.Loading:
                HandleLoading ();
                break;
            case GameState.MainMenu:
                HandleMainMenu();
                break;
            case GameState.InGame:
                HandleInGame();
                break;
            case GameState.Win:
                HandleWin();
                break;
            case GameState.Lose:
                HandleLose();
                break;
        }
    }
    void HandleLoading()
    {

    }    
    void HandleMainMenu()
    {
        isInMenu = true;

        UIIngame.SetActive(false) ;
        UIWin.SetActive(false);
        UILose.SetActive(false);
        UIMainMenu.SetActive(true);
       playerController.SetActive(false);
    }

    void HandleInGame()
    {
        
        isLose = false;
        int currentLevel = DataManager.Instance.data.currentLevel;
        Debug.Log("currentLevel"+currentLevel);
        if (currentLevel < 3)
        {
            buttonNuke.SetActive(false);
            buttonSpecial.SetActive(false);
        }
        if (currentLevel >= 3 && currentLevel <= 5)
        {
            buttonSpecial.SetActive(true);
            buttonNuke.SetActive(false);
        }
        else if(currentLevel >=6)
        {
            buttonNuke.SetActive(true);
            buttonSpecial.SetActive(true);
        }
        UIIngame.SetActive(true);
        UIWin.SetActive(false);
        UILose.SetActive(false);
        UIMainMenu.SetActive(false);
        DataManager.Instance.LoadData();
        playerController.SetActive(true);
    }

    void HandleWin()
    {
        AudioManager.Instance.Play("levelWin");
        UIIngame.SetActive(false);
        UIWin.SetActive(true);
        LevelManager.Instance.UpdateGoldAndDiamondAfterWin();
        UIMainMenu.SetActive(false);
        UILose.SetActive(false);
        playerController.SetActive(false);
    }

    void HandleLose()
    {
        AudioManager.Instance.Play("levelLose");
        isLose = true;
      UIIngame.SetActive(false);
        UILose.SetActive(true);
        LevelManager.Instance.UpdateGoldAndDiamondAfterLoose ();
        playerController.SetActive(false);
    }
    
    public GameState GetCurrentState()
    {
        return CurrentState;
    }
}
