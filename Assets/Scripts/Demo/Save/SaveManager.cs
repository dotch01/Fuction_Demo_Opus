using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// 存檔管理器：統一管理本地與雲端存檔的讀寫。
/// 從各 Manager 收集狀態組裝 GameSaveData，並在 Load 時還原。
/// </summary>
public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance { get; private set; }

    public event Action OnSaveCompleted;
    public event Action OnLoadCompleted;

    private ISaveProvider _localProvider;
    private ISaveProvider _cloudProvider; // Phase 3 注入

    /// <summary>雲端同步是否有未推送的本地變更。</summary>
    public bool IsDirty { get; private set; }

    // 下一次雲端讀取是否強制套用（不比較時間戳）
    private bool _forceNextCloudLoad;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public void Init()
    {
        _localProvider = new LocalSaveProvider();
    }

    /// <summary>Phase 3: 注入雲端存檔提供者。</summary>
    public void SetCloudProvider(ISaveProvider provider)
    {
        _cloudProvider = provider;
    }

    /// <summary>初始化雲端同步（AuthManager 就緒後呼叫）。</summary>
    /// <param name="forceCloudData">true = 強制套用雲端存檔，不比較時間戳（用於登入成功後）。</param>
    public void InitCloud(bool forceCloudData = false)
    {
        if (AuthManager.Instance == null || !AuthManager.Instance.IsReady) return;

        _forceNextCloudLoad = forceCloudData;

        if (IsWebGL)
        {
            // WebGL: 透過 jslib 讀取雲端存檔
            WebGLCloudLoad();

            // 推送 dirty 本地存檔
            if (IsDirty && _localProvider.HasSave())
            {
                var localData = _localProvider.Load();
                if (localData != null)
                    WebGLCloudSave(localData);
            }
            return;
        }

        var cloud = new CloudSaveProvider(AuthManager.Instance.UserId);
        SetCloudProvider(cloud);

        // 嘗試從雲端拉取存檔（forceCloudData 時無視時間戳直接套用）
        bool force = forceCloudData;
        cloud.LoadAsync(cloudData =>
        {
            if (cloudData == null)
            {
                // 雲端無存檔 → 把本地存檔推上去
                var local = _localProvider.Load();
                if (local != null)
                {
                    cloud.Save(local);
                    Debug.Log("[SaveManager] 雲端無存檔，已推送本地存檔至雲端");
                }
                return;
            }

            var localData = _localProvider.Load();
            if (force || localData == null || IsNewer(cloudData, localData))
            {
                _localProvider.Save(cloudData);
                ApplySaveData(cloudData);
                Debug.Log($"[SaveManager] 雲端存檔已同步至本地{(force ? "（強制）" : "")}");
            }
        });

        // 推送 dirty 本地存檔
        if (IsDirty && _localProvider.HasSave())
        {
            var localData = _localProvider.Load();
            if (localData != null)
            {
                cloud.Save(localData);
                IsDirty = false;
            }
        }
    }

    // ──────────────── 存檔 ────────────────

    public void Save()
    {
        var data = CollectSaveData();

        // 本地存檔
        bool localOk = _localProvider.Save(data);

        // 雲端存檔
        if (IsWebGL)
        {
            // WebGL 透過 jslib 非同步寫 Firestore
            if (AuthManager.Instance != null && AuthManager.Instance.IsAuthenticated)
                WebGLCloudSave(data);
            else if (localOk)
                IsDirty = true;
        }
        else if (_cloudProvider != null)
        {
            bool cloudOk = _cloudProvider.Save(data);
            if (!cloudOk) IsDirty = true;
        }
        else if (localOk)
        {
            IsDirty = true;
        }

        if (localOk) OnSaveCompleted?.Invoke();
    }

    // ──────────────── 讀檔 ────────────────

    public void Load()
    {
        GameSaveData data = null;

        // 嘗試讀本地
        if (_localProvider.HasSave())
            data = _localProvider.Load();

        // 若有雲端，比較時間戳取較新者
        if (_cloudProvider != null && _cloudProvider.HasSave())
        {
            var cloudData = _cloudProvider.Load();
            if (cloudData != null)
            {
                if (data == null || IsNewer(cloudData, data))
                {
                    data = cloudData;
                    // 用雲端覆蓋本地快取
                    _localProvider.Save(data);
                }
            }
        }

        if (data != null)
        {
            ApplySaveData(data);
            Debug.Log("[SaveManager] 存檔載入完成");
        }
        else
        {
            Debug.Log("[SaveManager] 無存檔，使用預設狀態");
        }

        OnLoadCompleted?.Invoke();
    }

    // ──────────────── 刪檔 ────────────────

    public void DeleteSave()
    {
        _localProvider.Delete();
        _cloudProvider?.Delete();
        IsDirty = false;
        Debug.Log("[SaveManager] 存檔已清除");
    }

    // ──────────────── 收集各 Manager 狀態 ────────────────

    private GameSaveData CollectSaveData()
    {
        var data = new GameSaveData();

        // Star 掃描狀態
        if (PlanetFactory.Instance != null)
        {
            var allStars = PlanetFactory.Instance.NormalStars
                .Concat(PlanetFactory.Instance.FilterStars);

            foreach (var star in allStars)
            {
                if (!star.IsScanned) continue;
                data.scannedPlanetIds.Add(star.Data.id);
                if (!string.IsNullOrEmpty(star.StarName))
                    data.planetNames.Add(new PlanetNameEntry(star.Data.id, star.StarName));
            }
        }

        // 任務進度
        if (QuestManager.Instance != null)
            data.completedQuestCount = QuestManager.Instance.CompletedCount;

        // 故事進度
        if (GameFlowManager.Instance != null)
        {
            data.gameFlowState = GameFlowManager.Instance.CurrentState.ToString();
            data.playedChapterIds = GameFlowManager.Instance.GetPlayedChapterIds();
        }

        // 相機位置
        var cam = Camera.main;
        if (cam != null)
        {
            data.cameraX = cam.transform.position.x;
            data.cameraY = cam.transform.position.y;
            data.cameraZoom = cam.orthographicSize;
        }

        return data;
    }

    // ──────────────── 還原各 Manager 狀態 ────────────────

    private void ApplySaveData(GameSaveData data)
    {
        // 建立名稱快查表
        var nameMap = new Dictionary<string, string>();
        foreach (var entry in data.planetNames)
            nameMap[entry.planetId] = entry.playerName;

        // 還原 Star 掃描狀態
        if (PlanetFactory.Instance != null)
        {
            var allStars = PlanetFactory.Instance.NormalStars
                .Concat(PlanetFactory.Instance.FilterStars);

            var scannedSet = new HashSet<string>(data.scannedPlanetIds);

            foreach (var star in allStars)
            {
                if (star.Data == null) continue;
                if (!scannedSet.Contains(star.Data.id)) continue;

                string name = nameMap.TryGetValue(star.Data.id, out var n) ? n : star.Data.defaultName;
                star.Scan(name);
            }
        }

        // 還原故事進度
        if (GameFlowManager.Instance != null)
            GameFlowManager.Instance.RestorePlayedChapters(data.playedChapterIds);

        // 還原相機位置
        var cam = Camera.main;
        if (cam != null && (data.cameraX != 0 || data.cameraY != 0))
        {
            cam.transform.position = new Vector3(data.cameraX, data.cameraY, cam.transform.position.z);
            if (data.cameraZoom > 0)
                cam.orthographicSize = data.cameraZoom;
        }

        // QuestManager 的 CompletedCount 靠 Star.IsScanned 自動推導，不需手動設定
        // 通知 UI 更新
        if (QuestManager.Instance != null)
            QuestManager.Instance.NotifyLoadComplete();
    }

    // ──────────────── Helpers ────────────────

    private static bool IsNewer(GameSaveData a, GameSaveData b)
    {
        if (string.IsNullOrEmpty(a.lastSavedUtc)) return false;
        if (string.IsNullOrEmpty(b.lastSavedUtc)) return true;

        if (DateTime.TryParse(a.lastSavedUtc, null, System.Globalization.DateTimeStyles.RoundtripKind, out var ta)
            && DateTime.TryParse(b.lastSavedUtc, null, System.Globalization.DateTimeStyles.RoundtripKind, out var tb))
        {
            return ta > tb;
        }

        return false;
    }

    private static bool IsWebGL =>
