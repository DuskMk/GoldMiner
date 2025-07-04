using UnityEngine;
using TMPro;

public class UIGame : MonoBehaviour
{
    [Header("UI 元素")]
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI totalScoreText;
    public TextMeshProUGUI timerText;
    public TextMeshProUGUI targetText;
    private int curScore;
    private int totalScore;
    private int targetScore;
    private int timeLimit;
    void OnEnable()
    {
        // 订阅GameManager的事件
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnScoreChanged += UpdateScoreText;
            GameManager.Instance.OnTimeChanged += UpdateTimerText;
            GameManager.Instance.OnGameStateChanged += HandleGameStateChange;
        }

        // 同时订阅LevelManager的事件
        if (LevelManager.Instance != null)
        {
            // 注意，我们这里直接用LevelManager发布的OnLevelDataLoaded事件来更新目标分数
            LevelManager.Instance.OnLevelDataLoaded += (levelData) => UpdateTargetText(levelData.targetScore);
        }
    }

    void OnDisable()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnScoreChanged -= UpdateScoreText;
            GameManager.Instance.OnTimeChanged -= UpdateTimerText;
            GameManager.Instance.OnGameStateChanged -= HandleGameStateChange;
        }
        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.OnLevelDataLoaded -= (levelData) => UpdateTargetText(levelData.targetScore);
        }
    }

    private void UpdateScoreText(int newScore, int totalScore)
    {
        this.totalScore = totalScore;
        curScore = newScore;
        scoreText.text = string.Format("得分:{0}", newScore);
        totalScoreText.text = string.Format("总计得分:{0}", totalScore);
    }

    private void UpdateTimerText(int newTime)
    {
        timerText.text = newTime.ToString();
    }

    private void UpdateTargetText(int newTarget)
    {
        targetScore = newTarget;
        targetText.text = string.Format("目标:{0}", newTarget);
    }

    private void HandleGameStateChange(GameManager.GameState newState)
    {
        // 根据新的游戏状态更新UI
        switch (newState)
        {
            case GameManager.GameState.Ready:
                timeLimit = LevelManager.Instance.GetCurLevelData().timeLimit;

                var uIr = UIManager.Instance.Show<UIReady>();
                uIr.SetInfo(targetScore, timeLimit);
                break;
            case GameManager.GameState.Playing:
                break;
            case GameManager.GameState.Victory:
                var uIv = UIManager.Instance.Show<UIVictory>();
                uIv.SetInfo(totalScore);
                break;
            case GameManager.GameState.Failure:
                break;
            case GameManager.GameState.Pause:
                break;
            default:
                break;
        }
        //readyPanel.SetActive(newState == GameManager.GameState.Ready);
        //victoryPanel.SetActive(newState == GameManager.GameState.Victory);
        //failurePanel.SetActive(newState == GameManager.GameState.Failure);
    }
}