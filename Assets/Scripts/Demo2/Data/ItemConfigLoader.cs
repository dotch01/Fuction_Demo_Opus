using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// 從 StreamingAssets 載入 ItemConfig.json 並快取。
/// WebGL 相容（使用 UnityWebRequest）。
/// </summary>
public class ItemConfigLoader : MonoBehaviour
{
    private static ItemConfig _cached;

    public static bool IsLoaded => _cached != null;
    public static ItemConfig Load() => _cached;

    public static void Preload(ItemConfig config)
    {
        _cached = config;
    }

    public static ItemEntry GetEntry(string id)
    {
        if (_cached == null) return null;
        return _cached.items.Find(e => e.id == id);
    }

    public static List<ItemEntry> GetByCategory(string category)
    {
        if (_cached == null) return new List<ItemEntry>();
        return _cached.items.FindAll(e => e.category == category);
    }

    /// <summary>
    /// 非同步載入 ItemConfig.json。
    /// </summary>
    public IEnumerator LoadItemConfig(System.Action onComplete = null)
    {
        string url = Application.streamingAssetsPath + "/ItemConfig.json";

        using var req = UnityWebRequest.Get(url);
        yield return req.SendWebRequest();

        if (req.result == UnityWebRequest.Result.Success)
        {
            var config = JsonUtility.FromJson<ItemConfig>(req.downloadHandler.text);
            if (config != null)
            {
                Preload(config);
                Debug.Log($"[ItemConfigLoader] Loaded {config.items.Count} items");
            }
        }
        else
        {
            Debug.LogWarning($"[ItemConfigLoader] Failed: {req.error}");
        }

        onComplete?.Invoke();
    }
}
