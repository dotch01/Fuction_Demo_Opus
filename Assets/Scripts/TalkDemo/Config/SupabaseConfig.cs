using UnityEngine;

// ============================================================
// SupabaseConfig.cs
// ScriptableObject：Supabase 向量搜尋（RAG）連線設定
//
// 使用方式：
// 在 Project 視窗右鍵 → Create → TalkDemo → Supabase Config
// ============================================================

[CreateAssetMenu(menuName = "TalkDemo/Supabase Config")]
public class SupabaseConfig : ScriptableObject
{
    [Header("Supabase 連線")]
    [Tooltip("Supabase 專案 URL，例如 https://xxxxx.supabase.co")]
    public string supabaseUrl = "https://your-project.supabase.co";

    [Tooltip("Supabase anon (public) key")]
    public string supabaseAnonKey = "your-anon-key";

    [Header("搜尋設定")]
    [Tooltip("每次搜尋回傳的最大段落數")]
    [Range(1, 10)]
    public int matchCount = 3;

    [Tooltip("注入 prompt 的最大字數上限")]
    public int maxContextChars = 2000;

    [Header("RAG 開關")]
    [Tooltip("關閉時直接使用原本的 System Prompt，不查詢 Supabase")]
    public bool enableRAG = true;
}
