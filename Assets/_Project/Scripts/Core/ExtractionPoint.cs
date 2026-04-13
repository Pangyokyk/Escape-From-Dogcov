using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExtractionPoint : MonoBehaviour
{
    [Header("추출 설정")]
    [SerializeField] private float extractionTime = 5f;

    [Header("UI 참조")]
    [SerializeField] private GameObject extractionUI;
    [SerializeField] private UnityEngine.UI.Image progressFill;

    // 상태
    private bool playerInZone;
    private bool isExtracting;
    private Coroutine extractionCoroutine;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInZone = true;
            extractionUI.SetActive(true);
            StartExtraction();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInZone = false;
            extractionUI.SetActive(false);
            CancelExtraction();
        }
    }

    private void StartExtraction()
    {
        if (extractionCoroutine != null)
        {
            StopCoroutine(extractionCoroutine);
        }
        extractionCoroutine = StartCoroutine(ExtractionRoutine());
    }

    private void CancelExtraction()
    {
        if (extractionCoroutine != null)
        {
            StopCoroutine(extractionCoroutine);
            extractionCoroutine = null;
        }
        isExtracting = false;

        if (progressFill != null)
        {
            progressFill.fillAmount = 0f;
        }
    }

    private IEnumerator ExtractionRoutine()
    {
        isExtracting = true;
        float elapsed = 0f;

        while (elapsed < extractionTime)
        {
            elapsed += Time.deltaTime;

            if (progressFill != null)
            {
                progressFill.fillAmount = elapsed / extractionTime;
            }

            yield return null;
        }

        // 추출 성공!
        ExtractionSuccess();
    }

    private void ExtractionSuccess()
    {
        Debug.Log("추출 성공!");

        // GameManager에게 알리기
        GameManager gameManager = FindObjectOfType<GameManager>();
        if (gameManager != null)
        {
            gameManager.OnExtractionSuccess();
        }
    }
}
