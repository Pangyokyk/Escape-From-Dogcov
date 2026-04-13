using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Interactable : MonoBehaviour
{
    [Header("상호작용 설정")]
    [SerializeField] private string interactText = "상호작용";
    [SerializeField] private Vector3 promptOffset = new Vector3(0, 2f, 0);

    [Header("프롬프트")]
    [SerializeField] private GameObject promptPrefab;

    private GameObject promptInstance;
    private TextMeshProUGUI promptTextUI;

    public string GetInteractText()
    {
        return interactText;
    }

    public void ShowPrompt()
    {
        if (promptInstance == null && promptPrefab != null)
        {
            promptInstance = Instantiate(promptPrefab, transform.position + promptOffset, Quaternion.identity);
            promptTextUI = promptInstance.GetComponentInChildren<TextMeshProUGUI>();
        }

        if (promptInstance != null)
        {
            promptInstance.SetActive(true);
            promptInstance.transform.position = transform.position + promptOffset;

            // 카메라 바라보기
            if (Camera.main != null)
            {
                promptInstance.transform.LookAt(Camera.main.transform);
                promptInstance.transform.Rotate(0, 180, 0);
            }

            if (promptTextUI != null)
            {
                promptTextUI.text = "[F] " + interactText;
            }
        }
    }

    public void HidePrompt()
    {
        if (promptInstance != null)
        {
            promptInstance.SetActive(false);
        }
    }

    public virtual void Interact()
    {
        // 자식 클래스에서 구현
    }

    private void OnDestroy()
    {
        if (promptInstance != null)
        {
            Destroy(promptInstance);
        }
    }
}
