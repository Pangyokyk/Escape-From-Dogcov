using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShopInteractable : Interactable
{
    [Header("상점 설정")]
    [SerializeField] private GameObject shopPanel;
    [SerializeField] private InventoryUI inventoryUI;
    [SerializeField] private ShopUI shopUI;

    [Header("판매 아이템")]
    [SerializeField] private ItemData[] shopItems;      // 상점에서 파는 아이템들 배열

    public override  void Interact()
    {
        OpenShop();
    }
    
    private void OpenShop()
    {
        // 인벤토리 열기
        if(inventoryUI != null)
        {
            inventoryUI.OpenInventory();
        }

        // 상점 열기
        if(shopPanel != null)
        {
            shopPanel.SetActive(true);
        }

        if(shopUI != null)
        {
            shopUI.OpenShop(shopItems);
        }

        Cursor.visible = true;
    }

    public void CloseShop()
    {
        if(shopPanel != null)
        {
            shopPanel.SetActive(false);
        }
    }

}
