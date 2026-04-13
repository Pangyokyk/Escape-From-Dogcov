using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerSceneSetup : MonoBehaviour
{
    [SerializeField] private HideoutPlayer hideoutPlayer;
    [SerializeField] private PlayerInteraction playerInteraction;

    private void Start()
    {
        SetupForCurrentScene();
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        SetupForCurrentScene();
    }

    private void SetupForCurrentScene()
    {
        string sceneName = SceneManager.GetActiveScene().name;

        if(sceneName == "Hideout")
        {
            if(hideoutPlayer != null)
            {
                hideoutPlayer.enabled = true;
            }

            if(playerInteraction != null)
            {
                playerInteraction.enabled = false;
            }
            Debug.Log("Hideout 모드: HideoutPlayer 활성화");
        }
        else if(sceneName == "Raid")
        {
            if (hideoutPlayer != null)
            {
                hideoutPlayer.enabled = false;
            }

            if (playerInteraction != null)
            {
                playerInteraction.enabled = true;
            }
            Debug.Log("Raid 모드: PlayerInteraction 활성화");
        }
    }
}
