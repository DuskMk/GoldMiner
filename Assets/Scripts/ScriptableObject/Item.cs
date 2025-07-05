using UnityEngine;

public enum ItemType
{
    StrengthPotion, // 增加力气
    Bomb,           // 炸弹
    TimeExtension,  // 增加时间
    LuckyClover,    // 幸运草
    Magnet          // 磁铁
}

[CreateAssetMenu(fileName = "New Item", menuName = "Gold Miner/Item")]
public class Item : ScriptableObject
{
    public ItemType itemType;
    public string itemName;
    [TextArea]
    public string description;
    public int price;
    public float duration; // For timed items like StrengthPotion, LuckyClover, Magnet
    public float value;    // Effect value (e.g., strength multiplier, score bonus percentage)
    public Sprite icon;
} 