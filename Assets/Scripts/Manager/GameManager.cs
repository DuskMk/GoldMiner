using UnityEngine;
using System; // 引入System命名空间以使用Action

public class GameManager : MonoSingleton<GameManager>
{
    // --- 事件定义 (观察者模式核心) ---
    // 当分数更新时触发的事件
    public event Action<int> OnScoreChanged;
    // 当时间更新时触发的事件
    public event Action<int> OnTimeChanged;
    //// 当关卡目标更新时触发的事件
    //public event Action<int> OnTargetChanged;
    // 当游戏状态改变时触发的事件 (准备、开始、胜利、失败)
    public enum GameState { Ready, Playing, Victory, Failure } 
    public event Action<GameState> OnGameStateChanged;


    //[Header("游戏设置")]
    //public int targetScore = 2000;
    //public float levelTime = 60f;

    [SerializeField] private int currentScore;
    [SerializeField] private float currentTime;
    [SerializeField] private int currentTargetScore;
    [SerializeField] private GameState currentGameState = GameState.Ready;
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
    private void InitializeLevel(int target, int time)
    {
        currentScore = 0;
        currentTargetScore = target;
        currentTime = time;
        currentGameState = GameState.Playing; // 关卡加载后，游戏正式开始

        // 发布初始游戏状态事件
        OnScoreChanged?.Invoke(currentScore);
        OnTimeChanged?.Invoke(Mathf.CeilToInt(currentTime));
        OnGameStateChanged?.Invoke(currentGameState);

        Debug.Log("GameManager received level data and is now Playing.");
    }

    void Start()
    {
        // GameManager在开始时处于“准备”状态，等待LevelManager加载关卡
        //currentGameState = GameState.Ready;
        OnGameStateChanged?.Invoke(currentGameState);
        
    }

    void Update()
    {
        if (currentGameState != GameState.Playing) return;

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
            currentGameState = GameState.Victory;
        }
        else
        {
            currentGameState = GameState.Failure;
        }

        OnGameStateChanged?.Invoke(currentGameState); // 发布游戏结束状态事件

        // 冻结爪子
        FindObjectOfType<ClawController>().enabled = false;
    }

    public void AddScore(int value)
    {
        if (currentGameState != GameState.Playing) return;
        Debug.Log($"Adding score: {value}");
        currentScore += value;
        OnScoreChanged?.Invoke(currentScore);
    }
}