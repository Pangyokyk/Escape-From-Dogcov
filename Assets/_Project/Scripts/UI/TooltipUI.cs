using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TooltipUI : MonoBehaviour
{
    public static TooltipUI Instance { get; private set; }

    [Header("UI 참조")]
    [SerializeField] private GameObject tooltipPanel;
    [SerializeField] private TextMeshProUGUI itemNameText;
    [SerializeField] private TextMeshProUGUI itemTypeText;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private TextMeshProUGUI statsText;

    [Header("설정")]
    [SerializeField] private Vector2 offset = new Vector2(100f, 0f);

    private RectTransform tooltipRect;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            // 루트 canvas의 sortingOrder 높이기
            Canvas rootCanvas = transform.root.GetComponent<Canvas>();
            if(rootCanvas != null )
            {
                rootCanvas.sortingOrder = 999;
            }
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        tooltipRect = tooltipPanel.GetComponent<RectTransform>();
        Hide();
    }

    private void Update()
    {
        if (tooltipPanel.activeSelf)
        {
            UpdatePosition();
        }
    }

    public void Show(ItemData item)
    {
        if (item == null) return;

        if (itemNameText != null)
            itemNameText.text = item.itemName;

        if (itemTypeText != null)
            itemTypeText.text = GetTypeText(item.itemType);

        if (descriptionText != null)
            descriptionText.text = item.description;

        if (statsText != null)
            statsText.text = GetStatsText(item);

        tooltipPanel.SetActive(true);

        // 크기 갱신 후 위치 설정
        LayoutRebuilder.ForceRebuildLayoutImmediate(tooltipRect);
        UpdatePosition();
    }

    public void Hide()
    {
        if (tooltipPanel != null)
        {
            tooltipPanel.SetActive(false);
        }
    }

    private void UpdatePosition()
    {
        Vector2 mousePos = Input.mousePosition;

        float tooltipWidth = tooltipRect.sizeDelta.x;
        float tooltipHeight = tooltipRect.sizeDelta.y;

        // 기본: 마우스 오른쪽에 표시
        float x = mousePos.x + offset.x;
        float y = mousePos.y - offset.y - tooltipHeight;

        // 오른쪽 넘어가면 왼쪽에 표시
        if (x + tooltipWidth > Screen.width)
        {
            x = mousePos.x - tooltipWidth + 100f;
        }

        // 왼쪽 넘어가면 조정
        if (x < 0)
        {
            x = 0;
        }

        // 아래로 넘어가면 위에 표시
        if (y < 0)
        {
            y = mousePos.y + offset.y;
        }

        // 위로 넘어가면 조정
        if (y + tooltipHeight > Screen.height)
        {
            y = Screen.height - tooltipHeight;
        }

        tooltipPanel.transform.position = new Vector3(x, y, 0);
    }

    private string GetTypeText(ItemData.ItemType type)
    {
        switch (type)
        {
            case ItemData.ItemType.Weapon: return "무기";
            case ItemData.ItemType.Ammo: return "탄약";
            case ItemData.ItemType.Medical: return "의료품";
            case ItemData.ItemType.Valuable: return "귀중품";
            case ItemData.ItemType.Helmet: return "헬멧";
            case ItemData.ItemType.Armor: return "방탄조끼";
            default: return "기타";
        }
    }

    private string GetStatsText(ItemData item)
    {
        string stats = "";

        stats += $"무게: {item.weight:F1}kg\n";
        stats += $"가격: {item.price}₩";

        switch (item.itemType)
        {
            case ItemData.ItemType.Medical:
                stats += $"\n회복량: {item.healAmount:F0}";
                if (item.maxUses > 1)
                    stats += $"\n사용 횟수: {item.maxUses}회";
                break;

            case ItemData.ItemType.Weapon:
                if (item.weaponData != null)
                {
                    stats += $"\n데미지: {item.weaponData.damage}";
                    stats += $"\n장탄수: {item.weaponData.magazineSize}";
                }
                break;

            case ItemData.ItemType.Helmet:
                stats += $"\n방어력: {item.armorValue}";
                stats += $"\n체력 보너스: +{item.healthBonus}";
                break;
            case ItemData.ItemType.Armor:
                stats += $"\n방어력: {item.armorValue}";
                stats += $"\n체력 보너스: +{item.healthBonus}";
                // 내구도 표시
                if(PlayerData.Instance != null && PlayerData.Instance.armorName == item.itemName)
                {
                    // 장착된 갑옷 현재 내구도 표시
                    stats += $"\n내구도: {PlayerData.Instance.armorCurrentDurability:F1} / {PlayerData.Instance.armorMaxDurability:F1}";
                }
                else
                {
                    // 인벤토리와 상점에서의 갑옷 - 최대내구도 표시
                    stats += $"\n내구도: {item.durability:F1} / {item.durability:F1}";
                }
                    break;
        }

        return stats;
    }
}