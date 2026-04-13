using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInteraction : MonoBehaviour
{
    [Header("상호작용 설정")]
    [SerializeField] private float interactRange = 2f;
    [SerializeField] private LayerMask lootLayer;

    [Header("UI 참조")]
    [SerializeField] private GameObject lootPanel;      // 적 죽이고 나온 LootBox UI전용
    [SerializeField] private GameObject objLootPanel;   // 맵 필드에 존재하는 루팅 오브젝트 UI전용
    [SerializeField] private GameObject lootProgressBar;
    [SerializeField] private UnityEngine.UI.Image progressFill;
    [SerializeField] private InventoryUI inventoryUI;

    [Header("드랍된 아이템")]
    [SerializeField] private LayerMask droppedItemLayer;
    private DroppedItem currentDroppedItem;

    // 컴포넌트
    private PlayerInputActions inputActions;
    private Inventory inventory;

    // 상태
    private LootContainer currentLoot;              // 적드롭
    private FieldLootContainer currentFieldLoot;    // 맵 파밍

    private bool isLooting;
    private Vector3 lootStartPosition;

    private void Awake()
    {
        inputActions = new PlayerInputActions();
        inventory = GetComponent<Inventory>();
    }

    private void OnEnable()
    {
        inputActions.Player.Enable();
        inputActions.Player.Interact.performed += OnInteract;
    }

    private void OnDisable()
    {
        inputActions.Player.Disable();
        inputActions.Player.Interact.performed -= OnInteract;
    }

    private void Update()
    {
        CheckForLoot();
        CheckForDroppedItem();
        CheckMovementCancel();
        CheckLootPanelClose();
        CheckDistanceClose();
    }

    private void CheckMovementCancel()
    {
        if (isLooting)
        {
            float distance = Vector3.Distance(transform.position, lootStartPosition);
            if (distance > 0.1f)
            {
                CancelLooting();
            }
        }
    }

    private void CancelLooting()
    {
        StopAllCoroutines();
        isLooting = false;
        if (lootProgressBar != null)
        {
            lootProgressBar.SetActive(false);
        }
        Debug.Log("루팅 취소됨");
    }

    private void CheckForDroppedItem()
    {
        // 주변 드랍 아이템 감지
        Collider[] colliders = Physics.OverlapSphere(transform.position, interactRange, droppedItemLayer);

        DroppedItem closest = null;
        float closestDist = float.MaxValue;

        foreach(Collider col in colliders)
        {
            DroppedItem dropped = col.GetComponent<DroppedItem>();
            if(dropped != null)
            {
                float dist = Vector3.Distance(transform.position, col.transform.position);
                if(dist < closestDist)
                {
                    closestDist = dist;
                    closest = dropped;
                }
            }
        }

        // 이전 아이템 프롬프트 숨기기
        if (currentDroppedItem != null && currentDroppedItem != closest)
        {
            currentDroppedItem.HidePrompt();
        }

        currentDroppedItem = closest;

        // 새 아이템 프롬프트 표시
        if (currentDroppedItem != null)
        {
            Debug.Log("현재 DroppedItem: " + currentDroppedItem.name);
            currentDroppedItem.ShowPrompt();
        }
    }

    private void CheckForLoot()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, interactRange, lootLayer);

        // 초기화
        LootContainer foundLoot = null;
        FieldLootContainer foundFieldLoot = null;

        if (colliders.Length > 0)
        {
            // 적 드롭 체크
            foundLoot = colliders[0].GetComponent<LootContainer>();
            foundFieldLoot = colliders[0].GetComponent<FieldLootContainer>();
        }

        // === 적 드롭 프롬프트 관리 ===
        if (foundLoot != currentLoot)
        {
            if (currentLoot != null) currentLoot.HidePrompt();
            currentLoot = foundLoot;
        }

        if (currentLoot != null && !isLooting && !IsAnyPanelOpen())
        {
            currentLoot.ShowPrompt();
        }

        // === 맵 파밍 프롬프트 관리 ===
        if (foundFieldLoot != currentFieldLoot)
        {
            if (currentFieldLoot != null) currentFieldLoot.HidePrompt();
            currentFieldLoot = foundFieldLoot;
        }

        if (currentFieldLoot != null && !isLooting && !IsAnyPanelOpen())
        {
            currentFieldLoot.ShowPrompt();
        }
    }

    private void OnInteract(InputAction.CallbackContext context)
    {
        // 루팅 중이면 취소
        if (isLooting)
        {
            CancelLooting();
            return;
        }

        // 오브젝트 루팅 UI 열려있으면 닫기
        if(objLootPanel != null && objLootPanel.activeSelf)
        {
            CloseObjLootPanel();
            return;
        }

        // 루팅 UI 열려있으면 닫기
        if (lootPanel != null && lootPanel.activeSelf)
        {
            CloseLootPanel();
            return;
        }

        // 드롭 아이템 줍
        if (currentDroppedItem != null)
        {
            currentDroppedItem.PickUp();
            currentDroppedItem = null;
            return;
        }

        if (currentFieldLoot != null)
        {
            StartCoroutine(FieldLootRoutine());
            return;
        }

        // 루팅 시작
        if (currentLoot != null)
        {
            StartCoroutine(LootRoutine());
        }
    }

    // 필드 오브젝트 파밍 루틴
    private IEnumerator FieldLootRoutine()
    {
        isLooting = true;
        lootStartPosition = transform.position;

        if (currentFieldLoot != null)
        {
            currentFieldLoot.HidePrompt();
        }

        if (lootProgressBar != null)
        {
            lootProgressBar.SetActive(true);
        }

        float lootTime = currentFieldLoot.GetLootTime();
        float elapsed = 0f;

        while (elapsed < lootTime)
        {
            elapsed += Time.deltaTime;
            if (progressFill != null)
            {
                progressFill.fillAmount = elapsed / lootTime;
            }
            yield return null;
        }

        if (lootProgressBar != null)
        {
            lootProgressBar.SetActive(false);
        }
        isLooting = false;

        OpenObjLootPanel();
    }

    // 적 드롭 루틴
    private IEnumerator LootRoutine()
    {
        isLooting = true;
        lootStartPosition = transform.position;

        if(currentLoot != null)
        {
            currentLoot.HidePrompt();
        }

        if (lootProgressBar != null)
        {
            lootProgressBar.SetActive(true);
        }

        float lootTime = currentLoot.GetLootTime();
        float elapsed = 0f;

        while (elapsed < lootTime)
        {
            elapsed += Time.deltaTime;
            if (progressFill != null)
            {
                progressFill.fillAmount = elapsed / lootTime;
            }
            yield return null;
        }

        // 루팅 완료
        if (lootProgressBar != null)
        {
            lootProgressBar.SetActive(false);
        }
        isLooting = false;

        OpenLootPanel();
    }

    // 맵 파밍 UI 열기
    private void OpenObjLootPanel()
    {
        if(objLootPanel != null)
        {
            objLootPanel.SetActive(true);
        }

        InventoryUI invUI = FindObjectOfType<InventoryUI>(true);
        if (invUI != null)
        {
            invUI.OpenInventory();
        }

        if (Crosshair.Instance != null)
        {
            Crosshair.Instance.ShowCursor();
        }

        // ObjLootUI에 데이터 전달
        ObjLootUI objLootUI = objLootPanel.GetComponent<ObjLootUI>();
        if (objLootUI != null)
        {
            objLootUI.ShowLoot(currentFieldLoot, inventory);
        }
    }

    private void OpenLootPanel()
    {
        // 인벤토리도 같이 열기
        if (lootPanel != null)
        {
            lootPanel.SetActive(true);
        }
        /*
        InventoryUI invUI = FindObjectOfType<InventoryUI>(true);
        if (invUI != null)
        {
            invUI.OpenInventory();
        }*/

        
        if (inventoryUI != null)
        {
            inventoryUI.OpenInventory();
        }

        if (Crosshair.Instance != null)
        {
            Crosshair.Instance.ShowCursor();
        }

        LootUI lootUI = lootPanel.GetComponent<LootUI>();
        if (lootUI != null)
        {
            lootUI.ShowLoot(currentLoot);
        }
    }

    private void CloseObjLootPanel()
    {
        if (objLootPanel != null)
        {
            objLootPanel.SetActive(false);
        }

        InventoryUI invUI = FindObjectOfType<InventoryUI>(true);
        if (invUI != null)
        {
            invUI.CloseInventory();
        }

        if (Crosshair.Instance != null)
        {
            Crosshair.Instance.HideCursor();
        }
    }

    public void CloseLootPanel()
    {
        // LootUI 닫기
        LootUI lootUI = lootPanel.GetComponent<LootUI>();
        if (lootUI != null)
        {
            lootUI.Close();
        }

        // 인벤토리도 닫기
        InventoryUI inventoryUI = FindObjectOfType<InventoryUI>();
        if (inventoryUI != null)
        {
            inventoryUI.CloseInventory();
        }

        if (Crosshair.Instance != null)
        {
            Crosshair.Instance.HideCursor();
        }
    }

    private void CheckLootPanelClose()
    {
        if (lootPanel != null && lootPanel.activeSelf && currentLoot == null)
        {
            CloseLootPanel();
        }
    }

    private void CheckDistanceClose()
    {
        if (objLootPanel != null && objLootPanel.activeSelf && currentFieldLoot == null)
        {
            CloseObjLootPanel();
        }
    }

    // 패널 열려있는지 체크
    private bool IsAnyPanelOpen()
    {
        return (lootPanel != null && lootPanel.activeSelf) ||
               (objLootPanel != null && objLootPanel.activeSelf);
    }

    public bool IsLooting()
    {
        return isLooting;
    }

    public bool IsLootPanelOpen()
    {
        return lootPanel != null && lootPanel.activeSelf;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, interactRange);
    }
}
