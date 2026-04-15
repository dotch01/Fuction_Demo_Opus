using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

/// <summary>
/// 場景啟動器：在 Runtime 自動建立所有必要的 GameObject 和 UI。
/// 掛在場景中任一空物件即可，不需手動設定 Inspector。
/// </summary>
public class GameSetup : MonoBehaviour
{
    [Header("鏡頭")]
    [SerializeField] private float orthographicSize = 12f;
    [SerializeField] private Color backgroundColor = new(0.02f, 0.02f, 0.08f, 1f);

    private void Awake()
    {
        SetupCamera();
        // JSON 資料需非同步載入（WebGL 須用 UnityWebRequest）
        // DataLoader 載完後再呼叫 CreateManagers / CreateUI
        var loaderGo = new GameObject("DataLoader");
        var loader = loaderGo.AddComponent<DataLoader>();
        StartCoroutine(loader.LoadAll(AfterLoad));
    }

    private void AfterLoad()
    {
        CreateManagers();
        CreateUI();
    }

    private Vector3 StartPosition => new(25f, 75f, -10f);

    private void SetupCamera()
    {
        Camera cam = Camera.main;
        cam.orthographic = true;
        cam.orthographicSize = orthographicSize;
        cam.backgroundColor = backgroundColor;
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.transform.position = StartPosition; // 起始在 Q1 中央

        if (cam.GetComponent<CameraController>() == null)
            cam.gameObject.AddComponent<CameraController>();
    }

    private void CreateManagers()
    {
        // QuadrantManager
        var qmGo = new GameObject("QuadrantManager");
        qmGo.AddComponent<QuadrantManager>();

        // 讀取 JSON 設定
        var config = PlanetConfigLoader.Load();
        if (config == null) return;

        // PlanetFactory (取代 StarSpawner)
        var pfGo = new GameObject("PlanetFactory");
        var factory = pfGo.AddComponent<PlanetFactory>();
        factory.BuildFromConfig(config);

        // StarSystemManager
        var ssmGo = new GameObject("StarSystemManager");
        var ssm = ssmGo.AddComponent<StarSystemManager>();
        ssm.Init(config.starSystems);

        // FilterManager
        var fmGo = new GameObject("FilterManager");
        fmGo.AddComponent<FilterManager>();
    }

    private void CreateUI()
    {
        // === EventSystem (必須，否則所有 UI 按鈕都不回應) ===
        if (FindAnyObjectByType<EventSystem>() == null)
        {
            var esGo = new GameObject("EventSystem");
            esGo.AddComponent<EventSystem>();
            esGo.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
        }

        // === Canvas ===
        var canvasGo = new GameObject("HUD_Canvas");
        var canvas = canvasGo.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;
        canvasGo.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        canvasGo.GetComponent<CanvasScaler>().referenceResolution = new Vector2(1920, 1080);
        canvasGo.AddComponent<GraphicRaycaster>();

        RectTransform canvasRT = canvasGo.GetComponent<RectTransform>();

        // === 座標文字 (上方中央) ===
        var posText = CreateUIText(canvasRT, "PositionText", "(0.00, 0.00)",
            new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0, -40f), 28);

