using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 任務面板 UI：兩個視圖切換。
///   列表視圖 — 按象限分群顯示「觀察目標_EMETH-XXX」，鎖定的灰色不可點。可捲動。
///   詳情視圖 — 顯示提示文字 + 「返回」/「定位行星」按鈕。
/// Q 鍵或「特別觀察」按鈕開關面板。
/// </summary>
public class QuestPanelUI : MonoBehaviour
{
    public static QuestPanelUI Instance { get; private set; }

    private GameObject _panel;
    private RectTransform _contentRoot;

    // 兩個子容器：列表 (含 ScrollRect) / 詳情
    private GameObject _scrollViewGo;
    private RectTransform _listContent;
    private GameObject _detailView;
    private Text _detailTitle;
    private Text _detailHints;

    private readonly List<GameObject> _listEntries = new();
    private Star _selectedStar;

    private static readonly Color ColorLocked = new(0.2f, 0.2f, 0.2f, 0.6f);
    private static readonly Color ColorNormal = new(0.12f, 0.12f, 0.2f, 0.85f);
    private static readonly Color ColorDone = new(0.15f, 0.3f, 0.15f, 0.8f);

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(this); return; }
        Instance = this;
    }

    public void Init(GameObject panel, RectTransform contentRoot)
    {
        _panel = panel;
        _contentRoot = contentRoot;
        BuildSubViews();
        _panel.SetActive(false);
    }

    private void Update()
    {
        if (UnityEngine.InputSystem.Keyboard.current != null &&
            UnityEngine.InputSystem.Keyboard.current.qKey.wasPressedThisFrame)
            TogglePanel();
    }

    public void TogglePanel()
    {
        if (_panel == null) return;
        bool opening = !_panel.activeSelf;
        _panel.SetActive(opening);
        if (opening) ShowList();
    }

    // ──────────────── 列表視圖 ────────────────

    private void ShowList()
    {
        _scrollViewGo.SetActive(true);
        _detailView.SetActive(false);
        RefreshList();
    }

    private void RefreshList()
    {
        foreach (var go in _listEntries)
            if (go != null) Object.Destroy(go);
        _listEntries.Clear();

        if (QuestManager.Instance == null) return;

        // 按象限分群（保持 questOrder 排序）
        var byQuadrant = new Dictionary<int, List<Star>>();
        foreach (var star in QuestManager.Instance.QuestStars)
        {
            int q = QuadrantManager.Instance != null
                ? QuadrantManager.Instance.GetQuadrant(star.Data.position.ToVector2()) : 0;
            if (!byQuadrant.ContainsKey(q))
                byQuadrant[q] = new List<Star>();
            byQuadrant[q].Add(star);
        }

        for (int q = 1; q <= 4; q++)
        {
            if (!byQuadrant.ContainsKey(q)) continue;
            AddHeader($"第{q}象限");
            foreach (var star in byQuadrant[q])
                AddListEntry(star);
        }
    }

    private void AddHeader(string title)
    {
        var go = new GameObject("Header");
        go.transform.SetParent(_listContent, false);
        go.AddComponent<LayoutElement>().preferredHeight = 36;

        go.AddComponent<Image>().color = new Color(0.1f, 0.6f, 0.35f, 0.9f);
        AddFullText(go.transform, title, 18, Color.white, TextAnchor.MiddleCenter);
        _listEntries.Add(go);
    }

    private void AddListEntry(Star star)
    {
        bool unlocked = QuestManager.Instance.IsUnlocked(star);
        bool scanned = star.IsScanned;

        var go = new GameObject("Entry_" + star.Data.id);
        go.transform.SetParent(_listContent, false);
        go.AddComponent<LayoutElement>().preferredHeight = 44;

        var bg = go.AddComponent<Image>();
        bg.color = scanned ? ColorDone : (unlocked ? ColorNormal : ColorLocked);

        string label;
        Color textColor;

        if (scanned)
        {
            label = $"  \u2713  觀察目標_{star.Data.id}";
            textColor = new Color(0.6f, 0.9f, 0.6f);
        }
        else if (unlocked)
        {
            label = $"  \u25cb  觀察目標_{star.Data.id}";
            textColor = Color.white;
        }
        else
        {
            label = $"  \ud83d\udd12  觀察目標_{star.Data.id}";
            textColor = new Color(0.5f, 0.5f, 0.5f);
        }

        AddFullText(go.transform, label, 18, textColor, TextAnchor.MiddleLeft);

        // 只有已解鎖才能點擊
        if (unlocked)
        {
            var btn = go.AddComponent<Button>();
            btn.targetGraphic = bg;
            Star captured = star;
            btn.onClick.AddListener(() => ShowDetail(captured));
        }

        _listEntries.Add(go);
    }

    // ──────────────── 詳情視圖 ────────────────

    private void ShowDetail(Star star)
    {
        _selectedStar = star;
        _scrollViewGo.SetActive(false);
        _detailView.SetActive(true);

        _detailTitle.text = $"觀察目標_{star.Data.id}";

        int q = QuadrantManager.Instance != null
            ? QuadrantManager.Instance.GetQuadrant(star.Data.position.ToVector2()) : 0;
        string sysName = StarSystemManager.Instance != null
            ? StarSystemManager.Instance.GetStarSystemForPlanet(star.Data) ?? "未知" : "未知";

        string hints = $"所在象限：第{q}象限\n" +
                       $"所在星域：{sysName}\n" +
                       $"座標範圍：({star.Data.position.x:F0}, {star.Data.position.y:F0})\n" +
                       (star.Data.filterOnly ? "需要開啟濾鏡才能看見" : "無需濾鏡");

        if (star.IsScanned)
            hints += "\n\n（已完成掃描）";

        _detailHints.text = hints;
    }

    // ──────────────── UI 建構 ────────────────

    private void BuildSubViews()
    {
        // ── 列表視圖 (ScrollRect) ──
        _scrollViewGo = new GameObject("ListScrollView");
        _scrollViewGo.transform.SetParent(_contentRoot, false);
        Stretch(_scrollViewGo.AddComponent<RectTransform>());

        var scrollRect = _scrollViewGo.AddComponent<ScrollRect>();
        scrollRect.horizontal = false;
        scrollRect.movementType = ScrollRect.MovementType.Clamped;

        // Viewport
        var viewportGo = new GameObject("Viewport");
        viewportGo.transform.SetParent(_scrollViewGo.transform, false);
        var vpRT = Stretch(viewportGo.AddComponent<RectTransform>());
        viewportGo.AddComponent<Image>().color = Color.clear;
        viewportGo.AddComponent<RectMask2D>();

        // Content (VLG 容器)
        var contentGo = new GameObject("Content");
        contentGo.transform.SetParent(viewportGo.transform, false);
        _listContent = contentGo.AddComponent<RectTransform>();
        _listContent.anchorMin = new Vector2(0, 1);
        _listContent.anchorMax = new Vector2(1, 1);
        _listContent.pivot = new Vector2(0.5f, 1);
        _listContent.anchoredPosition = Vector2.zero;
        _listContent.sizeDelta = new Vector2(0, 0);

        var vlg = contentGo.AddComponent<VerticalLayoutGroup>();
        vlg.childForceExpandWidth = true;
        vlg.childForceExpandHeight = false;
        vlg.childControlWidth = true;
        vlg.childControlHeight = false;
        vlg.spacing = 3;
        vlg.padding = new RectOffset(5, 5, 5, 5);

        var csf = contentGo.AddComponent<ContentSizeFitter>();
        csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        scrollRect.viewport = vpRT;
        scrollRect.content = _listContent;

        // ── 詳情視圖 ──
        _detailView = new GameObject("DetailView");
        _detailView.transform.SetParent(_contentRoot, false);
        var detailRT = Stretch(_detailView.AddComponent<RectTransform>());

        // 標題
        var titleGo = new GameObject("DetailTitle");
        titleGo.transform.SetParent(detailRT, false);
        var titleRT = titleGo.AddComponent<RectTransform>();
        titleRT.anchorMin = new Vector2(0, 1);
        titleRT.anchorMax = new Vector2(1, 1);
        titleRT.pivot = new Vector2(0.5f, 1);
        titleRT.anchoredPosition = new Vector2(0, -10);
        titleRT.sizeDelta = new Vector2(0, 40);
        _detailTitle = titleGo.AddComponent<Text>();
        _detailTitle.fontSize = 22;
        _detailTitle.alignment = TextAnchor.MiddleCenter;
        _detailTitle.color = Color.white;
        _detailTitle.font = FontHelper.GetFont();

        // 提示文字
        var hintsGo = new GameObject("DetailHints");
        hintsGo.transform.SetParent(detailRT, false);
        var hintsRT = hintsGo.AddComponent<RectTransform>();
        hintsRT.anchorMin = new Vector2(0, 0.25f);
        hintsRT.anchorMax = new Vector2(1, 0.85f);
        hintsRT.offsetMin = new Vector2(30, 0);
        hintsRT.offsetMax = new Vector2(-30, 0);
        _detailHints = hintsGo.AddComponent<Text>();
        _detailHints.fontSize = 18;
        _detailHints.alignment = TextAnchor.UpperLeft;
        _detailHints.color = new Color(0.9f, 0.9f, 0.9f);
        _detailHints.font = FontHelper.GetFont();
        _detailHints.lineSpacing = 1.4f;

        float btnY = 30f, btnH = 45f;

        CreateBottomButton(detailRT, "BackBtn", "返回",
            new Vector2(0, 0), new Vector2(0.48f, 0), new Vector2(0, btnY), btnH,
            new Color(0.2f, 0.2f, 0.3f, 0.9f), () => ShowList());

        CreateBottomButton(detailRT, "LocateBtn", "定位行星",
            new Vector2(0.52f, 0), new Vector2(1, 0), new Vector2(0, btnY), btnH,
            new Color(0.1f, 0.5f, 0.4f, 0.95f), () =>
            {
                if (_selectedStar != null && QuestManager.Instance != null)
                    QuestManager.Instance.StartLocating(_selectedStar);
                _panel.SetActive(false);
            });

        _detailView.SetActive(false);
    }

    // ──────────────── Helpers ────────────────

    private static RectTransform Stretch(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        return rt;
    }

    private void CreateBottomButton(RectTransform parent, string name, string label,
        Vector2 anchorMin, Vector2 anchorMax, Vector2 pos, float height,
        Color bgColor, UnityEngine.Events.UnityAction onClick)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.pivot = new Vector2(0.5f, 0);
        rt.anchoredPosition = pos;
        rt.sizeDelta = new Vector2(0, height);

        var img = go.AddComponent<Image>();
        img.color = bgColor;
        var btn = go.AddComponent<Button>();
        btn.targetGraphic = img;
        btn.onClick.AddListener(onClick);

        AddFullText(go.transform, label, 20, Color.white, TextAnchor.MiddleCenter);
    }

    private static void AddFullText(Transform parent, string content, int fontSize,
        Color color, TextAnchor alignment)
    {
        var go = new GameObject("Text");
        go.transform.SetParent(parent, false);
        Stretch(go.AddComponent<RectTransform>());

        var text = go.AddComponent<Text>();
        text.text = content;
        text.fontSize = fontSize;
        text.alignment = alignment;
        text.color = color;
        text.font = FontHelper.GetFont();
        text.raycastTarget = false;
    }
}
