using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// WebGL 相容的 JSON 資料載入器。
/// 使用 UnityWebRequest（支援 http:// 和 file://），
/// 載入完成後呼叫各 Loader 的 Preload() 注入快取。
/// Editor / Standalone 同樣走此路徑，避免平台差異。
/// </summary>
public class DataLoader : MonoBehaviour
{
    public IEnumerator LoadAll(System.Action onComplete = null)
    {
        yield return LoadJson("PlanetConfig.json", json =>
        {
            var cfg = JsonUtility.FromJson<PlanetConfig>(json);
            if (cfg != null) PlanetConfigLoader.Preload(cfg);
        });

        yield return LoadJson("StoryConfig.json", json =>
        {
            var cfg = JsonUtility.FromJson<StoryConfig>(json);
            if (cfg != null) StoryConfigLoader.Preload(cfg);
        });

        yield return LoadJson("SceneConfig.json", json =>
        {
            var cfg = JsonUtility.FromJson<SceneConfig>(json);
            if (cfg != null) SceneConfigLoader.Preload(cfg);
        });

        onComplete?.Invoke();
    }

    private IEnumerator LoadJson(string filename, System.Action<string> onSuccess)
    {
        // Application.streamingAssetsPath 在 WebGL 已是完整 URL（https://...）
        string url = Application.streamingAssetsPath + "/" + filename;

        using var req = UnityWebRequest.Get(url);
        yield return req.SendWebRequest();

        if (req.result == UnityWebRequest.Result.Success)
        {
            onSuccess(req.downloadHandler.text);
            Debug.Log($"[DataLoader] Loaded: {filename}");
        }
        else
        {
            Debug.LogWarning($"[DataLoader] Failed to load {filename}: {req.error}");
        }
    }
}
