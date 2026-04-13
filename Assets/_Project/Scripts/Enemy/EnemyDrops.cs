using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class EnemyDrops : MonoBehaviour
{
    [Header("드랍 설정")]
    [SerializeField] private GameObject lootBoxPrefab;

    [Header("확률 드랍 아이템")]
    [SerializeField] private ItemData medkitItem;   // 치료킷
    [SerializeField] private ItemData valuableItem; // 귀중품

    [Header("드랍 확률")]
    [Range(0f, 1f)]
    [SerializeField] private float medkitDropChance = 0.5f;
    [Range(0f, 1f)]
    [SerializeField] private float valuableDropChance = 0.2f;


    private EnemyEquipment equipment;

    private void Awake()
    {
        equipment = GetComponent<EnemyEquipment>();
    }

    public void DropLoot()
    {
        if (lootBoxPrefab == null) return;

        // lootbox 생성
        GameObject box = Instantiate(lootBoxPrefab, transform.position, Quaternion.identity);

        // lootContainer에 무기 추가
        LootContainer loot = box.GetComponent<LootContainer>();

        if(loot == null)
        {
            Debug.LogWarning("lootcontainer 없음");
            return;
        }

        // 무기 확정 드랍
        if(equipment != null)
        {
            ItemData weapon = equipment.GetEquippedWeapon();
            if(weapon != null)
            {
                loot.AddItem(weapon);
                Debug.Log("무기 드랍 : " + weapon.itemName);
            }

            // 탄약 확정 드랍
            ItemData ammo = equipment.GetAmmoType();
            if(ammo != null)
            {
                loot.AddItem(ammo);
                Debug.Log("탄약 드랍 : " + ammo.itemName);
            }
        }

        // 치료킷 확률 드랍
        if(medkitItem != null && Random.value <= medkitDropChance)
        {
            loot.AddItem(medkitItem);
            Debug.Log("치료킷 드랍");
        }

        // 귀중품 확률 드랍
        if (valuableItem != null && Random.value <= valuableDropChance)
        {
            loot.AddItem(valuableItem);
            Debug.Log("귀중품 드랍");
        }

        Debug.Log("루트박스 드랍");
    }

}
