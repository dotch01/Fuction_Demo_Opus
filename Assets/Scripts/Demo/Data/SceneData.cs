using System;

/// <summary>
/// 場景物件互動設定的 JSON 資料結構。
/// 由 StreamingAssets/SceneConfig.json 載入。
/// Excel 欄位對應：scene_id | object_id | label | pos_x | pos_y | width | height |
///                 color_r | color_g | color_b | speaker | portrait | text
/// </summary>
/// 
[Serializable]
public class SceneConfig
{
    public SceneEntry[] scenes;
}

[Serializable]
public class SceneEntry
{
    /// <summary>場景識別碼，例如 "spaceship"。</summary>
    public string id;

    public SceneObjectEntry[] objects;
}

[Serializable]
public class SceneObjectEntry
{
    /// <summary>物件識別碼（同 Excel 中的 object_id）。</summary>
    public string objectId;

    /// <summary>顯示在物件下方的名稱。</summary>
    public string label;

    /// <summary>相對於螢幕中心的位置（像素）。</summary>
    public float posX;
    public float posY;

    /// <summary>物件尺寸（像素）。</summary>
    public float width;
    public float height;

    /// <summary>顏色分量 0.0~1.0。</summary>
    public float colorR;
    public float colorG;
    public float colorB;

    /// <summary>點擊後播放的對話列表。</summary>
    public DialogueLine[] dialogues;
}
