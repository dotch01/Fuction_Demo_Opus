using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

/// <summary>
/// Demo2 場景啟動器：自動建立玩家、物品、鏡頭、地板、牆壁、HUD。
/// 道具資料從 StreamingAssets/ItemConfig.json 載入。
/// 掛在場景中任一空物件即可。
/// </summary>
public class Demo2Setup : MonoBehaviour
{
    [Header("場景設定")]
    [SerializeField] private float arenaWidth = 20f;
    [SerializeField] private float arenaHeight = 14f;
    [SerializeField] private int itemCount = 20;
    [SerializeField] private Color backgroundColor = new(0.08f, 0.1f, 0.14f, 1f);

    private Canvas _canvas;

    private void Start()
    {
        SetupCamera();
        SetupPhysics();
        CreateManagers();
        StartCoroutine(LoadAndBuild());
    }

    private IEnumerator LoadAndBuild()
    {
        // 載入道具配置
        var loader = gameObject.AddComponent<ItemConfigLoader>();
        yield return loader.LoadItemConfig();

        CreateArena();
        CreatePlayer();
        SpawnItems();
        CreateUI();
        StartItemSpawner();
    }

    // ──────────────── Camera ────────────────

    private void SetupCamera()
    {
        Camera cam = Camera.main;
        cam.orthographic = true;
        cam.orthographicSize = arenaHeight * 0.55f;
        cam.backgroundColor = backgroundColor;
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.transform.position = new Vector3(0, 0, -10);
    }

    // ──────────────── Physics ────────────────

    private void SetupPhysics()
    {
        Physics2D.gravity = Vector2.zero;
    }

    // ──────────────── Managers ────────────────

    private void CreateManagers()
    {
        var invGo = new GameObject("InventoryManager");
        invGo.AddComponent<InventoryManager>();

        var statsGo = new GameObject("PlayerStats");
        statsGo.AddComponent<PlayerStats>();
    }

    // ──────────────── Arena (地板 + 牆壁) ────────────────

    private void CreateArena()
    {
        // 地板格子
        CreateFloorGrid();

        // 四面牆壁（BoxCollider2D）
        float wallThick = 1f;
        CreateWall("WallTop", new Vector3(0, arenaHeight / 2 + wallThick / 2, 0), new Vector2(arenaWidth + wallThick * 2, wallThick));
        CreateWall("WallBottom", new Vector3(0, -arenaHeight / 2 - wallThick / 2, 0), new Vector2(arenaWidth + wallThick * 2, wallThick));
        CreateWall("WallLeft", new Vector3(-arenaWidth / 2 - wallThick / 2, 0, 0), new Vector2(wallThick, arenaHeight + wallThick * 2));
        CreateWall("WallRight", new Vector3(arenaWidth / 2 + wallThick / 2, 0, 0), new Vector2(wallThick, arenaHeight + wallThick * 2));
    }

    private void CreateFloorGrid()
    {
        var floorRoot = new GameObject("Floor");
        int cols = Mathf.CeilToInt(arenaWidth);
        int rows = Mathf.CeilToInt(arenaHeight);
        float startX = -arenaWidth / 2f + 0.5f;
        float startY = -arenaHeight / 2f + 0.5f;

        var lightTile = new Color(0.12f, 0.14f, 0.18f);
        var darkTile = new Color(0.09f, 0.11f, 0.15f);

        var sprite = CreateSquareSprite();

        for (int y = 0; y < rows; y++)
        {
            for (int x = 0; x < cols; x++)
            {
                var tile = new GameObject($"Tile_{x}_{y}");
                tile.transform.SetParent(floorRoot.transform);
                tile.transform.position = new Vector3(startX + x, startY + y, 1f); // z=1 在背景

                var sr = tile.AddComponent<SpriteRenderer>();
                sr.sprite = sprite;
                sr.color = (x + y) % 2 == 0 ? lightTile : darkTile;
                sr.sortingOrder = -10;
            }
        }
    }

    private void CreateWall(string name, Vector3 pos, Vector2 size)
    {
        var go = new GameObject(name);
        go.transform.position = pos;

        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = CreateSquareSprite();
        sr.color = new Color(0.2f, 0.25f, 0.35f);
        sr.sortingOrder = -5;
        go.transform.localScale = new Vector3(size.x, size.y, 1f);

        var col = go.AddComponent<BoxCollider2D>();
        // collider 預設符合 sprite 大小
    }

