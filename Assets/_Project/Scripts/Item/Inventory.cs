using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    [Header("인벤토리 설정")]
    [SerializeField] private int maxSlots = 25;
    [SerializeField] private bool isRaidInventory = true; // Raid용인지 HideOut용인지

    // 아이템 추가
    public bool AddItem(ItemData item, int count = 1)
    {
        if (GameData.Instance == null) return false;

        List<InventoryItem> items = GetItems();

        if (items.Count >= maxSlots)
        {
            Debug.Log("인벤토리가 가득 찼습니다!");
            return false;
        }

        // 무게 체크
        if(PlayerData.Instance != null)
        {
            PlayerData.Instance.AddWeight(item.weight);
        }

        if(isRaidInventory)
        {
            GameData.Instance.AddRaidItem(item, count);
        }
        else
        {
            GameData.Instance.AddToStash(item, count);
        }

        Debug.Log(item.itemName + "획득");
        return true;
    }

    // 아이템 제거
    public void RemoveItem(InventoryItem invitem)
    {
        if (GameData.Instance == null) return;

        if (isRaidInventory)
        {
            GameData.Instance.raidItems.Remove(invitem);
        }
        else
        {
            GameData.Instance.stashItems.Remove(invitem);
        }

        Debug.Log(invitem.itemData.itemName + " 제거됨");
    }

    // 아이템 목록 반환
    public List<InventoryItem> GetItems()
    {
        if (GameData.Instance == null) return new List<InventoryItem>();

        if (isRaidInventory)
        {
            return GameData.Instance.raidItems;
        }
        else
        {
            return GameData.Instance.stashItems;
        }
    }

    // 인벤토리 비우기
    public void Clear()
    {
        if (GameData.Instance == null) return;

        if (isRaidInventory)
        {
            GameData.Instance.raidItems.Clear();
        }
        else
        {
            GameData.Instance.stashItems.Clear();
        }

        Debug.Log("인벤토리 비움");
    }

    // 현재 아이템 수
    public int GetItemCount()
    {
        return GetItems().Count;
    }

    // 최대 슬롯 수
    public int GetMaxSlots()
    {
        return maxSlots;
    }
}
