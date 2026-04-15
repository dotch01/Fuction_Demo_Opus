using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

// ============================================================
// TalkDemoController.cs
// 場景唯一 MonoBehaviour：協調所有 TalkDemo 模組
//
// 使用方式：
// 1. 場景中建立空 GameObject，掛上此腳本
// 2. 在 Inspector 指定 CharacterPromptConfig 和 GeminiApiConfig
//    （右鍵 → Create → TalkDemo 可建立設定檔）
// 3. 把角色的 RectTransform 拖進 characterTransform 欄位
// 4. 聊天 UI（輸入框、回應顯示）會在場景中自動建立
// ============================================================

public class TalkDemoController : MonoBehaviour
{
    // --------------------------------------------------------
    // Inspector 設定區
    // --------------------------------------------------------

    [Header("設定檔")]
    [SerializeField] private CharacterPromptConfig promptConfig;
    [SerializeField] private GeminiApiConfig apiConfig;
    [SerializeField] private SupabaseConfig supabaseConfig;

    [Header("角色")]
    [SerializeField] private RectTransform characterTransform;
    [SerializeField] private float animationSpeed = 1f;
    [SerializeField] private bool centerCharacter = true;

    [Header("透明視窗設定")]
    [SerializeField] private bool enableTransparentWindow = true;
    [SerializeField] private bool alwaysOnTop = true;
    [SerializeField] private bool borderless = true;

    // --------------------------------------------------------
    // 內部模組與 UI 參照
    // --------------------------------------------------------

    private ConversationManager conversation;
    private CharacterAnimationHandler animator;
    private bool isWaitingForResponse = false;
    private float _lastResponseTime;         // 上次回應完成的時間
    private const float RandomTalkCooldown = 15f; // 回應後冷卻秒數

    private Text responseDisplayText;
    private GameObject responseBubble;
    private InputField chatInputField;

    // --------------------------------------------------------
    // Unity 生命週期
    // --------------------------------------------------------

    void Start()
    {
        // 置中角色圖片
        if (centerCharacter && characterTransform != null)
        {
            characterTransform.anchorMin = new Vector2(0.5f, 0.5f);
            characterTransform.anchorMax = new Vector2(0.5f, 0.5f);
            characterTransform.pivot = new Vector2(0.5f, 0.5f);
            characterTransform.anchoredPosition = Vector2.zero;
        }

        conversation = new ConversationManager(promptConfig);

        // 在置中之後初始化動畫，才能正確記錄起始位置
        if (characterTransform != null)
        {
            animator = new CharacterAnimationHandler(
                characterTransform, this, animationSpeed);
            animator.Initialize();
        }

        // 建立聊天 UI
        BuildChatUI();

        if (enableTransparentWindow)
            TransparentWindowHelper.Apply(borderless, alwaysOnTop);

        if (promptConfig != null && promptConfig.enableRandomTalk)
            StartCoroutine(RandomTalkTimer());

        // === 新手提示 ===
        Canvas hintCanvas = FindAnyObjectByType<Canvas>();
        if (hintCanvas != null)
        {
            TutorialHint.Show(hintCanvas.transform,
                "在下方輸入框輸入訊息，按 Enter 或點發送\n" +
                "角色會依情緒做出不同動畫反應\n" +
                "閒置一段時間後角色會主動找你聊天", this);
        }

        Debug.Log("[TalkDemo] 初始化完成。");
    }

    // --------------------------------------------------------
    // 建立聊天 UI（輸入框 + 回應泡泡）
    // --------------------------------------------------------

    private void BuildChatUI()
    {
        Canvas canvas = FindAnyObjectByType<Canvas>();
        if (canvas == null)
        {
            var go = new GameObject("Canvas");
            canvas = go.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            go.AddComponent<CanvasScaler>();
            go.AddComponent<GraphicRaycaster>();
        }

        var ui = ChatUIBuilder.Build(canvas.transform);
        responseDisplayText = ui.responseText;
        responseBubble = ui.responseBubble;
        chatInputField = ui.inputField;

        ui.sendButton.onClick.AddListener(OnSendClicked);
        chatInputField.onSubmit.AddListener(OnInputSubmit);
    }

    private void OnSendClicked()
    {
        SendCurrentInput();
    }

    private void OnInputSubmit(string text)
    {
        // onSubmit 只在按 Enter 時觸發（New Input System 相容）
        SendCurrentInput();
    }

    private void SendCurrentInput()
    {
        if (chatInputField == null) return;
        string message = chatInputField.text.Trim();
        if (string.IsNullOrEmpty(message)) return;

        chatInputField.text = "";
        chatInputField.ActivateInputField();
        OnPlayerSendMessage(message);
    }

    private void ShowResponse(string text)
    {
        if (responseDisplayText == null || responseBubble == null) return;
        responseDisplayText.text = text;
        responseBubble.SetActive(true);
    }

