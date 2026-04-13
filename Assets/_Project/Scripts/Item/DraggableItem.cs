using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class DraggableItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private Image image;
    private Color originalColor;
    private bool isDragging = false;

    private void Awake()
    {
        image = GetComponent<Image>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        // image 없으면 가져오기
        if(image  == null)
        {
            image = GetComponent<Image>();
        }

        ItemSlot slot = GetComponentInParent<ItemSlot>();
        if (slot == null || slot.GetItem() == null)
        {
            eventData.pointerDrag = null;
            return;
        }

        // 드래그 시작
        InventoryManager.Instance.StartDrag(slot, image.sprite);

        // 원본 반투명
        originalColor = image.color;
        image.color = new Color(originalColor.r, originalColor.g, originalColor.b, 0.3f);
        isDragging = true;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!isDragging) return;
        InventoryManager.Instance.UpdateDrag(eventData.position);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        // 드래그 중이었을 때만 복구
        if(isDragging && image != null)
        {
            image.color = originalColor;
        }
        isDragging = false;

        // 아이템 떨굴때 대상 확인
        GameObject dropTarget = eventData.pointerCurrentRaycast.gameObject;

        // UI 밖에 떨구면 아이템 버리기
        if(dropTarget == null)
        {
            if(InventoryManager.Instance != null)
            {
                InventoryManager.Instance.TryDropItem(eventData.position);
                return; // TryDropItem함수 안에 EndDrag 호출
            }
        }

        // 드래그 종료
        InventoryManager.Instance.EndDrag();
    }

    // 강제 리셋 (SetItem에서 호출)
    public void ResetState()
    {
        if (image == null)
        {
            image = GetComponent<Image>();
        }

        if (image != null)
        {
            Color c = image.color;
            c.a = 1f;
            image.color = c;
        }

        isDragging = false;
    }
}