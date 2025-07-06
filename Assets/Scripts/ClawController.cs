using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClawController : MonoBehaviour
{
    // ����צ�ӵļ���״̬
    public enum ClawState
    {
        Swinging,  // �ڶ���
        Launching, // ������
        Retracting, // �ջ���
        None,
    }

    [Header("״̬����")]
    public ClawState currentState = ClawState.Swinging; // צ�ӵ�ǰ��״̬

    [Header("�ڶ�����")]
    public float swingSpeed = 2f;      // �ڶ����ٶ�
    public float maxSwingAngle = 60f;    // �ڶ������Ƕ�

    [Header("�������ջ�����")]
    public float launchSpeed = 10f;    // �����ٶ�
    public float baseRetractSpeed = 10f;   // �ջ��ٶ�
    public Transform ropeStartPoint;   // ���ӵ���㣨��Ҫ�ֶ�ָ����
    public Transform ropeEndPoint;     // ���ӵ��յ㣨ͨ����צ�ӵ�λ�ã�

    public Vector3 initialPosition;   // צ������ĳ�ʼλ��
    private Quaternion initialRotation; // צ������ĳ�ʼ��ת

    private LineRenderer lineRenderer; // ���ڻ�������

    private GameObject grabbedItem = null; // �����洢ץ��������
    private float currentRetractSpeed; // ��ǰ��ʵ���ջ��ٶ�

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
        // ��ȡ����¼��ʼ״̬
        initialPosition = transform.position;
        initialRotation = transform.rotation;

        // ��ȡ������ͬһ�������ϵ�LineRenderer���
        lineRenderer = GetComponent<LineRenderer>();
        // �������ӵ����
        lineRenderer.SetPosition(0, ropeStartPoint.position);

        //originalBaseRetractSpeed = baseRetractSpeed;
    }

    void Update()
    {
        // ��ÿһ֡�������ӵ��յ�λ��
        UpdateRope();

        // ���ݵ�ǰ��״̬��ִ�в�ͬ���߼�
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
    /// ����ڶ�״̬
    /// </summary>
    void HandleSwinging()
    {
        // ȷ���ڿ�ʼ��һ�ְڶ�ʱ����������״̬
        if (grabbedItem != null)
        {
            // ���ٱ��ز�����״̬
            GameObjectManager.Instance.Release(grabbedItem);

            var treasure = grabbedItem.GetComponent<Treasure>();
            if (treasure.myType == TreasureType.Item)
            {
                var itemtype = (ItemType)Random.Range(0, (int)ItemType.Max);
                Debug.Log($"ץ������:{itemtype}");
                ItemManager.Instance.AddItem(itemtype);

                FloatingTextManager.Instance.ShowInWorld(
                    FloatingTextType.ItemGet,
                    $"��� {ItemManager.Instance.GetItemInfo(itemtype)?.itemName?? itemtype.ToString()}!",
                    80,
                    new Color(0.5f, 1f, 0.5f), // ǳ��ɫ
                    transform.position,
                    new Vector3(0, 100, 0),
                    2f
                );
            }
            else
            {
                // ����GameManager��AddScore�������ӷ�
                GameManager.Instance.AddScore(treasure.value);
            }
            
            grabbedItem = null;
        }

        // �ڶ��߼�
        float angle = Mathf.Sin(Time.time * swingSpeed) * maxSwingAngle;
        transform.rotation = initialRotation * Quaternion.Euler(0, 0, angle);
        
        // ����������
        if ((Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0)&& Input.mousePosition.y < Screen.height * 0.75f) && GameManager.Instance.CurrentGameState == GameManager.GameState.Playing)
        {
            currentState = ClawState.Launching; // �л�������״̬
        }
    }
    /// <summary>
    /// ������״̬
    /// </summary>
    void HandleLaunching()
    {
        // ��צ�ӵ����Ϸ���Y�᷽���ƶ�
        // transform.up ��ָ��ǰ����Y��������ռ��еķ���
        transform.position += transform.up * -1 * launchSpeed * Time.deltaTime * (isStrength ? strengthMultiplier : 1f);

        // ���ױ߽��⣺���צ�ӳ�����ĳ����Χ����ʼ�ջ�
        if (transform.position.y < clawMinY || Mathf.Abs(transform.position.x) > clawMaxX)
        {
            animator?.SetBool("IsRetracting", true);
            currentState = ClawState.Retracting;
            currentRetractSpeed = baseRetractSpeed; // ʹ�û����ٶ�
        }
    }
    /// <summary>
    /// �����ջ�״̬
    /// </summary>
    void HandleRetracting()
    {
        // ���㷵�س�ʼλ�õķ���
        Vector3 directionToInitial = (initialPosition - transform.position).normalized;
        transform.position += directionToInitial * currentRetractSpeed * Time.deltaTime * (isStrength? strengthMultiplier : 1f);
        animator?.SetBool("IsRetracting", true);

        // ����Ѿ��ǳ��ӽ���ʼλ�ã����ж�Ϊ�ѷ���
        if (Vector3.Distance(transform.position, initialPosition) < 0.1f)
        {
            animator.SetBool("IsRetracting", false);
            transform.position = initialPosition; // ��׼��λ
            currentState = ClawState.Swinging;   // �л��ذڶ�״̬
        }
    }
    /// <summary>
    /// �������ӵ��յ�λ��
    /// </summary>
    void UpdateRope()
    {
        // ���ӵ��յ����צ������ĵ�ǰλ��
        lineRenderer.SetPosition(1, ropeEndPoint.position);
    }

    // --- ����Unity����������Զ����õķ��� ---
    public void HandleCollision(Collider2D other)
    {
        // ��飺1. �������ڷ���״̬��������������
        // ��飺2. �����Ķ���������"Treasure"��ǩ
        if (currentState == ClawState.Launching && other.CompareTag("Treasure"))
        {
            Debug.Log("ץ���˶���!");
            
            // ��¼ץ��������
            grabbedItem = other.gameObject;
            Treasure treasure = grabbedItem.GetComponent<Treasure>();
            // ֹͣ������Ĵ����ƶ�������еĻ���
            treasure?.StopMagneticMove();

            // ������"ճ"��צ���ϣ���Ϊצ�ӵ��Ӷ���
            grabbedItem.transform.SetParent(this.transform);
            grabbedItem.transform.localPosition = treasure.ClawHoldOffset; // ����λ�õ�צ������
            // �л����ջ�״̬
            currentState = ClawState.Retracting;

            // ����ץ������������������㱾�ε��ջ��ٶ�
            float itemWeight = treasure.weight;
            currentRetractSpeed = baseRetractSpeed / itemWeight;
        }
    }
    /// <summary>
    /// ʹ��ը��
    /// </summary>
    public void DestroyGrabbedTreasure()
    {
        if (currentState == ClawState.Retracting && grabbedItem != null)
        {
            Debug.Log($"Bomb used on {grabbedItem.name}.");
            GameObjectManager.Instance.Release(grabbedItem);
            grabbedItem = null;
            currentRetractSpeed = baseRetractSpeed; // Reset to full speed
            animator?.SetTrigger("Bomb"); // ����ը������
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
    /// �������Ч��
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
    /// ��������Ч��
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
    /// ����צ�ӵ���ʼ״̬
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

        // ȷ��������ʱ������Ч��Ҳ�����
        ClearStrengthEffect();

        Debug.Log("Claw has been reset.");
    }
    /// <summary>
    /// ����צ�ӵı߽�
    /// </summary>
    /// <param name="clawMinY"></param>
    /// <param name="clawMaxX"></param>
    public void SetClaw(float clawMinY,  float clawMaxX)
    {
        this.clawMinY = clawMinY;
        this.clawMaxX = clawMaxX;
    }
}
