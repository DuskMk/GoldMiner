using UnityEngine;
using UnityEngine.Pool; // 引入对象池API的命名空间
using System.Collections.Generic; // 引入字典的命名空间

// 定义一个枚举来区分不同的宝藏类型，这比用字符串或数字更清晰安全
public enum TreasureType
{
    Gold_Big,
    Gold_Mid,
    Gold_Small,
    Rock_Big,
    Rock_Mid,
    Rock_Small,
    Diamond
}

public class GameObjectManager : MonoSingleton<GameObjectManager>
{

    // 用于在Inspector中配置每种宝藏的Prefab
    [System.Serializable]
    public class TreasurePrefab
    {
        public TreasureType type;
        public GameObject prefab;
    }

    public List<TreasurePrefab> treasurePrefabs; // 宝藏预制体列表

    // 核心：一个字典，键是宝藏类型，值是对应的对象池
    private Dictionary<TreasureType, IObjectPool<GameObject>> treasurePools;
    private Transform thisTransform;
    void Start()
    {
        thisTransform = transform;
        InitializePools();
    }

    private void InitializePools()
    {
        treasurePools = new Dictionary<TreasureType, IObjectPool<GameObject>>();

        foreach (var item in treasurePrefabs)
        {
            // 为列表中的每一种宝藏创建一个独立的对象池
            // localItem捕获循环变量，避免闭包问题
            var localItem = item;
            var pool = new ObjectPool<GameObject>(
                createFunc: () => Instantiate(localItem.prefab, thisTransform), // 如何创建新对象
                actionOnGet: (obj) => { obj.SetActive(true);  },      // 从池中取出时做什么
                actionOnRelease: (obj) => { obj.SetActive(false); obj.transform.SetParent(thisTransform); },   // 归还到池中时做什么
                actionOnDestroy: (obj) => Destroy(obj),         // 当池满或销毁时做什么
                collectionCheck: false,  // 是否检查对象是否已在池中（开发时可开true）
                defaultCapacity: 10,     // 池的默认容量
                maxSize: 20              // 池的最大容量
            );

            // 将创建好的池存入字典
            treasurePools.Add(localItem.type, pool);
        }
    }

    /// <summary>
    /// 从对象池获取一个指定类型的宝藏
    /// </summary>
    /// <param name="type">想要的宝藏类型</param>
    /// <returns>一个激活的宝藏游戏对象</returns>
    public GameObject Get(TreasureType type)
    {
        if (treasurePools.ContainsKey(type))
        {
            return treasurePools[type].Get();
        }
        else
        {
            Debug.LogError($"Pool for treasure type '{type}' does not exist.");
            return null;
        }
    }

    /// <summary>
    /// 将一个宝藏归还到它的对象池
    /// </summary>
    /// <param name="treasureObject">要归还的宝藏游戏对象</param>
    public void Release(GameObject treasureObject)
    {
        // 我们需要知道这个对象属于哪个池
        // 可以在对象上附加一个脚本来存储它的类型
        Treasure treasureComponent = treasureObject.GetComponent<Treasure>();
        if (treasureComponent != null)
        {
            TreasureType type = treasureComponent.myType; // 假设Treasure脚本有myType字段
            if (treasurePools.ContainsKey(type))
            {
                treasurePools[type].Release(treasureObject);
            }
            else
            {
                Debug.LogWarning($"Trying to release an object '{treasureObject.name}' to a non-existent pool. Destroying it instead.");
                Destroy(treasureObject);
            }
        }
    }
}