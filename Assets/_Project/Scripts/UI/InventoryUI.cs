using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using TMPro;
using Unity.VisualScripting;

public class InventoryUI : MonoBehaviour
{
    public static InventoryUI Instance { get; private set; }

    [Header("패널")]
    [SerializeField] private GameObject inventoryPanel;

    [Header("일반 슬롯 부모")]
    [SerializeField] private Transform slotContainer;

    [Header("장비 슬롯")]
    [SerializeField] private ItemSlot weapon1Slot;
    [SerializeField] private ItemSlot weapon2Slot;
    [SerializeField] private ItemSlot helmetSlot;
    [SerializeField] private ItemSlot armorSlot;

    [Header("보안 슬롯")]
    [SerializeField] private ItemSlot[] secureSlots;

    [Header("설정")]
    [SerializeField] private int maxSlots = 25;

    [Header("무게 표시")]
    [SerializeField] private TextMeshProUGUI weightText;

    [Header("돈 표시")]
    [SerializeField] private TextMeshProUGUI moneyText;

    // 슬롯 목록
    private List<ItemSlot> itemSlots = new List<ItemSlot>();
    private PlayerInputActions inputActions;

    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

            inputActions = new PlayerInputActions();
    }

    private void OnEnable()
    {
        if (inputActions == null) return;

        Debug.Log("InventoryUI OnEnable!");
        inputActions.Player.Enable();
        inputActions.Player.Inventory.performed += OnInventoryToggle;
    }

    private void OnDisable()
    {
        if (inputActions == null) return;

        inputActions.Player.Disable();
        inputActions.Player.Inventory.performed -= OnInventoryToggle;
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    private void Start()
    {
        if (inventoryPanel != null)
        {
            inventoryPanel.SetActive(false);
        }
    }

    private void FindAllSlots()
    {
        itemSlots.Clear();

        if (slotContainer == null)
        {
            Debug.LogError("SlotContainer가 연결되지 않았습니다!");
            return;
        }

        ItemSlot[] slots = slotContainer.GetComponentsInChildren<ItemSlot>(true);
        foreach (ItemSlot slot in slots)
        {
            slot.slotType = ItemSlot.SlotType.Inventory;
            itemSlots.Add(slot);
        }

        Debug.Log("인벤토리 슬롯 수: " + itemSlots.Count);
    }

    private void SetupEquipmentSlots()
    {
        if (weapon1Slot != null)
            weapon1Slot.slotType = ItemSlot.SlotType.Weapon1;

        if (weapon2Slot != null)
            weapon2Slot.slotType = ItemSlot.SlotType.Weapon2;

        if (helmetSlot != null)
            helmetSlot.slotType = ItemSlot.SlotType.Helmet;

        if (armorSlot != null)
            armorSlot.slotType = ItemSlot.SlotType.Armor;
    }

    private void OnInventoryToggle(InputAction.CallbackContext context)
    {
        if (inventoryPanel.activeSelf)
        {
            CloseInventory();
        }
        else
        {
            OpenInventory();
        }
    }

    public void OpenInventory()
    {
        inventoryPanel.SetActive(true);

        if (itemSlots.Count == 0)
        {
            FindAllSlots();
            SetupEquipmentSlots();
        }

        RefreshUI();

        // SecureSlots 새로고침
        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.RefreshSecureSlots();
        }

        if (Crosshair.Instance != null)
        {
            Crosshair.Instance.ShowCursor();
        }
    }

    public void CloseInventory()
    {
        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.EndDrag();
        }

        inventoryPanel.SetActive(false);

        if(TooltipUI.Instance != null)
        {
            TooltipUI.Instance.Hide();
        }

        if (Crosshair.Instance != null)
        {
            Crosshair.Instance.HideCursor();
        }
    }

    public void RefreshUI()
    {
        Debug.Log("RefreshUI 호출됨! 슬롯 수: " + itemSlots.Count);

        RefreshInventorySlots();
        RefreshEquipmentSlots();
        UpdateWeightText();
        UpdateMoneyText();
    }

    private void RefreshInventorySlots()
    {
        if (GameData.Instance == null) return;

        List<InventoryItem> items = GameData.Instance.raidItems;

        Debug.Log("인벤토리 아이템 수: " + items.Count + " / 슬롯 수: " + itemSlots.Count);

        for (int i = 0; i < itemSlots.Count; i++)
        {
            if (itemSlots[i] == null) continue;

            if (i >= maxSlots)
            {
                itemSlots[i].gameObject.SetActive(false);
                continue;
            }

            itemSlots[i].gameObject.SetActive(true);

            if (i < items.Count && items[i] != null)
            {
                itemSlots[i].SetItem(items[i]);
            }
            else
            {
                itemSlots[i].SetItem((InventoryItem)null);
            }
        }
    }

    private void RefreshEquipmentSlots()
    {
        // 싱글톤으로 접근!
        if (PlayerData.Instance == null || ItemDatabase.Instance == null) return;

        // 무기 1
        if (weapon1Slot != null)
        {
            ItemData weapon1 = ItemDatabase.Instance.GetItemByName(PlayerData.Instance.weapon1Name);
            weapon1Slot.SetItem(weapon1);
        }

        // 무기 2
        if (weapon2Slot != null)
        {
            ItemData weapon2 = ItemDatabase.Instance.GetItemByName(PlayerData.Instance.weapon2Name);
            weapon2Slot.SetItem(weapon2);
        }

        // 헬멧
        if (helmetSlot != null)
        {
            ItemData helmet = ItemDatabase.Instance.GetItemByName(PlayerData.Instance.helmetName);
            helmetSlot.SetItem(helmet);
        }

        // 방탄조끼
        if (armorSlot != null)
        {
            ItemData armor = ItemDatabase.Instance.GetItemByName(PlayerData.Instance.armorName);
            armorSlot.SetItem(armor);
        }
    }

    private void UpdateWeightText()
    {
        if(weightText != null && PlayerData.Instance != null)
        {
            float current = PlayerData.Instance.CalculateTotalWeight();
            float max = PlayerData.Instance.maxWeight;
            weightText.text = $"{current:F1} / {max:F1} kg";    // F1 은 소수점 1자리까지
        }
    }

    private void UpdateMoneyText()
    {
        if (moneyText != null && PlayerData.Instance != null)
        {
            moneyText.text = PlayerData.Instance.money.ToString("N0") + "원";
        }
    }

    public bool IsOpen()
    {
        return inventoryPanel.activeSelf;
    }

    // 빈 무기 슬롯 반환
    public ItemSlot GetEmptyWeaponSlot()
    {
        if (weapon1Slot != null && weapon1Slot.IsEmpty())
            return weapon1Slot;

        if (weapon2Slot != null && weapon2Slot.IsEmpty())
            return weapon2Slot;

        return null;
    }

    public ItemSlot[] GetSecureSlots()
    {
        return secureSlots;
    }
}
