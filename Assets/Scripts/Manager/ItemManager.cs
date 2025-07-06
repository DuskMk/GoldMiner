using System;
using System.Collections.Generic;
using UnityEngine;

public class ItemManager : MonoSingleton<ItemManager>
{
    // 在Unity编辑器的Inspector中，需要把所有道具的ScriptableObject文件拖到这里
    public List<Item> allItems; 
    public Dictionary<ItemType, int> OwnedItems { get; private set; } = new Dictionary<ItemType, int>();

    // 当道具数量变化时，通知UI更新的事件
    public event Action<ItemType, int> OnItemCountChanged;

    /// <summary>
    /// 购买道具
    /// </summary>
    /// <param name="item">要购买的道具</param>
    /// <returns>是否购买成功</returns>
    public bool BuyItem(Item item)
    {
        if (item == null) return false;

        int totalScore = GameManager.Instance.GetTotalScore();

        if (totalScore >= item.price)
        {
            GameManager.Instance.SpendScore(item.price);
            
            AddItem(item.itemType);
            Debug.Log($"成功购买 {item.itemName}.");
            return true;
        }
        else
        {
            Debug.Log($"分数不足，无法购买 {item.itemName}.");
            return false;
        }
    }

    /// <summary>
    /// 往库存中添加道具
    /// </summary>
    /// <param name="itemType">道具类型</param>
    /// <param name="count">数量</param>
    public void AddItem(ItemType itemType, int count = 1)
    {
        if (OwnedItems.ContainsKey(itemType))
        {
            OwnedItems[itemType] += count;
        }
        else
        {
            OwnedItems.Add(itemType, count);
        }
        // 触发事件，通知UI更新
        OnItemCountChanged?.Invoke(itemType, OwnedItems[itemType]);
    }

    /// <summary>
    /// 使用道具
    /// </summary>
    /// <param name="itemType">要使用的道具类型</param>
    /// <param name="consume">是否从库存中消耗该道具</param>
    /// <returns>是否使用成功</returns>
    public bool UseItem(ItemType itemType, bool consume = true)
    {
        if (consume)
        {
            if (!OwnedItems.ContainsKey(itemType) || OwnedItems[itemType] <= 0)
            {
                Debug.Log($"库存中没有 {itemType} 可用。");
                return false;
            }
            OwnedItems[itemType]--;
            OnItemCountChanged?.Invoke(itemType, OwnedItems[itemType]);
        }

        // 从所有道具列表中找到对应的ScriptableObject，以获取其详细数据
        Item itemToUse = GetItemInfo(itemType);
        if (itemToUse == null)
        {
            Debug.LogError($"在ItemManager的allItems列表中未找到 {itemType} 的数据。");
            return false;
        }

        // 触发该道具的实际效果
        TriggerItemEffect(itemToUse);
        
        Debug.Log($"已使用 {itemType}。");

        // 使用道具的跳字提示
        string richText = $"<color=#87CEFA>使用 {itemToUse.itemName}</color>\n<size=22>{itemToUse.description}</size>";
        FloatingTextManager.Instance.ShowAtScreenTop(FloatingTextType.ItemUse, richText, 80, Color.white, 3f);

        return true;
    }

    /// <summary>
    /// 获取指定道具的库存数量
    /// </summary>
    /// <param name="itemType">道具类型</param>
    /// <returns>库存数量</returns>
    public int GetItemCount(ItemType itemType)
    {
        OwnedItems.TryGetValue(itemType, out int count);
        return count;
    }

    /// <summary>
    /// 根据道具类型，触发不同的效果
    /// </summary>
    /// <param name="item">包含效果数据的道具对象</param>
    private void TriggerItemEffect(Item item)
    {
        switch (item.itemType)
        {
            case ItemType.Bomb:
                GameObject.FindObjectOfType<ClawController>()?.DestroyGrabbedTreasure();
                break;
            case ItemType.StrengthPotion:
                GameObject.FindObjectOfType<ClawController>()?.ActivateStrength(item.value, item.duration);
                break;
            case ItemType.TimeExtension:
                GameManager.Instance.AddTime(item.value);
                break;
            case ItemType.LuckyClover:
                GameManager.Instance.ActivateLuckyClover(item.value, item.duration);
                break;
            case ItemType.Magnet:
                LevelManager.Instance.ActivateMagnet(item.duration);
                break;
        }
    }
    public Item GetItemInfo(ItemType itemType)
    {
        return allItems.Find(item => item.itemType == itemType);
    }
}