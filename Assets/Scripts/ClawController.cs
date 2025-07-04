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

    private Vector3 initialPosition;   // צ������ĳ�ʼλ��
    private Quaternion initialRotation; // צ������ĳ�ʼ��ת

    private LineRenderer lineRenderer; // ���ڻ�������

    private GameObject grabbedItem = null; // �����洢ץ��������
    private float currentRetractSpeed; // ��ǰ��ʵ���ջ��ٶ�

    private float clawMinY = -5;
    private float clawMaxX = 9;

    void Start()
    {
        // ��ȡ����¼��ʼ״̬
        initialPosition = transform.position;
        initialRotation = transform.rotation;

        // ��ȡ������ͬһ�������ϵ�LineRenderer���
        lineRenderer = GetComponent<LineRenderer>();
        // �������ӵ����
        lineRenderer.SetPosition(0, ropeStartPoint.position);
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

    void HandleSwinging()
    {
        // ȷ���ڿ�ʼ��һ�ְڶ�ʱ����������״̬
        if (grabbedItem != null)
        {
            /// ��ȡ���صļ�ֵ
            int value = grabbedItem.GetComponent<Treasure>().value;

            // ����GameManager��AddScore�������ӷ�
            GameManager.Instance.AddScore(value);

            // ���ٱ��ز�����״̬
            GameObjectManager.Instance.Release(grabbedItem);
            Destroy(grabbedItem);
            grabbedItem = null;
        }

        // �ڶ��߼�
        float angle = Mathf.Sin(Time.time * swingSpeed) * maxSwingAngle;
        transform.rotation = initialRotation * Quaternion.Euler(0, 0, angle);

        // ����������
        if (Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0))
        {
            currentState = ClawState.Launching; // �л�������״̬
        }
    }

    void HandleLaunching()
    {
        // ��צ�ӵ����Ϸ���Y�᷽���ƶ�
        // transform.up ��ָ��ǰ����Y��������ռ��еķ���
        transform.position += transform.up * -1 * launchSpeed * Time.deltaTime;

        // ���ױ߽��⣺���צ�ӳ�����ĳ����Χ����ʼ�ջ�
        if (transform.position.y < clawMinY || Mathf.Abs(transform.position.x) > clawMaxX)
        {
            currentState = ClawState.Retracting;
            currentRetractSpeed = baseRetractSpeed; // ʹ�û����ٶ�
        }
    }

    void HandleRetracting()
    {
        // ���㷵�س�ʼλ�õķ���
        Vector3 directionToInitial = (initialPosition - transform.position).normalized;
        transform.position += directionToInitial * currentRetractSpeed * Time.deltaTime;

        // ����Ѿ��ǳ��ӽ���ʼλ�ã����ж�Ϊ�ѷ���
        if (Vector3.Distance(transform.position, initialPosition) < 0.1f)
        {
            transform.position = initialPosition; // ��׼��λ
            currentState = ClawState.Swinging;   // �л��ذڶ�״̬
        }
    }

    void UpdateRope()
    {
        // ���ӵ��յ����צ������ĵ�ǰλ��
        lineRenderer.SetPosition(1, transform.position);
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

            // �����塰ճ����צ���ϣ���Ϊצ�ӵ��Ӷ���
            grabbedItem.transform.SetParent(this.transform);

            // �л����ջ�״̬
            currentState = ClawState.Retracting;

            // ����ץ������������������㱾�ε��ջ��ٶ�
            float itemWeight = grabbedItem.GetComponent<Treasure>().weight;
            currentRetractSpeed = baseRetractSpeed / itemWeight;
        }
    }
    public void SetClaw(float clawMinY,  float clawMaxX)
    {
        this.clawMinY = clawMinY;
        this.clawMaxX = clawMaxX;
    }
}
