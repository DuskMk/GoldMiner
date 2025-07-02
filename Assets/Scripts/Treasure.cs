using UnityEngine;

public class Treasure : MonoBehaviour
{
    // 只用来存储数据

    [Header("宝藏属性")]
    public int value = 500;  // 这个宝藏的价值（金钱）
    public float weight = 5f;  // 这个宝藏的重量（影响收回速度）
}