    // ──────────────── Player ────────────────

    private void CreatePlayer()
    {
        var playerGo = new GameObject("Player");
        playerGo.transform.position = Vector3.zero;

        // Rigidbody2D
        var rb = playerGo.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.freezeRotation = true;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;

        // Collider（碰牆壁用）
        var col = playerGo.AddComponent<CircleCollider2D>();
        col.radius = 0.4f;

        // Trigger（偵測收集物用）— 稍大一點
        var triggerGo = new GameObject("Trigger");
        triggerGo.transform.SetParent(playerGo.transform, false);
        var trigger = triggerGo.AddComponent<CircleCollider2D>();
        trigger.radius = 0.6f;
        trigger.isTrigger = true;
        var triggerRb = triggerGo.AddComponent<Rigidbody2D>();
        triggerRb.gravityScale = 0;
        triggerRb.bodyType = RigidbodyType2D.Kinematic;

        // 外觀 — 簡單的雙層圓形
        var spriteRoot = new GameObject("SpriteRoot");
        spriteRoot.transform.SetParent(playerGo.transform, false);

        var bodySprite = CreateCircleSprite(32);

        // 身體
        var body = new GameObject("Body");
        body.transform.SetParent(spriteRoot.transform, false);
        body.transform.localScale = Vector3.one * 0.8f;
        var bodySR = body.AddComponent<SpriteRenderer>();
        bodySR.sprite = bodySprite;
        bodySR.color = new Color(0.3f, 0.75f, 0.95f);
        bodySR.sortingOrder = 5;

        // 眼睛
        CreateEye(spriteRoot.transform, bodySprite, new Vector3(-0.15f, 0.12f, 0), 0.15f);
        CreateEye(spriteRoot.transform, bodySprite, new Vector3(0.15f, 0.12f, 0), 0.15f);

        // PlayerController
        var pc = playerGo.AddComponent<Demo2PlayerController>();
        pc.Init(spriteRoot.transform);

        // 鏡頭跟隨
        var camFollow = Camera.main.gameObject.AddComponent<Demo2CameraFollow>();
        camFollow.Init(playerGo.transform);
    }

    private void CreateEye(Transform parent, Sprite sprite, Vector3 localPos, float scale)
    {
        var eye = new GameObject("Eye");
        eye.transform.SetParent(parent, false);
        eye.transform.localPosition = localPos;
        eye.transform.localScale = Vector3.one * scale;
        var sr = eye.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.color = Color.white;
        sr.sortingOrder = 6;

        // 瞳孔
        var pupil = new GameObject("Pupil");
        pupil.transform.SetParent(eye.transform, false);
        pupil.transform.localPosition = new Vector3(0, 0.1f, 0);
        pupil.transform.localScale = Vector3.one * 0.5f;
        var pupilSR = pupil.AddComponent<SpriteRenderer>();
        pupilSR.sprite = sprite;
        pupilSR.color = new Color(0.1f, 0.1f, 0.2f);
        pupilSR.sortingOrder = 7;
    }

    // ──────────────── Items ────────────────

