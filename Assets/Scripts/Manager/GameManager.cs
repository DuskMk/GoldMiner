using UnityEngine;
using System; // ����System�����ռ���ʹ��Action

public class GameManager : MonoSingleton<GameManager>
{
    // --- �¼����� (�۲���ģʽ����) ---
    // ����������ʱ�������¼�
    public event Action<int> OnScoreChanged;
    // ��ʱ�����ʱ�������¼�
    public event Action<int> OnTimeChanged;
    //// ���ؿ�Ŀ�����ʱ�������¼�
    //public event Action<int> OnTargetChanged;
    // ����Ϸ״̬�ı�ʱ�������¼� (׼������ʼ��ʤ����ʧ��)
    public enum GameState { Ready, Playing, Victory, Failure } 
    public event Action<GameState> OnGameStateChanged;


    //[Header("��Ϸ����")]
    //public int targetScore = 2000;
    //public float levelTime = 60f;

    [SerializeField] private int currentScore;
    [SerializeField] private float currentTime;
    [SerializeField] private int currentTargetScore;
    [SerializeField] private GameState currentGameState = GameState.Ready;
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
    private void InitializeLevel(int target, int time)
    {
        currentScore = 0;
        currentTargetScore = target;
        currentTime = time;
        currentGameState = GameState.Playing; // �ؿ����غ���Ϸ��ʽ��ʼ

        // ������ʼ��Ϸ״̬�¼�
        OnScoreChanged?.Invoke(currentScore);
        OnTimeChanged?.Invoke(Mathf.CeilToInt(currentTime));
        OnGameStateChanged?.Invoke(currentGameState);

        Debug.Log("GameManager received level data and is now Playing.");
    }

    void Start()
    {
        // GameManager�ڿ�ʼʱ���ڡ�׼����״̬���ȴ�LevelManager���عؿ�
        //currentGameState = GameState.Ready;
        OnGameStateChanged?.Invoke(currentGameState);
        
    }

    void Update()
    {
        if (currentGameState != GameState.Playing) return;

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
            currentGameState = GameState.Victory;
        }
        else
        {
            currentGameState = GameState.Failure;
        }

        OnGameStateChanged?.Invoke(currentGameState); // ������Ϸ����״̬�¼�

        // ����צ��
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