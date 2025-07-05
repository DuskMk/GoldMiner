using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UIStore : UIWindow
{
    [Header("商店")]
    public List<Item> itemsForSale;
    public GameObject itemSlotPrefab;
    public Transform itemContainer;
    public TMP_Text totalScoreText;


    protected override void OnEnable()
    {
        GameManager.Instance.OnScoreChanged += UpdateScoreText;
        UpdateScoreText(0, GameManager.Instance.GetTotalScore()); // Initial update
    }

    protected override void OnDisable()
    {
        GameManager.Instance.OnScoreChanged -= UpdateScoreText;
    }

    void Start()
    {
        // Clear existing items before populating
        foreach (Transform child in itemContainer)
        {
            Destroy(child.gameObject);
        }

        // Populate store with items
        foreach (var item in itemsForSale)
        {
            GameObject slotGO = Instantiate(itemSlotPrefab, itemContainer);
            UIItemSlot slot = slotGO.GetComponent<UIItemSlot>();
            slot.SetItem(item);
        }
    }

    private void UpdateScoreText(int score, int totalScore)
    {
        if (totalScoreText != null)
        {
            totalScoreText.text = $"可用总分: {totalScore}";
        }
    }

    public void OnNextLevelButtonClick()
    {
        OnClickClose();
        GameManager.Instance.StartReady(emLoadLevelType.LoadNextLevel);
    }
} 