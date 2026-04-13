using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using static ItemSlot;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance { get; private set; }

    [Header("UI 참조")]
    [SerializeField] private StashUI stashUI;
    [SerializeField] private InventoryUI inventoryUI;
    [SerializeField] private LootUI lootUI;

    [Header("ObjLoot 슬롯")]
    [SerializeField] private ItemSlot[] objLootSlots;

    [Header("보호 슬롯")]
    [SerializeField] private ItemSlot[] secureSlots;    // 2개

    [Header("드래그 설정")]
    [SerializeField] private Canvas mainCanvas;

    [Header("아이템 DB")]
    [SerializeField] private ItemDatabase itemDatabase;

    [Header("아이템 버리기")]
    [SerializeField] private GameObject droppedItemPrefab;  // DroppedItem 프리팹

    // 드래그 상태
    private bool isDragging = false;
    private ItemSlot dragSourceSlot;
    private GameObject ghostObject;

    // 현재 루트 컨테이너
    private LootContainer currentLootContainer;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        FindSecureSlots();
        RefreshSecureSlots();
    }

    // === 드래그 관리 ===
    public void StartDrag(ItemSlot slot, Sprite icon)
    {
        isDragging = true;
        dragSourceSlot = slot;
        CreateGhost(icon);
    }

    public void UpdateDrag(Vector2 position)
    {
        if (ghostObject != null)
        {
            ghostObject.transform.position = position;
        }
    }

    public void EndDrag()
    {
        DestroyGhost();
        dragSourceSlot = null;
        isDragging = false;
    }

    public bool IsDragging()
    {
        return isDragging;
    }

    public ItemSlot GetDragSourceSlot()
    {
        return dragSourceSlot;
    }

    // === 고스트 ===
    private void CreateGhost(Sprite icon)
    {
        Debug.Log("=== CreateGhost 호출 ===");

        DestroyGhost();

        // Screen Space - Overlay Canvas 찾기!
        Canvas[] allCanvases = FindObjectsOfType<Canvas>();
        mainCanvas = null;

        foreach (Canvas c in allCanvases)
        {
            if (c.renderMode == RenderMode.ScreenSpaceOverlay)
            {
                mainCanvas = c;
                break;
            }
        }

        // 못 찾으면 아무 Canvas 사용
        if (mainCanvas == null)
        {
            mainCanvas = FindObjectOfType<Canvas>();
        }

        if (mainCanvas == null)
        {
            Debug.LogError("Canvas를 찾을 수 없습니다!");
            return;
        }

        ghostObject = new GameObject("DragGhost");

        // 별도로 canvas 추가해서 무조건 맨앞에 놓이도록
        Canvas ghostCanvas = ghostObject.AddComponent<Canvas>();
        ghostCanvas.overrideSorting = true;
        ghostCanvas.sortingOrder = 999;     // 매우 높은값임

        ghostObject.AddComponent<GraphicRaycaster>();

        RectTransform rect = ghostObject.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(80f, 80f);

        ghostObject.transform.SetParent(mainCanvas.transform, false);
        ghostObject.transform.SetAsLastSibling();
        rect.localScale = Vector3.one;  // 스케일 강제 설정

        Image img = ghostObject.AddComponent<Image>();
        img.sprite = icon;
        img.color = new Color(1f, 1f, 1f, 0.7f);
        img.raycastTarget = false;

        ghostObject.transform.position = Input.mousePosition;

        Debug.Log("Ghost 생성 완료! Position: " + ghostObject.transform.position);
    }

    private void DestroyGhost()
    {
        if (ghostObject != null)
        {
            Destroy(ghostObject);
            ghostObject = null;
        }
    }

    // === 아이템 교환 ===
    public void SwapItems(ItemSlot fromSlot, ItemSlot toSlot)
    {
        // ObjLoot > Inventory 처리
        if (fromSlot.slotType == ItemSlot.SlotType.ObjLoot &&
            toSlot.slotType == ItemSlot.SlotType.Inventory)
        {
            HandleObjLootSlot(fromSlot, toSlot, fromSlot.GetItem());
            EndDrag();
            RefreshAllUI();
            return;
        }

        InventoryItem fromItem = fromSlot.GetInventoryItem();
        InventoryItem toItem = toSlot.GetInventoryItem();
        ItemData fromItemData = fromSlot.GetItem();
        ItemData toItemData = toSlot.GetItem();

        // 데이터 이동 부분
        RemoveFromData(fromSlot, fromItem);

        if (toItem != null)
        {
            RemoveFromData(toSlot, toItem);
            AddToData(fromSlot, toItem, toItemData);
        }

        AddToData(toSlot, fromItem, fromItemData);

        Debug.Log("=== SwapItems 완료 ===");
        Debug.Log("PlayerData helmetName: " + PlayerData.Instance.helmetName);
        Debug.Log("PlayerData armorName: " + PlayerData.Instance.armorName);

        EndDrag();
        RefreshAllUI();
        Debug.Log(fromItemData.itemName + " 이동 완료!");
    }

    // === 빠른 이동 ===
    public void QuickMoveItem(ItemSlot fromSlot)
    {
        InventoryItem invItem = fromSlot.GetInventoryItem();
        ItemData item = fromSlot.GetItem();
        if (item == null) return;

        switch (fromSlot.slotType)
        {
            case ItemSlot.SlotType.Stash:
                if (CanAddToInventory())
                {
                    GameData.Instance.stashItems.Remove(invItem);
                    GameData.Instance.raidItems.Add(invItem);
                    Debug.Log(item.itemName + " → 인벤토리");
                }
                else
                {
                    Debug.Log("인벤토리가 가득 찼습니다!");
                }
                break;

            case ItemSlot.SlotType.Inventory:
                // 상점 열려있으면 판매!
                if (ShopUI.Instance != null && ShopUI.Instance.IsOpen())
                {
                    ShopUI.Instance.SellItem(item, fromSlot);
                    Debug.Log(item.itemName + " 판매!");
                }
                // 창고 열려있으면 창고로
                else if (stashUI != null && stashUI.IsOpen() && CanAddToStash())
                {
                    GameData.Instance.raidItems.Remove(invItem);
                    GameData.Instance.stashItems.Add(invItem);
                    Debug.Log(item.itemName + " → 창고");
                }
                break;

            case ItemSlot.SlotType.Loot:
                Debug.Log("=== Loot 클릭 이동 ===");
                Debug.Log("currentLootContainer: " + (currentLootContainer != null));
                if (CanAddToInventory())
                {
                    if (currentLootContainer != null)
                    {
                        Debug.Log("TakeItem 호출: " + invItem.itemData.itemName);
                        currentLootContainer.TakeItem(invItem.itemData);
                        Debug.Log("남은 아이템 수: " + currentLootContainer.GetItems().Count);
                    }
                    else
                    {
                        Debug.Log("currentLootContainer가 null!");
                    }

                        GameData.Instance.AddRaidItem(invItem.itemData, invItem.count);
                    Debug.Log(item.itemName + " → 인벤토리 (루팅)");
                }
                else
                {
                    Debug.Log("인벤토리가 가득 찼습니다!");
                }
                break;

            case ItemSlot.SlotType.Weapon1:
                if (CanAddToInventory())
                {
                    PlayerData.Instance.weapon1Name = "";
                    PlayerData.Instance.weapon1Ammo = 0;
                    GameData.Instance.raidItems.Add(new InventoryItem(item, 1));
                    RefreshWeaponManager();
                    Debug.Log(item.itemName + " 장착 해제");
                }
                break;

            case ItemSlot.SlotType.Weapon2:
                if (CanAddToInventory())
                {
                    PlayerData.Instance.weapon2Name = "";
                    PlayerData.Instance.weapon2Ammo = 0;
                    GameData.Instance.raidItems.Add(new InventoryItem(item, 1));
                    RefreshWeaponManager();
                    Debug.Log(item.itemName + " 장착 해제");
                }
                break;

            case ItemSlot.SlotType.Helmet:
                if (CanAddToInventory())
                {
                    PlayerData.Instance.helmetName = "";
                    GameData.Instance.raidItems.Add(new InventoryItem(item, 1));
                    UpdateEquipmentStats();
                    Debug.Log(item.itemName + " 장착 해제");
                }
                break;

            case ItemSlot.SlotType.Armor:
                if (CanAddToInventory())
                {
                    PlayerData.Instance.armorName = "";
                    GameData.Instance.raidItems.Add(new InventoryItem(item, 1));
                    UpdateEquipmentStats();
                    Debug.Log(item.itemName + " 장착 해제");
                }
                break;
        }

        EndDrag();
        RefreshAllUI();
    }

    // === 데이터 관리 ===
    private void RemoveFromData(ItemSlot slot, InventoryItem invItem)
    {
        if (GameData.Instance == null) return;

        switch (slot.slotType)
        {
            case ItemSlot.SlotType.Inventory:
                GameData.Instance.raidItems.Remove(invItem);
                break;
            case ItemSlot.SlotType.Stash:
                GameData.Instance.stashItems.Remove(invItem);
                break;
            case ItemSlot.SlotType.Secure:
                GameData.Instance.secureItems.Remove(invItem);
                break;
            case ItemSlot.SlotType.Loot:
                Debug.Log("=== Loot에서 제거 시도 ===");
                Debug.Log("currentLootContainer: " + (currentLootContainer != null));
                Debug.Log("invItem: " + (invItem != null));
                if (currentLootContainer != null && invItem != null)
                {
                    Debug.Log("TakeItem 호출: " + invItem.itemData.itemName);
                    currentLootContainer.TakeItem(invItem.itemData);
                    Debug.Log("남은 아이템 수: " + currentLootContainer.GetItems().Count);
                }
                else
                {
                    Debug.Log("TakeItem 실패! Container 또는 Item이 null");
                }
                    break;
            case ItemSlot.SlotType.Weapon1:
                PlayerData.Instance.weapon1Name = "";
                PlayerData.Instance.weapon1Ammo = 0;
                RefreshWeaponManager();
                break;
            case ItemSlot.SlotType.Weapon2:
                PlayerData.Instance.weapon2Name = "";
                PlayerData.Instance.weapon2Ammo = 0;
                RefreshWeaponManager();
                break;
            case ItemSlot.SlotType.Helmet:
                PlayerData.Instance.helmetName = "";
                UpdateEquipmentStats();
                break;
            case ItemSlot.SlotType.Armor:
                PlayerData.Instance.armorName = "";
                // 내구도 초기화
                PlayerData.Instance.armorMaxDurability = 0f;
                PlayerData.Instance.armorCurrentDurability = 0f;
                UpdateEquipmentStats();
                break;
        }
    }

    private void AddToData(ItemSlot slot, InventoryItem invItem, ItemData item)
    {
        if (GameData.Instance == null) return;

        switch (slot.slotType)
        {
            case ItemSlot.SlotType.Inventory:
                if (invItem != null)
                    GameData.Instance.raidItems.Add(invItem);
                else
                    GameData.Instance.raidItems.Add(new InventoryItem(item, 1));
                break;
            case ItemSlot.SlotType.Stash:
                if (invItem != null)
                    GameData.Instance.stashItems.Add(invItem);
                else
                    GameData.Instance.stashItems.Add(new InventoryItem(item, 1));
                break;
            case ItemSlot.SlotType.Secure:
                if (invItem != null)
                    GameData.Instance.secureItems.Add(invItem);
                else
                    GameData.Instance.secureItems.Add(new InventoryItem(item, 1));
                    break;
            case ItemSlot.SlotType.Loot:
                if (currentLootContainer != null && item != null)
                {
                    currentLootContainer.AddItem(item);
                    Debug.Log("루트박스에 아이템 추가: " + item.itemName);
                }
                break;
            case ItemSlot.SlotType.Weapon1:
                PlayerData.Instance.weapon1Name = item.itemName;
                PlayerData.Instance.weapon1Ammo = GetMagazineSize(item);
                Debug.Log("무기1 저장: " + item.itemName);
                RefreshWeaponManager();
                break;
            case ItemSlot.SlotType.Weapon2:
                PlayerData.Instance.weapon2Name = item.itemName;
                PlayerData.Instance.weapon2Ammo = GetMagazineSize(item);
                RefreshWeaponManager();
                break;
            case ItemSlot.SlotType.Helmet:
                PlayerData.Instance.helmetName = item.itemName;
                Debug.Log("헬멧 저장됨: " + item.itemName);
                UpdateEquipmentStats();
                break;
            case ItemSlot.SlotType.Armor:
                PlayerData.Instance.armorName = item.itemName;
                // 내구도 설정
                PlayerData.Instance.armorMaxDurability = item.durability;
                PlayerData.Instance.armorCurrentDurability = item.durability;
                Debug.Log("아머 저장됨: " + item.itemName + " 내구도 : " + item.durability);
                UpdateEquipmentStats();
                break;
        }
    }

    // OnItemDropped에서 ObjLoot 처리 추가
    private void HandleObjLootSlot(ItemSlot fromSlot, ItemSlot toSlot, ItemData item)
    {
        if (fromSlot.slotType == SlotType.ObjLoot && toSlot.slotType == SlotType.Inventory)
        {
            // fromSlot에서 InventoryItem 가져오기 (개수 정보 포함)
            InventoryItem fromItem = fromSlot.GetInventoryItem();
            int count = fromItem != null ? fromItem.count : 1;

            // ObjLootUI에서 컨테이너 업데이트
            ObjLootUI objLootUI = FindObjectOfType<ObjLootUI>();
            if (objLootUI != null)
            {
                objLootUI.OnItemTaken(item);
            }

            // 인벤토리에 추가 (개수 포함)
            InventoryItem newItem = new InventoryItem(item, count);
            GameData.Instance.raidItems.Add(newItem);

            // 슬롯 업데이트
            //fromSlot.SetItem((InventoryItem)null);
            toSlot.SetItem(newItem);  // InventoryItem으로
        }
    }

    // --- 사용 아이템 제거 ---
    public void RemoveItem(ItemSlot slot)
    {
        if (slot == null) return;

        InventoryItem invItem = slot.GetInventoryItem();
        if(invItem == null) return;

        // GameData에서 제거
        switch(slot.slotType)
        {
            case ItemSlot.SlotType.Inventory:
                if(GameData.Instance != null)
                {
                    GameData.Instance.raidItems.Remove(invItem);
                }
                break;
            case ItemSlot.SlotType.Stash:
                if(GameData.Instance != null)
                {
                    GameData.Instance.stashItems.Remove(invItem);
                }
                break;
        }

        // 슬롯 비우기
        slot.SetItem((InventoryItem)null);

        // UI 새로고침
        RefreshAllUI();

        Debug.Log("아이템 사용완료되서 아이템 삭제" + invItem.itemData.itemName);
    }

    private int GetMagazineSize(ItemData item)
    {
        if (item.weaponData != null)
        {
            return item.weaponData.magazineSize;
        }
        return 30;
    }

    // === 무기 갱신 ===
    private void RefreshWeaponManager()
    {
        WeaponManager weaponManager = FindObjectOfType<WeaponManager>();
        if (weaponManager != null)
        {
            weaponManager.LoadWeaponsFromPlayerData();
        }
    }

    // === 루트 관리 ===
    public void SetCurrentLoot(LootContainer loot)
    {
        currentLootContainer = loot;
    }

    public LootContainer GetCurrentLoot()
    {
        return currentLootContainer;
    }

    // === 용량 체크 ===
    private bool CanAddToInventory()
    {
        if (GameData.Instance == null) return false;
        return GameData.Instance.raidItems.Count < 25;
    }

    private bool CanAddToStash()
    {
        if (GameData.Instance == null) return false;
        return GameData.Instance.stashItems.Count < 60;
    }

    // === UI 새로고침 ===
    public void RefreshAllUI()
    {
        // 씬 전환 후 UI 다시 찾기
        if (stashUI == null)
            stashUI = FindObjectOfType<StashUI>();

        if (inventoryUI == null)
            inventoryUI = FindObjectOfType<InventoryUI>();

        if (lootUI == null)
            lootUI = FindObjectOfType<LootUI>();

        FindSecureSlots();

        if (stashUI != null && stashUI.IsOpen())
        {
            stashUI.RefreshUI();
        }

        if (inventoryUI != null && inventoryUI.IsOpen())
        {
            Debug.Log("inventoryUI.RefreshUI() 호출!");
            inventoryUI.RefreshUI();
        }

        if (lootUI != null && lootUI.IsOpen())
        {
            Debug.Log("LootUI RefreshUI 호출!");
            lootUI.RefreshUI();
        }

        RefreshSecureSlots();

        if(PlayerData.Instance != null)
        {
            PlayerData.Instance.CalculateTotalWeight();
        }

        if (TooltipUI.Instance != null)
        {
            TooltipUI.Instance.Hide();
        }
    }

    // secureSlots 새로고침
    public void RefreshSecureSlots()
    {
        if (secureSlots == null || GameData.Instance == null) return;

        for(int i = 0; i < secureSlots.Length; i++)
        {
            if(i < GameData.Instance.secureItems.Count)
            {
                secureSlots[i].SetItem(GameData.Instance.secureItems[i]);
            }
            else
            {
                secureSlots[i].SetItem((InventoryItem)null);
            }
        }
    }

    // SecureSlots 찾기
    private void FindSecureSlots()
    {
        if (secureSlots != null && secureSlots.Length > 0 && secureSlots[0] != null)
            return;

        InventoryUI invUI = FindObjectOfType<InventoryUI>(true);
        if (invUI != null)
        {
            secureSlots = invUI.GetSecureSlots();

            if (secureSlots != null && secureSlots.Length > 0)
            {
                Debug.Log("SecureSlots 찾음: " + secureSlots.Length + "개");
            }
        }
    }

    // === 장비 스탯 갱신 ===
    private void UpdateEquipmentStats()
    {
        if (PlayerData.Instance != null && ItemDatabase.Instance != null)
        {
            PlayerData.Instance.UpdateEquipmentStats(ItemDatabase.Instance);
        }
    }

    // === UI 밖에 아이템 드롭 시도했는데 체크
    public void TryDropItem(Vector2 screenPosition)
    {
        if (dragSourceSlot == null) return;

        InventoryItem invItem = dragSourceSlot.GetInventoryItem();
        ItemData itemData = dragSourceSlot.GetItem();
        if (itemData == null) return;

        // UI 위인지 확인
        if(!IsPointerOverUI(screenPosition))
        {
            // UI밖이면 아이템 버리기
            DropItemToWorld(dragSourceSlot, invItem, itemData);
        }

        EndDrag();
    }

    // UI 위에 마우스 있는지 확인
    private bool IsPointerOverUI(Vector2 screenPosition)
    {
        var eventData = new UnityEngine.EventSystems.PointerEventData(UnityEngine.EventSystems.EventSystem.current);
        eventData.position = screenPosition;

        var results = new System.Collections.Generic.List<UnityEngine.EventSystems.RaycastResult>();
        UnityEngine.EventSystems.EventSystem.current.RaycastAll(eventData, results);

        return results.Count > 0;
    }

    // 필드에 아이템 떨구기
    private void DropItemToWorld(ItemSlot slot, InventoryItem invItem, ItemData itemData)
    {
        // 플레이어 위치
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if(player == null) return;

        // 드랍했을 때 플레이어 앞에 떨구게
        Vector3 dropPosition = player.transform.position + player.transform.forward * 1f;
        dropPosition.y = player.transform.position.y;

        // DropItem 생성
        if(droppedItemPrefab != null)
        {
            GameObject dropped = Instantiate(droppedItemPrefab, dropPosition, Quaternion.identity);
            DroppedItem droppedItem = dropped.GetComponent<DroppedItem>();

            if(droppedItem != null)
            {
                int count = invItem != null ? invItem.count : 1;
                droppedItem.Setup(itemData, count);
            }
        }

        // 데이터에서 제거
        RemoveFromData(slot, invItem);

        // 슬롯 비우기
        slot.SetItem((InventoryItem)null);

        Debug.Log(itemData.itemName + " 버림");

        RefreshAllUI();
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // 씬 전환 후 UI 참조 초기화
        stashUI = null;
        inventoryUI = null;
        lootUI = null;
        secureSlots = null;  // 강제로 다시 찾게!

        // 약간의 딜레이 후 찾기 (씬 로드 완료 대기)
        StartCoroutine(DelayedRefresh());
    }

    private System.Collections.IEnumerator DelayedRefresh()
    {
        yield return null;  // 1프레임 대기
        FindSecureSlots();
        RefreshSecureSlots();
    }
}
