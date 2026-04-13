using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StashInteractable : Interactable
{
    [Header("UI 참조")]
    [SerializeField] private StashUI stashUI;

    public override void Interact()
    {
        if(stashUI != null)
        {
            stashUI.OpenStash();
        }
    }

    public void CloseStash()
    {
        if(stashUI != null)
        {
            stashUI.CloseStash();
        }
    }
}