    // --------------------------------------------------------
    // 公開方法：給 UI 呼叫
    // --------------------------------------------------------

    /// <summary>
    /// 玩家傳送訊息時呼叫
    /// 例如：controller.OnPlayerSendMessage(inputField.text);
    /// </summary>
    public void OnPlayerSendMessage(string playerMessage)
    {
        if (isWaitingForResponse)
        {
            Debug.Log("[TalkDemo] 還在等待回應，請稍候。");
            return;
        }
        if (string.IsNullOrEmpty(playerMessage)) return;

        StartCoroutine(SendToGemini(playerMessage, isRandomTalk: false));
    }

    /// <summary>
    /// 切換透明／一般視窗模式，給 UI 按鈕用
    /// </summary>
    public void ToggleTransparentWindow()
    {
        TransparentWindowHelper.Toggle(borderless, alwaysOnTop);
    }

    // --------------------------------------------------------
    // 隨機發話計時器
    // --------------------------------------------------------

    private IEnumerator RandomTalkTimer()
    {
        while (true)
        {
            float waitTime = Random.Range(
                promptConfig.randomTalkMinSeconds,
                promptConfig.randomTalkMaxSeconds);
            yield return new WaitForSeconds(waitTime);

            if (!isWaitingForResponse && Time.time - _lastResponseTime > RandomTalkCooldown)
            {
                string prompt = conversation.BuildRandomTalkPrompt();
                StartCoroutine(SendToGemini(prompt, isRandomTalk: true));
            }
        }
    }

    // --------------------------------------------------------
    // 發送 API 請求
    // --------------------------------------------------------

