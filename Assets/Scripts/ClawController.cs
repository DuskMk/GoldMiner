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

    private Vector3 initialPosition;   // 爪子枢轴的初始位置
    private Quaternion initialRotation; // 爪子枢轴的初始旋转

    private LineRenderer lineRenderer; // 用于绘制绳子

    private GameObject grabbedItem = null; // 用来存储抓到的物体
    private float currentRetractSpeed; // 当前的实际收回速度

    private float clawMinY = -5;
    private float clawMaxX = 9;
    private float originalBaseRetractSpeed;
    private Coroutine strengthCoroutine;

    [SerializeField]private bool isStrength;
    private float strengthMultiplier = 3f;
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

        originalBaseRetractSpeed = baseRetractSpeed;
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

    void HandleSwinging()
    {
        // 确保在开始新一轮摆动时，重置所有状态
        if (grabbedItem != null)
        {
            /// 获取宝藏的价值
            int value = grabbedItem.GetComponent<Treasure>().value;

            // 销毁宝藏并重置状态
            GameObjectManager.Instance.Release(grabbedItem);

            // 调用GameManager的AddScore方法来加分
            GameManager.Instance.AddScore(value);
            
            grabbedItem = null;
        }

        // 摆动逻辑
        float angle = Mathf.Sin(Time.time * swingSpeed) * maxSwingAngle;
        transform.rotation = initialRotation * Quaternion.Euler(0, 0, angle);

        // 检测玩家输入
        if ((Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0) )&& GameManager.Instance.CurrentGameState == GameManager.GameState.Playing)
        {
            currentState = ClawState.Launching; // 切换到发射状态
        }
    }

    void HandleLaunching()
    {
        // 沿爪子的正上方（Y轴方向）移动
        // transform.up 是指当前对象Y轴在世界空间中的方向
        transform.position += transform.up * -1 * launchSpeed * Time.deltaTime * (isStrength ? strengthMultiplier : 1f);

        // 简易边界检测：如果爪子超出了某个范围，则开始收回
        if (transform.position.y < clawMinY || Mathf.Abs(transform.position.x) > clawMaxX)
        {
            currentState = ClawState.Retracting;
            currentRetractSpeed = baseRetractSpeed; // 使用基础速度
        }
    }

    void HandleRetracting()
    {
        // 计算返回初始位置的方向
        Vector3 directionToInitial = (initialPosition - transform.position).normalized;
        transform.position += directionToInitial * currentRetractSpeed * Time.deltaTime * (isStrength? strengthMultiplier : 1f);

        // 如果已经非常接近初始位置，则判定为已返回
        if (Vector3.Distance(transform.position, initialPosition) < 0.1f)
        {
            transform.position = initialPosition; // 精准归位
            currentState = ClawState.Swinging;   // 切换回摆动状态
        }
    }

    void UpdateRope()
    {
        // 绳子的终点就是爪子枢轴的当前位置
        lineRenderer.SetPosition(1, transform.position);
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

            // 让物体"粘"在爪子上，成为爪子的子对象
            grabbedItem.transform.SetParent(this.transform);

            // 切换到收回状态
            currentState = ClawState.Retracting;

            // 根据抓到的物体的重量，计算本次的收回速度
            float itemWeight = grabbedItem.GetComponent<Treasure>().weight;
            currentRetractSpeed = baseRetractSpeed / itemWeight;
        }
    }

    public void DestroyGrabbedTreasure()
    {
        if (currentState == ClawState.Retracting && grabbedItem != null)
        {
            Debug.Log($"Bomb used on {grabbedItem.name}.");
            GameObjectManager.Instance.Release(grabbedItem);
            grabbedItem = null;
            currentRetractSpeed = baseRetractSpeed; // Reset to full speed
        }
    }

    private void HandleGameStateChanged(GameManager.GameState newState)
    {
        if (newState != GameManager.GameState.Playing && newState != GameManager.GameState.Pause)
        {
            ClearStrengthEffect();
        }
    }

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

    public void ActivateStrength(float multiplier, float duration)
    {
        ClearStrengthEffect(); // Clear previous before starting a new one
        strengthCoroutine = StartCoroutine(StrengthCoroutine(multiplier, duration));
    }

    private IEnumerator StrengthCoroutine(float multiplier, float duration)
    {
        Debug.Log($"Strength potion activated! Speed multiplier: {multiplier} for {duration}s.");
        //baseRetractSpeed = originalBaseRetractSpeed * multiplier;

        //if (currentState == ClawState.Retracting && grabbedItem != null)
        //{
        //    float itemWeight = grabbedItem.GetComponent<Treasure>().weight;
        //    currentRetractSpeed = baseRetractSpeed / itemWeight;
        //}
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
        //baseRetractSpeed = originalBaseRetractSpeed;
        strengthCoroutine = null;
    }

    public void SetClaw(float clawMinY,  float clawMaxX)
    {
        this.clawMinY = clawMinY;
        this.clawMaxX = clawMaxX;
    }
}
