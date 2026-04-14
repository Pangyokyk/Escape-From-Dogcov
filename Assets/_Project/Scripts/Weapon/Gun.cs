using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class Gun : MonoBehaviour
{
    [Header("무기 데이터")]
    [SerializeField] private WeaponData weaponData;

    [Header("참조")]
    [SerializeField] private Transform firePoint;          // 총구 위치
    [SerializeField] private GameObject bulletPrefab;      // 투사체 (선택)
    [SerializeField] private ParticleSystem muzzleFlash;   // 총구 화염 (선택)
    [SerializeField] private GameObject hitEffectPrefab; // 
    [SerializeField] private LayerMask groundLayer;

    [Header("사운드")]
    [SerializeField] private AudioClip fireSound; // 총 발사 사운드
    [SerializeField] private AudioClip reloadSound; // 총 장전소리
    [SerializeField] private AudioClip emptySound;  // 탄약 없을 때
    private AudioSource audioSource;
    private InventoryUI inventoryUI;

    //스탯 (weaponData에서 로드함)
    private float damage;
    private float fireRate;
    private float range;
    private float reloadTime;
    private int magazineSize;
    private float recoilAmount;

    // 상태
    private int currentAmmo;
    private float nextFireTime;
    private bool isReloading;

    // 입력
    private PlayerInputActions inputActions;

    // 총 발사 회전막기
    private Quaternion originalRotation;

    private void Awake()
    {
        inputActions = new PlayerInputActions();
        audioSource = GetComponent<AudioSource>();

        LoadWeaponData();
    }

    private void Start()
    {
        originalRotation = transform.localRotation;
    }

    private void LoadWeaponData()
    {
        if(weaponData != null)
        {
            damage = weaponData.damage;
            fireRate = weaponData.fireRate;
            range = weaponData.range;
            reloadTime = weaponData.reloadTime;
            magazineSize = weaponData.magazineSize;
            recoilAmount = weaponData.recoilAmount;
            currentAmmo = magazineSize;
        }
    }

    private void OnEnable()
    {
        inputActions.Player.Enable();
        inputActions.Player.Reload.performed += OnReload;
        
        // 무기 교체 시 탄약 유지 (재장전 상태 초기화)
    }

    private void OnDisable()
    {
        inputActions.Player.Disable();
        inputActions.Player.Reload.performed -= OnReload;
    }

    private void Update()
    {
        // 재장전 중이면 발사 불가
        if (isReloading) return;

        // UI 위에 마우스 있으면 발사 안함
        if (EventSystem.current.IsPointerOverGameObject()) return;

        // Player만 발사
        if (!transform.root.CompareTag("Player")) return;

        // 드래그 중이면 발사 안되게
        if(InventoryManager.Instance != null && InventoryManager.Instance.IsDragging())
        {
            return;
        }

        // UI열려있을 시 발사불가
        if(inventoryUI == null)
        {
            inventoryUI = FindObjectOfType<InventoryUI>();
        }

        if (inventoryUI != null && inventoryUI.IsOpen())
            return;
        
        /*InventoryUI inventoryUI = FindObjectOfType<InventoryUI>();
        if(inventoryUI != null && inventoryUI.IsOpen())
        {
            return;
        }*/

        // 마우스 왼쪽 버튼 누르고 있으면 발사
        if (inputActions.Player.Fire.IsPressed() && Time.time >= nextFireTime)
        {
            if (currentAmmo > 0)
            {
                Fire();
                nextFireTime = Time.time + fireRate;
            }
            else
            {
                // 탄약 없음 - 자동 재장전 시도
                if(!isReloading)
                {
                    // 빈 탄창 소리
                    if(emptySound != null && audioSource != null)
                    {
                        audioSource.PlayOneShot(emptySound);
                    }

                    // 탄약 없으면 자동 재장전
                    StartCoroutine(Reload());
                }
                nextFireTime = Time.time + 0.5f;    // 연속 클릭 방지
            }
        }
    }

    private void Fire()
    {
        currentAmmo--;

        // 총구 화염 (있으면)
        if (muzzleFlash != null)
        {
            muzzleFlash.Play();
        }

        // 발사 사운드 추가
        if(audioSource != null && fireSound != null)
        {
            audioSource.PlayOneShot(fireSound);
        }

        // 반동
        ApplyRecoil();

        // 크로스헤어 타겟 계산
        Vector3 targetPoint = GetCrosshairTarget();

        // 발사 시작점
        Vector3 spawnPoint = firePoint.position;
        spawnPoint.y = targetPoint.y;

        Vector3 direction = (targetPoint - spawnPoint).normalized;

        // 최소 거리 체크 + 뒤로 발사되서 플레이어 맞는거 방지
        float distanceToTarget = Vector3.Distance(spawnPoint, targetPoint);
        Vector3 playerForward = transform.root.forward;

        // 타겟이 너무 가깝거나 뒤에 있으면 플레이어가 바라보는 방향으로 발사
        // Dot Product로 방향 체크
        // > 0 = 앞쪽
        // < 0 = 뒤쪽
        // 0.1f = 약간의 여유
        if (distanceToTarget < 2f || Vector3.Dot(direction, playerForward) < 0.1f)  // 조건: 거리 2m 미만 OR 뒤쪽이면
        {
            // 플레이어가 바라보는 방향으로 강제시킴
            direction = playerForward;
            direction.y = 0;
            direction.Normalize();
        }

        // 총알 생성 발사
        if (bulletPrefab != null)
        {
            // direction 방향을 바라보도록 회전시킴
            Quaternion bulletRotation = Quaternion.LookRotation(direction);
            GameObject bulletObj = Instantiate(bulletPrefab, spawnPoint, bulletRotation);
            Bullet bullet = bulletObj.GetComponent<Bullet>();
            if (bullet != null)
            {
                // 최상위 부모(Player 또는 Enemy) 전달
                GameObject rootOwner = transform.root.gameObject;
                bullet.Initialize(direction, damage, weaponData.bulletSpeed, rootOwner);
            }
        }

        // Raycast 방식 (즉시 판정)
        if (Physics.Raycast(spawnPoint, direction, out RaycastHit hit, range))
        {
            //Debug.Log("Hit: " + hit.collider.name);

            // 데미지 처리
            Health health = hit.collider.GetComponent<Health>();
            if (health != null)
            {
                health.TakeDamage(damage);

                // 피격 이펙트 생성
                if (hitEffectPrefab != null)
                {
                    GameObject effect = Instantiate(hitEffectPrefab, hit.point, Quaternion.identity);
                    Destroy(effect, 0.5f); // 0.5초 후 자동삭제
                }
            }
        }

        // 디버그용 Ray 그리기 (Scene뷰에서 보임)
        Debug.DrawRay(spawnPoint, direction * range, Color.red, 0.1f);
    }

    private void ApplyRecoil()
    {
        // 총 모델 반동(현실감 더 줄려고, 밋밋하니깐)
        transform.localRotation = Quaternion.Euler(-recoilAmount * 10f, 0f, 0f);
        Invoke(nameof(ResetRecoil), 0.05f);

        if (Crosshair.Instance != null)
        {
            Crosshair.Instance.ApplyRecoil(recoilAmount);
        }
    }

    private void ResetRecoil()
    {
        transform.localRotation = originalRotation;
    }

    private void OnReload(InputAction.CallbackContext context)
    {
        if (!isReloading && currentAmmo < magazineSize)
        {
            StartCoroutine(Reload());
        }
    }

    private System.Collections.IEnumerator Reload()
    {
        // 인벤토리에서 탄약 확인
        int ammoNeeded = magazineSize - currentAmmo;
        int ammoFound = GetAmmoFromInventory(ammoNeeded);

        if(ammoFound == 0)
        {
            Debug.Log("탄약이 없음");
            yield break;    // 재장전 취소
        }

        isReloading = true;
        Debug.Log(weaponData.weaponName + "재장전 중...");

        if (audioSource != null && reloadSound != null)
        {
            audioSource.PlayOneShot(reloadSound);
        }

        yield return new WaitForSeconds(reloadTime);

        currentAmmo += ammoFound;
        isReloading = false;
        Debug.Log(weaponData.weaponName + "재장전 완료!");
    }

    // 크로스헤어 위치를 월드 좌표로 전환
    private Vector3 GetCrosshairTarget()
    {
        Camera cam = Camera.main;

        // 크로스헤어 있으면 크로스헤어 위치 사용
        if(Crosshair.Instance != null)
        {
            Ray ray = cam.ScreenPointToRay(Crosshair.Instance.GetScreenPosition());

            if(Physics.Raycast(ray, out RaycastHit hit, range, groundLayer))
            {
                // Y축은 firepoint와 같게(수평 발사)
                Vector3 targetPoint = hit.point;
                targetPoint.y = firePoint.position.y;
                return targetPoint;
            }

            // 바닥에 안 맞으면 앞쪽으로;
            return firePoint.position + firePoint.forward * range;
        }

        // 없으면 기존 방식 (마우스 위치)
        Ray mouseRay = cam.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(mouseRay, out RaycastHit mouseHit, range, groundLayer))
        {
            Vector3 targetPoint = mouseHit.point;
            targetPoint.y = firePoint.position.y;
            return targetPoint;
        }

        return firePoint.position + firePoint.forward * range;
    }

    private int GetAmmoFromInventory(int ammoNeeded)
    {
        if (GameData.Instance == null) return 0;
        if (weaponData == null) return 0;

        // GameData의 UseAmmo() 사용!
        return GameData.Instance.UseAmmo(weaponData.ammoType, ammoNeeded);
    }

    public void SetCurrentAmmo(int ammo)
    {
        currentAmmo = Mathf.Clamp(ammo, 0, magazineSize);
    }

    public AudioClip GetFireSound()
    {
        return fireSound;
    }

    public AudioClip GetReloadSound()
    {
        return reloadSound;
    }

    // UI에서 사용할 수 있도록
    public int GetCurrentAmmo() => currentAmmo;
    public int GetMaxAmmo() => magazineSize;
    public bool IsReloading() => isReloading;
    public WeaponData GetWeaponData() => weaponData;
}
