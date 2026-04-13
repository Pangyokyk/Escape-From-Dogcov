using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngineInternal;

public class EnemyAI : MonoBehaviour
{
    // AI 상태 정의
    public enum State
    {
        Idle,       // 대기
        Patrol,     // 순찰
        Chase,      // 추적
        Attack,     // 공격
        Search,     // 수색
        Return      // 복귀
    }

    [Header("AI 설정")]
    [SerializeField] private float detectMultiplier = 1.5f; // 탐지거리 = 공격거리 x detect
    [SerializeField] private float attackCooldown = 1f;  // 공격 간격
    [SerializeField] private float maxChaseRange = 20f;  // 스폰위치 기준 최대 추적거리
    [SerializeField] private float searchDuration = 3f;   // 수색 시간

    [Header("이동 설정")]
    [SerializeField] private float moveSpeed = 6f;

    [Header("순찰")]
    [SerializeField] private Transform[] patrolPoints;   // 순찰 지점들
    private int currentPatrolIndex;

    [Header("이펙트")]
    [SerializeField] private GameObject hitEffectPrefab;    // 피격이펙트

    // 컴포넌트
    private NavMeshAgent agent;
    private Transform player;
    private Animator animator;
    private EnemyEquipment equipment;

    // 무기에서 가져올 거리
    private float detectRange;
    private float attackRange;

    // 상태
    private State currentState = State.Idle;
    private float lastAttackTime;

    // 복귀
    private Vector3 spawnPosition;
    private bool hasSeenPlayer = false;

