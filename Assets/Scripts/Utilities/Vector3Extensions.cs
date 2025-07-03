using UnityEngine;

public static class Vector3Extensions
{
    /// <summary>
    /// 返回一个新的 Vector3，其 Y 分量被设置为 0，表示在 XZ 水平面上的投影。
    /// </summary>
    /// <param name="v">要处理的原始 Vector3。</param>
    /// <returns>Y 分量为 0 的新 Vector3。</returns>
    public static Vector3 XZPlane(this Vector3 v)
    {
        return new Vector3(v.x, 0, v.z);
    }
    /// <summary>
    /// 返回两个三维向量在 水平方向XZ轴 的距离
    /// </summary>
    public static float DistenceXZ(this Vector3 a, Vector3 b)
    {
        float dx = a.x - b.x;
        float dy = a.y - b.y;
        return Mathf.Sqrt(dx * dx + dy * dy);
    }
    /// <summary>
    /// 两个三维向量在 水平方向XZ轴距离 不开方，用于比较距离
    /// </summary>
    public static float DistenceXZ_NoSqrt(this Vector3 a, Vector3 b)
    {
        float dx = a.x - b.x;
        float dy = a.y - b.y;
        return dx * dx + dy * dy;
    }
}

