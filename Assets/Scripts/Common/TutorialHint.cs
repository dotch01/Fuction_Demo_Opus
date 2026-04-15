using System.Collections;
using UnityEngine;
using UnityEngine.UI;

// ============================================================
// TutorialHint.cs
// 左上角半透明小視窗：顯示操作提示，點擊 ✕ 或一段時間後自動消失
// 不使用 Update，使用 Coroutine 做淡出
// ============================================================

public class TutorialHint : MonoBehaviour
{
    private CanvasGroup canvasGroup;
    private const float AutoDismissSeconds = 15f;

    /// <summary>
    /// 在左上角建立操作提示小視窗。
    /// </summary>
    /// <param name="canvasTransform">Canvas 的 Transform</param>
    /// <param name="text">提示文字（支援 \n 換行）</param>
    /// <param name="host">用來啟動 Coroutine 的 MonoBehaviour</param>
    public static TutorialHint Show(Transform canvasTransform, string text, MonoBehaviour host)
    {
        var go = new GameObject("TutorialHint");
        go.transform.SetParent(canvasTransform, false);
        var th = go.AddComponent<TutorialHint>();
        th.Build(text);
        return th;
    }

    private void Build(string text)
    {
        Font font = FontHelper.GetFont();

        // 面板（左上角）
        var panel = new GameObject("HintPanel");
        panel.transform.SetParent(transform, false);
        var panelRT = panel.AddComponent<RectTransform>();
        panelRT.anchorMin = new Vector2(0, 1);
        panelRT.anchorMax = new Vector2(0, 1);
        panelRT.pivot = new Vector2(0, 1);
        panelRT.anchoredPosition = new Vector2(16, -16);
        // 寬度固定，高度自適應
        panelRT.sizeDelta = new Vector2(380, 0);

        var bg = panel.AddComponent<Image>();
        bg.color = new Color(0.05f, 0.07f, 0.14f, 0.8f);

        canvasGroup = panel.AddComponent<CanvasGroup>();
        canvasGroup.alpha = 1f;

        var vlg = panel.AddComponent<VerticalLayoutGroup>();
        vlg.padding = new RectOffset(14, 14, 10, 10);
        vlg.spacing = 0;
        vlg.childForceExpandWidth = true;
        vlg.childForceExpandHeight = false;
        vlg.childControlWidth = true;
        vlg.childControlHeight = true;

        var fitter = panel.AddComponent<ContentSizeFitter>();
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        // 標題列（標題＋關閉按鈕）
        var header = new GameObject("Header");
        header.transform.SetParent(panel.transform, false);
        var headerLE = header.AddComponent<LayoutElement>();
        headerLE.preferredHeight = 30;

        // 標題
        var titleGo = new GameObject("Title");
        titleGo.transform.SetParent(header.transform, false);
        var titleRT = titleGo.AddComponent<RectTransform>();
        titleRT.anchorMin = Vector2.zero;
        titleRT.anchorMax = Vector2.one;
        titleRT.offsetMin = Vector2.zero;
        titleRT.offsetMax = Vector2.zero;

        var titleText = titleGo.AddComponent<Text>();
        titleText.font = font;
        titleText.fontSize = 16;
        titleText.color = new Color(0.6f, 0.8f, 1f);
        titleText.text = "操作提示";
        titleText.alignment = TextAnchor.MiddleLeft;
        titleText.raycastTarget = false;

        // 關閉按鈕
        var closeGo = new GameObject("CloseBtn");
        closeGo.transform.SetParent(header.transform, false);
        var closeRT = closeGo.AddComponent<RectTransform>();
        closeRT.anchorMin = new Vector2(1, 0);
        closeRT.anchorMax = new Vector2(1, 1);
        closeRT.pivot = new Vector2(1, 0.5f);
        closeRT.anchoredPosition = Vector2.zero;
        closeRT.sizeDelta = new Vector2(28, 28);

        var closeBg = closeGo.AddComponent<Image>();
        closeBg.color = new Color(0, 0, 0, 0);
        var closeBtn = closeGo.AddComponent<Button>();
        closeBtn.targetGraphic = closeBg;
        closeBtn.onClick.AddListener(Dismiss);

        var xText = new GameObject("X");
        xText.transform.SetParent(closeGo.transform, false);
        var xRT = xText.AddComponent<RectTransform>();
        xRT.anchorMin = Vector2.zero;
        xRT.anchorMax = Vector2.one;
        xRT.offsetMin = Vector2.zero;
        xRT.offsetMax = Vector2.zero;
        var xLabel = xText.AddComponent<Text>();
        xLabel.font = font;
        xLabel.fontSize = 16;
        xLabel.color = new Color(1, 1, 1, 0.5f);
        xLabel.text = "✕";
        xLabel.alignment = TextAnchor.MiddleCenter;

        // 內容文字
        var contentGo = new GameObject("Content");
        contentGo.transform.SetParent(panel.transform, false);

        var contentText = contentGo.AddComponent<Text>();
        contentText.font = font;
        contentText.fontSize = 15;
        contentText.color = new Color(0.9f, 0.9f, 0.9f, 0.9f);
        contentText.text = text;
        contentText.alignment = TextAnchor.UpperLeft;
        contentText.horizontalOverflow = HorizontalWrapMode.Wrap;
        contentText.verticalOverflow = VerticalWrapMode.Overflow;
        contentText.lineSpacing = 1.2f;
        contentText.raycastTarget = false;

        var contentLE = contentGo.AddComponent<LayoutElement>();
        contentLE.flexibleWidth = 1;

        // 自動消失
        StartCoroutine(AutoDismiss());
    }

    private IEnumerator AutoDismiss()
    {
        yield return new WaitForSeconds(AutoDismissSeconds);

        // 淡出
        float duration = 1f;
        float elapsed = 0f;
        while (elapsed < duration && canvasGroup != null)
        {
            elapsed += Time.deltaTime;
            canvasGroup.alpha = 1f - (elapsed / duration);
            yield return null;
        }

        Dismiss();
    }

    private void Dismiss()
    {
        if (gameObject != null)
            Destroy(gameObject);
    }
}