    // 수색
    private float searchTimer = 0f;
    private Vector3 lastSeenPosition;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        equipment = GetComponent<EnemyEquipment>();
    }

    private void Start()
    {
        // 스폰 위치 저장
        spawnPosition = transform.position;

        // 플레이어 찾기
        player = GameObject.FindGameObjectWithTag("Player")?.transform;

        if (player == null)
        {
            Debug.LogWarning("Player 태그를 가진 오브젝트가 없습니다!");
        }

        // NavMeshAgent 설정
        agent.speed = moveSpeed;
        agent.acceleration = 100f; // 즉시가속
        agent.angularSpeed = 360f; // 빠른회전
        agent.autoBraking = false; // 감속안함 

        // 무기 사거리에서 감지/공격 거리 설정
        SetRangeFromWeapon();

        // 순찰 지점 있으면 순찰 시작
        if (patrolPoints != null && patrolPoints.Length > 0)
        {
            currentState = State.Patrol;
        }
    }

    private void Update()
    {
        if (player == null) return;

        // 속도 강제 유지
        ForceConstantSpeed();

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        // 상태 전환 로직
        UpdateState(distanceToPlayer);

        // 상태별 행동
        ExecuteState(distanceToPlayer);

        if(animator != null)
        {
            bool isMoving = agent.velocity.magnitude > 0.1f;
            animator.SetBool("isMoving", isMoving);
        }
    }

    // 무기 사거리로 거리 설정
    private void SetRangeFromWeapon()
    {
        if(equipment != null)
        {
            WeaponData weaponData = equipment.GetWeaponData();
            if(weaponData != null)
            {
                attackRange = weaponData.range;                     // 공격 거리 = 무기 사거리
                detectRange = weaponData.range * detectMultiplier;
            }
        }

        // 무기 없으면 기본값
        if (attackRange <= 0) attackRange = 10f;
        if (detectRange <= 0) detectRange = 15f;
    }

    private void ForceConstantSpeed()
    {
        if(agent.velocity.magnitude > 0.1f && currentState == State.Chase)
        {
            // 현재 이동 방향 유지하면서 속도만 고정
            agent.velocity = agent.velocity.normalized * moveSpeed;
        }
    }

    private void UpdateState(float distanceToPlayer)
    {
        float distanceFromSpawn = Vector3.Distance(transform.position, spawnPosition);

        switch(currentState)
        {
            case State.Idle:
            case State.Patrol:
                // 탐지 범위 안에 플레이어가 있으면 추적
                if(distanceToPlayer <= detectRange)
                {
                    currentState = State.Chase;
                    hasSeenPlayer = true;
                }
                break;
            case State.Chase:
                // 공격 범위 안이면 공격
                if(distanceToPlayer <= attackRange)
                {
                    currentState = State.Attack;
                }
                else if(distanceToPlayer > detectRange)
                {
                    currentState = State.Search;
                    searchTimer = 0f;
                    lastSeenPosition = player.position;
                    Debug.Log("플레이어 놓침 수색시작");
                }
                // enemy가 최대범위 밖으로 나가면 복귀
                else if (distanceFromSpawn > maxChaseRange)
                {
                    currentState = State.Return;
                    Debug.Log("플레이어가 최대영역 벗어남 복귀시작");
                }
                break;
            case State.Attack:
                // 공격 범위 밖이면 추적
                if(distanceToPlayer > attackRange)
                {
                    currentState = State.Chase;
                }
                // 공격 중에도 영역 체크
                if(distanceFromSpawn > maxChaseRange)
                {
                    currentState = State.Return;
                    Debug.Log("영역벗어남 복귀");
                }
                break;
            case State.Search:
                // 수색 중 다시 발견하면 추적
                if(distanceToPlayer <= detectRange)
                {
                    currentState = State.Chase;
                    Debug.Log("플레이어 재발견 추적 다시 시작");
                }
                else if(searchTimer >= searchDuration)
                {
                    currentState = State.Return;
                    Debug.Log("수색 시간 초과하면 복귀");
                }
                    break;
            case State.Return:
                // 감지 범위 안에 들어오면 추적
                if (distanceToPlayer <= detectRange)
                {
                    currentState = State.Chase;
                    hasSeenPlayer = true;
                    Debug.Log("플레이어 추적 시작");
                }
                break;

        }
    }

    private void ExecuteState(float distanceToPlayer)
    {
        switch (currentState)
        {
            case State.Idle:
                Idle();
                break;
            case State.Patrol:
                Patrol();
                break;
            case State.Chase:
                Chase();
                break;
            case State.Attack:
                Attack();
                break;
            case State.Search:
                Search();
                break;
            case State.Return:
                Return();
                break;
        }
    }

    private void Idle()
    {
        // 대기 - 아무것도 안 함
        agent.SetDestination(transform.position);
    }

    private void Patrol()
    {
        // 순찰 지점 없으면 대기
        if (patrolPoints == null || patrolPoints.Length == 0) return;

        // 현재 목표 지점으로 이동
        Transform targetPoint = patrolPoints[currentPatrolIndex];
        agent.SetDestination(targetPoint.position);

        // 목표 도착했으면 다음 지점으로
        if (Vector3.Distance(transform.position, targetPoint.position) < 1f)
        {
            currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length;
        }
    }

    private void Chase()
    {
        // 플레이어 추적
        agent.SetDestination(player.position);
    }

    private void Attack()
    {
        // 이동 멈춤
        agent.SetDestination(transform.position);

        // 플레이어 바라보기
        Vector3 direction = (player.position - transform.position).normalized;
        direction.y = 0;
        if (direction != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(direction);
        }

        // 재장전 중이면 대기
        if(equipment != null && equipment.IsReloading())
        {
            return;
        }

        // 탄약 없으면 재장전
        if(equipment != null && equipment.NeedsReload())
        {
            equipment.StartReload();
            return;
        }

        // 공격 쿨다운 체크
        if (Time.time >= lastAttackTime + attackCooldown)
        {
            Fire();
            lastAttackTime = Time.time;
        }
    }

    private void Search()
    {
        // 제자리 멈춤
        agent.SetDestination(transform.position);

        // 타이머 증가
        searchTimer += Time.deltaTime;

        // 주변 둘러보기
        float lookSpeed = 30f;
        transform.Rotate(0f, lookSpeed * Time.deltaTime, 0f);
    }
    
    // 플레이어 놓치면 복귀
    private void Return()
    {
        agent.SetDestination(spawnPosition);

        // 스폰 위치 도착하면 Idle 또는 patrol로 상태 변경
        float distanceSpawn = Vector3.Distance(transform.position, spawnPosition);
        if(distanceSpawn < 1f)
        {
            hasSeenPlayer = false;

            if(patrolPoints != null && patrolPoints.Length > 0)
            {
                currentState = State.Patrol;
                Debug.Log("복귀, 순찰상태");
            }
            else
            {
                currentState = State.Idle;
                Debug.Log("복귀, 대기상태");
            }
        }

    }

    // 총 발사
    private void Fire()
    {
        if (equipment == null) return;
        if (!equipment.CanFire()) return;

        WeaponData weaponData = equipment.GetWeaponData();
        Transform firePoint = equipment.GetFirePoint();

        if (weaponData == null || firePoint == null)
        {
            Debug.Log("무기 데이터 또는 총구 위치 없음");
            return;
        }

        equipment.UseAmmo();
        equipment.PlayMuzzleFlash();
        equipment.PlayFireSound();

        // 플레이어 Collider 중심으로 조준
        Vector3 targetPosition;
        Collider playerCollider = player.GetComponent<Collider>();

        if (playerCollider != null)
        {
            targetPosition = playerCollider.bounds.center;
        }
        else
        {
            targetPosition = player.position + Vector3.up * 1f;
        }

        Vector3 direction = (targetPosition - firePoint.position).normalized;

        Debug.DrawRay(firePoint.position, direction * weaponData.range, Color.red, 1f);

        // 모든 맞은 것 확인
        RaycastHit[] hits = Physics.RaycastAll(firePoint.position, direction, weaponData.range);

        // 거리순 정렬! (가까운 것부터)
        System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

        foreach (RaycastHit hit in hits)
        {
            // 자기 자신 무시
            if (hit.collider.transform.root == transform.root) continue;

            Debug.Log("Ray 맞음: " + hit.collider.name + " / Tag: " + hit.collider.tag + " / 거리: " + hit.distance);

            // 플레이어 맞으면 데미지
            if (hit.collider.CompareTag("Player"))
            {
                if (hitEffectPrefab != null)
                {
                    GameObject effect = Instantiate(hitEffectPrefab, hit.point, Quaternion.identity);
                    Destroy(effect, 0.5f);
                }

                Health health = hit.collider.GetComponent<Health>();
                if (health != null)
                {
                    health.TakeDamage(weaponData.damage);
                    Debug.Log("적이 플레이어 공격! 데미지: " + weaponData.damage);
                }
                return;  // 플레이어 맞으면 종료
            }

            // Trigger는 무시하고 계속
            if (hit.collider.isTrigger) continue;

            // 다른 장애물 맞으면 (벽 등) 멈춤
            Debug.Log("장애물에 막힘: " + hit.collider.name);
            return;
        }
    }



    // Scene 뷰에서 범위 표시 (디버그용)
    private void OnDrawGizmosSelected()
    {
        // 탐지 범위 (노란색)
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectRange);

        // 공격 범위 (빨간색)
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        // 최대 추적 범위 (파란색)
        Gizmos.color = Color.blue;
        if(Application.isPlaying)
        {
            Gizmos.DrawWireSphere(spawnPosition, maxChaseRange);
        }
        else
        {
            Gizmos.DrawWireSphere(transform.position, maxChaseRange);
        }

        // 스폰 위치 (초록색)
        if (Application.isPlaying)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(spawnPosition, 0.5f);
        }
    }
}
