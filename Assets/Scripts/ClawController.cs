using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClawController : MonoBehaviour
{
    // 定义爪子的几种状态
    public enum ClawState
    {
        Swinging,  // 摆动中
        Launching, // 发射中
        Retracting, // 收回中
        None,
    }

    [Header("状态管理")]
    public ClawState currentState = ClawState.Swinging; // 爪子当前的状态

    [Header("摆动设置")]
    public float swingSpeed = 2f;      // 摆动的速度
    public float maxSwingAngle = 60f;    // 摆动的最大角度

    [Header("发射与收回设置")]
    public float launchSpeed = 10f;    // 发射速度
    public float baseRetractSpeed = 10f;   // 收回速度
    public Transform ropeStartPoint;   // 绳子的起点（需要手动指定）
    public Transform ropeEndPoint;     // 绳子的终点（通常是爪子的位置）

    public Vector3 initialPosition;   // 爪子枢轴的初始位置
    private Quaternion initialRotation; // 爪子枢轴的初始旋转

    private LineRenderer lineRenderer; // 用于绘制绳子

    private GameObject grabbedItem = null; // 用来存储抓到的物体
    private float currentRetractSpeed; // 当前的实际收回速度

    private float clawMinY = -5;
    private float clawMaxX = 9;
    //private float originalBaseRetractSpeed;
    private Coroutine strengthCoroutine;

    [SerializeField]private bool isStrength;
    private float strengthMultiplier = 3f;

    public Animator animator;
    void OnEnable()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnGameStateChanged += HandleGameStateChanged;
        }
    }

    void OnDisable()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnGameStateChanged -= HandleGameStateChanged;
        }
    }

    void Start()
    {
        // 获取并记录初始状态
        initialPosition = transform.position;
        initialRotation = transform.rotation;

        // 获取附加在同一个对象上的LineRenderer组件
        lineRenderer = GetComponent<LineRenderer>();
        // 设置绳子的起点
        lineRenderer.SetPosition(0, ropeStartPoint.position);

        //originalBaseRetractSpeed = baseRetractSpeed;
    }

    void Update()
    {
        // 在每一帧更新绳子的终点位置
        UpdateRope();

        // 根据当前的状态，执行不同的逻辑
        switch (currentState)
        {
            case ClawState.Swinging:
                HandleSwinging();
                break;
            case ClawState.Launching:
                HandleLaunching();
                break;
            case ClawState.Retracting:
                HandleRetracting();
                break;
        }
    }
    /// <summary>
    /// 处理摆动状态
    /// </summary>
    void HandleSwinging()
    {
        // 确保在开始新一轮摆动时，重置所有状态
        if (grabbedItem != null)
        {
            // 销毁宝藏并重置状态
            GameObjectManager.Instance.Release(grabbedItem);

            var treasure = grabbedItem.GetComponent<Treasure>();
            if (treasure.myType == TreasureType.Item)
            {
                var itemtype = (ItemType)Random.Range(0, (int)ItemType.Max);
                Debug.Log($"抓到道具:{itemtype}");
                ItemManager.Instance.AddItem(itemtype);

                FloatingTextManager.Instance.ShowInWorld(
                    FloatingTextType.ItemGet,
                    $"获得 {ItemManager.Instance.GetItemInfo(itemtype)?.itemName?? itemtype.ToString()}!",
                    80,
                    new Color(0.5f, 1f, 0.5f), // 浅绿色
                    transform.position,
                    new Vector3(0, 100, 0),
                    2f
                );
            }
            else
            {
                // 调用GameManager的AddScore方法来加分
                GameManager.Instance.AddScore(treasure.value);
            }
            
            grabbedItem = null;
        }

        // 摆动逻辑
        float angle = Mathf.Sin(Time.time * swingSpeed) * maxSwingAngle;
        transform.rotation = initialRotation * Quaternion.Euler(0, 0, angle);
        
        // 检测玩家输入
        if ((Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0)&& Input.mousePosition.y < Screen.height * 0.75f) && GameManager.Instance.CurrentGameState == GameManager.GameState.Playing)
        {
            currentState = ClawState.Launching; // 切换到发射状态
        }
    }
    /// <summary>
    /// 处理发射状态
    /// </summary>
    void HandleLaunching()
    {
        // 沿爪子的正上方（Y轴方向）移动
        // transform.up 是指当前对象Y轴在世界空间中的方向
        transform.position += transform.up * -1 * launchSpeed * Time.deltaTime * (isStrength ? strengthMultiplier : 1f);

        // 简易边界检测：如果爪子超出了某个范围，则开始收回
        if (transform.position.y < clawMinY || Mathf.Abs(transform.position.x) > clawMaxX)
        {
            animator?.SetBool("IsRetracting", true);
            currentState = ClawState.Retracting;
            currentRetractSpeed = baseRetractSpeed; // 使用基础速度
        }
    }
    /// <summary>
    /// 处理收回状态
    /// </summary>
    void HandleRetracting()
    {
        // 计算返回初始位置的方向
        Vector3 directionToInitial = (initialPosition - transform.position).normalized;
        transform.position += directionToInitial * currentRetractSpeed * Time.deltaTime * (isStrength? strengthMultiplier : 1f);
        animator?.SetBool("IsRetracting", true);

        // 如果已经非常接近初始位置，则判定为已返回
        if (Vector3.Distance(transform.position, initialPosition) < 0.1f)
        {
            animator.SetBool("IsRetracting", false);
            transform.position = initialPosition; // 精准归位
            currentState = ClawState.Swinging;   // 切换回摆动状态
        }
    }
    /// <summary>
    /// 更新绳子的终点位置
    /// </summary>
    void UpdateRope()
    {
        // 绳子的终点就是爪子枢轴的当前位置
        lineRenderer.SetPosition(1, ropeEndPoint.position);
    }

    // --- 这是Unity物理引擎会自动调用的方法 ---
    public void HandleCollision(Collider2D other)
    {
        // 检查：1. 必须是在发射状态下碰到东西才算
        // 检查：2. 碰到的东西必须有"Treasure"标签
        if (currentState == ClawState.Launching && other.CompareTag("Treasure"))
        {
            Debug.Log("抓到了东西!");
            
            // 记录抓到的物体
            grabbedItem = other.gameObject;
            Treasure treasure = grabbedItem.GetComponent<Treasure>();
            // 停止该物体的磁力移动（如果有的话）
            treasure?.StopMagneticMove();

            // 让物体"粘"在爪子上，成为爪子的子对象
            grabbedItem.transform.SetParent(this.transform);
            grabbedItem.transform.localPosition = treasure.ClawHoldOffset; // 重置位置到爪子中心
            // 切换到收回状态
            currentState = ClawState.Retracting;

            // 根据抓到的物体的重量，计算本次的收回速度
            float itemWeight = treasure.weight;
            currentRetractSpeed = baseRetractSpeed / itemWeight;
        }
    }
    /// <summary>
    /// 使用炸弹
    /// </summary>
    public void DestroyGrabbedTreasure()
    {
        if (currentState == ClawState.Retracting && grabbedItem != null)
        {
            Debug.Log($"Bomb used on {grabbedItem.name}.");
            GameObjectManager.Instance.Release(grabbedItem);
            grabbedItem = null;
            currentRetractSpeed = baseRetractSpeed; // Reset to full speed
            animator?.SetTrigger("Bomb"); // 播放炸弹动画
        }
    }

    private void HandleGameStateChanged(GameManager.GameState newState)
    {
        if (newState != GameManager.GameState.Playing && newState != GameManager.GameState.Pause)
        {
            ClearStrengthEffect();
        }
    }
    /// <summary>
    /// 清除力量效果
    /// </summary>
    private void ClearStrengthEffect()
    {
        if (strengthCoroutine != null)
        {
            StopCoroutine(strengthCoroutine);
            this.isStrength = false;
            //baseRetractSpeed = originalBaseRetractSpeed;
            strengthCoroutine = null;
            Debug.Log("Strength potion effect cleared due to game state change.");
        }
    }
    /// <summary>
    /// 激活力量效果
    /// </summary>
    /// <param name="multiplier"></param>
    /// <param name="duration"></param>
    public void ActivateStrength(float multiplier, float duration)
    {
        animator?.SetTrigger("Strength");
        ClearStrengthEffect(); // Clear previous before starting a new one
        strengthCoroutine = StartCoroutine(StrengthCoroutine(multiplier, duration));
    }

    private IEnumerator StrengthCoroutine(float multiplier, float duration)
    {
        Debug.Log($"Strength potion activated! Speed multiplier: {multiplier} for {duration}s.");
        
        if(strengthMultiplier != multiplier) strengthMultiplier = multiplier;
        this.isStrength = true;
        float timer = duration;
        while (timer > 0)
        {
            if (GameManager.Instance.CurrentGameState == GameManager.GameState.Playing)
            {
                timer -= Time.deltaTime;
            }
            yield return null; // Wait for the next frame
        }
        this.isStrength = false;

        Debug.Log("Strength potion wore off.");
        strengthCoroutine = null;
    }
    /// <summary>
    /// 重置爪子到初始状态
    /// </summary>
    public void ResetClaw()
    {
        transform.position = initialPosition;
        transform.rotation = initialRotation;
        currentState = ClawState.Swinging;

        if (grabbedItem != null)
        {
            GameObjectManager.Instance.Release(grabbedItem);
            grabbedItem = null;
        }
        animator?.SetBool("IsRetracting", false);

        // 确保所有临时的增益效果也被清除
        ClearStrengthEffect();

        Debug.Log("Claw has been reset.");
    }
    /// <summary>
    /// 设置爪子的边界
    /// </summary>
    /// <param name="clawMinY"></param>
    /// <param name="clawMaxX"></param>
    public void SetClaw(float clawMinY,  float clawMaxX)
    {
        this.clawMinY = clawMinY;
        this.clawMaxX = clawMaxX;
    }
}
