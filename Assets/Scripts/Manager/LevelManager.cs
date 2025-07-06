using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using System.Collections; 
public enum emLoadLevelType
{
    LoadNextLevel, //加载下一关
    LoadCurrentLevel, //加载当前关卡
    LoadFirstLevel, //加载第一关
}
public class LevelManager : MonoSingleton<LevelManager>
{
    [Header("关卡配置")]
    public List<LevelData> levels; // 将所有关卡的LevelData文件拖到这里
    private int currentLevelIndex = 0;

    private List<GameObject> spawnedTreasures = new List<GameObject>(); // 跟踪本关生成的所有宝藏
    public event System.Action<LevelData> OnLevelDataLoaded; //参数targetScore, time

    public ClawController clawController;

    // 游戏开始时自动加载第一关
    void Start()
    {
        UIManager.Instance.Show<UIStart>();
        // 延迟一小段时间，确保其他Manager都已初始化完毕
        //StartCoroutine(LoadLevelAfterDelay(0, 0.1f));
    }

    // 公开接口，用于加载下一关
    public bool LoadNextLevel()
    {
        currentLevelIndex++;
        if (currentLevelIndex < levels.Count)
        {
            LoadLevel(currentLevelIndex);
            return true; // 成功加载下一关
        }
        else
        {
            Debug.Log("所有关卡已完成！");
            UIManager.Instance.Show<UICompleted>();
            // 在这里处理游戏通关逻辑
            return false; 
        }
    }

    public void LoadLevel(int levelIndex)
    {
        currentLevelIndex = levelIndex;
        LevelData levelData = levels[levelIndex];

        // 1. 清空上一关的宝藏
        ClearPreviousLevel();

        // 2. 应用相机和区域设置
        ApplySpawnAreaProfile(levelData.spawnAreaProfile);

        // 3. 生成新关卡的宝藏
        SpawnTreasures(levelData);

        OnLevelDataLoaded?.Invoke(levelData);

        // 4. 通知GameManager更新UI和游戏状态 (假设GameManager有这个方法)
        // GameManager.Instance.StartNewLevel(levelData.targetScore, levelData.timeLimit);
        Debug.Log($"开始加载关卡 {levelData.levelIndex}. 目标分数: {levelData.targetScore}");
    }
    
    private IEnumerator LoadLevelAfterDelay(int levelIndex, float delay)
    {
        yield return new WaitForSeconds(delay);
        LoadLevel(levelIndex);
    }


    private void ClearPreviousLevel()
    {
        foreach (var treasure in spawnedTreasures)
        {
            // 确保物体还存在才归还
            if (treasure != null && treasure.activeInHierarchy)
            {
                GameObjectManager.Instance.Release(treasure);
            }
        }
        spawnedTreasures.Clear();
    }
    public bool CheckIsTresureActive()
    {
        foreach (var treasure in spawnedTreasures)
        {
            if (treasure != null && treasure.activeInHierarchy)
            {
                return true;
            }
        }
        return false;
    }
    private void ApplySpawnAreaProfile(SpawnAreaProfile profile)
    {
        if (profile == null) return;
        Camera mainCamera = Camera.main;
        if (mainCamera != null)
        {
            mainCamera.transform.position = profile.cameraPosition;
            mainCamera.orthographicSize = profile.cameraOrthographicSize;
        }
    }

    private void SpawnTreasures(LevelData levelData)
    {
        Rect spawnRect = levelData.spawnAreaProfile.spawnBounds;
        int maxAttempts = 100; // 防止因区域太小或物体太多导致死循环

        foreach (var treasureInfo in levelData.treasuresToSpawn)
        {
            for (int i = 0; i < treasureInfo.count; i++)
            {
                int attempts = 0;
                bool positionFound = false;
                while (!positionFound && attempts < maxAttempts)
                {
                    attempts++;
                    // 在矩形区域内随机生成一个点
                    Vector2 randomPoint = new Vector2(
                        Random.Range(spawnRect.xMin, spawnRect.xMax),
                        Random.Range(spawnRect.yMin, spawnRect.yMax)
                    );

                    // 检查这个点是否与其他已生成的宝藏重叠
                    if (!IsOverlapping(randomPoint))
                    {
                        GameObject treasure = GameObjectManager.Instance.Get(treasureInfo.type);
                        if (treasure != null)
                        {
                            treasure.transform.position = randomPoint;
                            treasure.transform.rotation = Quaternion.Euler(0, 0, Random.Range(0, 360)); // 随机旋转增加多样性
                            spawnedTreasures.Add(treasure);
                            positionFound = true;
                        }
                    }
                }
                if (!positionFound)
                {
                    Debug.LogWarning($"无法为 {treasureInfo.type} 找到一个不重叠的位置！");
                }
            }
        }
    }

    // 检查点是否与已生成的宝藏重叠
    private bool IsOverlapping(Vector2 point)
    {
        foreach (var treasure in spawnedTreasures)
        {
            if (treasure.activeInHierarchy)
            {
                // 使用宝藏的Collider来判断距离
                Collider2D col = treasure.GetComponent<Collider2D>();
                if (col != null)
                {
                    // 计算点到宝藏中心点的距离是否小于一个安全半径
                    // 这里用一个简单的半径估算，更精确可以用Collider2D.OverlapPoint
                    float distance = Vector2.Distance(point, treasure.transform.position);
                    float safeRadius = (col.bounds.size.x + col.bounds.size.y) / 2; // 用平均半径作为安全距离
                    if (distance < safeRadius)
                    {
                        return true; // 重叠了
                    }
                }
            }
        }
        return false; // 未重叠
    }
    public LevelData GetCurLevelData()
    {
        if (currentLevelIndex < levels.Count)
        {
            return levels[currentLevelIndex];
        }
        return null; // 如果没有当前关卡数据，返回null
    }
    public bool StartLoadLevel(emLoadLevelType loadLevelType)
    {
        switch (loadLevelType)
        {
            case emLoadLevelType.LoadNextLevel:
                return LoadNextLevel();
            case emLoadLevelType.LoadCurrentLevel:
                LoadLevel(currentLevelIndex);
                return true;
            case emLoadLevelType.LoadFirstLevel:
                currentLevelIndex = 0;
                LoadLevel(currentLevelIndex);
                return true;
            default:
                return false;
        }
    }
    
    

    public void ActivateMagnet(float duration)
    {
        // ... (Find all active gold treasures logic is the same)
        var goldTreasures = spawnedTreasures.Where(t => 
            t.activeInHierarchy &&
            t.GetComponent<Treasure>() != null &&
            (t.GetComponent<Treasure>().myType == TreasureType.Gold_Big ||
             t.GetComponent<Treasure>().myType == TreasureType.Gold_Mid ||
             t.GetComponent<Treasure>().myType == TreasureType.Gold_Small)
        ).ToList();

        if (goldTreasures.Count == 0)
        {
            Debug.Log("Magnet used, but no gold found.");
            return;
        }

        GameObject targetTreasureGO = goldTreasures[Random.Range(0, goldTreasures.Count)];
        Treasure targetTreasure = targetTreasureGO.GetComponent<Treasure>();

        if (targetTreasure == null) return;
        
        Debug.Log($"Magnet targeting: {targetTreasure.name}");

        Vector3 targetPosition = clawController?.initialPosition?? FindObjectOfType<ClawController>().initialPosition;
        targetPosition.y -= 2; 

        // Call the treasure's own method
        targetTreasure.StartMagneticMove(targetPosition, duration);
    }
}