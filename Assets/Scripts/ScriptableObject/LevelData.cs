using UnityEngine;
using System.Collections.Generic;

// 这个内部类用于在Inspector中方便地配置宝藏数量
[System.Serializable]
public class TreasureSpawnInfo
{
    public TreasureType type;
    public int count;
}

[CreateAssetMenu(fileName = "Level 01", menuName = "Gold Miner/Level Data")]
public class LevelData : ScriptableObject
{
    [Header("关卡基础信息")]
    public int levelIndex;
    public int targetScore;
    public int timeLimit;

    [Header("宝藏生成配置")]
    public List<TreasureSpawnInfo> treasuresToSpawn;

    [Header("区域与相机配置")]
    public SpawnAreaProfile spawnAreaProfile;
}