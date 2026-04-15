using System.Collections.Generic;
using UnityEngine;

// ============================================================
// ConversationManager.cs
// 純 C# 類別：管理對話歷史、組裝 API JSON、生成隨機發話 Prompt
// ============================================================

public class ConversationManager
{
    private readonly List<(string role, string content)> history = new List<(string, string)>();
    private readonly CharacterPromptConfig config;

    public ConversationManager(CharacterPromptConfig config)
    {
        this.config = config;
    }

    // --------------------------------------------------------
    // 對話歷史管理
    // --------------------------------------------------------

    public void AddMessage(string role, string content)
    {
        history.Add((role, content));
        TrimHistory();
    }

    private void TrimHistory()
    {
        while (history.Count > config.maxHistoryRounds * 2)
            history.RemoveAt(0);
    }

    // --------------------------------------------------------
    // 組裝對話歷史 JSON（送往 Gemini API 的 contents 欄位）
    // --------------------------------------------------------

    public string BuildContentsJson(string currentMessage, bool isRandomTalk)
    {
        var parts = new List<string>();

        foreach (var (role, content) in history)
        {
            parts.Add($@"{{
                ""role"": ""{role}"",
                ""parts"": [{{ ""text"": {JsonEscape(content)} }}]
            }}");
        }

        if (isRandomTalk)
        {
            parts.Add($@"{{
                ""role"": ""user"",
                ""parts"": [{{ ""text"": {JsonEscape(currentMessage)} }}]
            }}");
        }

        return string.Join(",\n", parts);
    }

    // --------------------------------------------------------
    // 組裝隨機發話 Prompt
    // 結合最近對話歷史 + 隨機話題，讓主動發話有上下文脈絡
    // --------------------------------------------------------

    public string BuildRandomTalkPrompt()
    {
        string recentContext = "";
        int start = Mathf.Max(0, history.Count - 4);
        for (int i = start; i < history.Count; i++)
        {
            var (role, content) = history[i];
            string speaker = role == "user" ? "對方" : "你";
            recentContext += $"{speaker}說過：「{content}」\n";
        }

        string[] topics = config.randomTopics;
        string topic = topics != null && topics.Length > 0
            ? topics[Random.Range(0, topics.Length)]
            : "你所在世界裡的某件事";

        if (string.IsNullOrEmpty(recentContext))
        {
            return $"你突然想起了關於「{topic}」的事，" +
                   $"用你自己的方式自然地說出來，不超過兩句話。";
        }
        else
        {
            return $"你們剛才聊到：\n{recentContext}\n" +
                   $"你因此聯想到了關於「{topic}」的事，" +
                   $"自然地延伸這個話題說出來，不超過兩句話。";
        }
    }

    // --------------------------------------------------------
    // 解析 Gemini API 回應 JSON，擷取 text 欄位
    // --------------------------------------------------------

    public string ParseGeminiResponse(string json)
    {
        try
        {
            int textIndex = json.IndexOf("\"text\":");
            if (textIndex == -1) return null;

            int start = json.IndexOf("\"", textIndex + 7) + 1;
            int end   = json.IndexOf("\"", start);
            string result = json.Substring(start, end - start);
            result = result.Replace("\\n", "\n").Replace("\\\"", "\"");
            return result;
        }
        catch
        {
            Debug.LogError("[TalkDemo] 解析回應失敗");
            return null;
        }
    }

    // --------------------------------------------------------
    // 情緒標籤解析
    // 從回應文字開頭讀取 [HAPPY] 等標籤，並從文字中移除
    // --------------------------------------------------------

    public static EmotionType ParseEmotion(ref string text)
    {
        if (text.StartsWith("[HAPPY]"))     { text = text.Substring(7).TrimStart();  return EmotionType.Happy; }
        if (text.StartsWith("[SAD]"))       { text = text.Substring(5).TrimStart();  return EmotionType.Sad; }
        if (text.StartsWith("[SURPRISED]")) { text = text.Substring(11).TrimStart(); return EmotionType.Surprised; }
        if (text.StartsWith("[ANGRY]"))     { text = text.Substring(7).TrimStart();  return EmotionType.Angry; }
        if (text.StartsWith("[CALM]"))      { text = text.Substring(6).TrimStart();  return EmotionType.Calm; }
        return EmotionType.Neutral;
    }

    // --------------------------------------------------------
    // JSON 字串跳脫工具
    // --------------------------------------------------------

    public static string JsonEscape(string text)
    {
        text = text.Replace("\\", "\\\\")
                   .Replace("\"", "\\\"")
                   .Replace("\n", "\\n")
                   .Replace("\r", "\\r")
                   .Replace("\t", "\\t");
        return $"\"{text}\"";
    }
}