#if UNITY_WEBGL && !UNITY_EDITOR
        true;
#else
        false;
#endif

    // ──────────────── WebGL 雲端存檔 ────────────────

    /// <summary>WebGL: 存檔到 Firestore（透過 jslib）。</summary>
    public void WebGLCloudSave(GameSaveData data)
    {
        if (!IsWebGL) return;
        string json = JsonUtility.ToJson(data);
        WebGLFirebaseInterop.FirebaseBridge_SaveToFirestore(json);
    }

    /// <summary>WebGL: 從 Firestore 讀檔（透過 jslib）。</summary>
    public void WebGLCloudLoad()
    {
        if (!IsWebGL) return;
        WebGLFirebaseInterop.FirebaseBridge_LoadFromFirestore();
    }

    /// <summary>WebGL: jslib 存檔結果回呼。格式: "ok|" 或 "fail|errorMsg"</summary>
    public void OnWebGLCloudSaveResult(string data)
    {
        var parts = data.Split('|');
        bool ok = parts.Length > 0 && parts[0] == "ok";

        if (ok)
        {
            IsDirty = false;
            Debug.Log("[SaveManager] WebGL 雲端存檔成功");
        }
        else
        {
            IsDirty = true;
            Debug.LogWarning($"[SaveManager] WebGL 雲端存檔失敗: {(parts.Length > 1 ? parts[1] : "")}");
        }
    }

    /// <summary>WebGL: jslib 讀檔結果回呼。空字串表示無資料。</summary>
    public void OnWebGLCloudLoadResult(string json)
    {
        if (string.IsNullOrEmpty(json))
        {
            // 雲端無存檔 → 把本地存檔推上去
            var local = _localProvider.Load();
            if (local != null && AuthManager.Instance != null && AuthManager.Instance.IsAuthenticated)
            {
                WebGLCloudSave(local);
                Debug.Log("[SaveManager] WebGL 雲端無存檔，已推送本地存檔");
            }
            else
            {
                Debug.Log("[SaveManager] WebGL 雲端無存檔");
            }
            _forceNextCloudLoad = false;
            return;
        }

        var cloudData = JsonUtility.FromJson<GameSaveData>(json);
        if (cloudData == null) return;

        var localData = _localProvider.Load();
        bool force = _forceNextCloudLoad;
        _forceNextCloudLoad = false;
        if (force || localData == null || IsNewer(cloudData, localData))
        {
            _localProvider.Save(cloudData);
            ApplySaveData(cloudData);
            Debug.Log($"[SaveManager] WebGL 雲端存檔已同步{(force ? "（強制）" : "")}");
        }
    }
}
