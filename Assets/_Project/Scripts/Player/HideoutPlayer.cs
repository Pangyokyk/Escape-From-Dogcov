using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class HideoutPlayer : MonoBehaviour
{
    [Header("상호작용 설정")]
    [SerializeField] private float interactRange = 2f;
    [SerializeField] private LayerMask interactLayer;

    [Header("드롭 아이템")]
    [SerializeField] private LayerMask droppedItemLayer;
    private DroppedItem currentDroppedItem;

    private PlayerInputActions inputActions;
    private Interactable currentInteractable;
    private Interactable lastInteractable;

    private void Awake()
    {
        inputActions = new PlayerInputActions();
    }

    private void OnEnable()
    {
        inputActions.Player.Enable();
        inputActions.Player.Interact.performed += OnInteract;
    }

    private void OnDisable()
    {
        inputActions.Player.Disable();
        inputActions.Player.Interact.performed -= OnInteract;
    }

    private void Update()
    {
        CheckForInteractable();
        CheckForDroppedItem();
    }

    private void CheckForInteractable()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, interactRange, interactLayer);

        if (colliders.Length > 0)
        {
            Interactable interactable = colliders[0].GetComponent<Interactable>();
            if (interactable != null)
            {
                // 새 상호작용 대상
                if (currentInteractable != interactable)
                {
                    // 이전 대상 숨기기
                    if (currentInteractable != null)
                    {
                        currentInteractable.HidePrompt();
                    }

                    currentInteractable = interactable;
                    currentInteractable.ShowPrompt();
                }
                return;
            }
        }

        // 범위 벗어남
        if (currentInteractable != null)
        {
            currentInteractable.HidePrompt();
            currentInteractable = null;
        }
    }

    private void CheckForDroppedItem()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, interactRange, droppedItemLayer);

        DroppedItem closest = null;
        float closestDist = float.MaxValue;

        foreach (Collider col in colliders)
        {
            DroppedItem dropped = col.GetComponent<DroppedItem>();
            if (dropped != null)
            {
                float dist = Vector3.Distance(transform.position, col.transform.position);
                if (dist < closestDist)
                {
                    closestDist = dist;
                    closest = dropped;
                }
            }
        }

        if (currentDroppedItem != null && currentDroppedItem != closest)
        {
            currentDroppedItem.HidePrompt();
        }

        currentDroppedItem = closest;

        if (currentDroppedItem != null)
        {
            currentDroppedItem.ShowPrompt();
        }
    }

    private void OnInteract(InputAction.CallbackContext context)
    {
        // 드랍 아이템 먼저 추가
        if (currentDroppedItem != null)
        {
            currentDroppedItem.PickUp();
            currentDroppedItem = null;
            return;
        }

        if (currentInteractable != null)
        {
            currentInteractable.Interact();
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactRange);
    }
}
