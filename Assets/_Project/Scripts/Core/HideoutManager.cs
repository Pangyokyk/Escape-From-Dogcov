using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class HideoutManager : MonoBehaviour
{
    public void StartRaid()
    {
        // 모든 UI 닫기
        if (UIManager.Instance != null)
        {
            UIManager.Instance.CloseAllPanels();
        }

        Cursor.visible = false;

        if (GameData.Instance != null)
        {
            GameData.Instance.StartRaid();
        }

        if (PlayerData.Instance != null)
        {
            PlayerData.Instance.StartRaid();
        }

        SceneManager.LoadScene("Raid");
    }

    private void CloseAllPanels()
    {
        // UIManager로 닫기
        if (UIManager.Instance != null)
        {
            UIManager.Instance.CloseAllPanels();
        }

        // 또는 개별로 닫기
        StashUI stashUI = FindObjectOfType<StashUI>();
        if (stashUI != null) stashUI.CloseStash();

        InventoryUI inventoryUI = FindObjectOfType<InventoryUI>();
        if (inventoryUI != null) inventoryUI.CloseInventory();

        // 툴팁 숨기기
        if (TooltipUI.Instance != null)
        {
            TooltipUI.Instance.Hide();
        }
    }

}
