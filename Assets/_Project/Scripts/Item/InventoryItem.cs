using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class InventoryItem
{
    public ItemData itemData;   // 아이템 정보
    public int count;           // 개수(탄약, 의료품 등)
    public int currentUses;     // 남은 사용 횟수

    public InventoryItem(ItemData data, int amount = 1)
    {
        itemData = data;
        count = amount;
        currentUses = data.maxUses;     // 초기값 = 최대 사용 횟수
    }

    // 스택 가능한 아이템인지(총알만)
    public bool IsStackable()
    {
        if(itemData == null) return false;
        return itemData.itemType == ItemData.ItemType.Ammo;
    }

    // 최대 스택 개수
    public int GetMaxStack()
    {
        if (itemData == null) return 1;

        switch(itemData.itemType)
        {
            case ItemData.ItemType.Ammo:
                return 300;     // 탄약 최대 300발
            default:
                return 1;
        }
    }
}
