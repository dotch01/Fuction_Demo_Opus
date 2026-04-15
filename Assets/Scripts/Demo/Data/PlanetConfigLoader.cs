using UnityEngine;

/// <summary>
/// 從 StreamingAssets/PlanetConfig.json 讀取行星設定。
/// WebGL 由 DataLoader 預先注入快取，其他平台亦同。
/// </summary>
public static class PlanetConfigLoader
{
    private static PlanetConfig _cached;

    /// <summary>由 DataLoader 在啟動時注入已解析的資料。</summary>
    public static void Preload(PlanetConfig config) => _cached = config;

    public static PlanetConfig Load(bool forceReload = false)
    {
        if (_cached != null && !forceReload)
            return _cached;

        // 快取未命中時回退（Editor 除錯用）
        Debug.LogWarning("PlanetConfigLoader: cache miss, data may not have loaded yet.");
        return null;
    }
}