    private void SpawnItems()
    {
        var config = ItemConfigLoader.Load();
        if (config == null || config.items.Count == 0)
        {
            Debug.LogWarning("[Demo2Setup] ItemConfig not loaded, skipping item spawn.");
            return;
        }

        var circleSprite = CreateCircleSprite(32);
        var diamondSprite = CreateDiamondSprite();
        var squareSprite = CreateSquareSprite();

        float margin = 1.5f;
        float xRange = arenaWidth / 2 - margin;
        float yRange = arenaHeight / 2 - margin;

        for (int i = 0; i < itemCount; i++)
        {
            ItemEntry entry = config.items[i % config.items.Count];

            var go = new GameObject($"Item_{entry.id}_{i}");
            float x = Random.Range(-xRange, xRange);
            float y = Random.Range(-yRange, yRange);

            // 避開玩家起始位置
            if (Mathf.Abs(x) < 2f && Mathf.Abs(y) < 2f)
                x += 3f;

            go.transform.position = new Vector3(x, y, 0);

            var col = go.AddComponent<CircleCollider2D>();
            col.radius = 0.4f;
            col.isTrigger = true;

            // 外觀
            Sprite sprite = entry.spriteName switch
            {
                "diamond" => diamondSprite,
                "square" => squareSprite,
                _ => circleSprite
            };

            var spriteRoot = new GameObject("Sprite");
            spriteRoot.transform.SetParent(go.transform, false);
            spriteRoot.transform.localScale = Vector3.one * entry.spriteScale;

            var sr = spriteRoot.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.color = entry.color.ToColor();
            sr.sortingOrder = 3;

            // 光暈
            var glow = new GameObject("Glow");
            glow.transform.SetParent(spriteRoot.transform, false);
            glow.transform.localScale = Vector3.one * 1.8f;
            var glowSR = glow.AddComponent<SpriteRenderer>();
            glowSR.sprite = circleSprite;
            Color c = entry.color.ToColor();
            glowSR.color = new Color(c.r, c.g, c.b, 0.15f);
            glowSR.sortingOrder = 2;

            var collectible = go.AddComponent<Collectible>();
            collectible.Init(entry, spriteRoot.transform);
        }
    }

    // ──────────────── Item Spawner ────────────────

    private void StartItemSpawner()
    {
        var spawnerGo = new GameObject("ItemSpawner");
        var spawner = spawnerGo.AddComponent<ItemSpawner>();
        spawner.Init(arenaWidth, arenaHeight,
            CreateCircleSprite(32), CreateDiamondSprite(), CreateSquareSprite(),
            spawnInterval: 4f, maxItems: 30);
    }

    // ──────────────── UI ────────────────

    private void CreateUI()
    {
        if (FindAnyObjectByType<EventSystem>() == null)
        {
            var esGo = new GameObject("EventSystem");
            esGo.AddComponent<EventSystem>();
            esGo.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
        }

        var canvasGo = new GameObject("Demo2_Canvas");
        var canvas = canvasGo.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;
        var scaler = canvasGo.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        canvasGo.AddComponent<GraphicRaycaster>();

        RectTransform canvasRT = canvasGo.GetComponent<RectTransform>();

        // 分數（左上）
        var scoreText = CreateUIText(canvasRT, "ScoreText", "分數: 0",
            new Vector2(0, 1), new Vector2(0, 1), new Vector2(120, -30), 28);
        scoreText.alignment = TextAnchor.MiddleLeft;

        // 物品數（左上下方）
        var itemText = CreateUIText(canvasRT, "ItemText", "物品: 0",
            new Vector2(0, 1), new Vector2(0, 1), new Vector2(120, -65), 22);
        itemText.alignment = TextAnchor.MiddleLeft;

        // 各類型計數（底部中央）
        var detailText = CreateUIText(canvasRT, "DetailText", "",
            new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0, 30), 18);

        // HP 條
        var hpBar = CreateStatBar(canvasRT, "HPBar", new Vector2(120, -100),
            new Color(0.85f, 0.2f, 0.2f), "HP", out var hpFill, out var hpText);

        // MP 條
        var mpBar = CreateStatBar(canvasRT, "MPBar", new Vector2(120, -130),
            new Color(0.2f, 0.4f, 0.9f), "MP", out var mpFill, out var mpText);

        // 操作提示（右下）
        var hint = CreateUIText(canvasRT, "HintText", "WASD 移動 | B 背包",
            new Vector2(1, 0), new Vector2(1, 0), new Vector2(-120, 30), 16);
        hint.color = new Color(1, 1, 1, 0.4f);

        // HUD 元件
        var hud = canvasGo.AddComponent<Demo2HUD>();
        hud.Init(scoreText, itemText, detailText, hpFill, mpFill, hpText, mpText);

        // 背包 UI
        var invUI = canvasGo.AddComponent<InventoryUI>();
        invUI.Init(canvasRT);

