using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Item", menuName = "Game/Item Data")]
public class ItemData : ScriptableObject
{
    [Header("기본 정보")]
    public string itemName;
    public Sprite icon;
    public string description;

    [Header("아이템 타입")]
    public ItemType itemType;

    [Header("가치")]
    public int price;

    [Header("무게")]
    public float weight = 1f;

    [Header("장비 스탯 Helmet/Armor")]
    public float armorValue = 0f;   // 방어력
    public float healthBonus = 0f;  // 체력 보너스
    public float durability = 0f;   // 갑옷 내구도, 60이면 실제 hp는 600

    [Header("무기 연결")]
    public WeaponData weaponData;

    [Header("탄약 설정")]
    [SerializeField] public WeaponData.AmmoType ammoType;
    public int ammoCount = 30; // 묶음당 탄약수

    [Header("치료 아이템")]
    public float healAmount = 0f;   // 회복량
    public int maxUses = 1;         // 최대 사용 횟수

    public enum ItemType
    {
        Weapon,     // 무기
        Ammo,       // 탄약
        Medical,    // 의료품
        Valuable,   // 귀중품 (판매용)
        Helmet,     // 헬멧
        Armor       // 방탄조끼
    }
}
