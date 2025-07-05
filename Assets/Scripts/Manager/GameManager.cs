using UnityEngine;
using System; // 引入System命名空间以使用Action
using System.Collections;

public class GameManager : MonoSingleton<GameManager>
{
    // --- 事件定义 (观察者模式核心) ---
    // 当分数更新时触发的事件
    public event Action<int, int> OnScoreChanged;
    // 当时间更新时触发的事件
    public event Action<int> OnTimeChanged;

    // 当游戏状态改变时触发的事件 (准备、开始、胜利、失败)
    public enum GameState { Ready, Playing, Victory, Failure, Store, Pause }
    public event Action<GameState> OnGameStateChanged;

    private ClawController clawController;
    [SerializeField] private int currentScore;
    [SerializeField] private int lastLevelTotalScore;

    [SerializeField] private float currentTime;
    [SerializeField] private int currentTargetScore;

    [SerializeField] private GameState currentGameState = GameState.Pause;

    private float scoreMultiplier = 1f;
    private Coroutine luckyCloverCoroutine;

    private WaitForSeconds waitForSeconds;
    public GameState CurrentGameState
    {
        get { return currentGameState; }
        set
        {
            if (currentGameState != value)
            {
                currentGameState = value;
                OnGameStateChanged?.Invoke(currentGameState);
                SetClawEnable();

                // 当游戏状态改变时，检查是否要清除增益效果
                if (currentGameState != GameState.Playing && currentGameState != GameState.Pause)
                {
                    ClearLuckyCloverEffect();
                }
            }
        }
    }
    private void SetClawEnable()
    {
        clawController.enabled = CurrentGameState == GameState.Playing;
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
        clawController.SetClaw(levelData.spawnAreaProfile.spawnBounds.yMin - 0.5f, levelData.spawnAreaProfile.spawnBounds.xMax + 0.5f);
        //CurrentGameState = GameState.Ready; // 关卡加载后，游戏正式开始

        // 发布初始游戏状态事件
        OnScoreChanged?.Invoke(currentScore, lastLevelTotalScore);
        OnTimeChanged?.Invoke(Mathf.CeilToInt(currentTime));
        //OnGameStateChanged?.Invoke(CurrentGameState);

        Debug.Log("GameManager received level data and is now Playing.");
    }

    void Start()
    {
        // GameManager在开始时处于"准备"状态，等待LevelManager加载关卡
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

    public void AddTime(float timeToAdd)
    {
        currentTime += timeToAdd;
        OnTimeChanged?.Invoke(Mathf.CeilToInt(currentTime)); // Immediately update UI
        Debug.Log($"Added {timeToAdd}s to the timer.");
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
        //clawController.enabled = false;
    }

    public void AddScore(int value)
    {
        if (CurrentGameState != GameState.Playing) return;
        int scoreToAdd = Mathf.RoundToInt(value * scoreMultiplier);
        Debug.Log($"Adding score: {value} * {scoreMultiplier} = {scoreToAdd}");
        currentScore += scoreToAdd;
        OnScoreChanged?.Invoke(currentScore, lastLevelTotalScore + currentScore);
        if (!LevelManager.Instance.CheckIsTresureActive())
        {
            LevelEnd();
        }
    }

    public int GetTotalScore()
    {
        return lastLevelTotalScore + currentScore;
    }

    public void SpendScore(int amount)
    {
        if (amount > GetTotalScore())
        {
            Debug.LogError("Trying to spend more score than available.");
            return;
        }

        if (currentScore >= amount)
        {
            currentScore -= amount;
        }
        else
        {
            int remainingAmount = amount - currentScore;
            currentScore = 0;
            lastLevelTotalScore -= remainingAmount;
        }

        OnScoreChanged?.Invoke(currentScore, GetTotalScore());
    }

    public void ActivateLuckyClover(float multiplier, float duration)
    {
        // 开始新效果前，先清除旧的
        ClearLuckyCloverEffect();
        luckyCloverCoroutine = StartCoroutine(LuckyCloverCoroutine(multiplier, duration));
    }

    private void ClearLuckyCloverEffect()
    {
        if (luckyCloverCoroutine != null)
        {
            StopCoroutine(luckyCloverCoroutine);
            scoreMultiplier = 1f;
            luckyCloverCoroutine = null;
            Debug.Log("Lucky Clover effect cleared.");
        }
    }

    private IEnumerator LuckyCloverCoroutine(float multiplier, float duration)
    {
        Debug.Log($"Lucky Clover activated! Score multiplier: {multiplier} for {duration}s.");
        scoreMultiplier = multiplier;

        float timer = duration;
        while (timer > 0)
        {
            if (CurrentGameState == GameState.Playing)
            {
                timer -= Time.deltaTime;
            }
            yield return null; // 每帧等待
        }

        Debug.Log("Lucky Clover wore off.");
        scoreMultiplier = 1f;
        luckyCloverCoroutine = null;
    }

    public void StartGame()
    {
        this.CurrentGameState = GameState.Playing; // 设置游戏状态为Playing
    }
    public void StartReady(emLoadLevelType emLoadLevel = emLoadLevelType.LoadNextLevel)
    {
        if(emLoadLevel == emLoadLevelType.LoadFirstLevel)
        {
            lastLevelTotalScore = 0;
        }
        else if(emLoadLevel == emLoadLevelType.LoadNextLevel)
        {
            lastLevelTotalScore += currentScore; // 累加上一关的总分
        }

        currentScore = 0;
        if(LevelManager.Instance.StartLoadLevel(emLoadLevel)) 
            CurrentGameState = GameState.Ready; // 设置游戏状态为准备
    }
    public void PauseGame()
    {
        if (CurrentGameState == GameState.Playing)
        {
            CurrentGameState = GameState.Pause; // 暂停游戏
        }
        else if (CurrentGameState == GameState.Pause)
        {
            CurrentGameState = GameState.Playing; // 恢复游戏
        }
    }
}