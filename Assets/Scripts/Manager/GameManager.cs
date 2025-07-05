using UnityEngine;
using System; // ����System�����ռ���ʹ��Action

public class GameManager : MonoSingleton<GameManager>
{
    // --- �¼����� (�۲���ģʽ����) ---
    // ����������ʱ�������¼�
    public event Action<int, int> OnScoreChanged;
    // ��ʱ�����ʱ�������¼�
    public event Action<int> OnTimeChanged;

    // ����Ϸ״̬�ı�ʱ�������¼� (׼������ʼ��ʤ����ʧ��)
    public enum GameState { Ready, Playing, Victory, Failure, Pause }
    public event Action<GameState> OnGameStateChanged;

    private ClawController clawController;
    [SerializeField] private int currentScore;
    [SerializeField] private int lastLevelTotalScore;

    [SerializeField] private float currentTime;
    [SerializeField] private int currentTargetScore;

    [SerializeField] private GameState currentGameState = GameState.Pause;

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
            }
        }
    }
    private void SetClawEnable()
    {
        clawController.enabled = CurrentGameState == GameState.Playing;
    }

    // --- �����¼� ---
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

    // LevelManager֪ͨ���ǹؿ��Ѽ��أ���������Щ��������ʼ����Ϸ
    private void InitializeLevel(LevelData levelData)
    {
        currentScore = 0;
        currentTargetScore = levelData.targetScore;
        currentTime = levelData.timeLimit;
        clawController.SetClaw(levelData.spawnAreaProfile.spawnBounds.yMin - 0.5f, levelData.spawnAreaProfile.spawnBounds.xMax + 0.5f);
        //CurrentGameState = GameState.Ready; // �ؿ����غ���Ϸ��ʽ��ʼ

        // ������ʼ��Ϸ״̬�¼�
        OnScoreChanged?.Invoke(currentScore, lastLevelTotalScore);
        OnTimeChanged?.Invoke(Mathf.CeilToInt(currentTime));
        //OnGameStateChanged?.Invoke(CurrentGameState);

        Debug.Log("GameManager received level data and is now Playing.");
    }

    void Start()
    {
        // GameManager�ڿ�ʼʱ���ڡ�׼����״̬���ȴ�LevelManager���عؿ�
        //CurrentGameState = GameState.Ready;
        clawController = FindObjectOfType<ClawController>();
        //OnGameStateChanged?.Invoke(CurrentGameState);

    }

    void Update()
    {
        if (CurrentGameState != GameState.Playing) return;

        currentTime -= Time.deltaTime;
        OnTimeChanged?.Invoke(Mathf.CeilToInt(currentTime)); // ÿ֡����ʱ������¼�

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

        OnGameStateChanged?.Invoke(CurrentGameState); // ������Ϸ����״̬�¼�

        // ����צ��
        //clawController.enabled = false;
    }

    public void AddScore(int value)
    {
        if (CurrentGameState != GameState.Playing) return;
        Debug.Log($"Adding score: {value}");
        currentScore += value;
        OnScoreChanged?.Invoke(currentScore, lastLevelTotalScore + currentScore);
        if (!LevelManager.Instance.CheckIsTresureActive())
        {
            LevelEnd();
        }
    }
    public void StartGame()
    {
        this.CurrentGameState = GameState.Playing; // ������Ϸ״̬ΪPlaying
    }
    public void StartReady(emLoadLevelType emLoadLevel = emLoadLevelType.LoadNextLevel)
    {
        if(emLoadLevel == emLoadLevelType.LoadFirstLevel)
        {
            lastLevelTotalScore = 0;
        }
        else if(emLoadLevel == emLoadLevelType.LoadNextLevel)
        {
            lastLevelTotalScore += currentScore; // �ۼ���һ�ص��ܷ�
        }

        currentScore = 0;
        if(LevelManager.Instance.StartLoadLevel(emLoadLevel)) 
            CurrentGameState = GameState.Ready; // ������Ϸ״̬Ϊ׼��
    }
    public void PauseGame()
    {
        if (CurrentGameState == GameState.Playing)
        {
            CurrentGameState = GameState.Pause; // ��ͣ��Ϸ
        }
        else if (CurrentGameState == GameState.Pause)
        {
            CurrentGameState = GameState.Playing; // �ָ���Ϸ
        }
    }
}