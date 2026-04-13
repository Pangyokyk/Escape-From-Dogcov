using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance { get; private set; }

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

    // 저장
    public void SaveGame()
    {
        if (GameData.Instance == null) return;

        SaveStashItems();
        SaveSecureItems();

        PlayerPrefs.Save();
        Debug.Log("게임 저장됨!");
    }

    // 불러오기
    public void LoadGame()
    {
        if (GameData.Instance == null) return;

        LoadStashItems();
        LoadSecureItems();

        Debug.Log("게임 불러옴!");
    }

    // 창고 아이템 저장
    private void SaveStashItems()
    {
        List<InventoryItem> stash = GameData.Instance.stashItems;
        PlayerPrefs.SetInt("StashCount", stash.Count);

        for (int i = 0; i < stash.Count; i++)
        {
            PlayerPrefs.SetString("StashItem_" + i, stash[i].itemData.itemName);
            PlayerPrefs.SetInt("StashItemCount_" + i, stash[i].count);
        }
    }

    // 창고 아이템 불러오기
    private void LoadStashItems()
    {
        GameData.Instance.stashItems.Clear();
        int count = PlayerPrefs.GetInt("StashCount", 0);

        ItemDatabase itemDB = FindObjectOfType<ItemDatabase>();
        if (itemDB == null) return;

        for (int i = 0; i < count; i++)
        {
            string itemName = PlayerPrefs.GetString("StashItem_" + i, "");
            int itemCount = PlayerPrefs.GetInt("StashItemCount_" + i, 1);
            ItemData item = itemDB.GetItemByName(itemName);
            if (item != null)
            {
                GameData.Instance.stashItems.Add(new InventoryItem(item, itemCount));
            }
        }
    }

    // === 보안 슬롯 저장 === 
    private void SaveSecureItems()
    {
        List<InventoryItem> secure = GameData.Instance.secureItems;
        PlayerPrefs.SetInt("SecureCount", secure.Count);

        for (int i = 0; i < secure.Count; i++)
        {
            PlayerPrefs.SetString("SecureItem_" + i, secure[i].itemData.itemName);
            PlayerPrefs.SetInt("SecureItemCount_" + i, secure[i].count);
        }
    }

    // === 보안 슬롯 불러오기 ===
    private void LoadSecureItems()
    {
        GameData.Instance.secureItems.Clear();
        int count = PlayerPrefs.GetInt("SecureCount", 0);

        ItemDatabase itemDB = FindObjectOfType<ItemDatabase>();
        if (itemDB == null) return;

        for (int i = 0; i < count; i++)
        {
            string itemName = PlayerPrefs.GetString("SecureItem_" + i, "");
            int itemCount = PlayerPrefs.GetInt("SecureItemCount_" + i, 1);
            ItemData item = itemDB.GetItemByName(itemName);
            if (item != null)
            {
                GameData.Instance.secureItems.Add(new InventoryItem(item, itemCount));
            }
        }
    }

    // 세이브 삭제
    public void DeleteSave()
    {
        PlayerPrefs.DeleteAll();
        Debug.Log("세이브 삭제됨!");
    }
}