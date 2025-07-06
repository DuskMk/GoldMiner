using UnityEngine;
using TMPro;
using UnityEngine.UI;
using DG.Tweening;

public class UIGame : MonoBehaviour
{
    [Header("UI 元素")]
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI totalScoreText;
    public TextMeshProUGUI timerText;
    public TextMeshProUGUI targetText;
    public TextMeshProUGUI levelText;
    [Header("时间紧急效果")]
    [Tooltip("从几秒开始触发时间紧急效果")]
    public int lowTimeThreshold = 10;
    private int lastDisplayedTime = -1;
    private Tween timerTween;
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

    public void OnPauseClick()
    {
        UIManager.Instance.Show<UIPause>();
    }
    private void UpdateScoreText(int newScore, int totalScore)
    {
        int scoreDifference = newScore - this.curScore;

        if (scoreDifference > 0)
        {
            FloatingTextManager.Instance.Show(
                FloatingTextType.Score,
                $"获得金币{scoreDifference}", 
                80, 
                Color.yellow, 
                scoreText.transform.position + Vector3.up *30f, 
                new Vector3(0, 50, 0), 
                1f
            );
        }

        this.totalScore = totalScore;
        curScore = newScore;
        scoreText.text = string.Format("得分:{0}", newScore);
        totalScoreText.text = string.Format("总计得分:{0}", totalScore);
    }

    private void UpdateTimerText(int newTime)
    {
        // 避免在同一秒内重复触发动画
        if (newTime == lastDisplayedTime) return;
        lastDisplayedTime = newTime;

        timerText.text = newTime.ToString();

        // 检查是否达到触发阈值
        if (newTime > 0 && newTime <= lowTimeThreshold)
        {
            // 如果上一个动画还在播放，先终止它
            if (timerTween != null && timerTween.IsActive())
            {
                timerTween.Kill(true); // true表示立即完成动画状态
            }

            SoundManager.Instance?.PlaySound(SoundDefine.SFX_UI_Timer_Tick); 

            // 创建一个新的动画
            timerText.transform.localScale = Vector3.one; // 重置大小
            timerTween = timerText.transform.DOPunchScale(new Vector3(1.5f, 1.5f, 1.5f), 0.5f, 5, 0.5f)
                .SetUpdate(true); // SetUpdate(true) 可以在Time.timeScale为0时继续播放，适用于暂停
        }
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
                // 注意：这里的 levelIndex 是从 0 开始的，所以需要加 1 来显示为第几关
                int level = LevelManager.Instance.GetCurLevelData().levelIndex + 1;
                uIr.SetInfo(targetScore, timeLimit, level);
                levelText.text = $"第{level}关";
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