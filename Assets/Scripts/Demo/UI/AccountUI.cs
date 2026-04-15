using System;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 帳號管理 UI：顯示目前登入狀態，提供匿名/Email 登入、帳號連結、登出。
/// 透過 GameSetup 動態建立 UI 元素後，呼叫 Init() 注入參照。
/// </summary>
public class AccountUI : MonoBehaviour
{
    public static AccountUI Instance { get; private set; }

    private GameObject _panel;
    private Text _statusText;
    private GameObject _emailGroup;
    private InputField _emailInput;
    private InputField _passwordInput;
    private Button _loginBtn;
    private Button _registerBtn;
    private Button _linkBtn;
    private Button _logoutBtn;
    private Button _anonymousBtn;
    private Button _googleBtn;
    private Button _linkGoogleBtn;
    private Button _closeBtn;
    private Text _messageText;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(this); return; }
        Instance = this;
    }

    public void Init(GameObject panel, Text statusText, GameObject emailGroup,
        InputField emailInput, InputField passwordInput,
        Button loginBtn, Button registerBtn, Button linkBtn,
        Button logoutBtn, Button anonymousBtn, Button googleBtn, Button linkGoogleBtn,
        Button closeBtn, Text messageText)
    {
        _panel = panel;
        _statusText = statusText;
        _emailGroup = emailGroup;
        _emailInput = emailInput;
        _passwordInput = passwordInput;
        _loginBtn = loginBtn;
        _registerBtn = registerBtn;
        _linkBtn = linkBtn;
        _logoutBtn = logoutBtn;
        _anonymousBtn = anonymousBtn;
        _googleBtn = googleBtn;
        _linkGoogleBtn = linkGoogleBtn;
        _closeBtn = closeBtn;
        _messageText = messageText;

        _loginBtn.onClick.AddListener(OnLoginClicked);
        _registerBtn.onClick.AddListener(OnRegisterClicked);
        _linkBtn.onClick.AddListener(OnLinkClicked);
        _logoutBtn.onClick.AddListener(OnLogoutClicked);
        _anonymousBtn.onClick.AddListener(OnAnonymousClicked);
        _googleBtn.onClick.AddListener(OnGoogleClicked);
        _linkGoogleBtn.onClick.AddListener(OnLinkGoogleClicked);
        _closeBtn.onClick.AddListener(Hide);

        if (AuthManager.Instance != null)
            AuthManager.Instance.OnAuthStateChanged += _ => RefreshUI();

        _panel.SetActive(false);
    }

    public void Show()
    {
        _panel.SetActive(true);
        RefreshUI();
        ClearMessage();
    }

    public void Hide()
    {
        _panel.SetActive(false);
    }

    public void Toggle()
    {
        if (_panel.activeSelf) Hide(); else Show();
    }

    private void RefreshUI()
    {
        var auth = AuthManager.Instance;
        if (auth == null || !auth.IsReady)
        {
            _statusText.text = "初始化中...";
            return;
        }

        if (!auth.IsAuthenticated)
        {
            _statusText.text = "未登入";
            _emailGroup.SetActive(true);
            _loginBtn.gameObject.SetActive(true);
            _registerBtn.gameObject.SetActive(true);
            _anonymousBtn.gameObject.SetActive(true);
            _linkBtn.gameObject.SetActive(false);
            _logoutBtn.gameObject.SetActive(false);
            _googleBtn.gameObject.SetActive(true);
            _linkGoogleBtn.gameObject.SetActive(false);
        }
        else if (auth.IsAnonymous)
        {
            _statusText.text = $"匿名登入中\nID: {Truncate(auth.UserId, 12)}";
            _emailGroup.SetActive(true);
            _loginBtn.gameObject.SetActive(true);
            _registerBtn.gameObject.SetActive(false);
            _anonymousBtn.gameObject.SetActive(false);
            _linkBtn.gameObject.SetActive(true);
            _logoutBtn.gameObject.SetActive(true);
            _googleBtn.gameObject.SetActive(false);
            _linkGoogleBtn.gameObject.SetActive(true);
        }
        else
        {
            _statusText.text = $"已登入\nID: {Truncate(auth.UserId, 12)}";
            _emailGroup.SetActive(false);
            _loginBtn.gameObject.SetActive(false);
            _registerBtn.gameObject.SetActive(false);
            _anonymousBtn.gameObject.SetActive(false);
            _linkBtn.gameObject.SetActive(false);
            _logoutBtn.gameObject.SetActive(true);
            _googleBtn.gameObject.SetActive(false);
            _linkGoogleBtn.gameObject.SetActive(false);
        }
    }

    // ──────────────── Button Handlers ────────────────

    private void OnLoginClicked()
    {
        if (!ValidateInput()) return;
        SetInteractable(false);
        ShowMessage("登入中...", Color.white);

        AuthManager.Instance.SignInWithEmail(_emailInput.text.Trim(), _passwordInput.text, (ok, err) =>
        {
            SetInteractable(true);
            if (ok)
            {
                ShowMessage("登入成功！", Color.green);
                OnLoginSuccess();
            }
            else
            {
                ShowMessage($"登入失敗: {err}", Color.red);
            }
        });
    }

    private void OnRegisterClicked()
    {
        if (!ValidateInput()) return;
        SetInteractable(false);
        ShowMessage("註冊中...", Color.white);

        AuthManager.Instance.RegisterWithEmail(_emailInput.text.Trim(), _passwordInput.text, (ok, err) =>
        {
            SetInteractable(true);
            if (ok)
            {
                ShowMessage("註冊成功！", Color.green);
                OnLoginSuccess();
            }
            else
            {
                ShowMessage($"註冊失敗: {err}", Color.red);
            }
        });
    }

    private void OnLinkClicked()
    {
        if (!ValidateInput()) return;
        SetInteractable(false);
        ShowMessage("正在連結帳號...", Color.white);

        AuthManager.Instance.LinkEmailToAnonymous(_emailInput.text.Trim(), _passwordInput.text, (ok, err) =>
        {
            SetInteractable(true);
            if (ok)
            {
                ShowMessage("帳號連結成功！匿名進度已保留", Color.green);
                OnLinkSuccess();
            }
            else
            {
                ShowMessage($"連結失敗: {err}", Color.red);
            }
        });
    }

    private void OnAnonymousClicked()
    {
        SetInteractable(false);
        ShowMessage("匿名登入中...", Color.white);

        AuthManager.Instance.SignInAnonymously(() =>
        {
            SetInteractable(true);
            ShowMessage("匿名登入成功", Color.green);
            OnLoginSuccess();
        });
    }

    private void OnLogoutClicked()
    {
        AuthManager.Instance.SignOut();
        ShowMessage("已登出", Color.yellow);
        RefreshUI();
    }

    private void OnGoogleClicked()
    {
        SetInteractable(false);
        ShowMessage("Google 登入中...", Color.white);

        AuthManager.Instance.SignInWithGoogle((ok, err) =>
        {
            SetInteractable(true);
            if (ok)
            {
                ShowMessage("Google 登入成功！", Color.green);
                OnLoginSuccess();
            }
            else
            {
                ShowMessage($"Google 登入失敗: {err}", Color.red);
            }
        });
    }

    private void OnLinkGoogleClicked()
    {
        SetInteractable(false);
        ShowMessage("正在連結 Google 帳號...", Color.white);

        AuthManager.Instance.LinkGoogleToAnonymous((ok, err) =>
        {
            SetInteractable(true);
            if (ok)
            {
                ShowMessage("Google 帳號連結成功！進度已保留", Color.green);
                OnLinkSuccess();
            }
            else
            {
                ShowMessage($"Google 連結失敗: {err}", Color.red);
            }
        });
    }

    // 登入成功（非連結）：強制從雲端載入存檔，確保跨平台同步
    private void OnLoginSuccess()
    {
        RefreshUI();
        Hide();
        if (SaveManager.Instance != null)
            SaveManager.Instance.InitCloud(forceCloudData: true);
    }

    // 帳號連結成功：保留本地進度，不覆蓋
    private void OnLinkSuccess()
    {
        RefreshUI();
        if (SaveManager.Instance != null)
            SaveManager.Instance.InitCloud(forceCloudData: false);
    }

    // ──────────────── Helpers ────────────────

    private bool ValidateInput()
    {
        if (string.IsNullOrWhiteSpace(_emailInput.text))
        {
            ShowMessage("請輸入 Email", Color.red);
            return false;
        }
        if (string.IsNullOrWhiteSpace(_passwordInput.text) || _passwordInput.text.Length < 6)
        {
            ShowMessage("密碼至少 6 個字元", Color.red);
            return false;
        }
        return true;
    }

    private void SetInteractable(bool interactable)
    {
        _loginBtn.interactable = interactable;
        _registerBtn.interactable = interactable;
        _linkBtn.interactable = interactable;
        _anonymousBtn.interactable = interactable;
        _googleBtn.interactable = interactable;
        _linkGoogleBtn.interactable = interactable;
        _logoutBtn.interactable = interactable;
    }

    private void ShowMessage(string msg, Color color)
    {
        _messageText.text = msg;
        _messageText.color = color;
    }

    private void ClearMessage()
    {
        _messageText.text = "";
    }

    private static string Truncate(string s, int maxLen)
    {
        if (string.IsNullOrEmpty(s)) return "";
        return s.Length <= maxLen ? s : s[..maxLen] + "...";
    }
}
