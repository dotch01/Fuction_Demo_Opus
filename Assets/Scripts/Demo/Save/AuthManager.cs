using System;
using System.Collections.Generic;
using UnityEngine;
#if FIREBASE_AUTH
using Firebase;
using Firebase.Auth;
using Firebase.Extensions;
#endif

/// <summary>
/// Firebase 驗證管理器：支援匿名登入、Email 登入、帳號連結升級。
/// PC/Android: 使用 Firebase Auth SDK（需 FIREBASE_AUTH 定義）。
/// WebGL: 使用 JS SDK 透過 jslib 橋接。
/// 無 SDK 時: fallback 到本地 GUID。
/// </summary>
public class AuthManager : MonoBehaviour
{
    public static AuthManager Instance { get; private set; }

    public event Action<string> OnAuthStateChanged;

    public string UserId { get; private set; }
    public bool IsAuthenticated { get; private set; }
    public bool IsAnonymous { get; private set; }
    public bool IsReady { get; private set; }

    private bool IsWebGL =>
#if UNITY_WEBGL && !UNITY_EDITOR
        true;
#else
        false;
#endif

#if FIREBASE_AUTH
    private FirebaseAuth _auth;
#endif

    // WebGL callback 暫存
    private Action _pendingReadyCallback;
    private readonly Dictionary<string, Action<bool, string>> _pendingCallbacks = new();

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public void Init(Action onReady = null)
    {
        if (IsWebGL)
        {
            InitWebGL(onReady);
            return;
        }

#if FIREBASE_AUTH
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
        {
            if (task.Result == DependencyStatus.Available)
            {
                _auth = FirebaseAuth.DefaultInstance;
                _auth.StateChanged += OnFirebaseAuthStateChanged;

                if (_auth.CurrentUser != null)
                {
                    UpdateUserInfo(_auth.CurrentUser);
                    IsReady = true;
                    onReady?.Invoke();
                }
                else
                {
                    SignInAnonymously(onReady);
                }

                Debug.Log("[AuthManager] Firebase 初始化完成");
            }
            else
            {
                Debug.LogError($"[AuthManager] Firebase 依賴問題: {task.Result}");
            }
        });
#else
        UserId = LoadOrCreateLocalId();
        IsAuthenticated = true;
        IsAnonymous = true;
        IsReady = true;
        Debug.Log($"[AuthManager] 離線模式，本地 ID: {UserId}");
        onReady?.Invoke();
#endif
    }

    // ──────────────── 匿名登入 ────────────────

    public void SignInAnonymously(Action onComplete = null)
    {
        if (IsWebGL)
        {
            _pendingReadyCallback = onComplete;
            WebGLFirebaseInterop.FirebaseBridge_SignInAnonymously();
            return;
        }

#if FIREBASE_AUTH
        _auth.SignInAnonymouslyAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted)
            {
                Debug.LogError($"[AuthManager] 匿名登入失敗: {task.Exception}");
                UserId = LoadOrCreateLocalId();
                IsAuthenticated = true;
                IsAnonymous = true;
            }
            else
            {
                UpdateUserInfo(task.Result.User);
            }
            IsReady = true;
            onComplete?.Invoke();
        });
#else
        onComplete?.Invoke();
#endif
    }

    // ──────────────── Email 登入 / 註冊 ────────────────

    public void SignInWithEmail(string email, string password, Action<bool, string> callback)
    {
        if (IsWebGL)
        {
            _pendingCallbacks["email"] = callback;
            WebGLFirebaseInterop.FirebaseBridge_SignInWithEmail(email, password);
            return;
        }

#if FIREBASE_AUTH
        _auth.SignInWithEmailAndPasswordAsync(email, password).ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted)
                callback?.Invoke(false, task.Exception?.GetBaseException()?.Message ?? "登入失敗");
            else
            {
                UpdateUserInfo(task.Result.User);
                callback?.Invoke(true, null);
            }
        });
#else
        callback?.Invoke(false, "Firebase SDK 未安裝");
#endif
    }

    public void RegisterWithEmail(string email, string password, Action<bool, string> callback)
    {
        if (IsWebGL)
        {
            _pendingCallbacks["register"] = callback;
            WebGLFirebaseInterop.FirebaseBridge_RegisterWithEmail(email, password);
            return;
        }

#if FIREBASE_AUTH
        _auth.CreateUserWithEmailAndPasswordAsync(email, password).ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted)
                callback?.Invoke(false, task.Exception?.GetBaseException()?.Message ?? "註冊失敗");
            else
            {
                UpdateUserInfo(task.Result.User);
                callback?.Invoke(true, null);
            }
        });
#else
        callback?.Invoke(false, "Firebase SDK 未安裝");
#endif
    }

    // ──────────────── 帳號連結（匿名 → Email）────────────────

    public void LinkEmailToAnonymous(string email, string password, Action<bool, string> callback)
    {
        if (IsWebGL)
        {
            _pendingCallbacks["link"] = callback;
            WebGLFirebaseInterop.FirebaseBridge_LinkEmailToAnonymous(email, password);
            return;
        }

#if FIREBASE_AUTH
        if (_auth.CurrentUser == null || !_auth.CurrentUser.IsAnonymous)
        {
            callback?.Invoke(false, "目前帳號非匿名");
            return;
        }

        var credential = EmailAuthProvider.GetCredential(email, password);
        _auth.CurrentUser.LinkWithCredentialAsync(credential).ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted)
                callback?.Invoke(false, task.Exception?.GetBaseException()?.Message ?? "連結失敗");
            else
            {
                UpdateUserInfo(task.Result.User);
                callback?.Invoke(true, null);
            }
        });
#else
        callback?.Invoke(false, "Firebase SDK 未安裝");
