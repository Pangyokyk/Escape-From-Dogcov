using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class WeaponManager : MonoBehaviour
{
    [Header("무기 프리팹들")]
    [SerializeField] private GameObject[] weaponPrefabs;

    [Header("무기 장착 위치")]
    [SerializeField] private Transform weaponHolder;

    // 현재 장착된 무기들
    private Gun[] equippedWeapons = new Gun[2];
    private int currentWeaponIndex = 0;

    private PlayerInputActions inputActions;

    private void Awake()
    {
        inputActions = new PlayerInputActions();
    }

    private void OnEnable()
    {
        inputActions.Player.Enable();
        inputActions.Player.Weapon1.performed += OnWeapon1;
        inputActions.Player.Weapon2.performed += OnWeapon2;
    }

    private void OnDisable()
    {
        inputActions.Player.Disable();
        inputActions.Player.Weapon1.performed -= OnWeapon1;
        inputActions.Player.Weapon2.performed -= OnWeapon2;
    }

    private void Start()
    {
        Debug.Log("=== WeaponManager Start ===");

        if (PlayerData.Instance == null)
        {
            Debug.LogError("PlayerData.Instance가 null!");
            return;
        }

        Debug.Log("weapon1Name: [" + PlayerData.Instance.weapon1Name + "]");
        Debug.Log("weapon2Name: [" + PlayerData.Instance.weapon2Name + "]");

        LoadWeaponsFromPlayerData();
    }

    // PlayerData에서 무기 로드
    public void LoadWeaponsFromPlayerData()
    {
        Debug.Log("=== 무기 로드 시작 ===");

        if (PlayerData.Instance == null)
        {
            Debug.LogWarning("PlayerData.Instance가 null!");
            return;
        }

        Debug.Log("weapon1Name: " + PlayerData.Instance.weapon1Name);
        Debug.Log("weapon2Name: " + PlayerData.Instance.weapon2Name);

        // 기존 무기 제거
        ClearWeapons();

        // 무기 1 로드
        if (!string.IsNullOrEmpty(PlayerData.Instance.weapon1Name))
        {
            SpawnWeapon(0, PlayerData.Instance.weapon1Name);
        }

        // 무기 2 로드
        if (!string.IsNullOrEmpty(PlayerData.Instance.weapon2Name))
        {
            SpawnWeapon(1, PlayerData.Instance.weapon2Name);
        }

        // 현재 무기 장착
        currentWeaponIndex = PlayerData.Instance.currentWeaponIndex;
        EquipWeapon(currentWeaponIndex);
    }

    private void SpawnWeapon(int slot, string itemName)
    {
        Debug.Log("=== SpawnWeapon 시작: " + itemName + " ===");

        // ItemDatabase 확인
        if (ItemDatabase.Instance == null)
        {
            Debug.LogWarning("ItemDatabase.Instance가 null!");
            return;
        }
        Debug.Log("1. ItemDatabase OK");

        // ItemData 찾기
        ItemData itemData = ItemDatabase.Instance.GetItemByName(itemName);
        if (itemData == null)
        {
            Debug.LogWarning("ItemData를 찾을 수 없음: " + itemName);
            return;
        }
        Debug.Log("2. ItemData OK: " + itemData.itemName);

        if (itemData.weaponData == null)
        {
            Debug.LogWarning("WeaponData가 null: " + itemName);
            return;
        }
        Debug.Log("3. WeaponData OK: " + itemData.weaponData.weaponName);

        // WeaponData의 weaponName으로 프리팹 찾기
        string weaponName = itemData.weaponData.weaponName;
        GameObject prefab = FindWeaponPrefab(weaponName);
        if (prefab == null)
        {
            Debug.LogWarning("무기 프리팹을 찾을 수 없음: " + weaponName);
            return;
        }
        Debug.Log("4. 프리팹 OK: " + prefab.name);

        // weaponHolder 확인
        if (weaponHolder == null)
        {
            Debug.LogWarning("weaponHolder가 null!");
            return;
        }
        Debug.Log("5. WeaponHolder OK");

        // 무기 생성
        GameObject weaponObj = Instantiate(prefab, weaponHolder);
        weaponObj.transform.localPosition = Vector3.zero;
        weaponObj.transform.localRotation = Quaternion.identity;

        Gun gun = weaponObj.GetComponent<Gun>();
        if (gun != null)
        {
            equippedWeapons[slot] = gun;
            weaponObj.SetActive(false);
            Debug.Log("6. 무기 로드 완료: " + weaponName + " → 슬롯 " + (slot + 1));
        }
        else
        {
            Debug.LogWarning("Gun 컴포넌트가 없음!");
        }
    }

    private GameObject FindWeaponPrefab(string weaponName)
    {
        foreach (GameObject prefab in weaponPrefabs)
        {
            Gun gun = prefab.GetComponent<Gun>();
            if (gun != null && gun.GetWeaponData() != null)
            {
                if (gun.GetWeaponData().weaponName == weaponName)
                {
                    return prefab;
                }
            }
        }
        return null;
    }

    private void ClearWeapons()
    {
        for (int i = 0; i < equippedWeapons.Length; i++)
        {
            if (equippedWeapons[i] != null)
            {
                Destroy(equippedWeapons[i].gameObject);
                equippedWeapons[i] = null;
            }
        }
    }

    private void OnWeapon1(InputAction.CallbackContext context)
    {
        if (equippedWeapons[0] != null)
        {
            EquipWeapon(0);
        }
        else
        {
            Debug.Log("무기 슬롯 1이 비어있습니다!");
        }
    }

    private void OnWeapon2(InputAction.CallbackContext context)
    {
        if (equippedWeapons[1] != null)
        {
            EquipWeapon(1);
        }
        else
        {
            Debug.Log("무기 슬롯 2가 비어있습니다!");
        }
    }

    private void EquipWeapon(int index)
    {
        if (index < 0 || index >= equippedWeapons.Length) return;

        // 모든 무기 비활성화
        for (int i = 0; i < equippedWeapons.Length; i++)
        {
            if (equippedWeapons[i] != null)
            {
                equippedWeapons[i].gameObject.SetActive(i == index);
            }
        }

        currentWeaponIndex = index;

        // PlayerData에 저장
        if (PlayerData.Instance != null)
        {
            PlayerData.Instance.currentWeaponIndex = index;
        }

        if (equippedWeapons[index] != null)
        {
            Debug.Log("무기 교체: " + equippedWeapons[index].GetWeaponData().weaponName);
        }
    }

    public Gun GetCurrentWeapon()
    {
        if (currentWeaponIndex < equippedWeapons.Length)
        {
            return equippedWeapons[currentWeaponIndex];
        }
        return null;
    }

    public bool HasAnyWeapon()
    {
        return equippedWeapons[0] != null || equippedWeapons[1] != null;
    }
}