        // === 新手提示 ===
        TutorialHint.Show(canvasRT,
            "WASD / 方向鍵：移動角色\n" +
            "碰觸物品即可收集\n" +
            "B 鍵：開啟背包使用道具\n" +
            "紅色/藍色藥水可恢復 HP/MP\n" +
            "紫色物品碰觸會扣 HP/MP！", this);
    }

    /// <summary>
    /// 建立 HP / MP 狀態條。
    /// </summary>
    private GameObject CreateStatBar(RectTransform parent, string name, Vector2 pos,
        Color fillColor, string label, out Image fill, out Text valueText)
    {
        var barGo = new GameObject(name);
        barGo.transform.SetParent(parent, false);
        var barRT = barGo.AddComponent<RectTransform>();
        barRT.anchorMin = new Vector2(0, 1);
        barRT.anchorMax = new Vector2(0, 1);
        barRT.pivot = new Vector2(0.5f, 0.5f);
        barRT.anchoredPosition = pos;
        barRT.sizeDelta = new Vector2(200, 20);

        // 背景
        var bgImg = barGo.AddComponent<Image>();
        bgImg.color = new Color(0.15f, 0.15f, 0.2f, 0.8f);

        // 填充
        var fillGo = new GameObject("Fill");
        fillGo.transform.SetParent(barGo.transform, false);
        var fillRT = fillGo.AddComponent<RectTransform>();
        fillRT.anchorMin = Vector2.zero;
        fillRT.anchorMax = Vector2.one;
        fillRT.offsetMin = new Vector2(2, 2);
        fillRT.offsetMax = new Vector2(-2, -2);
        fill = fillGo.AddComponent<Image>();
        fill.color = fillColor;
        fill.type = Image.Type.Filled;
        fill.fillMethod = Image.FillMethod.Horizontal;
        fill.fillAmount = 1f;

        // 文字 (label + value)
        var labelText = CreateUIText(barRT, "Label", label,
            new Vector2(0, 0), new Vector2(0, 1), new Vector2(-25, 0), 14);
        labelText.alignment = TextAnchor.MiddleRight;
        var labelRT = labelText.GetComponent<RectTransform>();
        labelRT.sizeDelta = new Vector2(40, 20);

        valueText = CreateUIText(barRT, "Value", "100/100",
            new Vector2(0, 0), new Vector2(1, 1), Vector2.zero, 13);
        valueText.alignment = TextAnchor.MiddleCenter;

        return barGo;
    }

    // ──────────────── Sprite 工具 ────────────────

    private static Sprite CreateCircleSprite(int size)
    {
        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Bilinear;
        Vector2 center = new(size * 0.5f, size * 0.5f);
        float radius = size * 0.5f;

        for (int y = 0; y < size; y++)
        for (int x = 0; x < size; x++)
        {
            float dist = Vector2.Distance(new Vector2(x, y), center);
            float alpha = Mathf.Clamp01((radius - dist) * 2f);
            tex.SetPixel(x, y, new Color(1, 1, 1, alpha));
        }
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
    }

    private static Sprite CreateSquareSprite()
    {
        var tex = new Texture2D(4, 4, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Point;
        for (int y = 0; y < 4; y++)
        for (int x = 0; x < 4; x++)
            tex.SetPixel(x, y, Color.white);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, 4, 4), new Vector2(0.5f, 0.5f), 4);
    }

    private static Sprite CreateDiamondSprite()
    {
        const int size = 32;
        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Bilinear;
        float half = size * 0.5f;

        for (int y = 0; y < size; y++)
        for (int x = 0; x < size; x++)
        {
            float dx = Mathf.Abs(x - half) / half;
            float dy = Mathf.Abs(y - half) / half;
            float d = dx + dy; // 菱形距離
            float alpha = Mathf.Clamp01((1f - d) * size * 0.15f);
            tex.SetPixel(x, y, new Color(1, 1, 1, alpha));
        }
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
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
        rt.sizeDelta = new Vector2(500, 40);

        var uiText = go.AddComponent<Text>();
        uiText.text = text;
        uiText.fontSize = fontSize;
        uiText.alignment = TextAnchor.MiddleCenter;
        uiText.color = Color.white;
        uiText.horizontalOverflow = HorizontalWrapMode.Overflow;
        uiText.verticalOverflow = VerticalWrapMode.Overflow;
        uiText.raycastTarget = false;
        uiText.font = FontHelper.GetFont();

        var outline = go.AddComponent<Outline>();
        outline.effectColor = new Color(0, 0, 0, 0.7f);
        outline.effectDistance = new Vector2(1.5f, -1.5f);

        return uiText;
    }
}
