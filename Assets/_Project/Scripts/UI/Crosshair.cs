using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Crosshair : MonoBehaviour
{
    public static Crosshair Instance { get; private set; }

    [Header("크로스헤어 파츠")]
    [SerializeField] private RectTransform top;
    [SerializeField] private RectTransform bottom;
    [SerializeField] private RectTransform left;
    [SerializeField] private RectTransform right;

    [Header("기본 설정")]
    [SerializeField] private float defaultSpread = 15f;     // 기본 간격
    [SerializeField] private float maxSpread = 50f;         // 최대 벌어짐

    [Header("애니메이션")]
    [SerializeField] private float spreadSpeed = 10f;       // 벌어지는 속도
    [SerializeField] private float returnSpeed = 5f;        // 복귀 속도

    [Header("반동")]
    [SerializeField] private float recoilReturnSpeed = 5f;

    private float currentSpread;
    private float targetSpread;
    private Vector3 currentRecoil = Vector3.zero;
    private Vector3 targetRecoil = Vector3.zero;

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
        Debug.Log("Crosshair Start 호출됨!");
        HideCursor();
        currentSpread = defaultSpread;
        targetSpread = defaultSpread;
        UpdateCrosshairSpread();
    }

    private void Update()
    {
        if (!Cursor.visible)
        {
            // 반동 처리
            currentRecoil = Vector3.Lerp(currentRecoil, targetRecoil, spreadSpeed * Time.deltaTime);
            targetRecoil = Vector3.Lerp(targetRecoil, Vector3.zero, recoilReturnSpeed * Time.deltaTime);

            // 마우스 위치 + 반동
            transform.position = Input.mousePosition + currentRecoil;

            // 크로스헤어 벌어짐 처리
            currentSpread = Mathf.Lerp(currentSpread, targetSpread, spreadSpeed * Time.deltaTime);
            targetSpread = Mathf.Lerp(targetSpread, defaultSpread, returnSpeed * Time.deltaTime);

            UpdateCrosshairSpread();
        }
    }

    // 크로스헤어 벌어짐 업데이트
    private void UpdateCrosshairSpread()
    {
        if (top != null) top.anchoredPosition = new Vector2(0, currentSpread);
        if (bottom != null) bottom.anchoredPosition = new Vector2(0, -currentSpread);
        if (left != null) left.anchoredPosition = new Vector2(-currentSpread, 0);
        if (right != null) right.anchoredPosition = new Vector2(currentSpread, 0);
    }

    // 반동 적용 (Gun에서 호출)
    public void ApplyRecoil(float amount)
    {
        // 크로스헤어 위치 반동
        float recoilX = Random.Range(-amount * 30f, amount * 30f);
        float recoilY = amount * 80f;
        targetRecoil += new Vector3(recoilX, recoilY, 0f);

        // 크로스헤어 벌어짐
        targetSpread += amount * 100f;
        targetSpread = Mathf.Clamp(targetSpread, defaultSpread, maxSpread);
    }

    public void ShowCursor()
    {
        Cursor.visible = true;
        gameObject.SetActive(false);
    }

    public void HideCursor()
    {
        Cursor.visible = false;
        gameObject.SetActive(true);
    }

    // 현재 크로스헤어 스크린 위치 반환
    public Vector3 GetScreenPosition()
    {
        return transform.position;
    }   

    private void OnDisable()
    {

    }

    private void OnDestroy()
    {
        // 인스턴트 정리
        if (Instance == this)
        {
            Instance = null;
        }

    }
}
