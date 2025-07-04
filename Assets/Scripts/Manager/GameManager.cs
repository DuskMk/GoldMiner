using UnityEngine;
using System; // 引入System命名空间以使用Action

public class GameManager : MonoSingleton<GameManager>
{
    // --- 事件定义 (观察者模式核心) ---
    // 当分数更新时触发的事件
    public event Action<int, int> OnScoreChanged;
    // 当时间更新时触发的事件
    public event Action<int> OnTimeChanged;
    
    // 当游戏状态改变时触发的事件 (准备、开始、胜利、失败)
    public enum GameState { Ready, Playing, Victory, Failure, Pause } 
    public event Action<GameState> OnGameStateChanged;

    private ClawController clawController;
    [SerializeField] private int currentScore;
    [SerializeField] private int totalScore;

    [SerializeField] private float currentTime;
    [SerializeField] private int currentTargetScore;

    [SerializeField] private GameState currentGameState = GameState.Pause;
    public GameState CurrentGameState
    {
        get { return currentGameState; }
        set
        {
            if (currentGameState != value)
            {
                currentGameState = value;
                clawController.enabled = value == GameState.Playing;
                OnGameStateChanged?.Invoke(currentGameState);
            }
        }
    }
    // --- 订阅事件 ---
    void OnEnable()
    {
        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.OnLevelDataLoaded += InitializeLevel;
        }
    }

    void OnDisable()
    {
        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.OnLevelDataLoaded -= InitializeLevel;
        }
    }
    
    // LevelManager通知我们关卡已加载，我们用这些数据来初始化游戏
    private void InitializeLevel(LevelData levelData)
    {
        currentScore = 0;
        currentTargetScore = levelData.targetScore;
        currentTime = levelData.timeLimit;
        clawController.SetClaw(levelData.spawnAreaProfile.spawnBounds.yMin-0.5f, levelData.spawnAreaProfile.spawnBounds.xMax+0.5f);
        //CurrentGameState = GameState.Ready; // 关卡加载后，游戏正式开始

        // 发布初始游戏状态事件
        OnScoreChanged?.Invoke(currentScore, totalScore);
        OnTimeChanged?.Invoke(Mathf.CeilToInt(currentTime));
        //OnGameStateChanged?.Invoke(CurrentGameState);

        Debug.Log("GameManager received level data and is now Playing.");
    }

    void Start()
    {
        // GameManager在开始时处于“准备”状态，等待LevelManager加载关卡
        //CurrentGameState = GameState.Ready;
        clawController = FindObjectOfType<ClawController>();
        //OnGameStateChanged?.Invoke(CurrentGameState);
        
    }

    void Update()
    {
        if (CurrentGameState != GameState.Playing) return;

        currentTime -= Time.deltaTime;
        OnTimeChanged?.Invoke(Mathf.CeilToInt(currentTime)); // 每帧发布时间更新事件

        if (currentTime <= 0)
        {
            LevelEnd();
        }
    }


    void LevelEnd()
    {
        if (currentScore >= currentTargetScore)
        {
            CurrentGameState = GameState.Victory;
        }
        else
        {
            CurrentGameState = GameState.Failure;
        }

        OnGameStateChanged?.Invoke(CurrentGameState); // 发布游戏结束状态事件

        // 冻结爪子
        clawController.enabled = false;
    }

    public void AddScore(int value)
    {
        if (CurrentGameState != GameState.Playing) return;
        Debug.Log($"Adding score: {value}");
        totalScore += value;
        currentScore += value;
        OnScoreChanged?.Invoke(currentScore, totalScore);
    }
    public void StartGame()
    {
        this.CurrentGameState = GameState.Playing; // 设置游戏状态为Playing
    }
    public void StartReady()
    {
        currentScore = 0;
        CurrentGameState = GameState.Ready; // 设置游戏状态为准备
        LevelManager.Instance.StartLoadLevel();
    }
}