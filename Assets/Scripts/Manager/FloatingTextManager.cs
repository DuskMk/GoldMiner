using UnityEngine;
using UnityEngine.Pool;
using TMPro;
using DG.Tweening;
using System.Collections.Generic;
using System.Linq;

// 定义可用的跳字UI类型
public enum FloatingTextType
{
    Default,
    Score,
    ItemGet,
    ItemUse
}

public class FloatingTextManager : MonoSingleton<FloatingTextManager>
{
    [System.Serializable]
    public class FloatingTextPrefabMapping
    {
        public FloatingTextType type;
        public GameObject prefab;
    }

    [Header("设置")]
    [Tooltip("配置不同类型的跳字UI预制件")]
    public List<FloatingTextPrefabMapping> prefabMappings; 
    
    [Tooltip("所有跳字UI的父级Canvas，如果为空会自动寻找")]
    public Canvas parentCanvas; 

    private Dictionary<FloatingTextType, IObjectPool<TextMeshProUGUI>> textPools;

    void Start()
    {
        if (parentCanvas == null) parentCanvas = FindObjectOfType<Canvas>();

        if (prefabMappings == null || prefabMappings.Count == 0 || parentCanvas == null)
        {
            Debug.LogError("FloatingTextManager 未正确配置！请检查 Prefab Mappings 和 Canvas 设置。");
            this.enabled = false;
            return;
        }

        textPools = new Dictionary<FloatingTextType, IObjectPool<TextMeshProUGUI>>();

        foreach (var mapping in prefabMappings)
        {
            var localMapping = mapping; // 避免闭包问题
            var pool = new ObjectPool<TextMeshProUGUI>(
                createFunc: () => {
                    GameObject textGO = Instantiate(localMapping.prefab, parentCanvas.transform);
                    return textGO.GetComponentInChildren<TextMeshProUGUI>(); // 使用GetComponentInChildren更灵活
                },
                actionOnGet: (text) => text.gameObject.SetActive(true),
                actionOnRelease: (text) => text.gameObject.SetActive(false),
                actionOnDestroy: (text) => Destroy(text.gameObject),
                collectionCheck: false,
                defaultCapacity: 10,
                maxSize: 30
            );
            textPools.Add(mapping.type, pool);
        }
    }

    /// <summary>
    /// 在指定屏幕位置显示跳字
    /// </summary>
    public void Show(FloatingTextType type, string text, int fontSize, Color color, Vector3 screenPosition, Vector3 motion, float duration)
    {
        if (!this.enabled || !textPools.ContainsKey(type)) return;

        TextMeshProUGUI textInstance = textPools[type].Get();

        textInstance.transform.position = screenPosition;
        textInstance.text = text;
        textInstance.fontSize = fontSize;
        textInstance.color = new Color(color.r, color.g, color.b, 0); // Start transparent

        // 使用DOTween创建动画序列
        Sequence sequence = DOTween.Sequence();
        
        // 动画效果: 弹出 -> 上浮 -> 渐隐
        sequence.Append(textInstance.DOFade(1, 0.2f)); // 快速淡入
        sequence.Join(textInstance.transform.DOScale(1.5f, 0.3f).SetEase(Ease.OutQuad)); // 弹出效果
        sequence.Append(textInstance.transform.DOScale(1f, 0.2f)); // 恢复正常大小
        sequence.Join(textInstance.transform.DOMove(screenPosition + motion, duration * 0.8f).SetEase(Ease.OutCubic)); // 在大部分时间里上浮
        sequence.AppendInterval(duration * 0.1f); // 短暂悬停
        sequence.Append(textInstance.DOFade(0, 0.5f)); // 渐隐消失
        
        // 动画结束后，将对象归还到池中
        sequence.OnComplete(() => {
            textPools[type].Release(textInstance);
        });
    }

    /// <summary>
    /// 在指定世界坐标位置显示跳字 (会自动转换到屏幕坐标)
    /// </summary>
    public void ShowInWorld(FloatingTextType type, string text, int fontSize, Color color, Vector3 worldPosition, Vector3 motion, float duration)
    {
        if (!this.enabled) return;
        Vector3 screenPosition = Camera.main.WorldToScreenPoint(worldPosition);
        if (screenPosition.z < 0) return; // 不显示在镜头后的物体

        Show(type, text, fontSize, color, screenPosition, motion, duration);
    }
    
    /// <summary>
    /// 在屏幕顶部中心显示跳字
    /// </summary>
    public void ShowAtScreenTop(FloatingTextType type, string text, int fontSize, Color color, float duration)
    {
        if (!this.enabled) return;
        Vector3 position = new Vector3(Screen.width / 2, Screen.height - 150, 0);
        Vector3 motion = new Vector3(0, 60, 0);
        Show(type, text, fontSize, color, position, motion, duration);
    }
} 