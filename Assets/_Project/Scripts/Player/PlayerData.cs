using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerData : MonoBehaviour
{
    public static PlayerData Instance { get; private set; }

    [Header("장착된 무기 (null이면 빈 슬롯)")]
    public string weapon1Name = "";  // 무기 이름으로 저장
    public string weapon2Name = "";
    public int weapon1Ammo = 0;
    public int weapon2Ammo = 0;
    public int currentWeaponIndex = 0;

    [Header("장착된 장비")]
    public string helmetName = ""; // 헬멧
    public string armorName = ""; // 방탄조끼

    [Header("장비 스탯")]
    public float totalArmorValue = 0f;
    public float totalHealthBonus = 0f;

    [Header("갑옷 내구도")]
    public float armorMaxDurability = 0f;       // 최대내구도
    public float armorCurrentDurability = 0f;   // 현재 내구도

    [Header("기본 무기 설정")]
    [SerializeField] private string defaultWeaponName = "AK-47";
    [SerializeField] private int defaultAmmo = 30;

    [Header("무게 제한")]
    public float baseWeight = 30f; // 기본무게
    public float maxWeight = 60f; // 최대 무게
    public float currentWeight = 0f; // 현재 무게

    [Header("돈")]
    public int money = 3000;     // 초기 자금


    [Header("플레이어 상태")]
    public float baseHealth = 100f;     // 기본 체력
    public float currentHealth = 100f;  // 현제 체력

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

    // 최대 체력 (기본 + 장비 스탯)
    public float GetMaxHealth()
    {
        return baseHealth + totalHealthBonus;
    }

    // 받는 데미지 계산
    public float CalculateDamage(float rawDamage)
    {
        // 갑옷 내구도가 남아있으면 방어력 적용 + 내구도 감소
        if (armorCurrentDurability > 0)
        {
            // 방어력으로 데미지 감소
            float reduction = Mathf.Clamp(totalArmorValue, 0f, 80f) / 100f;
            float finalDamage = rawDamage * (1f - reduction);

            // 갑옷 내구도 감소 (총알 막았으니까!)
            DamageArmor(rawDamage);

            return finalDamage;
        }

        // 갑옷 없거나 내구도 0이면 풀 데미지
        return rawDamage;
    }

    // 장비 장착 시 스탯 갱신
    public void UpdateEquipmentStats(ItemDatabase itemDB)
    {
        totalArmorValue = 0f;
        totalHealthBonus = 0f;

        // 헬멧
        if(!string.IsNullOrEmpty(helmetName))
        {
            ItemData helmet = itemDB.GetItemByName(helmetName);
            if(helmet != null)
            {
                totalArmorValue += helmet.armorValue;
                totalHealthBonus += helmet.healthBonus;
            }
        }

        // 방탄 조끼
        if (!string.IsNullOrEmpty(armorName))
        {
            ItemData armor = itemDB.GetItemByName(armorName);
            if (armor != null)
            {
                totalArmorValue += armor.armorValue;
                totalHealthBonus += armor.healthBonus;
            }
        }

        // 현재체력이 최대체력 넘지않도록
        float maxHp = GetMaxHealth();
        if(currentHealth > maxHp)
        {
            currentHealth = maxHp;
        }

        Debug.Log("방어력 - " + totalArmorValue + "체력 증가 - " + totalHealthBonus);
    }

    // 이동속도 배율
    public float GetSpeedMultiplier()
    {
        if(currentWeight <= maxWeight)
        {
            return 1f;
        }

        // maxWeight를 넘어야 이동속도가 저하되게
        float overWeight = currentWeight - maxWeight;   // 현재무게 - 최대무게
        float penalty = 0.04f;

        float multiplier = 1f - (overWeight * penalty);

        // 최소 0으로 제한, 음수가 되면 날아갈 수 도 있으니
        return Mathf.Clamp(multiplier, 0f, 1f);
    }

    public void AddWeight(float weight)
    {
        currentWeight += weight;
        Debug.Log("현재무게 : " + currentWeight + " / 이동속도 : " + (GetSpeedMultiplier() * 100f) + "%");
    }

    public void RemoveWeight(float weight)
    {
        currentWeight -= weight;
        if(currentWeight < 0f)
        {
            currentWeight = 0f;
        }
    }

    // 현재 총 무게 계산 (인벤토리 + 장비)
    public float CalculateTotalWeight()
    {
        float total = 0f;

        // 인벤토리 아이템 무게
        if (GameData.Instance != null)
        {
            foreach (InventoryItem invItem in GameData.Instance.raidItems)
            {
                if (invItem != null && invItem.itemData != null)
                {
                    total += invItem.itemData.weight * invItem.count;
                }
            }
        }

        // 장비 무게 (ItemDatabase에서 찾기)
        if (ItemDatabase.Instance != null)
        {
            // 무기1
            if (!string.IsNullOrEmpty(weapon1Name))
            {
                ItemData w1 = ItemDatabase.Instance.GetItemByName(weapon1Name);
                if (w1 != null) total += w1.weight;
            }

            // 무기2
            if (!string.IsNullOrEmpty(weapon2Name))
            {
                ItemData w2 = ItemDatabase.Instance.GetItemByName(weapon2Name);
                if (w2 != null) total += w2.weight;
            }

            // 헬멧
            if (!string.IsNullOrEmpty(helmetName))
            {
                ItemData h = ItemDatabase.Instance.GetItemByName(helmetName);
                if (h != null) total += h.weight;
            }

            // 아머
            if (!string.IsNullOrEmpty(armorName))
            {
                ItemData a = ItemDatabase.Instance.GetItemByName(armorName);
                if (a != null) total += a.weight;
            }
        }

        currentWeight = total;
        return total;
    }

    public void StartRaid()
    {
        // 체력이 0이거나 설정 안 됐을 때만 초기화
        if (currentHealth <= 0)
        {
            currentHealth = GetMaxHealth();
        }

        // 현재체력이 최대체력 넘으면 맞추기
        float maxHP = GetMaxHealth();
        if (currentHealth > maxHP)
        {
            currentHealth = maxHP;
        }
    }

    // 무기 장착
    public void EquipWeapon(int slot, WeaponData weapon)
    {
        if (slot == 0)
        {
            weapon1Name = weapon != null ? weapon.weaponName : "";
            weapon1Ammo = weapon != null ? weapon.magazineSize : 0;
        }
        else if (slot == 1)
        {
            weapon2Name = weapon != null ? weapon.weaponName : "";
            weapon2Ammo = weapon != null ? weapon.magazineSize : 0;
        }
    }

    public void EquipGear(EquipmentData equipment)
    {
        if (equipment == null) return;

        if (equipment.equipmentType == EquipmentData.EquipmentType.Helmet)
        {
            helmetName = equipment.equipmentName;
        }
        else if (equipment.equipmentType == EquipmentData.EquipmentType.Armor)
        {
            armorName = equipment.equipmentName;
        }
    }

    public void UnequipGear(EquipmentData.EquipmentType type)
    {
        if(type == EquipmentData.EquipmentType.Helmet)
        {
            helmetName = "";
        }
        else if(type == EquipmentData.EquipmentType.Armor)
        {
            armorName = "";
        }
    }

    // 갑옷 장착시 내구도 설정
    public void EquipArmor(ItemData armor)
    {
        if (armor == null) return;

        armorName = armor.itemName;
        armorMaxDurability = armor.durability;
        armorCurrentDurability = armor.durability;

        Debug.Log("갑옷 장착 : " + armorName + " 내구도 : " + armorCurrentDurability);
    }
    
    // 갑옷 해제
    public void UnequipArmor()
    {
        armorName = "";
        armorMaxDurability = 0f;
        armorCurrentDurability = 0f;
    }

    // 갑옷에 데미지(감소된 데미지 이전 원본 데미지로 내구도를 차감할거임
    public void DamageArmor(float rawDamage)
    {
        if(string.IsNullOrEmpty(armorName)) return;
        if (armorCurrentDurability <= 0) return;

        // 내구도 감소 (원본 데미지 / 10)
        float durabilityDamage = rawDamage / 10f;
        armorCurrentDurability -= durabilityDamage;

        Debug.Log("갑옷 내구도 감소 : " + armorCurrentDurability.ToString("F1") + " / " + armorMaxDurability);

        // 갑옷 내구도 0이하면 없어지게
        if(armorCurrentDurability <= 0 )
        {
            armorCurrentDurability = 0f;
            Debug.Log("갑옷내구도 0이 되어 파괴");

            // 방어력 다시 계산(갑옷 효과 없어짐)
            if(ItemDatabase.Instance != null)
            {
                UpdateEquipmentStats(ItemDatabase.Instance);
            }
        }
    }

    // 갑옷 방어력 (내구도 0이면 방어력 0)
    public float GetArmorValue()
    {
        if (armorCurrentDurability <= 0) return 0f;
        return totalArmorValue;
    }

    // 갑옷 실제 내구도 계산(내구도 * 10의 숨겨진 hp를 가지고있음)
    public float GetArmorHp()
    {
        return armorCurrentDurability * 10;
    }

    // 무기 있는지 확인
    public bool HasWeapon(int slot)
    {
        if (slot == 0) return !string.IsNullOrEmpty(weapon1Name);
        if (slot == 1) return !string.IsNullOrEmpty(weapon2Name);
        return false;
    }

    public bool HasHelmet()
    {
        return !string.IsNullOrEmpty(helmetName);
    }

    public bool HasArmor()
    {
        return !string.IsNullOrEmpty(armorName);
    }

    // 무게 체크
    public bool CanCarry(float itemWeight)
    {
        return (currentWeight + itemWeight) <= maxWeight;
    }

    // 사망 시 모든 것 손실
    public void OnPlayerDeath()
    {
        weapon1Name = "";
        weapon2Name = "";
        weapon1Ammo = 0;
        weapon2Ammo = 0;
        currentWeaponIndex = 0;
        currentHealth = 100f;
        currentWeight = 0f;
        helmetName = "";
        armorName = "";

        // 내구도 초기화
        armorMaxDurability = 0f;
        armorCurrentDurability = 0f;

        totalHealthBonus = 0f;
        totalHealthBonus = 0f;

        Debug.Log("사망! 모든 장비 손실");
    }
}
