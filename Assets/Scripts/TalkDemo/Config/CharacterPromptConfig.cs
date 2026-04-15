using UnityEngine;

// ============================================================
// CharacterPromptConfig.cs
// ScriptableObject：角色人設、隨機發話、對話歷史等設定
//
// 使用方式：
// 在 Project 視窗右鍵 → Create → TalkDemo → Character Prompt Config
// 可建立多份設定檔，隨時切換不同角色人設
// ============================================================

[CreateAssetMenu(menuName = "TalkDemo/Character Prompt Config")]
public class CharacterPromptConfig : ScriptableObject
{
    [Header("角色人設")]
    [TextArea(5, 15)]
    public string systemPrompt =
        "你是來自某個奇幻世界的角色。你只談論這個世界裡的事情，" +
        "不提及現實世界。你的說話風格是沉穩且帶有神秘感的。" +
        "每次回應不超過三句話。";

    [Header("隨機發話設定")]
    public float randomTalkMinSeconds = 30f;
    public float randomTalkMaxSeconds = 120f;
    public bool enableRandomTalk = true;

    [Header("對話話題範圍")]
    public string[] randomTopics = {
        "你所在世界最近發生的某件奇怪的事",
        "你過去某段印象深刻的記憶",
        "你對某個地方的特殊感受",
        "你正在思考的某個問題或謎題",
        "你曾經遇過的某個有趣的人"
    };

    [Header("對話歷史上限（輪數）")]
    public int maxHistoryRounds = 10;
}
