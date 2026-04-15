using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

/// <summary>
/// 背包 UI 面板：按 B 鍵開關，顯示所有道具，
/// 可使用消耗品，點擊任務/收藏品道具顯示詳情。
/// </summary>
public class InventoryUI : MonoBehaviour
{
    public static InventoryUI Instance { get; private set; }

    private GameObject _panel;
    private Transform _slotContainer;
    private GameObject _detailPanel;
    private Text _detailName;
    private Text _detailDesc;
    private Image _detailIcon;
    private Text _detailAction;
    private Button _useButton;

    // 分頁
    private string _currentTab = "All";
    private readonly List<Button> _tabButtons = new();
    private readonly List<GameObject> _slotObjects = new();

    private InventoryManager.ItemSlot _selectedSlot;
    private bool _isOpen;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(this); return; }
        Instance = this;
    }

    public void Init(RectTransform canvasRT)
    {
        BuildPanel(canvasRT);
        _panel.SetActive(false);

        if (InventoryManager.Instance != null)
            InventoryManager.Instance.OnInventoryChanged += RefreshIfOpen;
    }

    private void OnDestroy()
    {
        if (InventoryManager.Instance != null)
            InventoryManager.Instance.OnInventoryChanged -= RefreshIfOpen;
    }

    private void Update()
    {
        if (Keyboard.current != null && Keyboard.current.bKey.wasPressedThisFrame)
            Toggle();
    }

    public void Toggle()
    {
        _isOpen = !_isOpen;
        _panel.SetActive(_isOpen);
        if (_isOpen) RefreshSlots();
    }

    // ──────────────── Build UI ────────────────

    private void BuildPanel(RectTransform canvasRT)
    {
        // 半透明背景遮罩
        _panel = new GameObject("InventoryPanel");
        _panel.transform.SetParent(canvasRT, false);
        var panelRT = _panel.AddComponent<RectTransform>();
        panelRT.anchorMin = Vector2.zero;
        panelRT.anchorMax = Vector2.one;
        panelRT.offsetMin = Vector2.zero;
        panelRT.offsetMax = Vector2.zero;

        var panelImg = _panel.AddComponent<Image>();
        panelImg.color = new Color(0, 0, 0, 0.7f);
        panelImg.raycastTarget = true;

        // 主容器
        var container = CreateRect(_panel.transform, "Container",
            new Vector2(0.1f, 0.08f), new Vector2(0.9f, 0.92f));
        var containerImg = container.gameObject.AddComponent<Image>();
        containerImg.color = new Color(0.12f, 0.14f, 0.2f, 0.95f);

        // 標題
        var title = CreateText(container, "Title", "背包 (B 鍵關閉)",
            new Vector2(0, 0.92f), new Vector2(1, 1f), 24);
        title.alignment = TextAnchor.MiddleCenter;

        // 分頁按鈕列
        BuildTabs(container);

        // 道具格子區（ScrollRect）
        var scrollArea = BuildScrollArea(container);
        _slotContainer = scrollArea;

        // 右側詳情面板
        BuildDetailPanel(container);

        // 關閉按鈕
        BuildCloseButton(container);
    }

    private void BuildTabs(RectTransform parent)
    {
        var tabBar = CreateRect(parent, "TabBar",
            new Vector2(0.02f, 0.84f), new Vector2(0.58f, 0.91f));

        string[] tabs = { "All", "Consumable", "Quest", "Collection", "Currency" };
        string[] labels = { "全部", "消耗品", "任務", "收藏品", "貨幣" };

        for (int i = 0; i < tabs.Length; i++)
        {
            float xMin = i / (float)tabs.Length;
            float xMax = (i + 1) / (float)tabs.Length;

            var btnRT = CreateRect(tabBar, $"Tab_{tabs[i]}",
                new Vector2(xMin + 0.005f, 0.05f), new Vector2(xMax - 0.005f, 0.95f));

            var btnImg = btnRT.gameObject.AddComponent<Image>();
            btnImg.color = new Color(0.2f, 0.25f, 0.35f);

            var btnText = CreateText(btnRT, "Label", labels[i],
                Vector2.zero, Vector2.one, 14);
            btnText.alignment = TextAnchor.MiddleCenter;

            var btn = btnRT.gameObject.AddComponent<Button>();
            btn.targetGraphic = btnImg;
            string tabName = tabs[i];
            btn.onClick.AddListener(() => SwitchTab(tabName));

            _tabButtons.Add(btn);
        }
    }

    private Transform BuildScrollArea(RectTransform parent)
    {
        var scrollGo = CreateRect(parent, "ScrollArea",
            new Vector2(0.02f, 0.05f), new Vector2(0.58f, 0.83f));

        var scrollImg = scrollGo.gameObject.AddComponent<Image>();
        scrollImg.color = new Color(0.08f, 0.1f, 0.15f);

        var scrollRect = scrollGo.gameObject.AddComponent<ScrollRect>();
        scrollRect.horizontal = false;
        scrollRect.movementType = ScrollRect.MovementType.Clamped;

        // Content
        var content = new GameObject("Content");
        content.transform.SetParent(scrollGo, false);
        var contentRT = content.AddComponent<RectTransform>();
        contentRT.anchorMin = new Vector2(0, 1);
        contentRT.anchorMax = new Vector2(1, 1);
        contentRT.pivot = new Vector2(0.5f, 1);
        contentRT.anchoredPosition = Vector2.zero;

        var vlg = content.AddComponent<VerticalLayoutGroup>();
        vlg.spacing = 4;
        vlg.padding = new RectOffset(8, 8, 8, 8);
        vlg.childForceExpandHeight = false;
        vlg.childForceExpandWidth = true;
        vlg.childControlHeight = true;
        vlg.childControlWidth = true;

        var csf = content.AddComponent<ContentSizeFitter>();
        csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        scrollRect.content = contentRT;
        content.AddComponent<Image>().color = Color.clear; // raycaster needs

        var mask = scrollGo.gameObject.AddComponent<Mask>();
        mask.showMaskGraphic = true;

        return content.transform;
    }

    private void BuildDetailPanel(RectTransform parent)
    {
        _detailPanel = CreateRect(parent, "DetailPanel",
            new Vector2(0.6f, 0.05f), new Vector2(0.98f, 0.91f)).gameObject;

        var detailImg = _detailPanel.AddComponent<Image>();
        detailImg.color = new Color(0.1f, 0.12f, 0.18f);
        var detailRT = _detailPanel.GetComponent<RectTransform>();

        // 道具圖示
        var iconRT = CreateRect(detailRT, "Icon",
            new Vector2(0.25f, 0.65f), new Vector2(0.75f, 0.9f));
        _detailIcon = iconRT.gameObject.AddComponent<Image>();
        _detailIcon.color = Color.white;
        _detailIcon.preserveAspect = true;

        // 名稱
        _detailName = CreateText(detailRT, "Name", "",
            new Vector2(0.05f, 0.55f), new Vector2(0.95f, 0.65f), 22);
        _detailName.alignment = TextAnchor.MiddleCenter;

        // 說明
        _detailDesc = CreateText(detailRT, "Desc", "",
            new Vector2(0.05f, 0.2f), new Vector2(0.95f, 0.55f), 16);
        _detailDesc.alignment = TextAnchor.UpperLeft;

        // 操作提示
        _detailAction = CreateText(detailRT, "ActionHint", "",
            new Vector2(0.05f, 0.12f), new Vector2(0.95f, 0.2f), 14);
        _detailAction.alignment = TextAnchor.MiddleCenter;
        _detailAction.color = new Color(1, 1, 0.6f);

        // 使用按鈕
        var useBtnRT = CreateRect(detailRT, "UseButton",
            new Vector2(0.2f, 0.03f), new Vector2(0.8f, 0.11f));
        var useBtnImg = useBtnRT.gameObject.AddComponent<Image>();
        useBtnImg.color = new Color(0.2f, 0.6f, 0.3f);
        var useBtnText = CreateText(useBtnRT, "Label", "使用",
            Vector2.zero, Vector2.one, 18);
        useBtnText.alignment = TextAnchor.MiddleCenter;
        _useButton = useBtnRT.gameObject.AddComponent<Button>();
        _useButton.targetGraphic = useBtnImg;
        _useButton.onClick.AddListener(OnUseClicked);

        _detailPanel.SetActive(false);
    }

    private void BuildCloseButton(RectTransform parent)
    {
        var closeBtnRT = CreateRect(parent, "CloseBtn",
            new Vector2(0.92f, 0.92f), new Vector2(0.98f, 0.99f));
        var closeBtnImg = closeBtnRT.gameObject.AddComponent<Image>();
        closeBtnImg.color = new Color(0.7f, 0.2f, 0.2f);
        var closeBtnText = CreateText(closeBtnRT, "X", "✕",
            Vector2.zero, Vector2.one, 20);
        closeBtnText.alignment = TextAnchor.MiddleCenter;
        var closeBtn = closeBtnRT.gameObject.AddComponent<Button>();
        closeBtn.targetGraphic = closeBtnImg;
        closeBtn.onClick.AddListener(Toggle);
    }

    // ──────────────── Logic ────────────────

    private void SwitchTab(string tab)
    {
        _currentTab = tab;
        RefreshSlots();
    }

    private void RefreshIfOpen()
    {
        if (_isOpen) RefreshSlots();
    }

    private void RefreshSlots()
    {
        // 清除舊格子
        foreach (var go in _slotObjects)
            if (go != null) Destroy(go);
        _slotObjects.Clear();
        _detailPanel.SetActive(false);
        _selectedSlot = null;

        var inv = InventoryManager.Instance;
        if (inv == null) return;

        foreach (var slot in inv.Slots)
        {
            if (_currentTab != "All" && slot.Entry.category != _currentTab)
                continue;

            CreateSlotUI(slot);
        }
    }

    private void CreateSlotUI(InventoryManager.ItemSlot slot)
    {
        var go = new GameObject($"Slot_{slot.Entry.id}");
        go.transform.SetParent(_slotContainer, false);

        var le = go.AddComponent<LayoutElement>();
        le.preferredHeight = 44;

        var bg = go.AddComponent<Image>();
        bg.color = new Color(0.15f, 0.18f, 0.25f);

        // 彩色圓點
        var iconGo = new GameObject("Icon");
        iconGo.transform.SetParent(go.transform, false);
        var iconRT = iconGo.AddComponent<RectTransform>();
        iconRT.anchorMin = new Vector2(0, 0.1f);
        iconRT.anchorMax = new Vector2(0, 0.9f);
        iconRT.offsetMin = new Vector2(8, 0);
        iconRT.offsetMax = new Vector2(40, 0);
        var iconImg = iconGo.AddComponent<Image>();
        iconImg.color = slot.Entry.color.ToColor();

        // 名稱 + 數量
        string countStr = slot.Entry.stackable && slot.Count > 1 ? $" ×{slot.Count}" : "";
        string categoryIcon = slot.Entry.category switch
        {
            "Consumable" => "◈ ",
            "Quest" => "◉ ",
            "Collection" => "★ ",
            _ => "● "
        };
        var nameText = CreateText(go.GetComponent<RectTransform>(), "Name",
            $"{categoryIcon}{slot.Entry.name}{countStr}",
            new Vector2(0, 0), new Vector2(1, 1), 16);
        nameText.alignment = TextAnchor.MiddleLeft;
        var nameRT = nameText.GetComponent<RectTransform>();
        nameRT.offsetMin = new Vector2(48, 0);
        nameRT.offsetMax = new Vector2(-8, 0);

        // 點擊事件
        var btn = go.AddComponent<Button>();
        btn.targetGraphic = bg;
        var captured = slot;
        btn.onClick.AddListener(() => SelectSlot(captured));

        _slotObjects.Add(go);
    }

    private void SelectSlot(InventoryManager.ItemSlot slot)
    {
        _selectedSlot = slot;
        _detailPanel.SetActive(true);

        _detailName.text = slot.Entry.name;
        _detailDesc.text = slot.Entry.description;
        _detailIcon.color = slot.Entry.color.ToColor();

        bool isConsumable = slot.Entry.category == "Consumable" && slot.Entry.effect != null;
        _useButton.gameObject.SetActive(isConsumable);

        string actionHint = slot.Entry.category switch
        {
            "Consumable" => GetEffectDescription(slot.Entry.effect),
            "Quest" => "任務道具 — 不可使用",
            "Collection" => $"收藏品 — 價值 {slot.Entry.scoreValue} 分",
            _ => $"+{slot.Entry.scoreValue} 分"
        };
        _detailAction.text = actionHint;
    }

    private string GetEffectDescription(ItemEffect effect)
    {
        if (effect == null) return "";
        return effect.type switch
        {
            "RestoreHP" => $"恢復 {effect.value} HP",
            "RestoreMP" => $"恢復 {effect.value} MP",
            "DamageHP" => $"失去 {effect.value} HP",
            "DamageMP" => $"失去 {effect.value} MP",
            _ => ""
        };
    }

    private void OnUseClicked()
    {
        if (_selectedSlot == null) return;
        var inv = InventoryManager.Instance;
        if (inv == null) return;

        if (inv.UseItem(_selectedSlot))
        {
            // 如果使用完了就關閉詳情
            if (_selectedSlot.Count <= 0)
                _detailPanel.SetActive(false);
            RefreshSlots();
        }
    }

    // ──────────────── UI 工具 ────────────────

    private static RectTransform CreateRect(Transform parent, string name,
        Vector2 anchorMin, Vector2 anchorMax)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        return rt;
    }

    private static Text CreateText(RectTransform parent, string name, string text,
        Vector2 anchorMin, Vector2 anchorMax, int fontSize)
    {
        var rt = CreateRect(parent, name, anchorMin, anchorMax);
        var t = rt.gameObject.AddComponent<Text>();
        t.text = text;
        t.fontSize = fontSize;
        t.color = Color.white;
        t.font = FontHelper.GetFont();
        t.horizontalOverflow = HorizontalWrapMode.Wrap;
        t.verticalOverflow = VerticalWrapMode.Overflow;
        t.raycastTarget = false;
        return t;
    }
}