        // === 象限文字 (座標下方) ===
        var quadText = CreateUIText(canvasRT, "QuadrantText", "第1象限",
            new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0, -75f), 22);

        // === 星域文字 (象限下方) ===
        var sysText = CreateUIText(canvasRT, "StarSystemText", "無星域",
            new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0, -105f), 20);

        // === HUDManager ===
        var hudManager = canvasGo.AddComponent<HUDManager>();
        SetPrivateField(hudManager, "positionText", posText);
        SetPrivateField(hudManager, "quadrantText", quadText);
        SetPrivateField(hudManager, "starSystemText", sysText);

        // === 掃描框 (螢幕中央) ===
        var reticleFrame = CreateReticle(canvasRT);

        // === 掃描面板 (掃描框下方) ===
        var scanPanel = new GameObject("ScanPanel");
        scanPanel.transform.SetParent(canvasRT, false);
        var scanPanelRT = scanPanel.AddComponent<RectTransform>();
        scanPanelRT.anchorMin = new Vector2(0.5f, 0.5f);
        scanPanelRT.anchorMax = new Vector2(0.5f, 0.5f);
        scanPanelRT.anchoredPosition = new Vector2(0, -120f);
        scanPanelRT.sizeDelta = new Vector2(250f, 100f);

        // 目標名稱
        var targetName = CreateUIText(scanPanelRT, "TargetName", "",
            new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0, 0), 20);

        // 掃描按鈕
        var btnGo = new GameObject("ScanButton");
        btnGo.transform.SetParent(scanPanelRT, false);
        var btnRT = btnGo.AddComponent<RectTransform>();
        btnRT.anchorMin = new Vector2(0.5f, 0.5f);
        btnRT.anchorMax = new Vector2(0.5f, 0.5f);
        btnRT.anchoredPosition = new Vector2(0, -15f);
        btnRT.sizeDelta = new Vector2(200f, 45f);

        var btnImage = btnGo.AddComponent<Image>();
        btnImage.color = new Color(0.3f, 0.6f, 0.85f, 0.85f);
        var btn = btnGo.AddComponent<Button>();
        btn.targetGraphic = btnImage;

        var btnText = CreateUIText(btnRT, "BtnText", "掃描",
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, 22);

        // 掃描提示
        var hintText = CreateUIText(scanPanelRT, "HintText", "掃描 [ 空白鍵 ]",
            new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0, -5f), 16);
        hintText.color = new Color(1, 1, 1, 0.5f);

        // === ScanSystem ===
        var scanSystem = canvasGo.AddComponent<ScanSystem>();
        SetPrivateField(scanSystem, "reticleFrame", reticleFrame.GetComponent<RectTransform>());
        SetPrivateField(scanSystem, "scanPanel", scanPanel);
        SetPrivateField(scanSystem, "scanButton", btn);
        SetPrivateField(scanSystem, "scanButtonText", btnText);
        SetPrivateField(scanSystem, "scanHintText", hintText);
        SetPrivateField(scanSystem, "targetNameText", targetName);
        scanSystem.LateInit();

        // === 回到初始位置按鈕 (左下角) ===
        CreateActionButton(canvasRT, "ReturnBtn", "回到起點",
            new Vector2(0, 0), new Vector2(0, 0), new Vector2(90, 40),
            new Vector2(120, 40), () =>
            {
                var cc = Camera.main.GetComponent<CameraController>();
                if (cc != null) cc.ReturnToStart();
            });

        // === 濾鏡按鈕 (右下角) ===
        CreateActionButton(canvasRT, "FilterBtn", "濾鏡",
            new Vector2(1, 0), new Vector2(1, 0), new Vector2(-80, 40),
            new Vector2(120, 40), () =>
            {
                if (FilterManager.Instance != null) FilterManager.Instance.Toggle();
            });

        // === 特別觀察按鈕 (上方中央偏上) ===
        CreateActionButton(canvasRT, "QuestBtn", "特別觀察 [ Q ]",
            new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0, -10f),
            new Vector2(180, 35), () =>
            {
                if (QuestPanelUI.Instance != null) QuestPanelUI.Instance.TogglePanel();
            });

        // === 任務面板 ===
        var (questPanelGo, questContentRT) = CreateQuestPanel(canvasRT);

        // === 定位提示面板 ===
        CreateLocatorPanel(canvasRT);

        // === 方向提示系統 ===
        CreateQuestHintUI(canvasRT);

        // === 行星詳情面板 ===
        CreateDetailPanel(canvasRT);

        // === QuestManager ===
        var qmGo = new GameObject("QuestManager");
        var qm = qmGo.AddComponent<QuestManager>();
        qm.Init();

        // === QuestPanelUI (掛在 Canvas 上，確保 Update 持續執行) ===
        var questUI = canvasGo.AddComponent<QuestPanelUI>();
        questUI.Init(questPanelGo, questContentRT);

        // === 將星圖專用 UI 收進 StarMapGroup（切太空船時隱藏此群組）===
        var starMapGroup = new GameObject("StarMapGroup");
        starMapGroup.transform.SetParent(canvasRT, false);
        var smgRT = starMapGroup.AddComponent<RectTransform>();
        smgRT.anchorMin = Vector2.zero;
        smgRT.anchorMax = Vector2.one;
        smgRT.offsetMin = Vector2.zero;
        smgRT.offsetMax = Vector2.zero;

        // 把目前 canvasRT 下所有子物件（StarMapGroup 本身除外）移進去
        var children = new System.Collections.Generic.List<Transform>();
        for (int i = 0; i < canvasRT.childCount; i++)
        {
            var child = canvasRT.GetChild(i);
            if (child.gameObject != starMapGroup)
                children.Add(child);
        }
        foreach (var child in children)
            child.SetParent(starMapGroup.transform, false);

        // === 對話系統 UI（留在 Canvas 上，不進 StarMapGroup）===
        CreateDialoguePanel(canvasRT);

        // === 太空船視圖 ===
        var shipGo = new GameObject("SpaceshipManager");
        var shipView = shipGo.AddComponent<SpaceshipView>();
        shipView.Init();

        // === 模式切換按鈕（留在 Canvas 上，永遠可見）===
        CreateActionButton(canvasRT, "ModeBtn", "望遠鏡 / 太空船",
            new Vector2(1, 0), new Vector2(1, 0), new Vector2(-110, 90),
            new Vector2(180, 40), () =>
            {
                if (GameFlowManager.Instance != null)
                    GameFlowManager.Instance.ToggleMode();
            });

        // === 帳號按鈕（留在 Canvas 上，永遠可見，左上角）===
        CreateActionButton(canvasRT, "AccountBtn", "帳號",
            new Vector2(0, 1), new Vector2(0, 1), new Vector2(60, -40),
            new Vector2(90, 35), () =>
            {
                if (AccountUI.Instance != null) AccountUI.Instance.Toggle();
            });

        // === 帳號管理面板（留在 Canvas 上）===
        CreateAccountPanel(canvasRT);

        // === GameFlowManager（傳入 StarMapGroup 而非整個 Canvas）===
        var flowGo = new GameObject("GameFlowManager");
        var flow = flowGo.AddComponent<GameFlowManager>();
        flow.Init(starMapGroup);

        // === SaveManager（最後初始化，確保所有 Manager 已就緒）===
        var saveGo = new GameObject("SaveManager");
        var save = saveGo.AddComponent<SaveManager>();
        save.Init();
        save.Load();

        // === AuthManager（Firebase 驗證）===
        var authGo = new GameObject("AuthManager");
        var auth = authGo.AddComponent<AuthManager>();
        auth.Init(() =>
        {
            // 驗證完成後啟動雲端同步
            if (SaveManager.Instance != null)
                SaveManager.Instance.InitCloud();
            // 更新帳號 UI
            if (AccountUI.Instance != null)
                AccountUI.Instance.Show();
        });

        // === 新手提示 ===
        TutorialHint.Show(canvasRT,
            "WASD / 滑鼠拖曳：移動鏡頭\n" +
            "將掃描框對準星球，按空白鍵掃描\n" +
            "Q：任務面板　右下：模式切換\n" +
            "左上帳號可登入並同步雲端存檔", this);
    }

    /// <summary>
    /// 建立帳號管理面板。
    /// </summary>
    private void CreateAccountPanel(RectTransform canvasRT)
    {
        // 全螢幕半透明遮罩
        var panelGo = new GameObject("AccountPanel");
        panelGo.transform.SetParent(canvasRT, false);
        var panelRT = panelGo.AddComponent<RectTransform>();
        panelRT.anchorMin = Vector2.zero;
        panelRT.anchorMax = Vector2.one;
        panelRT.offsetMin = Vector2.zero;
        panelRT.offsetMax = Vector2.zero;
        panelGo.AddComponent<Image>().color = new Color(0, 0, 0, 0.6f);

        // 中央卡片
        var card = new GameObject("Card");
        card.transform.SetParent(panelRT, false);
        var cardRT = card.AddComponent<RectTransform>();
        cardRT.anchorMin = new Vector2(0.5f, 0.5f);
        cardRT.anchorMax = new Vector2(0.5f, 0.5f);
        cardRT.sizeDelta = new Vector2(420, 480);
        card.AddComponent<Image>().color = new Color(0.08f, 0.08f, 0.14f, 0.97f);

        // 標題
        var title = CreateUIText(cardRT, "Title", "帳號管理",
            new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -25), 24);

        // 狀態文字
        var statusText = CreateUIText(cardRT, "StatusText", "初始化中...",
            new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -65), 16);
        statusText.color = new Color(0.7f, 0.85f, 1f);

        // Email 輸入區
        var emailGroup = new GameObject("EmailGroup");
        emailGroup.transform.SetParent(cardRT, false);
        var egRT = emailGroup.AddComponent<RectTransform>();
        egRT.anchorMin = new Vector2(0.5f, 0.5f);
        egRT.anchorMax = new Vector2(0.5f, 0.5f);
        egRT.anchoredPosition = new Vector2(0, 50);
        egRT.sizeDelta = new Vector2(340, 120);

        // Email InputField
        var emailInput = CreateInputField(egRT, "EmailInput", "輸入 Email...",
            new Vector2(0, 30), new Vector2(340, 40));

        // Password InputField
        var passInput = CreateInputField(egRT, "PasswordInput", "輸入密碼（至少6字）...",
            new Vector2(0, -20), new Vector2(340, 40));
        passInput.contentType = InputField.ContentType.Password;

        // 按鈕區
        float btnY = -60f;
        float btnSpacing = 48f;

        var loginBtn = CreatePanelButton(cardRT, "LoginBtn", "Email 登入",
            new Vector2(0, btnY), new Color(0.2f, 0.5f, 0.8f, 0.9f));

        var registerBtn = CreatePanelButton(cardRT, "RegisterBtn", "Email 註冊",
            new Vector2(0, btnY - btnSpacing), new Color(0.3f, 0.6f, 0.4f, 0.9f));

        var linkBtn = CreatePanelButton(cardRT, "LinkBtn", "連結帳號（保留匿名進度）",
            new Vector2(0, btnY - btnSpacing), new Color(0.6f, 0.5f, 0.2f, 0.9f));

        var anonymousBtn = CreatePanelButton(cardRT, "AnonymousBtn", "匿名登入",
            new Vector2(0, btnY - btnSpacing * 2), new Color(0.4f, 0.4f, 0.5f, 0.9f));

        var googleBtn = CreatePanelButton(cardRT, "GoogleBtn", "Google 登入",
            new Vector2(0, btnY - btnSpacing * 3), new Color(0.85f, 0.33f, 0.24f, 0.9f));

        var linkGoogleBtn = CreatePanelButton(cardRT, "LinkGoogleBtn", "連結 Google（保留進度）",
            new Vector2(0, btnY - btnSpacing * 3), new Color(0.85f, 0.5f, 0.2f, 0.9f));

        var logoutBtn = CreatePanelButton(cardRT, "LogoutBtn", "登出",
            new Vector2(0, btnY - btnSpacing * 4), new Color(0.7f, 0.3f, 0.3f, 0.9f));

        // 訊息文字（底部）
        var msgText = CreateUIText(cardRT, "MessageText", "",
            new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0, 50), 14);
        msgText.color = Color.white;

        // 關閉按鈕（右上角）
        var closeGo = new GameObject("CloseBtn");
        closeGo.transform.SetParent(cardRT, false);
        var closeRT = closeGo.AddComponent<RectTransform>();
        closeRT.anchorMin = new Vector2(1, 1);
        closeRT.anchorMax = new Vector2(1, 1);
        closeRT.anchoredPosition = new Vector2(-20, -20);
        closeRT.sizeDelta = new Vector2(30, 30);
        var closeBg = closeGo.AddComponent<Image>();
        closeBg.color = new Color(0, 0, 0, 0);
        var closeBtn = closeGo.AddComponent<Button>();
        closeBtn.targetGraphic = closeBg;
        CreateUIText(closeRT, "X", "X",
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, 22);

        // 掛 AccountUI 元件
        var accountUI = canvasRT.gameObject.AddComponent<AccountUI>();
        accountUI.Init(panelGo, statusText, emailGroup, emailInput, passInput,
            loginBtn, registerBtn, linkBtn, logoutBtn, anonymousBtn, googleBtn, linkGoogleBtn, closeBtn, msgText);
    }

    private InputField CreateInputField(RectTransform parent, string name, string placeholder,
        Vector2 pos, Vector2 size)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;

        var bg = go.AddComponent<Image>();
        bg.color = new Color(0.15f, 0.15f, 0.22f, 0.9f);

        // Text component
        var textGo = new GameObject("Text");
        textGo.transform.SetParent(go.transform, false);
        var textRT = textGo.AddComponent<RectTransform>();
        textRT.anchorMin = Vector2.zero;
        textRT.anchorMax = Vector2.one;
        textRT.offsetMin = new Vector2(10, 2);
        textRT.offsetMax = new Vector2(-10, -2);
        var inputText = textGo.AddComponent<Text>();
        inputText.font = FontHelper.GetFont();
        inputText.fontSize = 16;
        inputText.color = Color.white;
        inputText.alignment = TextAnchor.MiddleLeft;
        inputText.supportRichText = false;

        // Placeholder
        var phGo = new GameObject("Placeholder");
        phGo.transform.SetParent(go.transform, false);
        var phRT = phGo.AddComponent<RectTransform>();
        phRT.anchorMin = Vector2.zero;
        phRT.anchorMax = Vector2.one;
        phRT.offsetMin = new Vector2(10, 2);
        phRT.offsetMax = new Vector2(-10, -2);
        var phText = phGo.AddComponent<Text>();
        phText.font = FontHelper.GetFont();
        phText.fontSize = 16;
        phText.fontStyle = FontStyle.Italic;
        phText.color = new Color(1, 1, 1, 0.3f);
        phText.alignment = TextAnchor.MiddleLeft;
        phText.text = placeholder;

        var inputField = go.AddComponent<InputField>();
        inputField.textComponent = inputText;
        inputField.placeholder = phText;
        inputField.targetGraphic = bg;

        return inputField;
    }

    private Button CreatePanelButton(RectTransform parent, string name, string label,
        Vector2 pos, Color color)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = pos;
        rt.sizeDelta = new Vector2(300, 40);

        var img = go.AddComponent<Image>();
        img.color = color;
        var btn = go.AddComponent<Button>();
        btn.targetGraphic = img;

        CreateUIText(rt, name + "_Text", label,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, 16);

        return btn;
    }

    /// <summary>
    /// 建立任務面板 (ScrollView)。回傳 (panelGo, contentRT)。
    /// </summary>
    private (GameObject, RectTransform) CreateQuestPanel(RectTransform canvasRT)
    {
        // Panel 背景
        var panelGo = new GameObject("QuestPanel");
        panelGo.transform.SetParent(canvasRT, false);
        var panelRT = panelGo.AddComponent<RectTransform>();
        panelRT.anchorMin = new Vector2(0.15f, 0.05f);
        panelRT.anchorMax = new Vector2(0.85f, 0.95f);
        panelRT.offsetMin = Vector2.zero;
        panelRT.offsetMax = Vector2.zero;
        var panelBg = panelGo.AddComponent<Image>();
        panelBg.color = new Color(0.08f, 0.08f, 0.12f, 0.95f);

        // 標題
        var title = CreateUIText(panelRT, "Title", "地球探測儀",
            new Vector2(0, 1), new Vector2(1, 1), new Vector2(0, -20f), 22);
        title.alignment = TextAnchor.MiddleLeft;
        title.GetComponent<RectTransform>().offsetMin = new Vector2(20, -40);
        title.GetComponent<RectTransform>().offsetMax = new Vector2(-60, 0);

        // 關閉按鈕
        var closeGo = new GameObject("CloseBtn");
        closeGo.transform.SetParent(panelRT, false);
        var closeRT = closeGo.AddComponent<RectTransform>();
        closeRT.anchorMin = new Vector2(1, 1);
        closeRT.anchorMax = new Vector2(1, 1);
        closeRT.anchoredPosition = new Vector2(-25, -20);
        closeRT.sizeDelta = new Vector2(30, 30);
        var closeBg = closeGo.AddComponent<Image>();
        closeBg.color = new Color(0, 0, 0, 0);
        var closeBtn = closeGo.AddComponent<Button>();
        closeBtn.targetGraphic = closeBg;
        var closeText = CreateUIText(closeRT, "X", "✕",
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, 22);
        closeBtn.onClick.AddListener(() => { if (QuestPanelUI.Instance != null) QuestPanelUI.Instance.TogglePanel(); });

        // 內容區（QuestPanelUI 會在此建立 ListView / DetailView）
        var contentGo = new GameObject("Content");
        contentGo.transform.SetParent(panelGo.transform, false);
        var contentRT = contentGo.AddComponent<RectTransform>();
        contentRT.anchorMin = new Vector2(0, 0);
        contentRT.anchorMax = new Vector2(1, 1);
        contentRT.offsetMin = new Vector2(10, 10);
        contentRT.offsetMax = new Vector2(-10, -50);

        return (panelGo, contentRT);
    }

    /// <summary>
    /// 建立定位提示面板 (畫面上方)。
    /// </summary>
    private void CreateLocatorPanel(RectTransform canvasRT)
    {
        var panelGo = new GameObject("LocatorPanel");
        panelGo.transform.SetParent(canvasRT, false);
        var panelRT = panelGo.AddComponent<RectTransform>();
        panelRT.anchorMin = new Vector2(0.5f, 1f);
        panelRT.anchorMax = new Vector2(0.5f, 1f);
        panelRT.anchoredPosition = new Vector2(0, -150f);
        panelRT.sizeDelta = new Vector2(350, 110);

        var bg = panelGo.AddComponent<Image>();
        bg.color = new Color(0.05f, 0.05f, 0.1f, 0.85f);

        var condQ = CreateUIText(panelRT, "CondQuadrant", "[ ] 象限",
            new Vector2(0, 1), new Vector2(1, 1), new Vector2(0, -8), 16);
        condQ.alignment = TextAnchor.MiddleLeft;

        var condS = CreateUIText(panelRT, "CondSystem", "[ ] 星域",
            new Vector2(0, 1), new Vector2(1, 1), new Vector2(0, -32), 16);
        condS.alignment = TextAnchor.MiddleLeft;

        var condC = CreateUIText(panelRT, "CondCoord", "[ ] 座標",
            new Vector2(0, 1), new Vector2(1, 1), new Vector2(0, -56), 16);
        condC.alignment = TextAnchor.MiddleLeft;

        var condF = CreateUIText(panelRT, "CondFilter", "[ ] 濾鏡",
            new Vector2(0, 1), new Vector2(1, 1), new Vector2(0, -80), 16);
        condF.alignment = TextAnchor.MiddleLeft;

        var locator = canvasRT.gameObject.AddComponent<QuestLocator>();
        locator.Init(panelGo, condQ, condS, condC, condF);
    }

    /// <summary>
    /// 建立方向提示 UI（提示按鈕 + 箭頭指示器）。
    /// </summary>
    private void CreateQuestHintUI(RectTransform canvasRT)
    {
        // ── 提示按鈕（螢幕右側偏中）──
        var btnGo = new GameObject("HintButton");
        btnGo.transform.SetParent(canvasRT, false);
        var btnRT = btnGo.AddComponent<RectTransform>();
        btnRT.anchorMin = new Vector2(1, 0.5f);
        btnRT.anchorMax = new Vector2(1, 0.5f);
        btnRT.anchoredPosition = new Vector2(-80, 60);
        btnRT.sizeDelta = new Vector2(100, 40);

        var btnImg = btnGo.AddComponent<Image>();
        btnImg.color = new Color(0.6f, 0.4f, 0.1f, 0.9f);
        var btn = btnGo.AddComponent<Button>();
        btn.targetGraphic = btnImg;

        CreateUIText(btnRT, "HintBtnText", "提示",
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, 18);

        // ── 箭頭指示器（螢幕中央）──
        var arrowRoot = new GameObject("ArrowRoot");
        arrowRoot.transform.SetParent(canvasRT, false);
        var arrowRootRT = arrowRoot.AddComponent<RectTransform>();
        arrowRootRT.anchorMin = new Vector2(0.5f, 0.5f);
        arrowRootRT.anchorMax = new Vector2(0.5f, 0.5f);
        arrowRootRT.anchoredPosition = Vector2.zero;
        arrowRootRT.sizeDelta = new Vector2(200, 200);

        // 箭頭主體（用三角形模擬：一條長線 + 三角頭）
        // 箭桿
        var shaftGo = new GameObject("Shaft");
        shaftGo.transform.SetParent(arrowRootRT, false);
        var shaftRT = shaftGo.AddComponent<RectTransform>();
        shaftRT.anchorMin = new Vector2(0.5f, 0.5f);
        shaftRT.anchorMax = new Vector2(0.5f, 0.5f);
        shaftRT.anchoredPosition = new Vector2(30, 0);
        shaftRT.sizeDelta = new Vector2(60, 6);
        shaftGo.AddComponent<Image>().color = new Color(1f, 0.7f, 0.1f, 0.9f);

        // 箭頭（用菱形旋轉 45° 模擬）
        var headGo = new GameObject("Head");
        headGo.transform.SetParent(arrowRootRT, false);
        var headRT = headGo.AddComponent<RectTransform>();
        headRT.anchorMin = new Vector2(0.5f, 0.5f);
        headRT.anchorMax = new Vector2(0.5f, 0.5f);
        headRT.anchoredPosition = new Vector2(70, 0);
        headRT.sizeDelta = new Vector2(16, 16);
        headRT.localRotation = Quaternion.Euler(0, 0, 45);
        headGo.AddComponent<Image>().color = new Color(1f, 0.7f, 0.1f, 0.9f);

        // 掛 QuestHintUI
        var hintUI = canvasRT.gameObject.AddComponent<QuestHintUI>();
        hintUI.Init(btnGo, arrowRoot, arrowRootRT);

        btn.onClick.AddListener(() => hintUI.OnHintClicked());
    }

    /// <summary>
    /// 建立行星詳情面板。
    /// </summary>
    private void CreateDetailPanel(RectTransform canvasRT)
    {
        // 全螢幕遮罩面板
        var panelGo = new GameObject("DetailPanel");
        panelGo.transform.SetParent(canvasRT, false);
        var panelRT = panelGo.AddComponent<RectTransform>();
        panelRT.anchorMin = Vector2.zero;
        panelRT.anchorMax = Vector2.one;
        panelRT.offsetMin = Vector2.zero;
        panelRT.offsetMax = Vector2.zero;

        var bg = panelGo.AddComponent<Image>();
        bg.color = new Color(0.05f, 0.05f, 0.08f, 0.95f);

        // 內容區
        var contentGo = new GameObject("Content");
        contentGo.transform.SetParent(panelGo.transform, false);
        var contentRT = contentGo.AddComponent<RectTransform>();
        contentRT.anchorMin = new Vector2(0.1f, 0.1f);
        contentRT.anchorMax = new Vector2(0.9f, 0.9f);
        contentRT.offsetMin = Vector2.zero;
        contentRT.offsetMax = Vector2.zero;

        // 名稱 InputField (上方)
        var nameFieldGo = new GameObject("NameField");
        nameFieldGo.transform.SetParent(contentRT, false);
        var nameFieldRT = nameFieldGo.AddComponent<RectTransform>();
        nameFieldRT.anchorMin = new Vector2(0.1f, 1);
        nameFieldRT.anchorMax = new Vector2(0.9f, 1);
        nameFieldRT.anchoredPosition = new Vector2(0, -30);
        nameFieldRT.sizeDelta = new Vector2(0, 50);

        var nfBg = nameFieldGo.AddComponent<Image>();
        nfBg.color = new Color(0.15f, 0.15f, 0.2f, 0.9f);

        var textGo = new GameObject("Text");
        textGo.transform.SetParent(nameFieldGo.transform, false);
        var textRT = textGo.AddComponent<RectTransform>();
        textRT.anchorMin = Vector2.zero;
        textRT.anchorMax = Vector2.one;
        textRT.offsetMin = new Vector2(10, 0);
        textRT.offsetMax = new Vector2(-10, 0);
        var inputText = textGo.AddComponent<Text>();
        inputText.font = FontHelper.GetFont();
        inputText.fontSize = 28;
        inputText.color = Color.white;
        inputText.alignment = TextAnchor.MiddleCenter;
        inputText.supportRichText = false;

        var inputField = nameFieldGo.AddComponent<InputField>();
        inputField.textComponent = inputText;
        inputField.targetGraphic = nfBg;

        // EMETH ID
        var emethText = CreateUIText(contentRT, "EmethId", "EMETH-000",
            new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -75), 18);

        // 描述
        var descText = CreateUIText(contentRT, "Description", "",
            new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -105), 15);
        descText.GetComponent<RectTransform>().sizeDelta = new Vector2(600, 60);
        descText.color = new Color(0.8f, 0.8f, 0.8f);

        // 地球相似度
        var simLabel = CreateUIText(contentRT, "SimLabel", "地球相似度",
            new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(80, -30), 20);
        simLabel.color = new Color(0.4f, 0.85f, 0.4f);
        simLabel.alignment = TextAnchor.MiddleLeft;

        var simValue = CreateUIText(contentRT, "SimValue", "0%",
            new Vector2(1, 0.5f), new Vector2(1, 0.5f), new Vector2(-80, -30), 22);
        simValue.color = new Color(0.4f, 0.85f, 0.4f);
        simValue.alignment = TextAnchor.MiddleRight;

        // 數值表
        var radiusLabel = CreateUIText(contentRT, "RL", "半徑",
            new Vector2(0, 0), new Vector2(0, 0), new Vector2(80, 90), 16);
        radiusLabel.alignment = TextAnchor.MiddleLeft;
        var radiusVal = CreateUIText(contentRT, "RV", "0",
            new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(-40, 90), 16);
        radiusVal.alignment = TextAnchor.MiddleRight;

        var massLabel = CreateUIText(contentRT, "ML", "質量",
            new Vector2(0, 0), new Vector2(0, 0), new Vector2(80, 55), 16);
        massLabel.alignment = TextAnchor.MiddleLeft;
        var massVal = CreateUIText(contentRT, "MV", "0",
            new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(-40, 55), 16);
        massVal.alignment = TextAnchor.MiddleRight;

        var tempLabel = CreateUIText(contentRT, "TL", "溫度",
            new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(40, 90), 16);
        tempLabel.alignment = TextAnchor.MiddleLeft;
        var tempVal = CreateUIText(contentRT, "TV", "0°C",
            new Vector2(1, 0), new Vector2(1, 0), new Vector2(-80, 90), 16);
        tempVal.alignment = TextAnchor.MiddleRight;

        var waterLabel = CreateUIText(contentRT, "WL", "水分",
            new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(40, 55), 16);
        waterLabel.alignment = TextAnchor.MiddleLeft;
        var waterVal = CreateUIText(contentRT, "WV", "0%",
            new Vector2(1, 0), new Vector2(1, 0), new Vector2(-80, 55), 16);
        waterVal.alignment = TextAnchor.MiddleRight;

        // 關閉按鈕
        CreateActionButton(contentRT, "CloseDetail", "關閉",
            new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0, 20),
            new Vector2(150, 40), null);
        var closeBtn = contentRT.Find("CloseDetail").GetComponent<Button>();

        // PlanetDetailUI component
        var detail = panelGo.AddComponent<PlanetDetailUI>();
        SetPrivateField(detail, "panel", panelGo);
        SetPrivateField(detail, "nameInput", inputField);
        SetPrivateField(detail, "emethIdText", emethText);
        SetPrivateField(detail, "descriptionText", descText);
        SetPrivateField(detail, "similarityText", simValue);
        SetPrivateField(detail, "radiusText", radiusVal);
        SetPrivateField(detail, "massText", massVal);
        SetPrivateField(detail, "temperatureText", tempVal);
        SetPrivateField(detail, "waterText", waterVal);
        SetPrivateField(detail, "closeButton", closeBtn);
    }

    /// <summary>
    /// 建立對話系統 UI（底部對話條 + 左側立繪）。
    /// </summary>
    private void CreateDialoguePanel(RectTransform canvasRT)
    {
        // 根節點（全螢幕透明遮罩，擋住背後點擊）
        var root = new GameObject("DialogueRoot");
        root.transform.SetParent(canvasRT, false);
        var rootRT = root.AddComponent<RectTransform>();
        rootRT.anchorMin = Vector2.zero;
        rootRT.anchorMax = Vector2.one;
        rootRT.offsetMin = Vector2.zero;
        rootRT.offsetMax = Vector2.zero;
        root.AddComponent<Image>().color = new Color(0, 0, 0, 0.3f);

        // 底部對話條
        var barGo = new GameObject("DialogueBar");
        barGo.transform.SetParent(rootRT, false);
        var barRT = barGo.AddComponent<RectTransform>();
        barRT.anchorMin = new Vector2(0, 0);
        barRT.anchorMax = new Vector2(1, 0);
        barRT.pivot = new Vector2(0.5f, 0);
        barRT.anchoredPosition = Vector2.zero;
        barRT.sizeDelta = new Vector2(0, 200);
        barGo.AddComponent<Image>().color = new Color(0.05f, 0.05f, 0.1f, 0.92f);

        // 左側立繪框
        var portraitGo = new GameObject("Portrait");
        portraitGo.transform.SetParent(barRT, false);
        var pRT = portraitGo.AddComponent<RectTransform>();
        pRT.anchorMin = new Vector2(0, 0);
        pRT.anchorMax = new Vector2(0, 1);
        pRT.pivot = new Vector2(0, 0.5f);
        pRT.anchoredPosition = new Vector2(20, 0);
        pRT.sizeDelta = new Vector2(160, -20);
        var portraitImg = portraitGo.AddComponent<Image>();
        portraitImg.color = new Color(0.3f, 0.4f, 0.5f);

        // 角色名 (立繪右側上方)
        var speakerText = CreateUIText(barRT, "SpeakerName", "",
            new Vector2(0, 1), new Vector2(1, 1), new Vector2(100, -15), 20);
        speakerText.alignment = TextAnchor.MiddleLeft;
        speakerText.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -15);
        speakerText.GetComponent<RectTransform>().offsetMin = new Vector2(200, -40);
        speakerText.GetComponent<RectTransform>().offsetMax = new Vector2(-20, 0);
        speakerText.color = new Color(1f, 0.85f, 0.3f);

        // 對話內容 (立繪右側)
        var dialogueGo = new GameObject("DialogueText");
        dialogueGo.transform.SetParent(barRT, false);
        var dRT = dialogueGo.AddComponent<RectTransform>();
        dRT.anchorMin = new Vector2(0, 0);
        dRT.anchorMax = new Vector2(1, 1);
        dRT.offsetMin = new Vector2(200, 15);
        dRT.offsetMax = new Vector2(-20, -45);
        var dialogueText = dialogueGo.AddComponent<Text>();
        dialogueText.fontSize = 18;
        dialogueText.alignment = TextAnchor.UpperLeft;
        dialogueText.color = Color.white;
        dialogueText.font = FontHelper.GetFont();
        dialogueText.lineSpacing = 1.3f;
        dialogueText.raycastTarget = false;

        // 點擊提示
        var hintText = CreateUIText(barRT, "ClickHint", "點擊繼續...",
            new Vector2(1, 0), new Vector2(1, 0), new Vector2(-60, 15), 14);
        hintText.color = new Color(1, 1, 1, 0.4f);

        // 掛 DialogueUI 元件
        var dialogueUI = canvasRT.gameObject.AddComponent<DialogueUI>();
        dialogueUI.Init(root, portraitImg, speakerText, dialogueText);
    }

    /// <summary>
    /// 建立四角方框準心 UI。
    /// </summary>
    private GameObject CreateReticle(RectTransform parent)
    {
        var reticle = new GameObject("Reticle");
        reticle.transform.SetParent(parent, false);
        var rt = reticle.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = Vector2.zero;
        rt.sizeDelta = new Vector2(120, 100);

        float cornerLen = 20f;
        float thickness = 2f;
        Color color = new(0.5f, 0.85f, 1f, 0.8f);

        // 四個角落: 每個角 2 條線段 (水平+垂直)
        CreateCornerLine(rt, "TL_H", new Vector2(0, 1), new Vector2(0, 1), new Vector2(0, 0), new Vector2(cornerLen, thickness), color);
        CreateCornerLine(rt, "TL_V", new Vector2(0, 1), new Vector2(0, 1), new Vector2(0, 0), new Vector2(thickness, cornerLen), color);

        CreateCornerLine(rt, "TR_H", new Vector2(1, 1), new Vector2(1, 1), new Vector2(0, 0), new Vector2(cornerLen, thickness), color);
        CreateCornerLine(rt, "TR_V", new Vector2(1, 1), new Vector2(1, 1), new Vector2(0, 0), new Vector2(thickness, cornerLen), color);

        CreateCornerLine(rt, "BL_H", new Vector2(0, 0), new Vector2(0, 0), new Vector2(0, 0), new Vector2(cornerLen, thickness), color);
        CreateCornerLine(rt, "BL_V", new Vector2(0, 0), new Vector2(0, 0), new Vector2(0, 0), new Vector2(thickness, cornerLen), color);

        CreateCornerLine(rt, "BR_H", new Vector2(1, 0), new Vector2(1, 0), new Vector2(0, 0), new Vector2(cornerLen, thickness), color);
        CreateCornerLine(rt, "BR_V", new Vector2(1, 0), new Vector2(1, 0), new Vector2(0, 0), new Vector2(thickness, cornerLen), color);

        // 中心十字
        CreateCornerLine(rt, "CrossH", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(12, 1.5f), color);
        CreateCornerLine(rt, "CrossV", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(1.5f, 12), color);

        return reticle;
    }

    private void CreateCornerLine(RectTransform parent, string name, Vector2 anchorMin, Vector2 anchorMax,
        Vector2 pos, Vector2 size, Color color)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;
        var img = go.AddComponent<Image>();
        img.color = color;
        img.raycastTarget = false;
    }

    private Text CreateUIText(RectTransform parent, string name, string text,
        Vector2 anchorMin, Vector2 anchorMax, Vector2 pos, int fontSize)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = pos;
        rt.sizeDelta = new Vector2(400, 40);

        var uiText = go.AddComponent<Text>();
        uiText.text = text;
        uiText.fontSize = fontSize;
        uiText.alignment = TextAnchor.MiddleCenter;
        uiText.color = Color.white;
        uiText.horizontalOverflow = HorizontalWrapMode.Overflow;
        uiText.verticalOverflow = VerticalWrapMode.Overflow;
        uiText.raycastTarget = false;
        uiText.font = FontHelper.GetFont();

        // 加描邊提高可讀性
        var outline = go.AddComponent<Outline>();
        outline.effectColor = new Color(0, 0, 0, 0.7f);
        outline.effectDistance = new Vector2(1.5f, -1.5f);

        return uiText;
    }

    private void CreateActionButton(RectTransform parent, string name, string label,
        Vector2 anchorMin, Vector2 anchorMax, Vector2 pos, Vector2 size, UnityEngine.Events.UnityAction onClick)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;

        var img = go.AddComponent<Image>();
        img.color = new Color(0.25f, 0.25f, 0.35f, 0.85f);
        var btn = go.AddComponent<Button>();
        btn.targetGraphic = img;
        if (onClick != null)
            btn.onClick.AddListener(onClick);

        CreateUIText(rt, name + "_Text", label,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, 18);
    }

    private static void SetPrivateField(object target, string fieldName, object value)
    {
        var field = target.GetType().GetField(fieldName,
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        field?.SetValue(target, value);
    }
}
