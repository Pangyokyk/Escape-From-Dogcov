using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngineInternal;

public class Bullet : MonoBehaviour
{
    [Header("기본 설정")]
    [SerializeField] private float lifetime = 3f;

    [Header("이펙트")]
    [SerializeField] private GameObject hitEffectPrefab;

    private float speed;
    private float damage;
    private Vector3 direction;
    private GameObject owner;  // 발사자 오브젝트 자체 저장
    private bool hasHit = false;

    private void Start()
    {
        Destroy(gameObject, lifetime);
    }

    private void Update()
    {
        if (hasHit) return;

        float moveDistance = speed * Time.deltaTime;

        RaycastHit[] hits = Physics.RaycastAll(transform.position, direction, moveDistance);

        foreach (RaycastHit hit in hits)
        {
            // 발사자 본인이면 무시
            if (IsOwner(hit.collider)) continue;

            OnHit(hit.point, hit.collider);
            return;
        }

        transform.Translate(direction * moveDistance, Space.World);
    }

    // 발사자인지 체크 (본인 + 자식 오브젝트 모두)
    private bool IsOwner(Collider col)
    {
        if (owner == null) return false;

        // 발사자 본인이거나 발사자의 자식이면 true
        Transform check = col.transform;
        while (check != null)
        {
            if (check.gameObject == owner) return true;
            check = check.parent;
        }
        return false;
    }

    public void Initialize(Vector3 dir, float dmg, float spd, GameObject shooter)
    {
        direction = dir.normalized;
        damage = dmg;
        speed = spd;
        owner = shooter;
    }

    private void OnHit(Vector3 hitPoint, Collider hitCollider)
    {
        if (hasHit) return;
        hasHit = true;

        // 데미지 처리 + 이펙트 (Health 있는 대상만
        Health health = hitCollider.GetComponent<Health>();
        if(health != null)
        {
            health.TakeDamage(damage);

            // 피격 이펙트
            if(hitEffectPrefab != null)
            {
                GameObject effect = Instantiate(hitEffectPrefab , hitPoint, Quaternion.identity);
                Destroy(effect, 0.5f);
            }
        }

        Destroy(gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (hasHit) return;
        if (IsOwner(other)) return;

        OnHit(transform.position, other);
    }
}
