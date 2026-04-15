using UnityEngine;
using UnityEngine.UI;

// ============================================================
// InteractionPrompt.cs
// 畫面中央下方的 "按 E 互動" 提示
// ============================================================

public class InteractionPrompt : MonoBehaviour
{
    private Text promptText;
    private CanvasGroup cg;
    private float targetAlpha;

    public void Initialize(Transform canvasTransform, Font font)
    {
        var go = new GameObject("InteractionPrompt");
        go.transform.SetParent(canvasTransform, false);

        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.35f, 0.12f);
        rt.anchorMax = new Vector2(0.65f, 0.18f);
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        cg = go.AddComponent<CanvasGroup>();
        cg.alpha = 0;
        cg.blocksRaycasts = false;

        var bg = go.AddComponent<Image>();
        bg.color = new Color(0.05f, 0.05f, 0.1f, 0.7f);
        bg.raycastTarget = false;

        var textGo = new GameObject("Text");
        textGo.transform.SetParent(go.transform, false);
        var tRt = textGo.AddComponent<RectTransform>();
        tRt.anchorMin = Vector2.zero;
        tRt.anchorMax = Vector2.one;
        tRt.offsetMin = Vector2.zero;
        tRt.offsetMax = Vector2.zero;

        promptText = textGo.AddComponent<Text>();
        promptText.font = font;
        promptText.fontSize = 20;
        promptText.color = Color.white;
        promptText.alignment = TextAnchor.MiddleCenter;
        promptText.text = "按 E 開始挑戰";
        promptText.raycastTarget = false;
    }

    void Update()
    {
        if (cg == null) return;
        cg.alpha = Mathf.MoveTowards(cg.alpha, targetAlpha, Time.deltaTime * 6f);
    }

    public void Show(string text = null)
    {
        targetAlpha = 1f;
        if (text != null && promptText != null) promptText.text = text;
    }

    public void Hide()
    {
        targetAlpha = 0f;
    }
}
