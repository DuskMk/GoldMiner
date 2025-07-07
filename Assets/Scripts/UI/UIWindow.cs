using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;


public class UIWindow : MonoBehaviour
{
    public enum emWindowResult
    {
        None = 0,
        Yes,
        No,
    }
    public delegate void CloseHander(UIWindow sender, emWindowResult result);
    public event CloseHander OnClose;

    public Type type { get { return this.GetType(); } }

    public CanvasGroup canvasGroup;
    public RectTransform panelRectTransform; // 面板的主要 RectTransform，用于缩放和位移

    [Header("Animation Settings")]
    public float openDuration = 0.3f;
    public Ease openEase = Ease.OutBack; // 例如，带回弹的打开效果
    public Vector3 openStartScale = Vector3.one * 0.5f; // 打开时初始缩放
    public Vector3 openEndScale = Vector3.one;

    public float closeDuration = 0.2f;
    public Ease closeEase = Ease.InQuad;
    public Vector3 closeEndScale = Vector3.one * 0.5f; // 关闭时最终缩放

    private Sequence currentAnimation; // 存储当前正在播放的 DOTween Sequence
    public bool IsOpen { get; private set; } = false; // 标记面板当前是否“逻辑上”打开

    protected virtual void Awake()
    {
        if (canvasGroup == null) canvasGroup = GetComponent<CanvasGroup>();
        if (panelRectTransform == null) panelRectTransform = GetComponent<RectTransform>();

        // 初始状态：隐藏且非激活（通常由 UIManager 控制初始 SetActive）
        // canvasGroup.alpha = 0;
        // panelRectTransform.localScale = openStartScale;
        // gameObject.SetActive(false);
    }


    public void Close(emWindowResult result = emWindowResult.None, bool isPlaySound = true)
    {
        UIManager.Instance.Close(this.type, isPlaySound);
        this.OnClose?.Invoke(this, result);
        this.OnClose = null;
    }
    public virtual void OnClickClose()
    {
        Hide(() => Close());
        //Close();
    }
    public virtual void OnClickYes()
    {
        //SoundManager.Instance.PlaySound(SoundDefine.SFX_UI_Confirm);

        Hide(() => Close(emWindowResult.Yes));
        //Close(emWindowResult.Yes);
    }
    public virtual void OnClickNo()
    {
        Hide(() => Close(emWindowResult.No));
        //Close(emWindowResult.No);
    }

    /// <summary>
    /// 播放打开面板的动画。
    /// </summary>
    /// <param name="onComplete">动画完成后的回调。</param>
    public virtual void Show(UnityAction onComplete = null)
    {
        if (canvasGroup == null || panelRectTransform == null)
        {
            onComplete?.Invoke();
            return;
        };

        // 如果正在播放动画或已打开，则不执行
        if (currentAnimation != null && currentAnimation.IsActive()) return;
        // if (IsOpen && gameObject.activeSelf && canvasGroup.alpha == 1f) return;

        //gameObject.SetActive(true); // 确保 GameObject 激活以播放动画
        IsOpen = true;

        // 杀死可能存在的旧动画
        currentAnimation?.Kill();

        // 创建打开动画序列
        currentAnimation = DOTween.Sequence();
        currentAnimation.SetUpdate(true); // 可以在 Time.timeScale = 0 时也播放 (例如暂停菜单)

        // 1. 初始化状态 (确保从正确的起点开始)
        canvasGroup.alpha = 0f; // 从完全透明开始
        panelRectTransform.localScale = openStartScale; // 从初始缩放开始
                                                        // panelRectTransform.anchoredPosition = ... // 如果有位移动画，设置初始位置

        // 2. 添加动画到序列
        currentAnimation.Append(canvasGroup.DOFade(1f, openDuration * 0.7f).SetEase(Ease.Linear)) // 淡入 (可以比缩放稍快)
                      .Join(panelRectTransform.DOScale(openEndScale, openDuration).SetEase(openEase)) // 同时缩放
                                                                                                      // .Join(panelRectTransform.DOAnchorPos(...)) // 如果有位移动画
                      .OnComplete(() =>
                      {
                          currentAnimation = null;
                          onComplete?.Invoke(); // 执行完成回调
                                                // Debug.Log($"{gameObject.name} opened.");
                      });

        currentAnimation.Play();
    }

    /// <summary>
    /// 播放关闭面板的动画。
    /// </summary>
    /// <param name="onComplete">动画完成后的回调 (通常用于禁用 GameObject)。</param>
    public virtual void Hide(UnityAction onComplete = null)
    {
        if (canvasGroup == null || panelRectTransform == null)
        {
            onComplete?.Invoke();
            return;
        };

        if (currentAnimation != null && currentAnimation.IsActive()) return;
        // if (!IsOpen && (!gameObject.activeSelf || canvasGroup.alpha == 0f)) return;

        IsOpen = false;
        currentAnimation?.Kill();

        currentAnimation = DOTween.Sequence();
        currentAnimation.SetUpdate(true);

        // 1. 初始化状态 (如果需要，但通常从当前状态开始关闭)
        // canvasGroup.alpha = 1f;
        // panelRectTransform.localScale = openEndScale;

        // 2. 添加动画到序列
        currentAnimation.Append(canvasGroup.DOFade(0f, closeDuration).SetEase(Ease.Linear)) // 淡出
                      .Join(panelRectTransform.DOScale(closeEndScale, closeDuration).SetEase(closeEase)) // 同时缩小
                                                                                                         // .Join(panelRectTransform.DOAnchorPos(...)) // 如果有位移动画
                      .OnComplete(() =>
                      {
                          currentAnimation = null;
                          //gameObject.SetActive(false); // 动画完成后禁用 GameObject
                          onComplete?.Invoke();
                          // Debug.Log($"{gameObject.name} closed.");
                      });
        currentAnimation.Play();
    }

    // 方便外部直接调用，不带回调
    public void TriggerShow() => Show(null);
    public void TriggerHide() => Hide(null);

    protected virtual void OnEnable()
    {
        Show();
    }
    // 在对象被禁用时，确保停止动画 (例如父对象被禁用)
    protected virtual void OnDisable()
    {
        // 如果是因为 Hide 动画完成而禁用，currentAnimation 应该已经是 null
        // 但如果是外部强制禁用，需要杀死动画
        currentAnimation?.Kill();
        currentAnimation = null;
        // IsOpen 状态保持，因为对象只是被禁用了，逻辑上可能还是“打开”的，下次启用时恢复
    }
}




