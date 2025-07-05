using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class UIGame : MonoBehaviour
{
    [Header("UI 元素")]
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI totalScoreText;
    public TextMeshProUGUI timerText;
    public TextMeshProUGUI targetText;
    [Header("道具UI")]
    public Button useBombButton;
    public TextMeshProUGUI bombCountText;
    public Button useStrengthButton;
    public TextMeshProUGUI strengthCountText;
    public Button useCloverButton;
    public TextMeshProUGUI cloverCountText;
    public Button useMagnetButton;
    public TextMeshProUGUI magnetCountText;
    public Button useTimeAddButton;
    public TextMeshProUGUI timeCountText;
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
        if (ItemManager.Instance != null)
        {
            ItemManager.Instance.OnItemCountChanged += HandleItemCountChange;
        }

        InitializeItemUI();
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
        if (ItemManager.Instance != null)
        {
            ItemManager.Instance.OnItemCountChanged -= HandleItemCountChange;
        }
    }

    private void InitializeItemUI()
    {
        // Init Bomb UI
        int bombCount = ItemManager.Instance.GetItemCount(ItemType.Bomb);
        bombCountText.text = bombCount.ToString();
        useBombButton.interactable = bombCount > 0;

        // Init Strength Potion UI
        int strengthCount = ItemManager.Instance.GetItemCount(ItemType.StrengthPotion);
        strengthCountText.text = strengthCount.ToString();
        useStrengthButton.interactable = strengthCount > 0;

        // Init Lucky Clover UI
        int cloverCount = ItemManager.Instance.GetItemCount(ItemType.LuckyClover);
        cloverCountText.text = cloverCount.ToString();
        useCloverButton.interactable = cloverCount > 0;

        // Init Magnet UI
        int magnetCount = ItemManager.Instance.GetItemCount(ItemType.Magnet);
        magnetCountText.text = magnetCount.ToString();
        useMagnetButton.interactable = magnetCount > 0;

        int timeCount = ItemManager.Instance.GetItemCount(ItemType.TimeExtension);
        timeCountText.text = magnetCount.ToString();
        useTimeAddButton.interactable = magnetCount > 0;
    }

    private void HandleItemCountChange(ItemType type, int count)
    {
        if (type == ItemType.Bomb)
        {
            bombCountText.text = count.ToString();
            useBombButton.interactable = count > 0;
        }
        else if (type == ItemType.StrengthPotion)
        {
            strengthCountText.text = count.ToString();
            useStrengthButton.interactable = count > 0;
        }
        else if (type == ItemType.LuckyClover)
        {
            cloverCountText.text = count.ToString();
            useCloverButton.interactable = count > 0;
        }
        else if (type == ItemType.Magnet)
        {
            magnetCountText.text = count.ToString();
            useMagnetButton.interactable = count > 0;
        }
        else if (type == ItemType.TimeExtension)
        {
            timeCountText.text = count.ToString();
            useTimeAddButton.interactable = count > 0;
        }
    }

    public void OnUseBombButtonClick()
    {
        ItemManager.Instance.UseItem(ItemType.Bomb);
    }

    public void OnUseStrengthButtonClick()
    {
        ItemManager.Instance.UseItem(ItemType.StrengthPotion);
    }

    public void OnUseCloverButtonClick()
    {
        ItemManager.Instance.UseItem(ItemType.LuckyClover);
    }

    public void OnUseMagnetButtonClick()
    {
        ItemManager.Instance.UseItem(ItemType.Magnet);
    }
    public void OnUseTimeExtensionClick()
    {
        ItemManager.Instance.UseItem(ItemType.TimeExtension);
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
                UIManager.Instance.Show<UIFailure>();
                break;
            case GameManager.GameState.Store:
                UIManager.Instance.Show<UIStore>();
                break;
            case GameManager.GameState.Pause:
                break;
            default:
                break;
        }
    }
}