using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjLootUI : MonoBehaviour
{
    [Header("슬롯 참조")]
    [SerializeField] private ItemSlot[] slots;  // 15개 (5x3)

    private FieldLootContainer currentContainer;

    public void ShowLoot(FieldLootContainer container, Inventory inventory)
    {
        currentContainer = container;
        gameObject.SetActive(true);
        RefreshUI();
    }

    private void RefreshUI()
    {
        // 모든 슬롯 비우기
        foreach (ItemSlot slot in slots)
        {
            slot.SetItem((InventoryItem)null);
        }

        // 아이템 배치 (LootResult 사용!)
        List<LootResult> items = currentContainer.GetItems();
        for (int i = 0; i < items.Count && i < slots.Length; i++)
        {
            // InventoryItem으로 변환 (개수 포함!)
            InventoryItem invItem = new InventoryItem(items[i].itemData, items[i].count);
            slots[i].SetItem(invItem);
        }
    }

    // 슬롯에서 아이템 가져갔을 때 호출 (수정!)
    public void OnItemTaken(InventoryItem item)
    {
        if (currentContainer != null && item != null)
        {
            currentContainer.TakeItem(item.itemData);
            RefreshUI();  // UI 새로고침
        }
    }

    // ItemData 오버로드
    public void OnItemTaken(ItemData itemData)
    {
        if (currentContainer != null && itemData != null)
        {
            currentContainer.TakeItem(itemData);
            RefreshUI();
        }
    }

    public void Close()
    {
        gameObject.SetActive(false);
    }

    public FieldLootContainer GetCurrentContainer()
    {
        return currentContainer;
    }
}