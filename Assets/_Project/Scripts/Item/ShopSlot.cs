using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class ShopSlot : MonoBehaviour, IPointerClickHandler, IDropHandler, IPointerEnterHandler, IPointerExitHandler
{
    [Header("UI 참조")]
    [SerializeField] private Image iconImage;

    private ItemData itemData;

    public void SetItem(ItemData item)
    {
        itemData = item;

        if (item != null && iconImage != null)
        {
            iconImage.sprite = item.icon;
            iconImage.enabled = true;
            iconImage.gameObject.SetActive(true);
        }
        else if (iconImage != null)
        {
            iconImage.enabled = false;
            iconImage.gameObject.SetActive(false);
        }
    }

    // 클릭으로 구매
    public void OnPointerClick(PointerEventData eventData)
    {
        if (itemData == null) return;

        if (ShopUI.Instance != null)
        {
            ShopUI.Instance.BuyItem(itemData);
        }
    }

    // 드래그로 판매 (인벤토리 아이템을 여기에 드롭)
    public void OnDrop(PointerEventData eventData)
    {
        if (InventoryManager.Instance == null) return;
        if (!InventoryManager.Instance.IsDragging()) return;

        ItemSlot sourceSlot = InventoryManager.Instance.GetDragSourceSlot();
        if (sourceSlot == null) return;

        // 인벤토리 아이템만 판매 가능
        if (sourceSlot.slotType != ItemSlot.SlotType.Inventory) return;

        ItemData item = sourceSlot.GetItem();
        if (item == null) return;

        if (ShopUI.Instance != null)
        {
            ShopUI.Instance.SellItem(item, sourceSlot);
        }

        InventoryManager.Instance.EndDrag();
    }

    // 툴팁
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (itemData != null && TooltipUI.Instance != null)
        {
            TooltipUI.Instance.Show(itemData);
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