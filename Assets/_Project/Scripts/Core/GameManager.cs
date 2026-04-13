using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [Header("UI 참조")]
    [SerializeField] private GameObject successUI;
    [SerializeField] private GameObject failUI;

    // 추출 성공
    public void OnExtractionSuccess()
    {
        Debug.Log("추출 성공!");

        // 아이템 창고로 이동 - 이 부분 추가!
        if (GameData.Instance != null)
        {
            GameData.Instance.ExtractionSuccess();
        }

        if (successUI != null)
        {
            successUI.SetActive(true);
        }

        Time.timeScale = 0f;
    }

    // 플레이어 사망
    public void OnPlayerDeath()
    {
        Debug.Log("게임 오버!");

        // 장비 손실
        if (PlayerData.Instance != null)
        {
            PlayerData.Instance.OnPlayerDeath();
        }

        // 인벤토리 손실
        if (GameData.Instance != null)
        {
            GameData.Instance.PlayerDied();
        }

        // GameInfoUI 끄기
        if(GameUI.Instance != null)
        {
            GameUI.Instance.gameObject.SetActive(false);
        }

        if (failUI != null)
        {
            failUI.SetActive(true);
        }

        Time.timeScale = 0f;
    }
    
    public void OnFailButtonClick()
    {
        // 레이드 아이템 손실
        if(GameData.Instance != null)
        {
            GameData.Instance.PlayerDied();
        }

        // 장비/무기 손실
        if(PlayerData.Instance != null)
        {
            PlayerData.Instance.OnPlayerDeath();
        }

        Debug.Log("레이드하다가 죽으면 모든 아이템 손실후 복귀");

        Time.timeScale = 1f;
        SceneManager.LoadScene("Hideout");
    }

    // 재시작
    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    // 기지로 돌아가기 (나중에 구현)
    public void ReturnToHideout()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("Hideout");
    }
}
