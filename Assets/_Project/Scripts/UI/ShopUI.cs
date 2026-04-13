using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ShopUI : MonoBehaviour
{
    public static ShopUI Instance { get; private set; }

    [Header("UI 참조")]
    [SerializeField] private GameObject shopPanel;
    [SerializeField] private Button closeButton;
    [SerializeField] private TextMeshProUGUI titleText;

    [Header("슬롯")]
    [SerializeField] private ShopSlot[] shopSlots;  // ★ 미리 배치된 15개 슬롯!

    private ItemData[] currentShopItems;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        if (closeButton != null)
            closeButton.onClick.AddListener(CloseShop);
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    public void OpenShop(ItemData[] items)
    {
        currentShopItems = items;
        shopPanel.SetActive(true);

        if (titleText != null)
            titleText.text = "상점";

        RefreshUI();
    }

    public void CloseShop()
    {
        shopPanel.SetActive(false);

        if (TooltipUI.Instance != null)
            TooltipUI.Instance.Hide();
    }

    public void RefreshUI()
    {
        // ★ 모든 슬롯 초기화 후 아이템 채우기! ★
        for (int i = 0; i < shopSlots.Length; i++)
        {
            if (shopSlots[i] == null) continue;

            // 아이템이 있으면 설정, 없으면 비우기
            if (currentShopItems != null && i < currentShopItems.Length)
            {
                shopSlots[i].SetItem(currentShopItems[i]);
            }
            else
            {
                shopSlots[i].SetItem(null);
            }
        }
    }

    // === 구매 ===
    public void BuyItem(ItemData item)
    {
        if (item == null) return;
        if (PlayerData.Instance == null || GameData.Instance == null) return;

        // 돈 체크
        if (PlayerData.Instance.money < item.price)
        {
            Debug.Log("돈이 부족합니다!");
            return;
        }

        // 인벤토리 공간 체크
        if (GameData.Instance.raidItems.Count >= 25)
        {
            Debug.Log("인벤토리가 가득 찼습니다!");
            return;
        }

        // 구매 실행
        PlayerData.Instance.money -= item.price;
        GameData.Instance.raidItems.Add(new InventoryItem(item, 1));

        Debug.Log($"{item.itemName} 구매! 남은 돈: {PlayerData.Instance.money}");

        // UI 갱신
        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.RefreshAllUI();
        }
    }

    // === 판매 ===
    public void SellItem(ItemData item, ItemSlot fromSlot)
    {
        if (item == null) return;
        if (PlayerData.Instance == null || GameData.Instance == null) return;

        if (fromSlot != null)
        {
            InventoryItem invItem = fromSlot.GetInventoryItem();
            if (invItem != null)
            {
                GameData.Instance.raidItems.Remove(invItem);
            }
        }

        PlayerData.Instance.money += item.price;

        Debug.Log($"{item.itemName} 판매! 현재 돈: {PlayerData.Instance.money}");

        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.RefreshAllUI();
        }
    }

    public bool IsOpen()
    {
        return shopPanel != null && shopPanel.activeSelf;
    }
}