using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FieldLootContainer : MonoBehaviour
{
    [Header("루트 테이블")]
    [SerializeField] private LootTable lootTable;

    [Header("설정")]
    [SerializeField] private float lootTime = 2f;
    [SerializeField] private string interactText = "수색";

    [Header("프롬프트")]
    [SerializeField] private GameObject promptPrefab;
    [SerializeField] private Vector3 promptOffset = new Vector3(0, 2f, 0);

    private GameObject promptInstance;

    // 생성된 아이템 (개수 포함!)
    private List<LootResult> generatedItems = new List<LootResult>();
    private bool isLooted = false;

    public void GenerateRandomItems()
    {
        if (lootTable != null && !isLooted)
        {
            generatedItems = lootTable.GenerateItemsWithCount();
            isLooted = true;

            Debug.Log("루트 생성: " + generatedItems.Count + "종류");
            foreach (var item in generatedItems)
            {
                Debug.Log("  - " + item.itemData.itemName + " x" + item.count);
            }
        }
    }

    // LootResult 리스트 반환 (수정!)
    public List<LootResult> GetItems()
    {
        if (!isLooted)
        {
            GenerateRandomItems();
        }
        return generatedItems;
    }

    // ItemData로 아이템 제거 (수정!)
    public void TakeItem(ItemData itemData)
    {
        LootResult toRemove = null;
        foreach (var item in generatedItems)
        {
            if (item.itemData == itemData)
            {
                toRemove = item;
                break;
            }
        }

        if (toRemove != null)
        {
            generatedItems.Remove(toRemove);
        }
    }

    public bool HasItems()
    {
        return generatedItems.Count > 0;
    }

    public float GetLootTime()
    {
        return lootTime;
    }

    public string GetInteractText()
    {
        return interactText;
    }

    public void ShowPrompt()
    {
        if (promptInstance == null && promptPrefab != null)
        {
            promptInstance = Instantiate(promptPrefab, transform.position + promptOffset, Quaternion.identity);
        }

        if (promptInstance != null)
        {
            promptInstance.SetActive(true);
            promptInstance.transform.position = transform.position + promptOffset;

            if (Camera.main != null)
            {
                promptInstance.transform.LookAt(Camera.main.transform);
                promptInstance.transform.Rotate(0, 180, 0);
            }
        }
    }

    public void HidePrompt()
    {
        if (promptInstance != null)
        {
            promptInstance.SetActive(false);
        }
    }

    private void OnDestroy()
    {
        if (promptInstance != null)
        {
            Destroy(promptInstance);
        }
    }
}