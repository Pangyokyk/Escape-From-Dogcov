using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using Unity.VisualScripting;

public class ItemSlot : MonoBehaviour, IDropHandler, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    [Header("슬롯 타입")]
    public SlotType slotType;

    [Header("장비 슬롯 설정")]
    [SerializeField] private ItemData.ItemType[] allowedTypes;

    [Header("개수 표시")]
    [SerializeField] private TextMeshProUGUI countText;

    [Header("슬롯 글씨")]
    [SerializeField] private TextMeshProUGUI slotLabel;

    [Header("슬롯 배경")]
    [SerializeField] private Image slotBackground;

    private Color emptyColor = Color.white;
    private Color equipmentColor = new Color(56f / 255f, 56f / 255f, 56f / 255f, 1f);

    public enum SlotType
    {
        Inventory,
        Stash,
        Loot,
        Weapon1,
        Weapon2,
        Helmet,
        Armor,
        Shop,
        ObjLoot,
        Secure
    }

    // 내부
    private Image iconImage;
    private ItemData currentItem;
    private int currentUses = 0;    // 남은 사용 횟수
    private InventoryItem currentInventoryItem;

    private void Awake()
    {
        Transform iconTr = transform.Find("Icon");
        if(iconTr != null)
        {
            iconImage = iconTr.GetComponent<Image>();
        }
    }

    // === 아이템 설정 (ItemData 버전 - 장비 슬롯용) ===
    public void SetItem(ItemData item)
    {
        if (item != null)
        {
            SetItem(new InventoryItem(item, 1));
        }
        else
        {
            SetItem((InventoryItem)null);
        }
    }

    // 아이템 설정
    public void SetItem(InventoryItem invItem)
    {
        currentInventoryItem = invItem;

        if (invItem != null && invItem.itemData != null)
        {
            currentItem = invItem.itemData;

            if (iconImage != null)
            {
                iconImage.sprite = invItem.itemData.icon;
                //iconImage.color = Color.white;
                iconImage.gameObject.SetActive(true);

                DraggableItem draggable = iconImage.GetComponent<DraggableItem>();
                if (draggable == null)
                {
                    draggable = iconImage.gameObject.AddComponent<DraggableItem>();
                }
                draggable.ResetState();

                // 알파값 설정 추가
                UpdateItemAlpha();
            }

            if (slotLabel != null)
            {
                slotLabel.gameObject.SetActive(false);
            }

            if (slotBackground != null)
            {
                slotBackground.color = equipmentColor;
            }

            // 개수 표시 (스택 가능한 아이템만)
            if (countText != null)
            {
                if (invItem.IsStackable() && invItem.count > 1)
                {
                    countText.gameObject.SetActive(true);
                    countText.text = invItem.count.ToString();
                }
                else
                {
                    countText.gameObject.SetActive(false);
                }
            }
        }
        else
        {
            currentItem = null;
            currentInventoryItem = null;

            if (iconImage != null)
            {
                iconImage.gameObject.SetActive(false);
            }

            if (slotLabel != null)
            {
                slotLabel.gameObject.SetActive(true);
            }

            if (slotBackground != null)
            {
                slotBackground.color = emptyColor;
            }

            if (countText != null)
            {
                countText.gameObject.SetActive(false);
            }
        }
    }

    public ItemData GetItem()
    {
        return currentItem;
    }

    public InventoryItem GetInventoryItem()
    {
        return currentInventoryItem;
    }

    public bool IsEmpty()
    {
        return currentItem == null;
    }

    public bool IsEquipmentSlot()
    {
        return slotType == SlotType.Weapon1 ||
               slotType == SlotType.Weapon2 ||
               slotType == SlotType.Helmet ||
               slotType == SlotType.Armor;
    }

    // === 드롭 처리 ===
    public void OnDrop(PointerEventData eventData)
    {
        ItemSlot sourceSlot = InventoryManager.Instance.GetDragSourceSlot();
        if (sourceSlot == null || sourceSlot == this) return;

        ItemData draggedItem = sourceSlot.GetItem();
        if (draggedItem == null) return;

        // 장비 슬롯이면 타입 체크
        if (IsEquipmentSlot() && !CanAcceptItem(draggedItem))
        {
            Debug.Log("이 슬롯에 장착할 수 없는 아이템!");
            return;
        }

        // 아이템 교환/이동
        InventoryManager.Instance.SwapItems(sourceSlot, this);
    }

    // === 클릭 처리 (빠른 이동) ===
    public void OnPointerClick(PointerEventData eventData)
    {
        // 우클릭 - 아이템 사용
        if (eventData.button == PointerEventData.InputButton.Right)
        {
            TryUseItem();
            return;
        }

        // 좌클릭
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            // ★ Shop 슬롯 클릭 = 구매 ★
            if (slotType == SlotType.Shop)
            {
                if (currentItem != null && ShopUI.Instance != null)
                {
                    ShopUI.Instance.BuyItem(currentItem);
                }
                return;
            }

            // 기존 퀵무브 로직...
            if (InventoryManager.Instance != null)
            {
                InventoryManager.Instance.QuickMoveItem(this);
            }
        }
    }

    // === 타입 체크 ===
    private bool CanAcceptItem(ItemData item)
    {
        if (allowedTypes == null || allowedTypes.Length == 0)
            return true;

        foreach (var type in allowedTypes)
        {
            if (item.itemType == type)
                return true;
        }
        return false;
    }

    // 사용 아이템
    private void TryUseItem()
    {
        if(currentItem == null) return;
        if (currentItem.itemType != ItemData.ItemType.Medical) return;

        // 최대 체력일 땐 사용불가
        float currentHealth = 0f;
        float maxHealth = 100f;
        Health playerHealth = null;

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if(player != null)
        {
            playerHealth = player.GetComponent<Health>();
        }

        if(playerHealth != null)
        {
            currentHealth = playerHealth.GetCurrentHealth();
            maxHealth = playerHealth.GetMaxHealth();
        }
        else if(PlayerData.Instance != null)
        {
            currentHealth = PlayerData.Instance.currentHealth;
            maxHealth = PlayerData.Instance.GetMaxHealth();
        }

        if (PlayerData.Instance != null)
        {
            currentHealth = PlayerData.Instance.currentHealth;
            maxHealth = PlayerData.Instance.GetMaxHealth();
        }

        if(currentHealth >= maxHealth)
        {
            Debug.Log("이미 최대 체력이라 사용불가");
            return;
        }

        // 체력 회복
        float healAmount = currentItem.healAmount;
        float newHealth = Mathf.Min(currentHealth + healAmount, maxHealth);

        if(PlayerData.Instance != null)
        {
            PlayerData.Instance.currentHealth = newHealth;
        }

        if(playerHealth != null)
        {
            playerHealth.SetCurrentHealth(newHealth);
        }

        Debug.Log($"체력 회복 {currentHealth:F1} > {newHealth:F1}");

        // 사용 횟수 감소
        currentInventoryItem.currentUses--;

        // 다 사용했으면 제거
        if(currentInventoryItem.currentUses <= 0 )
        {
            if(InventoryManager.Instance != null)
            {
                InventoryManager.Instance.RemoveItem(this);
            }
        }
        else
        {
            UpdateItemAlpha();
        }

        // UI 새로고침 해줘야함
        if(InventoryManager.Instance != null)
        {
            InventoryManager.Instance.RefreshAllUI();
        }
    }

    private void UpdateItemAlpha()
    {
        if (currentInventoryItem == null || iconImage == null) return;
        if (currentItem.maxUses <= 1) return;  // 1회용은 알파 변경 없음

        float alpha = 1f;

        switch (currentInventoryItem.currentUses)
        {
            case 3: alpha = 1f; break;
            case 2: alpha = 0.7f; break;
            case 1: alpha = 0.4f; break;
            default: alpha = 1f; break;
        }

        Color color = iconImage.color;
        color.a = alpha;
        iconImage.color = color;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (currentItem != null && TooltipUI.Instance != null)
        {
            TooltipUI.Instance.Show(currentItem);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (TooltipUI.Instance != null)
        {
            TooltipUI.Instance.Hide();
        }
    }
}
