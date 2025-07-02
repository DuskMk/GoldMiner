using UnityEngine;

public class ClawTrigger : MonoBehaviour
{
    // ����ű���Ҫ֪�����ġ����ԡ���˭��Ҳ����ClawController
    public ClawController clawController;

    void Start()
    {
        // �����Inspector��û���ֶ�ָ�������Զ�������Ѱ��
        if (clawController == null)
        {
            clawController = GetComponentInParent<ClawController>();
        }
    }

    // ����ű��ĺ���������Ǽ�����ײ
    void OnTriggerEnter2D(Collider2D other)
    {
        // һ��������ײ���Ͱ�����������(other)��Ϣ��
        // ���ݸ�ClawControllerȥ����
        clawController.HandleCollision(other);
    }
}