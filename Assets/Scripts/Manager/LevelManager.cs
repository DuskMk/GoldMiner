using UnityEngine;
using System.Collections.Generic;
using System;

public class LevelManager : MonoSingleton<LevelManager>
{
    // --- 事件定义 ---
    // 当一个新关卡准备好时，发布关卡数据
    public event Action<int, int> OnLevelDataLoaded; // 参数: targetScore, levelTime

    [Header("关卡资源")]
    public List<GameObject> treasurePrefabs; // 所有宝藏预制体的列表

    [Header("关卡生成设置")]
    public Transform generationAreaMin;      // 生成区域的左下角点
    public Transform generationAreaMax;      // 生成区域的右上角点
    public int itemsToGenerate = 15;         // 本关生成的物品数量

    // --- 模拟的关卡数据 ---
    // 在更复杂的项目中，这里会是一个关卡数据列表 (e.g., List<LevelData>)
    [SerializeField] private int currentLevelIndex = 1;
    [SerializeField] private int currentTargetScore = 200;
    [SerializeField] private float currentLevelTime = 60f;

    void Awake()
    {
        
    }

    void Start()
    {
        LoadLevel(currentLevelIndex);
    }

    public void LoadLevel(int levelIndex)
    {
        // 1. 清理上一关卡的残留物 (如果需要)
        // ClearPreviousLevel();

        // 2. 根据levelIndex加载关卡数据 (目前我们先用固定数据模拟)
        // 在真实项目中，你会从一个ScriptableObject或文件中读取数据
        this.currentTargetScore = 100 + levelIndex * 50; // 示例：目标分数随关卡增加
        this.currentLevelTime = 60f; // 示例：时间固定

        // 3. 发布关卡数据加载完成事件，通知GameManager和UIManager
        OnLevelDataLoaded?.Invoke(currentTargetScore, (int)currentLevelTime);

        // 4. 生成本关卡的宝藏
        GenerateTreasures();

        Debug.Log($"Level {levelIndex} loaded. Target: {currentTargetScore}");
    }

    void GenerateTreasures()
    {
        for (int i = 0; i < itemsToGenerate; i++)
        {
            int randomIndex = UnityEngine.Random.Range(0, treasurePrefabs.Count);
            GameObject selectedPrefab = treasurePrefabs[randomIndex];

            float randomX = UnityEngine.Random.Range(generationAreaMin.position.x, generationAreaMax.position.x);
            float randomY = UnityEngine.Random.Range(generationAreaMin.position.y, generationAreaMax.position.y);
            Vector3 randomPosition = new Vector3(randomX, randomY, 0);

            Instantiate(selectedPrefab, randomPosition, Quaternion.identity);
        }
    }
}