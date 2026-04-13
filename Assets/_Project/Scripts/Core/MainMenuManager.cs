using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    public void StartNewGame()
    {
        // 세이브 삭제 후 시작
        PlayerPrefs.DeleteAll();
        SceneManager.LoadScene("Hideout");
    }

    public void ContinueGame()
    {
        // 저장된 게임 불러오기
        SceneManager.LoadScene("Hideout");

        // Hideout에서 SaveManager가 자동으로 Load
    }

    public void QuitGame()
    {
        Debug.Log("게임 종료");
        Application.Quit();
    }
}
