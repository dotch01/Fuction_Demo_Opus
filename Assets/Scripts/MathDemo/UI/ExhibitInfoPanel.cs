using UnityEngine;
using UnityEngine.UI;

// ============================================================
// ExhibitInfoPanel.cs
// 展品資訊面板 — 接近展品時顯示名稱、說明、公式
// ============================================================

public class ExhibitInfoPanel : MonoBehaviour
{
    private CanvasGroup canvasGroup;
    private Text titleText;
    private Text descText;
    private Text formulaText;
    private Text challengeText;
    private Image challengeIcon;
    private float targetAlpha = 0f;

    private ExhibitBase currentExhibit;

    public void Initialize(Transform canvasTransform, Font font)
    {
        // 主面板
        var panel = new GameObject("ExhibitInfoPanel");
        panel.transform.SetParent(canvasTransform, false);

        var rt = panel.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.01f, 0.25f);
        rt.anchorMax = new Vector2(0.28f, 0.80f);
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        var bg = panel.AddComponent<Image>();
        bg.color = new Color(0.08f, 0.1f, 0.16f, 0.85f);
        bg.raycastTarget = false;

        canvasGroup = panel.AddComponent<CanvasGroup>();
        canvasGroup.alpha = 0f;
        canvasGroup.blocksRaycasts = false;

        var vlg = panel.AddComponent<VerticalLayoutGroup>();
        vlg.padding = new RectOffset(16, 16, 12, 12);
        vlg.spacing = 8;
        vlg.childForceExpandWidth = true;
        vlg.childForceExpandHeight = false;
        vlg.childControlWidth = true;
        vlg.childControlHeight = true;

        // 標題
        titleText = CreateText(panel.transform, font, "展品名稱", 24, Color.white, 36);

        // 分隔線
        CreateSeparator(panel.transform);

        // 說明
        descText = CreateText(panel.transform, font, "說明", 16, new Color(0.8f, 0.85f, 0.9f), 200);

        // 分隔線
        CreateSeparator(panel.transform);

        // 公式
        formulaText = CreateText(panel.transform, font, "公式", 18, new Color(0.5f, 0.9f, 0.6f), 32);

        // 挑戰說明
        challengeText = CreateText(panel.transform, font, "", 14, new Color(1f, 0.85f, 0.4f), 40);

        transform.SetParent(panel.transform.parent, false);
        gameObject.SetActive(true);
    }

    void Update()
    {
        if (canvasGroup == null) return;
        canvasGroup.alpha = Mathf.MoveTowards(canvasGroup.alpha, targetAlpha, Time.deltaTime * 5f);
    }

    public void Show(ExhibitBase exhibit)
    {
        currentExhibit = exhibit;
        targetAlpha = 1f;

        if (titleText != null) titleText.text = exhibit.exhibitName;
        if (descText != null) descText.text = exhibit.description;
        if (formulaText != null) formulaText.text = exhibit.formula;

        if (challengeText != null)
        {
            bool completed = ChallengeSystem.Instance != null &&
                             ChallengeSystem.Instance.IsCompleted(exhibit.exhibitName);
            if (completed)
                challengeText.text = "★ 挑戰已完成！";
            else if (!string.IsNullOrEmpty(exhibit.challengeDescription))
                challengeText.text = $"[E] 開始挑戰: {exhibit.challengeDescription}";
            else
                challengeText.text = "";
        }
    }

    public void Hide()
    {
        targetAlpha = 0f;
        currentExhibit = null;
    }

    // --------------------------------------------------------
    // 工具
    // --------------------------------------------------------

    private Text CreateText(Transform parent, Font font, string defaultText, int fontSize, Color color, float height)
    {
        var go = new GameObject("Text");
        go.transform.SetParent(parent, false);

        var le = go.AddComponent<LayoutElement>();
        le.preferredHeight = height;

        var text = go.AddComponent<Text>();
        text.font = font;
        text.fontSize = fontSize;
        text.color = color;
        text.alignment = TextAnchor.UpperLeft;
        text.horizontalOverflow = HorizontalWrapMode.Wrap;
        text.verticalOverflow = VerticalWrapMode.Overflow;
        text.text = defaultText;
        text.raycastTarget = false;

        var shadow = go.AddComponent<Shadow>();
        shadow.effectColor = new Color(0, 0, 0, 0.6f);
        shadow.effectDistance = new Vector2(1, -1);

        return text;
    }

    private void CreateSeparator(Transform parent)
    {
        var go = new GameObject("Separator");
        go.transform.SetParent(parent, false);
        var le = go.AddComponent<LayoutElement>();
        le.preferredHeight = 2;
        var img = go.AddComponent<Image>();
        img.color = new Color(0.3f, 0.35f, 0.45f, 0.5f);
        img.raycastTarget = false;
    }
}
