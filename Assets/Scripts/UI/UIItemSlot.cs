using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIItemSlot : MonoBehaviour
{
    public Image icon;
    public TMP_Text itemName;
    public TMP_Text price;
    public Button buyButton;

    private Item currentItem;

    public void SetItem(Item item)
    {
        currentItem = item;
        icon.sprite = item.icon;
        itemName.text = item.itemName;
        price.text = $"${item.price}";
        buyButton.onClick.AddListener(OnBuyButtonClick);
    }

    private void OnBuyButtonClick()
    {
        bool success = ItemManager.Instance.BuyItem(currentItem);
        if (success)
        {
            Debug.Log($"Successfully bought {currentItem.itemName}");
            // Optionally, update the button to show "Sold Out" or disable it
            // buyButton.interactable = false;
        }
        else
        {
            Debug.Log($"Failed to buy {currentItem.itemName}");
            // Optionally, give player feedback (e.g., a sound, a shake)
        }
    }

    private void OnDestroy()
    {
        buyButton.onClick.RemoveListener(OnBuyButtonClick);
    }
} 