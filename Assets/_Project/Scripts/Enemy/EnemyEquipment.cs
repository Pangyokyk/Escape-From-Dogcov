using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyEquipment : MonoBehaviour
{
    [Header("장착 무기")]
    [SerializeField] private ItemData equippedWeapon; // 들고있는 무기
    [SerializeField] private ItemData ammoType; // 무기에 맞는 탄약

    [Header("무기 데이터 (전투용)")]
    [SerializeField] private WeaponData weaponData; // 데미지, 사거리 등

    [Header("무기 표시")]
    [SerializeField] private Transform weaponHolder; // 무기 붙는 위치
    [SerializeField] private GameObject weaponModel; // 시각적 무기 모델
    [SerializeField] private Transform firePoint; 
    [SerializeField] private ParticleSystem muzzleFlash;

    private AudioClip fireSound;
    private AudioClip reloadSound;
    private AudioSource audioSource;

    // 탄창 
    private int currentAmmo;
    private bool isReloading = false;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        if (weaponModel != null)
        {
            // Inspector에서 연결 안 했으면 자동으로 찾기
            if (firePoint == null)
            {
                firePoint = weaponModel.transform.Find("FirePoint");

                // 직접 자식에 없으면 모든 자식에서 찾기
                if (firePoint == null)
                {
                    foreach (Transform child in weaponModel.GetComponentsInChildren<Transform>())
                    {
                        if (child.name == "FirePoint")
                        {
                            firePoint = child;
                            break;
                        }
                    }
                }
            }

            if (muzzleFlash == null)
            {
                muzzleFlash = weaponModel.GetComponentInChildren<ParticleSystem>();
            }

            Gun gun = weaponModel.GetComponent<Gun>();
            if (gun != null)
            {
                fireSound = gun.GetFireSound();
                reloadSound = gun.GetReloadSound();
            }
        }

        if (weaponData != null)
        {
            currentAmmo = weaponData.magazineSize;
        }

        // 디버그
        Debug.Log("firePoint 찾음: " + (firePoint != null));
        if (firePoint != null)
        {
            Debug.Log("firePoint 월드 위치: " + firePoint.position);
        }
    }

    // 무기 데이터 반환
    public ItemData GetEquippedWeapon()
    {
        return equippedWeapon;
    }

    // 탄약 데이터 반환
    public ItemData GetAmmoType()
    {
        return ammoType;
    }

    public WeaponData GetWeaponData()
    {
        return weaponData;
    }

    public Transform GetFirePoint()
    {
        return firePoint;
    }
    
    public void PlayMuzzleFlash()
    {
        if(muzzleFlash != null)
        {
            muzzleFlash.Play();
        }
    }

    public void PlayFireSound()
    {
        {
            if(audioSource != null && fireSound != null)
            {
                audioSource.PlayOneShot(fireSound);
            }
        }
    }

    // 탄창 시스템
    public bool CanFire()
    {
        return currentAmmo > 0 && !isReloading;
    }

    public bool IsReloading()
    {
        return isReloading;
    }

    public void UseAmmo()
    {
        currentAmmo--;
        Debug.Log("적 남은 탄약 : " + currentAmmo);
    }

    public bool NeedsReload()
    {
        return currentAmmo <= 0 && !isReloading;
    }

    public void StartReload()
    {
        if(!isReloading)
        {
            StartCoroutine(ReloadRoutine());
        }
    }

    private IEnumerator ReloadRoutine()
    {
        isReloading = true;
        Debug.Log("재장전 중...");

        // 재장전 소리
        if(audioSource != null && reloadSound != null)
        {
            audioSource.PlayOneShot(reloadSound);
        }

        // 재장전 시간 대기
        yield return new WaitForSeconds(weaponData.reloadTime);

        // 탄창 채우기
        currentAmmo = weaponData.magazineSize;
        isReloading = false;
        Debug.Log(" 적 재장전 완료 탄약 : " + currentAmmo);
    }

}