#endif
    }

    // ──────────────── 登出 ────────────────

    public void SignOut()
    {
        if (IsWebGL)
        {
            WebGLFirebaseInterop.FirebaseBridge_SignOut();
            // onAuthStateChanged callback 會處理狀態更新
            return;
        }

#if FIREBASE_AUTH
        _auth.SignOut();
#endif
        UserId = null;
        IsAuthenticated = false;
        IsAnonymous = false;
        OnAuthStateChanged?.Invoke(null);
    }

    // ──────────────── Google 登入 ────────────────

    public void SignInWithGoogle(Action<bool, string> callback)
    {
        if (IsWebGL)
        {
            _pendingCallbacks["google"] = callback;
            WebGLFirebaseInterop.FirebaseBridge_SignInWithGoogle();
            return;
        }

        // PC/Android: 需要額外的 Google Sign-In SDK
        callback?.Invoke(false, "Google 登入目前僅支援 WebGL 平台");
    }

    public void LinkGoogleToAnonymous(Action<bool, string> callback)
    {
        if (IsWebGL)
        {
            _pendingCallbacks["linkGoogle"] = callback;
            WebGLFirebaseInterop.FirebaseBridge_LinkGoogleToAnonymous();
            return;
        }

        callback?.Invoke(false, "Google 連結目前僅支援 WebGL 平台");
    }

    // ──────────────── WebGL Callbacks (由 jslib SendMessage 呼叫) ────────────────

    /// <summary>WebGL: 驗證狀態變化回呼。格式: "userId|isAnonymous(0/1)"</summary>
    public void OnWebGLAuthChanged(string data)
    {
        var parts = data.Split('|');
        string uid = parts.Length > 0 ? parts[0] : "";
        bool isAnon = parts.Length > 1 && parts[1] == "1";

        if (!string.IsNullOrEmpty(uid))
        {
            UserId = uid;
            IsAuthenticated = true;
            IsAnonymous = isAnon;
        }
        else
        {
            UserId = null;
            IsAuthenticated = false;
            IsAnonymous = false;
        }

        IsReady = true;
        OnAuthStateChanged?.Invoke(UserId);
    }

    /// <summary>WebGL: 驗證操作結果回呼。格式: "type|ok/fail|errorMsg"</summary>
    public void OnWebGLAuthResult(string data)
    {
        var parts = data.Split('|');
        string type = parts.Length > 0 ? parts[0] : "";
        bool ok = parts.Length > 1 && parts[1] == "ok";
        string error = parts.Length > 2 ? parts[2] : "";

        // 匿名登入透過 ready callback
        if (type == "anonymous")
        {
            IsReady = true;
            _pendingReadyCallback?.Invoke();
            _pendingReadyCallback = null;
            return;
        }

        // 其他操作透過 pending callbacks
        if (_pendingCallbacks.TryGetValue(type, out var cb))
        {
            _pendingCallbacks.Remove(type);
            cb?.Invoke(ok, ok ? null : error);
        }
    }

    // ──────────────── WebGL Init ────────────────

    private void InitWebGL(Action onReady)
    {
        _pendingReadyCallback = onReady;

        // 讀取 Firebase config（從 StreamingAssets/firebase-config.json）
        var configPath = System.IO.Path.Combine(Application.streamingAssetsPath, "firebase-config.json");

#if UNITY_WEBGL && !UNITY_EDITOR
        // WebGL 的 StreamingAssets 需要用 UnityWebRequest
        StartCoroutine(LoadConfigAndInit(configPath));
#else
        onReady?.Invoke();
#endif
    }

    private System.Collections.IEnumerator LoadConfigAndInit(string url)
    {
        using var request = UnityEngine.Networking.UnityWebRequest.Get(url);
        yield return request.SendWebRequest();

        if (request.result == UnityEngine.Networking.UnityWebRequest.Result.Success)
        {
            WebGLFirebaseInterop.FirebaseBridge_Init(request.downloadHandler.text);
            Debug.Log("[AuthManager] WebGL Firebase 初始化中...");

            // 等待 onAuthStateChanged 回呼
            yield return new WaitUntil(() => IsReady);
            _pendingReadyCallback?.Invoke();
            _pendingReadyCallback = null;
        }
        else
        {
            Debug.LogError($"[AuthManager] 無法載入 firebase-config.json: {request.error}");
            // Fallback 到本地
            UserId = LoadOrCreateLocalId();
            IsAuthenticated = true;
            IsAnonymous = true;
            IsReady = true;
            _pendingReadyCallback?.Invoke();
            _pendingReadyCallback = null;
        }
    }

    // ──────────────── Internal (Native) ────────────────

#if FIREBASE_AUTH
    private void OnFirebaseAuthStateChanged(object sender, EventArgs e)
    {
        var user = _auth.CurrentUser;
        if (user != null)
            UpdateUserInfo(user);
        else
        {
            UserId = null;
            IsAuthenticated = false;
        }
        OnAuthStateChanged?.Invoke(UserId);
    }

    private void UpdateUserInfo(FirebaseUser user)
    {
        UserId = user.UserId;
        IsAuthenticated = true;
        IsAnonymous = user.IsAnonymous;
        OnAuthStateChanged?.Invoke(UserId);
    }

    private void OnDestroy()
    {
        if (_auth != null)
            _auth.StateChanged -= OnFirebaseAuthStateChanged;
    }
#endif

    private static string LoadOrCreateLocalId()
    {
        const string key = "LocalUserId";
        if (PlayerPrefs.HasKey(key))
            return PlayerPrefs.GetString(key);

        string id = Guid.NewGuid().ToString();
        PlayerPrefs.SetString(key, id);
        PlayerPrefs.Save();
        return id;
    }
}
