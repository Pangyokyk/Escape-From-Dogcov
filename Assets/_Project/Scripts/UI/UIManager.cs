using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("모든 패널")]
    [SerializeField] private GameObject[] panels;

    [Header("ESC설정 패널")]
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private GameObject saveAndMainMenuButton;
    [SerializeField] private GameObject surrenderButton;
    [SerializeField] private GameObject quitButton;

    private PlayerInputActions inputActions;
    private bool isPaused = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

            inputActions = new PlayerInputActions();
    }

    private void OnEnable()
    {
        if (inputActions == null) return;

        inputActions.Player.Enable();
        inputActions.Player.Cancel.performed += OnCancel;
    }

    private void OnDisable()
    {
        if (inputActions == null) return;

        inputActions.Player.Disable();
        inputActions.Player.Cancel.performed -= OnCancel;
    }

    private void OnCancel(InputAction.CallbackContext context)
    {
        Debug.Log("=== OnCancel 호출 ===");
        Debug.Log("현재 씬: " + SceneManager.GetActiveScene().name);
        Debug.Log("isPaused: " + isPaused);
        Debug.Log("IsAnyPanelOpen: " + IsAnyPanelOpen());

        if (SceneManager.GetActiveScene().name == "MainMenu")
            return;

        if (isPaused)
        {
            Debug.Log("→ ResumeGame 실행");
            ResumeGame();
            return;
        }

        if (IsAnyPanelOpen())
        {
            Debug.Log("→ CloseAllPanels 실행");
            CloseAllPanels();
            return;
        }

        Debug.Log("→ PauseGame 실행");
        PauseGame();
    }

    public void CloseAllPanels()
    {
        bool anyClosed = false;

        foreach (GameObject panel in panels)
        {
            if (panel != null && panel.activeSelf)
            {
                panel.SetActive(false);
                anyClosed = true;
            }
        }

        if(TooltipUI.Instance != null)
        {
            TooltipUI.Instance.Hide();
        }

        // 패널 닫으면 마우스 숨기기
        if (anyClosed && Crosshair.Instance != null)
        {
            Crosshair.Instance.HideCursor();
        }
    }

    public bool IsAnyPanelOpen()
    {
        foreach (GameObject panel in panels)
        {
            if (panel != null && panel.activeSelf)
            {
                return true;
            }
        }
        return false;
    }

    // 일시정지기능
    private void PauseGame()
    {
        Debug.Log("PauseGame 시작");

        isPaused = true;
        pausePanel.SetActive(true);

        Debug.Log("pausePanel.activeSelf: " + pausePanel.activeSelf);
        Debug.Log("pausePanel 위치: " + pausePanel.transform.parent?.name);

        Time.timeScale = 0f;

        SetupButtonsForScene();

        if (Crosshair.Instance != null)
        {
            Crosshair.Instance.ShowCursor();
        }

        Debug.Log("PauseGame 끝");
    }

    public void ResumeGame()
    {
        Debug.Log("ResumeGame 호출됨!");

        isPaused = false;
        pausePanel.SetActive(false);
        Time.timeScale = 1f;

        if (Crosshair.Instance != null)
        {
            Crosshair.Instance.HideCursor();
        }
    }

    private void SetupButtonsForScene()
    {
        string sceneName = SceneManager.GetActiveScene().name;

        if(sceneName == "Hideout")
        {
            saveAndMainMenuButton.SetActive(true);
            surrenderButton.SetActive(false);
        }
        else if(sceneName == "Raid")
        {
            saveAndMainMenuButton.SetActive(false);
            surrenderButton.SetActive(true);
        }

        quitButton.SetActive(true);
    }

    // 버튼 함수들
    public void OnResumeButton()
    {
        ResumeGame();
    }

    // 저장하고 메인메뉴로 나가기(Hideout씬에서 보이는버튼)
    public void OnSaveAndMainMenu()
    {
        
        if(SaveManager.Instance != null)
        {
            SaveManager.Instance.SaveGame();
        }

        Time.timeScale = 1f;
        isPaused = false;
        SceneManager.LoadScene("MainMenu");
    }

    // 항복버튼, Raid씬에서만 보이는 버튼
    public void OnSurrender()
    {
        if(GameData.Instance != null)
        {
            GameData.Instance.PlayerDied();
        }

        // 장비/무기 손실
        if (PlayerData.Instance != null)
        {
            PlayerData.Instance.OnPlayerDeath();
        }

        Time.timeScale = 1f;
        isPaused = false;
        SceneManager.LoadScene("Hideout");
    }

    // 게임 완전종료 버튼
    public void OnQuitGame()
    {
        if(SaveManager.Instance != null)
        {
            SaveManager.Instance.SaveGame();
        }
        Debug.Log("게임종료");
        Application.Quit();

    #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
    #endif
    }

    public bool IsPaused()
    {
        return isPaused;
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }
}
