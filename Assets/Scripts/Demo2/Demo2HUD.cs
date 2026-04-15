using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// HUD：顯示分數、物品數、HP/MP 條。
/// </summary>
public class Demo2HUD : MonoBehaviour
{
    public static Demo2HUD Instance { get; private set; }

    private Text _scoreText;
    private Text _itemCountText;
    private Text _detailText;
    private Image _hpFill;
    private Image _mpFill;
    private Text _hpText;
    private Text _mpText;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(this); return; }
        Instance = this;
    }

    public void Init(Text scoreText, Text itemCountText, Text detailText,
                     Image hpFill, Image mpFill, Text hpText, Text mpText)
    {
        _scoreText = scoreText;
        _itemCountText = itemCountText;
        _detailText = detailText;
        _hpFill = hpFill;
        _mpFill = mpFill;
        _hpText = hpText;
        _mpText = mpText;

        if (InventoryManager.Instance != null)
            InventoryManager.Instance.OnInventoryChanged += Refresh;
        if (PlayerStats.Instance != null)
            PlayerStats.Instance.OnStatsChanged += Refresh;

        Refresh();
    }

    private void OnDestroy()
    {
        if (InventoryManager.Instance != null)
            InventoryManager.Instance.OnInventoryChanged -= Refresh;
        if (PlayerStats.Instance != null)
            PlayerStats.Instance.OnStatsChanged -= Refresh;
    }

    private void Refresh()
    {
        var inv = InventoryManager.Instance;
        if (inv != null)
        {
            _scoreText.text = $"分數: {inv.TotalScore}";
            _itemCountText.text = $"物品: {inv.TotalItems}";

            // 各分類計數摘要
            int currency = inv.GetSlotsByCategory("Currency").Count;
            int consumable = inv.GetSlotsByCategory("Consumable").Count;
            int quest = inv.GetSlotsByCategory("Quest").Count;
            int collection = inv.GetSlotsByCategory("Collection").Count;
            _detailText.text =
                $"貨幣×{currency}  消耗品×{consumable}  " +
                $"任務×{quest}  收藏品×{collection}";
        }

        var stats = PlayerStats.Instance;
        if (stats != null)
        {
            float hpRatio = (float)stats.CurrentHP / stats.MaxHP;
            float mpRatio = (float)stats.CurrentMP / stats.MaxMP;
            _hpFill.fillAmount = hpRatio;
            _mpFill.fillAmount = mpRatio;
            _hpText.text = $"{stats.CurrentHP}/{stats.MaxHP}";
            _mpText.text = $"{stats.CurrentMP}/{stats.MaxMP}";
        }
    }
}
