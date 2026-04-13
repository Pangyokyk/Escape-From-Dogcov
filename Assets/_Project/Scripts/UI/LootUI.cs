using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class LootUI : MonoBehaviour
{
    [Header("패널")]
    [SerializeField] private GameObject lootPanel;

    [Header("슬롯 부모")]
    [SerializeField] private Transform slotContainer;

    [Header("슬롯 프리팹")]
    [SerializeField] private GameObject slotPrefab;

    // 현재 루트 컨테이너
    private LootContainer currentLoot;
    private List<ItemSlot> itemSlots = new List<ItemSlot>();

    // esc 입력
    private PlayerInputActions inputActions;

    private void Awake()
    {
        inputActions = new PlayerInputActions();
    }

    private void FindAllSlots()
    {
        itemSlots.Clear();
        if (slotContainer == null) return;

        ItemSlot[] slots = slotContainer.GetComponentsInChildren<ItemSlot>(true);
        foreach (ItemSlot slot in slots)
        {
            slot.slotType = ItemSlot.SlotType.Loot;
            itemSlots.Add(slot);
        }

        Debug.Log("루트 슬롯 수 : " + itemSlots.Count);
    }


    public void ShowLoot(LootContainer loot)
    {
        currentLoot = loot;

        if(lootPanel != null)
        {
            lootPanel.SetActive(true);
        }

        // 슬롯 찾기
        if(itemSlots.Count == 0)
        {
            FindAllSlots();
        }

        // InventoryManager에 현재 루트 설정
        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.SetCurrentLoot(loot);
        }

        RefreshUI();

        // 마우스
        if(Crosshair.Instance != null)
        {
            Crosshair.Instance.ShowCursor();
        }
    }

    public void RefreshUI()
    {
        if (currentLoot == null) return;

        // 아이템 수만큼 슬롯 생성
        List<ItemData> items = currentLoot.GetItems();

        Debug.Log("루트 아이템 수: " + items.Count + " / 슬롯 수: " + itemSlots.Count);

        for (int i = 0; i < itemSlots.Count; i++)
        {
            if (itemSlots[i] == null) continue;

            if(i < items.Count && items[i] != null)
            {
                int count = 1;
                if (items[i].itemType == ItemData.ItemType.Ammo)
                {
                    count = items[i].ammoCount;
                }
                itemSlots[i].SetItem(new InventoryItem(items[i], count));
            }
            else
            {
                itemSlots[i].SetItem((InventoryItem)null);
            }
        }
    }

    public void Close()
    {
        if(InventoryManager.Instance != null)
        {
            InventoryManager.Instance.EndDrag();
            InventoryManager.Instance.SetCurrentLoot(null);
        }

        currentLoot = null;

        // 모든 슬롯 비우기
        foreach (ItemSlot slot in itemSlots)
        {
            if (slot != null)
            {
                slot.SetItem((InventoryItem)null);
            }
        }

        if (lootPanel != null)
        {
            lootPanel.SetActive(false);
        }

        // 인벤토리 닫기
        InventoryUI inventoryUI = FindObjectOfType<InventoryUI>();
        if(inventoryUI != null)
        {
            inventoryUI.CloseInventory();
        }

        // 마우스 숨기기
        if (Crosshair.Instance != null)
        {
            Crosshair.Instance.HideCursor();
        }
    }

    public bool IsOpen()
    {
        return lootPanel != null && lootPanel.activeSelf;
    }

    private void OnEnable()
    {
        inputActions.Player.Enable();
        inputActions.Player.Cancel.performed += OnCancel;
    }

    private void OnDisable()
    {
        inputActions.Player.Disable();
        inputActions.Player.Cancel.performed -= OnCancel;
    }

    private void OnCancel(InputAction.CallbackContext context)
    {
        if(IsOpen())
        {
            Close();
        }
    }
}