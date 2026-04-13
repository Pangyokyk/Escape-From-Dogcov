using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StashUI : MonoBehaviour
{
    [Header("패널")]
    [SerializeField] private GameObject stashPanel;

    [Header("슬롯 부모 (Content)")]
    [SerializeField] private Transform slotContainer;

    [Header("연결")]
    [SerializeField] private InventoryUI inventoryUI;

    [Header("설정")]
    [SerializeField] private int maxSlots = 60;

    // 슬롯 목록
    private List<ItemSlot> itemSlots = new List<ItemSlot>();

    private void Start()
    {
        FindAllSlots();
    }

    private void FindAllSlots()
    {
        itemSlots.Clear();

        // 모든 자식에서 ItemSlot 찾기
        ItemSlot[] slots = slotContainer.GetComponentsInChildren<ItemSlot>(true);
        foreach (ItemSlot slot in slots)
        {
            slot.slotType = ItemSlot.SlotType.Stash;
            itemSlots.Add(slot);
        }

        Debug.Log("창고 슬롯 수: " + itemSlots.Count);
    }

    public void OpenStash()
    {
        stashPanel.SetActive(true);

        if (itemSlots.Count == 0)
        {
            FindAllSlots();
        }

        RefreshUI();

        // 인벤토리도 열기
        if (inventoryUI != null)
        {
            inventoryUI.OpenInventory();
        }

        Cursor.visible = true;
    }

    public void CloseStash()
    {
        // 드래그 취소
        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.EndDrag();
        }

        stashPanel.SetActive(false);

        if (inventoryUI != null)
        {
            inventoryUI.CloseInventory();
        }

        Cursor.visible = false;
    }

    public void RefreshUI()
    {
        if (GameData.Instance == null) return;

        List<InventoryItem> items = GameData.Instance.stashItems;

        for (int i = 0; i < itemSlots.Count; i++)
        {
            if (itemSlots[i] == null) continue;

            // 최대 슬롯 초과하면 비활성화
            if (i >= maxSlots)
            {
                itemSlots[i].gameObject.SetActive(false);
                continue;
            }

            itemSlots[i].gameObject.SetActive(true);

            // 아이템 설정
            if (i < items.Count && items[i] != null)
            {
                itemSlots[i].SetItem(items[i]);             // InventoryItem 버전 호출됨
            }
            else
            {
                itemSlots[i].SetItem((InventoryItem)null);  // 명시적 캐스팅
            }
        }
    }

    public bool IsOpen()
    {
        return stashPanel.activeSelf;
    }

    public int GetMaxSlots()
    {
        return maxSlots;
    }
}
