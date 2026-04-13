using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RaidInteractable : Interactable
{
    [Header("UI 참조")]
    [SerializeField] private GameObject raidPanel;

    public override void Interact()
    {
        if (UIManager.Instance != null)
        {
            UIManager.Instance.CloseAllPanels();
        }

        if (raidPanel != null)
        {
            raidPanel.SetActive(true);
        }
    }

    public void CloseRaidPanel()
    {
        if (raidPanel != null)
        {
            raidPanel.SetActive(false);
        }
    }
}
