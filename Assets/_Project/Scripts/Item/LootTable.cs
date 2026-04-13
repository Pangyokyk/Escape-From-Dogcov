using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New LootTable", menuName = "Game/Loot Table")]
public class LootTable : ScriptableObject
{
    [System.Serializable]
    public class LootEntry
    {
        public ItemData item;
        public float dropChance = 1f;

        [Header("드롭 개수")]
        public int minCount = 1;
        public int maxCount = 1;
    }

    [Header("루트 설정")]
    public List<LootEntry> possibleItems = new List<LootEntry>();

    [Header("생성 개수")]
    public int minItems = 1;
    public int maxItems = 3;

    // 아이템 생성 (개수 포함)
    public List<LootResult> GenerateItemsWithCount()
    {
        List<LootResult> results = new List<LootResult>();

        if (possibleItems.Count == 0) return results;

        int itemCount = Random.Range(minItems, maxItems + 1);
        List<LootEntry> availableItems = new List<LootEntry>(possibleItems);

        for (int i = 0; i < itemCount && availableItems.Count > 0; i++)
        {
            // 가중치 기반 선택
            float totalWeight = 0f;
            foreach (var entry in availableItems)
            {
                totalWeight += entry.dropChance;
            }

            float randomValue = Random.Range(0f, totalWeight);
            float currentWeight = 0f;
            LootEntry selectedEntry = null;

            foreach (var entry in availableItems)
            {
                currentWeight += entry.dropChance;
                if (randomValue <= currentWeight)
                {
                    selectedEntry = entry;
                    break;
                }
            }

            if (selectedEntry != null)
            {
                int dropCount = Random.Range(selectedEntry.minCount, selectedEntry.maxCount + 1);

                results.Add(new LootResult
                {
                    itemData = selectedEntry.item,
                    count = dropCount
                });

                availableItems.Remove(selectedEntry);
            }
        }

        return results;
    }
}

// 드롭 결과 구조체
[System.Serializable]
public class LootResult
{
    public ItemData itemData;
    public int count;
}