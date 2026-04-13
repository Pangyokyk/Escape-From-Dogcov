using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Exercise : MonoBehaviour
{
    [Header("추적 대상")]
    [SerializeField] private Transform target;

    [Header("카메라 설정")]
    [SerializeField] private Vector3 offset = new Vector3(0f, 15f, -15f);
    [SerializeField] private float smoothspeed = 5f;

    private void LateUpdate()
    {
        if (target != null) return;

        Vector3 desirePosition = transform.position + offset;

        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desirePosition, smoothspeed);

        transform.position = smoothedPosition;
    }

}
