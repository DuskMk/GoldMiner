using UnityEngine;
using DG.Tweening;

public class Treasure : MonoBehaviour
{
    // 只用来存储数据

    [Header("宝藏属性")]
    public TreasureType myType;
    public int value = 500;  // 这个宝藏的价值（金钱）
    public float weight = 5f;  // 这个宝藏的重量（影响收回速度）
    public Vector3 ClawHoldOffset; // 宝藏被抓取时的偏移量
    [Tooltip("该宝藏是否会触发角力机制")]
    public bool causesStruggle = false;
    private Tween magnetTween;

    public void StartMagneticMove(Vector3 targetPosition, float duration)
    {
        // 确保不会重复启动
        if (magnetTween != null && magnetTween.IsActive()) return;

        magnetTween = transform.DOMove(targetPosition, duration)
            .SetEase(Ease.InOutSine)
            .OnComplete(() => magnetTween = null);
    }

    public void StopMagneticMove()
    {
        if (magnetTween != null && magnetTween.IsActive())
        {
            magnetTween.Kill();
            magnetTween = null;
        }
    }

    private void OnDisable()
    {
        // 当宝藏被对象池回收时，确保动画被终止
        StopMagneticMove();
    }
}