using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("이동")]
    [SerializeField] private float baseMoveSpeed = 6f;
    [SerializeField] private float rotationSpeed = 15f;

    [Header("참조")]
    [SerializeField] private Camera mainCamera;
    [SerializeField] private LayerMask groundLayer;

    // 컴포넌트
    private CharacterController controller;
    private PlayerInputActions inputActions;
    private Animator animator;

    // 상태
    private Vector2 moveInput;
    private Vector3 velocity;  // 추가

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        inputActions = new PlayerInputActions();
        animator = GetComponent<Animator>();

        if (mainCamera == null)
            mainCamera = Camera.main;
    }

    private void OnEnable()
    {
        inputActions.Player.Enable();
    }

    private void OnDisable()
    {
        inputActions.Player.Disable();
    }

    private void Update()
    {
        // 입력 읽기
        moveInput = inputActions.Player.Move.ReadValue<Vector2>();
        //이동
        Move();
        // 마우스방향으로 회전
        RotateTowardsMouse();
    }

    private void Move()
    {
        // 카메라 기준 방향 계산(쿼터뷰에서 중요함)
        Vector3 cameraForward = mainCamera.transform.forward;
        Vector3 cameraRight = mainCamera.transform.right;

        // Y축 제거 (수평 이동만)
        cameraForward.y = 0f;
        cameraRight.y = 0f;
        cameraForward.Normalize();
        cameraRight.Normalize();

        // 수평 이동 (이동 속도 적용)
        Vector3 moveDirection = (cameraForward * moveInput.y + cameraRight * moveInput.x);

        // 무게에 따른 이동속도 계산
        float currentMoveSpeed = baseMoveSpeed;
        if(PlayerData.Instance != null)
        {
            currentMoveSpeed = baseMoveSpeed * PlayerData.Instance.GetSpeedMultiplier();
        }
        Vector3 horizontalMove = moveDirection * currentMoveSpeed;

        // 중력 (별도 처리)
        if (controller.isGrounded)
        {
            velocity.y = -2f;
        }
        else
        {
            velocity.y += -9.81f * Time.deltaTime;
        }

        bool isMoving = moveInput.sqrMagnitude > 0.01f;
        if(animator != null)
        {
            animator.SetBool("isRunning", isMoving);
        }

        // 최종 이동 (한 번만 deltaTime 적용)
        controller.Move((horizontalMove + velocity) * Time.deltaTime);
    }

    private void RotateTowardsMouse()
    {
        Vector2 mouseScreenPos = inputActions.Player.Look.ReadValue<Vector2>();
        Ray ray = mainCamera.ScreenPointToRay(mouseScreenPos);

        if (Physics.Raycast(ray, out RaycastHit hit, 100f, groundLayer))
        {
            Vector3 direction = hit.point - transform.position;
            direction.y = 0f;

            if (direction.sqrMagnitude > 0.01f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Slerp(
                    transform.rotation,
                    targetRotation,
                    rotationSpeed * Time.deltaTime
                );
            }
        }
    }
}
