using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameData : MonoBehaviour
{
    public static GameData Instance { get; private set; }

    [Header("창고 아이템")]
    public List<InventoryItem> stashItems = new List<InventoryItem>();

    [Header("레이드 인벤토리")]
    public List<InventoryItem> raidItems = new List<InventoryItem>();

    [Header("보호 슬롯")]
    public List<InventoryItem> secureItems = new List<InventoryItem>();

    [Header("테스트용 초기 아이템")]
    [SerializeField] private List<ItemData> startingItems = new List<ItemData>();
    [SerializeField] private bool useTestItems = true;

    private void Awake()
    {
        // 싱글톤 - 씬 바뀌어도 유지
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            // 테스트 모드 초기아이템 지급
            if (useTestItems && stashItems.Count == 0)
            {
                foreach (ItemData itemData in startingItems)
                {
                    if (itemData != null)
                    {
                        int count = 1;
                        if(itemData.itemType == ItemData.ItemType.Ammo)
                        {
                            count = itemData.ammoCount; // ItemData에 설정된 개수
                        }
                        stashItems.Add(new InventoryItem(itemData, count));
                    }
                }
                Debug.Log("테스트 아이템 " + stashItems.Count + "개 지급됨");
            }
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        // 게임 시작 시 저장 데이터 불러오기
        if (SaveManager.Instance != null)
        {
            SaveManager.Instance.LoadGame();
        }
    }

    // 레이드 시작 - 인벤토리 비우기
    public void StartRaid()
    {
        Debug.Log("레이드 시작! 인벤토리 아이템 개수 : " + raidItems.Count + "개");
    }

    // 추출 성공 - 레이드 아이템을 창고로
    public void ExtractionSuccess()
    {
        // 자동 저장
        if(SaveManager.Instance != null)
        {
            SaveManager.Instance.SaveGame();
        }
    }

    // 사망 - 레이드 아이템 손실
    public void PlayerDied()
    {
        raidItems.Clear();
        Debug.Log("사망 레이드 아이템 손실, 팬티아이템은 살아남음");

        // 자동 저장
        if (SaveManager.Instance != null)
        {
            SaveManager.Instance.SaveGame();
        }
        Debug.Log("사망! 레이드 아이템 손실");
        
    }

    // 레이드 중 아이템 획득
    public void AddRaidItem(ItemData itemData, int count = 1)
    {
        AddItem(raidItems, itemData, count);
    }

    public void AddToStash(ItemData itemData, int count = 1)
    {
        AddItem(stashItems, itemData, count);
    }


    private void AddItem(List<InventoryItem> list, ItemData itemData, int count)
    {
        // 스택 가능한 아이템인지 확인
        if (itemData.itemType == ItemData.ItemType.Ammo)
        {
            // 같은 아이템 찾기
            foreach (InventoryItem existing in list)
            {
                if (existing.itemData == itemData)
                {
                    int maxStack = existing.GetMaxStack();
                    int canAdd = maxStack - existing.count;

                    if (canAdd > 0)
                    {
                        int toAdd = Mathf.Min(count, canAdd);
                        existing.count += toAdd;
                        count -= toAdd;

                        if (count <= 0) return;
                    }
                }
            }
        }

        // 새 슬롯에 추가
        if (count > 0)
        {
            list.Add(new InventoryItem(itemData, count));
        }
    }

    // === 아이템 제거 ===
    public void RemoveRaidItem(InventoryItem item)
    {
        raidItems.Remove(item);
    }

    public void RemoveStashItem(InventoryItem item)
    {
        stashItems.Remove(item);
    }

    // === 탄약 사용 (장전 시) ===
    public int UseAmmo(WeaponData.AmmoType ammoType, int amount)
    {
        int totalUsed = 0;
        List<InventoryItem> toRemove = new List<InventoryItem>();

        foreach (InventoryItem item in raidItems)
        {
            if (item.itemData.itemType != ItemData.ItemType.Ammo) continue;
            if (item.itemData.ammoType != ammoType) continue;

            int canUse = Mathf.Min(item.count, amount - totalUsed);
            item.count -= canUse;
            totalUsed += canUse;

            if (item.count <= 0)
            {
                toRemove.Add(item);
            }

            if (totalUsed >= amount) break;
        }

        foreach (InventoryItem item in toRemove)
        {
            raidItems.Remove(item);
        }

        return totalUsed;
    }

    // === 탄약 확인 ===
    public int GetAmmoCount(WeaponData.AmmoType ammoType)
    {
        int total = 0;
        foreach (InventoryItem item in raidItems)
        {
            if (item.itemData.itemType == ItemData.ItemType.Ammo &&
                item.itemData.ammoType == ammoType)
            {
                total += item.count;
            }
        }
        return total;
    }
}
