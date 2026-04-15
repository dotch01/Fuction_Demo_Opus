using System;
using System.Collections.Generic;

/// <summary>
/// 遊戲存檔資料模型。可序列化為 JSON，用於本地和雲端存檔。
/// </summary>
[Serializable]
public class GameSaveData
{
    public int version = 1;
    public string lastSavedUtc;

    // 星球掃描狀態
    public List<string> scannedPlanetIds = new();
    public List<PlanetNameEntry> planetNames = new();

    // 任務進度
    public int completedQuestCount;

    // 故事進度
    public List<string> playedChapterIds = new();

    // 遊戲流程狀態
    public string gameFlowState;

    // 相機狀態 (可選)
    public float cameraX;
    public float cameraY;
    public float cameraZoom;

    // 本地完整性校驗
    public string checksum;
}

/// <summary>
/// 玩家自訂星球名稱的鍵值對（因 JsonUtility 不支援 Dictionary）。
/// </summary>
[Serializable]
public class PlanetNameEntry
{
    public string planetId;
    public string playerName;

    public PlanetNameEntry() { }

    public PlanetNameEntry(string planetId, string playerName)
    {
        this.planetId = planetId;
        this.playerName = playerName;
    }
}
