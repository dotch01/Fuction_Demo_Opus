using System;

/// <summary>
/// 故事/對話的 JSON 資料結構。
/// 由 StreamingAssets/StoryConfig.json 載入。
/// Excel 欄位對應：chapter_id | triggerAfterQuest | speaker | portrait | text | onComplete
/// </summary>

[Serializable]
public class StoryConfig
{
    public ChapterEntry[] chapters;
}

[Serializable]
public class ChapterEntry
{
    /// <summary>章節唯一識別碼，例如 "chapter_0"。</summary>
    public string id;

    /// <summary>
    /// 觸發條件：完成第幾顆任務行星後播放。
    /// 0 = 遊戲開始時，1 = 第 1 顆完成後，2 = 第 2 顆完成後...
    /// </summary>
    public int triggerAfterQuest;

    /// <summary>對話行列表。</summary>
    public DialogueLine[] dialogues;

    /// <summary>
    /// 章節結束後執行的動作。
    /// "openStarMap" = 切到望遠鏡模式
    /// "returnToShip" = 切回太空船模式
    /// 空字串 = 不切換
    /// </summary>
    public string onComplete;
}

[Serializable]
public class DialogueLine
{
    /// <summary>說話者名稱。</summary>
    public string speaker;

    /// <summary>立繪圖片檔名（對應 Resources/Portraits/），空字串＝無立繪。</summary>
    public string portrait;

    /// <summary>對話內容。</summary>
    public string text;
}
