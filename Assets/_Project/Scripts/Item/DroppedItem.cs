using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DroppedItem : MonoBehaviour
{
    [Header("아이템 정보")]
    private ItemData itemData;
    private int count = 1;

    [Header("바닥에 떨굴때 이미지")]
    [SerializeField] private SpriteRenderer iconRenderer;

    [Header("프롬프트")]
    [SerializeField] private GameObject promptPrefab;
    [SerializeField] private Vector3 promptOffset = new Vector3(0, 2f, 0);

    private GameObject promptInstance;
    private bool playerInRange = false;


    // 아이템 설정
    public void Setup(ItemData item, int itemCount = 1)
    {
        itemData = item;
        count = itemCount;

        // 떨군 물체 아이콘으로 표시
        if(iconRenderer != null && item.icon != null)
        {
            iconRenderer.sprite = item.icon;

            if(Camera.main != null)
            {
                iconRenderer.transform.LookAt(Camera.main.transform);
                iconRenderer.transform.Rotate(0, 180, 0);
            }
            Debug.Log("아이콘 설정됨: " + item.itemName);
        }
        else
        {
            Debug.Log("아이콘 설정 실패! iconRenderer: " + (iconRenderer != null) + ", icon: " + (item.icon != null));
        }

        // 이름 변경 (디버그용)
        gameObject.name = "Dropped_" + item.itemName;
    }

    public ItemData GetItemData()
    {
        return itemData;
    }

    public int GetCount()
    {
        return count;
    }

    // 아이템 줍기
    public bool PickUp()
    {
        if (itemData == null) return false;

        // 인벤토리에 추가
        if(GameData.Instance != null)
        {
            GameData.Instance.raidItems.Add(new InventoryItem(itemData, count));
            Debug.Log(itemData.itemName + " x" + count + " 획득");

            // 무게 업뎃
            if(PlayerData.Instance != null)
            {
                PlayerData.Instance.CalculateTotalWeight();
            }

            // UI도 업뎃
            if(InventoryManager.Instance != null)
            {
                InventoryManager.Instance.RefreshAllUI();
            }

            HidePrompt();
            Destroy(gameObject);
            return true;
        }

        return false;
    }

    public void ShowPrompt()
    {
        if(promptInstance == null && promptPrefab != null)
        {
            promptInstance = Instantiate(promptPrefab, transform.position + promptOffset, Quaternion.identity);
        }

        if(promptInstance != null)
        {
            promptInstance.SetActive(true);
            promptInstance.transform.position = transform.position + promptOffset;

            // 텍스트 설정
            TextMeshProUGUI text = promptInstance.GetComponentInChildren<TextMeshProUGUI>();
            if(text != null)
            {
                string countText = count > 1 ? " x" + count : "";
                text.text = "[F] " + itemData.itemName + countText;
            }

            // 프롬프트 카메라 방향으로
            if(Camera.main != null)
            {
                promptInstance.transform.LookAt(Camera.main.transform);
                promptInstance.transform.Rotate(0, 180, 0);
            }
        }
    }

    public void HidePrompt()
    {
        if(promptInstance != null)
        {
            promptInstance.SetActive(false);
        }
    }

    private void OnDestroy()
    {
        if(promptInstance != null)
        {
            Destroy(promptInstance);
        }
    }
}
