using UnityEngine;

// ============================================================
// GeminiApiConfig.cs
// ScriptableObject：Gemini API 連線設定
//
// 使用方式：
// 在 Project 視窗右鍵 → Create → TalkDemo → Gemini API Config
// ============================================================

[CreateAssetMenu(menuName = "TalkDemo/Gemini API Config")]
public class GeminiApiConfig : ScriptableObject
{
    [Header("API 設定")]
    public string apiKey = "在這裡填入你的 Gemini API Key";

    [Header("API 端點")]
    [Tooltip("如果 flash-lite 回 429，試試改成 gemini-2.0-flash")]
    public string apiUrl =
        "https://generativelanguage.googleapis.com/v1beta/models/" +
        "gemini-2.0-flash:generateContent";
}
