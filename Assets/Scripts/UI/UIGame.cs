using UnityEngine;
using TMPro;

public class UIGame : MonoBehaviour
{
    [Header("UI 元素")]
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI timerText;
    public TextMeshProUGUI targetText;
    //public GameObject victoryPanel;
    //public GameObject failurePanel;
    //public GameObject readyPanel; // 新增一个准备开始的面板

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
            LevelManager.Instance.OnLevelDataLoaded += (target, time) => UpdateTargetText(target);
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
            LevelManager.Instance.OnLevelDataLoaded -= (target, time) => UpdateTargetText(target);
        }
    }

    private void UpdateScoreText(int newScore)
    {
        scoreText.text = "金钱: " + newScore;
    }

    private void UpdateTimerText(int newTime)
    {
        timerText.text = "时间: " + newTime;
    }

    private void UpdateTargetText(int newTarget)
    {
        targetText.text = "目标: " + newTarget;
    }

    private void HandleGameStateChange(GameManager.GameState newState)
    {
        // 根据新的游戏状态更新UI
        //readyPanel.SetActive(newState == GameManager.GameState.Ready);
        //victoryPanel.SetActive(newState == GameManager.GameState.Victory);
        //failurePanel.SetActive(newState == GameManager.GameState.Failure);
    }
}