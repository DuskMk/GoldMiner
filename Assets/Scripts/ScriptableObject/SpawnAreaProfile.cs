using UnityEngine;

[CreateAssetMenu(fileName = "New Spawn Area Profile", menuName = "Gold Miner/Spawn Area Profile")]
public class SpawnAreaProfile : ScriptableObject
{
    [Header("生成区域")]
    [Tooltip("宝藏将被生成在这个矩形区域内")]
    public Rect spawnBounds = new Rect(-8f, -10f, 16f, 8f); // X, Y, Width, Height

    [Header("相机设置")]
    public Vector3 cameraPosition = new Vector3(0, 1, -10);
    public float cameraOrthographicSize = 8f;
}