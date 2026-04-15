using UnityEngine;

/// <summary>
/// 從 StreamingAssets/SceneConfig.json 讀取場景物件互動設定。
/// WebGL 由 DataLoader 預先注入快取，其他平台亦同。
/// </summary>
public static class SceneConfigLoader
{
    private static SceneConfig _cached;

    /// <summary>由 DataLoader 在啟動時注入已解析的資料。</summary>
    public static void Preload(SceneConfig config) => _cached = config;

    public static SceneConfig Load(bool forceReload = false)
    {
        if (_cached != null && !forceReload)
            return _cached;

        Debug.LogWarning("SceneConfigLoader: cache miss, data may not have loaded yet.");
        return null;
    }

    /// <summary>取得指定 id 的場景資料，找不到回傳 null。</summary>
    public static SceneEntry GetScene(string sceneId)
    {
        var config = Load();
        if (config == null) return null;

        foreach (var scene in config.scenes)
            if (scene.id == sceneId)
                return scene;

        return null;
    }
}
