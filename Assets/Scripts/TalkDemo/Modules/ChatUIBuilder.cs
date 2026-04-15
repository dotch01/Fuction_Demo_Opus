using UnityEngine;
using UnityEngine.UI;

// ============================================================
// ChatUIBuilder.cs
// 靜態工具類：在 Canvas 上動態建立聊天 UI（輸入框 + 回應泡泡）
// ============================================================

public struct ChatUIElements
{
    public Text responseText;
    public GameObject responseBubble;
    public InputField inputField;
    public Button sendButton;
}

public static class ChatUIBuilder
{
    public static ChatUIElements Build(Transform parent)
    {
        Font font = FontHelper.GetFont();

        var elements = new ChatUIElements();
        BuildResponseBubble(parent, font, ref elements);
        BuildInputArea(parent, font, ref elements);
        return elements;
    }

    // --------------------------------------------------------
    // 回應泡泡：顯示角色說的話（畫面上方）
    // --------------------------------------------------------

    private static void BuildResponseBubble(Transform parent, Font font, ref ChatUIElements e)
    {
        var go = new GameObject("ResponseBubble");
        go.transform.SetParent(parent, false);

        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.1f, 0.65f);
        rt.anchorMax = new Vector2(0.9f, 0.92f);
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        var bg = go.AddComponent<Image>();
        bg.color = new Color(0f, 0f, 0f, 0.65f);

        // 泡泡內的文字
        var textGo = new GameObject("Text");
        textGo.transform.SetParent(go.transform, false);

        var textRT = textGo.AddComponent<RectTransform>();
        textRT.anchorMin = Vector2.zero;
        textRT.anchorMax = Vector2.one;
        textRT.offsetMin = new Vector2(15, 10);
        textRT.offsetMax = new Vector2(-15, -10);

        e.responseText = textGo.AddComponent<Text>();
        e.responseText.font = font;
        e.responseText.fontSize = 22;
        e.responseText.color = Color.white;
        e.responseText.alignment = TextAnchor.MiddleCenter;
        e.responseText.horizontalOverflow = HorizontalWrapMode.Wrap;
        e.responseText.verticalOverflow = VerticalWrapMode.Overflow;
        e.responseText.text = "";

        e.responseBubble = go;
        go.SetActive(false);
    }

    // --------------------------------------------------------
    // 輸入區：底部的輸入框 + 發送按鈕
    // --------------------------------------------------------

    private static void BuildInputArea(Transform parent, Font font, ref ChatUIElements e)
    {
        var panelGo = new GameObject("InputPanel");
        panelGo.transform.SetParent(parent, false);

        var panelRT = panelGo.AddComponent<RectTransform>();
        panelRT.anchorMin = new Vector2(0f, 0f);
        panelRT.anchorMax = new Vector2(1f, 0f);
        panelRT.pivot = new Vector2(0.5f, 0f);
        panelRT.sizeDelta = new Vector2(0f, 55f);

        var panelBg = panelGo.AddComponent<Image>();
        panelBg.color = new Color(0.12f, 0.12f, 0.12f, 0.9f);

        var layout = panelGo.AddComponent<HorizontalLayoutGroup>();
        layout.padding = new RectOffset(10, 10, 5, 5);
        layout.spacing = 8;
        layout.childAlignment = TextAnchor.MiddleCenter;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = false;
        layout.childForceExpandHeight = true;

        e.inputField = CreateInputField(panelGo.transform, font);
        e.sendButton = CreateSendButton(panelGo.transform, font);
    }

    // --------------------------------------------------------
    // InputField 建立（含 Text + Placeholder 子物件）
    // --------------------------------------------------------

    private static InputField CreateInputField(Transform parent, Font font)
    {
        var go = new GameObject("ChatInput");
        go.transform.SetParent(parent, false);

        var bg = go.AddComponent<Image>();
        bg.color = new Color(0.2f, 0.2f, 0.2f, 1f);

        var le = go.AddComponent<LayoutElement>();
        le.flexibleWidth = 1f;
        le.minHeight = 35f;

        var inputField = go.AddComponent<InputField>();

        // 顯示輸入文字的子物件
        var textGo = new GameObject("Text");
        textGo.transform.SetParent(go.transform, false);
        FillParent(textGo, 8f);

        var text = textGo.AddComponent<Text>();
        text.font = font;
        text.fontSize = 18;
        text.color = Color.white;
        text.supportRichText = false;
        text.alignment = TextAnchor.MiddleLeft;

        // 佔位提示文字
        var phGo = new GameObject("Placeholder");
        phGo.transform.SetParent(go.transform, false);
        FillParent(phGo, 8f);

        var ph = phGo.AddComponent<Text>();
        ph.font = font;
        ph.fontSize = 18;
        ph.fontStyle = FontStyle.Italic;
        ph.color = new Color(0.6f, 0.6f, 0.6f, 0.8f);
        ph.text = "輸入訊息...";
        ph.alignment = TextAnchor.MiddleLeft;

        inputField.textComponent = text;
        inputField.placeholder = ph;
        inputField.targetGraphic = bg;

        return inputField;
    }

    // --------------------------------------------------------
    // 發送按鈕
    // --------------------------------------------------------

    private static Button CreateSendButton(Transform parent, Font font)
    {
        var go = new GameObject("SendButton");
        go.transform.SetParent(parent, false);

        var bg = go.AddComponent<Image>();
        bg.color = new Color(0.2f, 0.5f, 0.9f, 1f);

        var le = go.AddComponent<LayoutElement>();
        le.minWidth = 80f;
        le.preferredWidth = 80f;
        le.minHeight = 48f;

        var button = go.AddComponent<Button>();
        button.targetGraphic = bg;

        var textGo = new GameObject("Text");
        textGo.transform.SetParent(go.transform, false);
        FillParent(textGo, 0f);

        var text = textGo.AddComponent<Text>();
        text.font = font;
        text.fontSize = 18;
        text.color = Color.white;
        text.text = "發送";
        text.alignment = TextAnchor.MiddleCenter;

        return button;
    }

    // --------------------------------------------------------
    // 工具：讓子物件填滿父物件
    // --------------------------------------------------------

    private static void FillParent(GameObject go, float padding)
    {
        var rt = go.GetComponent<RectTransform>();
        if (rt == null) rt = go.AddComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = new Vector2(padding, 2);
        rt.offsetMax = new Vector2(-padding, -2);
    }
}
