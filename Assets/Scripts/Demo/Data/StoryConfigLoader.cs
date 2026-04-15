using UnityEngine;

/// <summary>
/// 從 StreamingAssets/StoryConfig.json 讀取故事設定。
/// WebGL 由 DataLoader 預先注入快取，其他平台亦同。
/// </summary>
public static class StoryConfigLoader
{
    private static StoryConfig _cached;

    /// <summary>由 DataLoader 在啟動時注入已解析的資料。</summary>
    public static void Preload(StoryConfig config) => _cached = config;

    public static StoryConfig Load(bool forceReload = false)
    {
        if (_cached != null && !forceReload)
            return _cached;

        Debug.LogWarning("StoryConfigLoader: cache miss, data may not have loaded yet.");
        return null;
    }
}
