using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LootContainer : MonoBehaviour
{
    [Header("루트 아이템")]
    [SerializeField] private List<ItemData> items = new List<ItemData>();

    [Header("설정")]
    [SerializeField] private float lootTime = 2f;  // 루팅 시간
    [SerializeField] private bool destroyWhenEmpty = false;  // 비면 삭제할지

    [Header("상호작용 글씨")]
    [SerializeField] private GameObject promptPrefab;
    [SerializeField] private Vector3 promptOffset = new Vector3(0, 1.5f, 0);

    private GameObject promptInstance;

    // 아이템 목록 반환
    public List<ItemData> GetItems()
    {
        return items;
    }

    // 아이템 추가
    public void AddItem(ItemData item)
    {
        if (item != null)
        {
            items.Add(item);
        }
    }

    // 아이템 가져가기
    public void TakeItem(ItemData item)
    {
        if (items.Contains(item))
        {
            items.Remove(item);

            if (destroyWhenEmpty && items.Count == 0)
            {
                Destroy(gameObject);
            }
        }
    }

    // 루팅 시간 반환
    public float GetLootTime()
    {
        return lootTime;
    }

    // 아이템 남았는지
    public bool HasItems()
    {
        return items.Count > 0;
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

            if(Camera.main != null)
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
