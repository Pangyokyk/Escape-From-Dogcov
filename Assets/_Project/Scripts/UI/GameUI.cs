using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class GameUI : MonoBehaviour
{
    public static GameUI Instance { get; private set; }

    [Header("체력")]
    [SerializeField] private RectTransform healthMid;       // Mid 이미지
    [SerializeField] private RectTransform healthRight;     // Right 이미지
    [SerializeField] private float maxMidWidth = 170f;      // Mid 최대 너비
    [SerializeField] private float leftWidth = 9f;         // Left 이미지 너비
    [SerializeField] private TextMeshProUGUI healthText;

    [Header("탄약")]
    [SerializeField] private TextMeshProUGUI ammoText;

    [Header("무기 이름")]
    [SerializeField] private TextMeshProUGUI weaponNameText;

    // 자동 참조
    private WeaponManager weaponManager;
    private Health playerHealth;

    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void Start()
    {
        Debug.Log("GameUI Start - 오브젝트: " + gameObject.name);
        Debug.Log("healthMid: " + (healthMid != null));
        Debug.Log("healthRight: " + (healthRight != null));
        FindReferences();
    }

    private void Update()
    {
        UpdateHealthBar();
        UpdateAmmoText();
    }

    private void FindReferences()
    {
        weaponManager = FindObjectOfType<WeaponManager>();

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerHealth = player.GetComponent<Health>();
        }
    }

    private void UpdateHealthBar()
    {
        if (healthMid == null || healthRight == null)
        {
            return;
        }

        float ratio = 1f;
        float current = 100f;
        float max = 100f;

        // PlayerData에서 체력
        if (playerHealth != null)
        {
            current = playerHealth.GetCurrentHealth();
            max = playerHealth.GetMaxHealth();
            ratio = current / max;
        }
        // PlayerData에서 (Hideout)
        else if (PlayerData.Instance != null)
        {
            current = PlayerData.Instance.currentHealth;
            max = PlayerData.Instance.GetMaxHealth();
            ratio = current / max;
        }

        ratio = Mathf.Clamp01(ratio);

        // Mid 너비 조절
        float midWidth = maxMidWidth * ratio;
        healthMid.sizeDelta = new Vector2(midWidth, healthMid.sizeDelta.y);

        // Right 위치 조절
        float rightPosX = leftWidth + midWidth;
        healthRight.anchoredPosition = new Vector2(rightPosX, healthRight.anchoredPosition.y);

        if(healthText != null)
        {
            healthText.text = $"{current:F1} / {max:F1}";
        }
    }

    private void UpdateAmmoText()
    {
        if (ammoText == null) return;

        // WeaponManager 있으면 (Raid)
        if (weaponManager != null)
        {
            Gun currentGun = weaponManager.GetCurrentWeapon();
            if (currentGun != null)
            {
                int current = currentGun.GetCurrentAmmo();
                int max = currentGun.GetMaxAmmo();
                ammoText.text = current + " / " + max;

                if (weaponNameText != null)
                {
                    weaponNameText.text = currentGun.GetWeaponData().weaponName;
                }
                return;
            }
        }

        // PlayerData에서 (Hideout)
        if (PlayerData.Instance != null)
        {
            string weaponName = "";
            int ammo = 0;
            int maxAmmo = 0;

            // 현재 무기 확인
            int index = PlayerData.Instance.currentWeaponIndex;

            if (index == 0 && PlayerData.Instance.HasWeapon(0))
            {
                weaponName = PlayerData.Instance.weapon1Name;
                ammo = PlayerData.Instance.weapon1Ammo;
            }
            else if (index == 1 && PlayerData.Instance.HasWeapon(1))
            {
                weaponName = PlayerData.Instance.weapon2Name;
                ammo = PlayerData.Instance.weapon2Ammo;
            }

            // 무기 데이터에서 최대 탄약
            if (!string.IsNullOrEmpty(weaponName) && ItemDatabase.Instance != null)
            {
                ItemData item = ItemDatabase.Instance.GetItemByName(weaponName);
                if (item != null && item.weaponData != null)
                {
                    maxAmmo = item.weaponData.magazineSize;
                }
            }

            // 텍스트 표시
            if (!string.IsNullOrEmpty(weaponName))
            {
                ammoText.text = ammo + " / " + maxAmmo;
                if (weaponNameText != null)
                {
                    weaponNameText.text = weaponName;
                }
            }
            else
            {
                ammoText.text = "-- / --";
                if (weaponNameText != null)
                {
                    weaponNameText.text = "맨 손";
                }
            }
        }
    }
}