    private IEnumerator SendToGemini(string message, bool isRandomTalk)
    {
        isWaitingForResponse = true;
        ShowResponse("思考中...");

        if (!isRandomTalk)
            conversation.AddMessage("user", message);

        // --- RAG：查詢相關設定並注入 System Prompt ---
        string effectivePrompt = promptConfig.systemPrompt;

        if (supabaseConfig != null && supabaseConfig.enableRAG && !isRandomTalk)
        {
            // Step 1: 取得玩家訊息的 Embedding 向量
            float[] embedding = null;
            yield return GetEmbedding(message, result => embedding = result);
            Debug.Log($"[RAG] Embedding {(embedding != null ? $"成功，{embedding.Length} 維" : "失敗")}");

            // Step 2: 用向量搜尋 Supabase 取得相關段落
            if (embedding != null)
            {
                string retrievedContent = null;
                yield return SearchSupabase(embedding, result => retrievedContent = result);

                if (!string.IsNullOrEmpty(retrievedContent))
                {
                    effectivePrompt += "\n\n以下是和這次對話相關的設定資料，請參考：\n" + retrievedContent;
                    Debug.Log($"[RAG] 注入內容（{retrievedContent.Length} 字）：\n{retrievedContent}");
                }
                else
                {
                    Debug.LogWarning("[RAG] Supabase 搜尋無結果或失敗，使用原始 prompt");
                }
            }
        }

        string contentsJson = conversation.BuildContentsJson(message, isRandomTalk);
        string jsonBody = $@"{{
            ""system_instruction"": {{
                ""parts"": [{{ ""text"": {ConversationManager.JsonEscape(effectivePrompt)} }}]
            }},
            ""contents"": [{contentsJson}]
        }}";

        string fullUrl = $"{apiConfig.apiUrl}?key={apiConfig.apiKey}";
        var request = new UnityWebRequest(fullUrl, "POST");
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonBody);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            string reply = conversation.ParseGeminiResponse(
                request.downloadHandler.text);

            if (!string.IsNullOrEmpty(reply))
            {
                EmotionType emotion = ConversationManager.ParseEmotion(ref reply);

                conversation.AddMessage("model", reply);
                ShowResponse(reply);
                Debug.Log($"[角色說]: {reply}");

                if (animator != null)
                    animator.PlayEmotion(emotion);
            }
            else
            {
                ShowResponse("（無回應）");
            }
        }
        else
        {
            string errorBody = request.downloadHandler?.text ?? "";
            long statusCode = request.responseCode;
            ShowResponse($"（連線失敗：{statusCode}）");
            Debug.LogError($"[TalkDemo] API 錯誤（HTTP {statusCode}）：{request.error}");
            Debug.LogError($"[TalkDemo] 回應內容：{errorBody}");

            if (statusCode == 429)
                Debug.LogWarning("[TalkDemo] 429 = 超過配額限制。請確認 API Key 配額，或稍後再試。");
            else if (statusCode == 400)
                Debug.LogWarning("[TalkDemo] 400 = 請求格式有誤，請檢查 API URL 和 JSON 格式。");
            else if (statusCode == 403)
                Debug.LogWarning("[TalkDemo] 403 = API Key 無權限，請確認 Key 已啟用 Gemini API。");
        }

        isWaitingForResponse = false;
        _lastResponseTime = Time.time;
    }

    // --------------------------------------------------------
    // RAG：Embedding + Supabase 向量搜尋
    // --------------------------------------------------------

    private IEnumerator GetEmbedding(string text, System.Action<float[]> callback)
    {
        string url = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-embedding-001:embedContent?key={apiConfig.apiKey}";
        string body = $@"{{
            ""model"": ""models/gemini-embedding-001"",
            ""content"": {{ ""parts"": [{{ ""text"": {ConversationManager.JsonEscape(text)} }}] }},
            ""output_dimensionality"": 768
        }}";

        var request = new UnityWebRequest(url, "POST");
        request.uploadHandler = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(body));
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");
        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogWarning($"[TalkDemo] Embedding 失敗: {request.responseCode} {request.error}");
            callback(null);
            yield break;
        }

        callback(ParseEmbeddingValues(request.downloadHandler.text));
    }

    private IEnumerator SearchSupabase(float[] embedding, System.Action<string> callback)
    {
        string url = $"{supabaseConfig.supabaseUrl}/rest/v1/rpc/search_settings";

        var values = new string[embedding.Length];
        for (int i = 0; i < embedding.Length; i++)
            values[i] = embedding[i].ToString(System.Globalization.CultureInfo.InvariantCulture);
        string vectorJson = "[" + string.Join(",", values) + "]";

        string body = $@"{{
            ""query_embedding"": {vectorJson},
            ""match_count"": {supabaseConfig.matchCount}
        }}";

        var request = new UnityWebRequest(url, "POST");
        request.uploadHandler = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(body));
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");
        request.SetRequestHeader("apikey", supabaseConfig.supabaseAnonKey);
        request.SetRequestHeader("Authorization", $"Bearer {supabaseConfig.supabaseAnonKey}");
        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogWarning($"[TalkDemo] Supabase 搜尋失敗: {request.responseCode} {request.error}");
            callback(null);
            yield break;
        }

        // 回傳格式: [{"content":"...","similarity":0.85}, ...]
        string json = request.downloadHandler.text;
        Debug.Log($"[RAG] Supabase 原始回應：{json}");
        string combined = ParseSupabaseResults(json);
        callback(combined);
    }

    // 解析 Gemini Embedding 回應中的 values 陣列
    private static float[] ParseEmbeddingValues(string json)
    {
        // 找到 "values": [...] 區段
        int valuesIdx = json.IndexOf("\"values\"");
        if (valuesIdx < 0) return null;

        int bracketStart = json.IndexOf('[', valuesIdx);
        int bracketEnd = json.IndexOf(']', bracketStart);
        if (bracketStart < 0 || bracketEnd < 0) return null;

        string arrayStr = json.Substring(bracketStart + 1, bracketEnd - bracketStart - 1);
        string[] parts = arrayStr.Split(',');
        float[] result = new float[parts.Length];
        for (int i = 0; i < parts.Length; i++)
        {
            if (!float.TryParse(parts[i].Trim(),
                System.Globalization.NumberStyles.Float,
                System.Globalization.CultureInfo.InvariantCulture,
                out result[i]))
                return null;
        }
        return result;
    }

    // 解析 Supabase RPC 回傳的 JSON 陣列，組合 content 欄位
    private string ParseSupabaseResults(string json)
    {
        // 簡易解析 [{"content":"...","similarity":...}, ...]
        var sb = new System.Text.StringBuilder();
        int maxChars = supabaseConfig != null ? supabaseConfig.maxContextChars : 2000;

        int searchFrom = 0;
        while (true)
        {
            int contentIdx = json.IndexOf("\"content\"", searchFrom);
            if (contentIdx < 0) break;

            int colonIdx = json.IndexOf(':', contentIdx + 9);
            if (colonIdx < 0) break;

            // 找到值的開頭引號
            int quoteStart = json.IndexOf('"', colonIdx + 1);
            if (quoteStart < 0) break;

            // 找到值的結尾引號（處理跳脫字元）
            int quoteEnd = quoteStart + 1;
            while (quoteEnd < json.Length)
            {
                if (json[quoteEnd] == '\\') { quoteEnd += 2; continue; }
                if (json[quoteEnd] == '"') break;
                quoteEnd++;
            }

            string content = json.Substring(quoteStart + 1, quoteEnd - quoteStart - 1)
                .Replace("\\n", "\n").Replace("\\\"", "\"").Replace("\\\\", "\\");

            if (sb.Length + content.Length > maxChars) break;

            sb.AppendLine("---");
            sb.AppendLine(content);

            searchFrom = quoteEnd + 1;
        }

        return sb.Length > 0 ? sb.ToString() : null;
    }
}
