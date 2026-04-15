using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

// ============================================================
// MathMuseumManager.cs
// 場景唯一入口：建立博物館地形、展區、玩家、燈光、UI
// 使用方式：空場景 → 空 GameObject → 掛此腳本 → Play
// ============================================================

public class MathMuseumManager : MonoBehaviour
{
    // --------------------------------------------------------
    // Zone 定義
    // --------------------------------------------------------

    private struct ZoneInfo
    {
        public string name;
        public Vector3 center;    // 世界座標
        public float size;        // 正方形邊長
        public Color floorTint;
        public Color edgeColor;
    }

    private readonly List<ZoneInfo> zones = new List<ZoneInfo>();
    private readonly List<ExhibitBase> allExhibits = new List<ExhibitBase>();

    // --------------------------------------------------------
    // Unity 生命週期
    // --------------------------------------------------------

    void Start()
    {
        DefineZones();
        BuildFloor();
        BuildZones();
        BuildWalls();
        SetupLighting();
        SpawnPlayer();
        BuildUI();
        SetupSystems();

        // 建立所有展品
        BuildAllExhibits();

        // 強制同步物理，確保動態建立的碰撞器立即可被 Raycast 偵測（WebGL 必要）
        Physics.SyncTransforms();

        // === 新手提示 ===
        var tutorialCanvas = GetComponentInChildren<Canvas>();
        if (tutorialCanvas != null)
        {
            TutorialHint.Show(tutorialCanvas.transform,
                "WASD：移動　右鍵按住：旋轉視角\n" +
                "走近展品自動顯示說明\n" +
                "E：開始互動挑戰\n" +
                "右鍵按住可旋轉視角 + 左鍵移動到球體上可拖曳球體操作展品", this);
        }

        Debug.Log($"[MathMuseum] 初始化完成，{zones.Count} 個展區，{allExhibits.Count} 個展品。");
    }

    // --------------------------------------------------------
    // Zone 佈局
    // --------------------------------------------------------

    private void DefineZones()
    {
        float S = 20f; // 每區大小
        float G = 4f;  // 走廊寬度
        float step = S + G;

        // 中央大廳
        zones.Add(new ZoneInfo {
            name = "中央大廳\nCentral Hall",
            center = Vector3.zero,
            size = S,
            floorTint = new Color(0.15f, 0.15f, 0.2f),
            edgeColor = new Color(0.5f, 0.5f, 0.6f, 0.4f)
        });

        // Zone 1: 向量大廳 (南)
        zones.Add(new ZoneInfo {
            name = "向量大廳\nVector Hall",
            center = new Vector3(0, 0, -step),
            size = S,
            floorTint = new Color(0.15f, 0.12f, 0.12f),
            edgeColor = new Color(1f, 0.4f, 0.4f, 0.5f)
        });

        // Zone 2: 旋轉殿堂 (東南)
        zones.Add(new ZoneInfo {
            name = "旋轉殿堂\nRotation Wing",
            center = new Vector3(step, 0, -step),
            size = S,
            floorTint = new Color(0.12f, 0.15f, 0.12f),
            edgeColor = new Color(0.4f, 1f, 0.4f, 0.5f)
        });

        // Zone 3: 曲線走廊 (西南)
        zones.Add(new ZoneInfo {
            name = "曲線走廊\nCurve Corridor",
            center = new Vector3(-step, 0, -step),
            size = S,
            floorTint = new Color(0.12f, 0.12f, 0.15f),
            edgeColor = new Color(0.4f, 0.4f, 1f, 0.5f)
        });

        // Zone 4: 三角學塔 (西)
        zones.Add(new ZoneInfo {
            name = "三角學塔\nTrig Tower",
            center = new Vector3(-step, 0, 0),
            size = S,
            floorTint = new Color(0.15f, 0.14f, 0.1f),
            edgeColor = new Color(1f, 0.8f, 0.3f, 0.5f)
        });

        // Zone 5: 碰撞競技場 (東)
        zones.Add(new ZoneInfo {
            name = "碰撞競技場\nCollision Arena",
            center = new Vector3(step, 0, 0),
            size = S,
            floorTint = new Color(0.15f, 0.1f, 0.1f),
            edgeColor = new Color(1f, 0.3f, 0.3f, 0.5f)
        });

        // Zone 6: 物理實驗室 (南面遠)
        zones.Add(new ZoneInfo {
            name = "物理實驗室\nPhysics Lab",
            center = new Vector3(-step * 0.5f, 0, -step * 2),
            size = S,
            floorTint = new Color(0.1f, 0.13f, 0.15f),
            edgeColor = new Color(0.3f, 0.7f, 1f, 0.5f)
        });

        // Zone 7: 渲染畫廊 (北)
        zones.Add(new ZoneInfo {
            name = "渲染畫廊\nRender Gallery",
            center = new Vector3(0, 0, step),
            size = S,
            floorTint = new Color(0.13f, 0.1f, 0.15f),
            edgeColor = new Color(0.8f, 0.3f, 1f, 0.5f)
        });

        // Zone 8: 微積分花園 (南面遠右)
        zones.Add(new ZoneInfo {
            name = "微積分花園\nCalculus Garden",
            center = new Vector3(step * 0.5f, 0, -step * 2),
            size = S,
            floorTint = new Color(0.1f, 0.15f, 0.1f),
            edgeColor = new Color(0.3f, 1f, 0.5f, 0.5f)
        });

        // Zone 9: 程序生成 + 機率 (南面最遠)
        zones.Add(new ZoneInfo {
            name = "程序生成 + 機率\nProcedural & Probability",
            center = new Vector3(0, 0, -step * 2.5f),
            size = S,
            floorTint = new Color(0.14f, 0.12f, 0.15f),
            edgeColor = new Color(0.7f, 0.4f, 1f, 0.5f)
        });
    }

    // --------------------------------------------------------
    // 地面
    // --------------------------------------------------------

    private void BuildFloor()
    {
        // 大地面（GridFloor shader）
        var floor = GameObject.CreatePrimitive(PrimitiveType.Plane);
        floor.name = "MuseumFloor";
        floor.transform.SetParent(transform, false);
        floor.transform.position = Vector3.zero;
        floor.transform.localScale = new Vector3(20f, 1f, 20f); // 200x200 單位

        // 增加一個有厚度的碰撞盒，防止 WebGL 首幀穿地
        var box = floor.AddComponent<BoxCollider>();
        box.center = Vector3.zero;
        box.size = new Vector3(10f, 0.2f, 10f); // Plane mesh 原始大小為 10x10

        var shader = Shader.Find("MathDemo/GridFloor");
        if (shader != null)
        {
            var mat = new Material(shader);
            floor.GetComponent<Renderer>().material = mat;
        }
        else
        {
            Debug.LogWarning("[MathMuseum] GridFloor shader not found, using fallback");
            var fallbackShader = Shader.Find("Universal Render Pipeline/Unlit")
                                 ?? Shader.Find("Unlit/Color");
            if (fallbackShader != null)
            {
                var mat = new Material(fallbackShader);
                mat.color = new Color(0.12f, 0.12f, 0.15f);
                floor.GetComponent<Renderer>().material = mat;
            }
        }
    }

    // --------------------------------------------------------
    // Zone 區域標示
    // --------------------------------------------------------

    private void BuildZones()
    {
        var highlightShader = Shader.Find("MathDemo/ZoneHighlight");

        foreach (var zone in zones)
        {
            // 地面高亮框
            if (highlightShader != null)
            {
                var highlight = GameObject.CreatePrimitive(PrimitiveType.Quad);
                highlight.name = $"Zone_{zone.name.Split('\n')[0]}";
                highlight.transform.SetParent(transform, false);
                highlight.transform.position = zone.center + Vector3.up * 0.02f;
                highlight.transform.rotation = Quaternion.Euler(90, 0, 0);
                highlight.transform.localScale = new Vector3(zone.size, zone.size, 1);

                var mat = new Material(highlightShader);
                mat.SetColor("_Color", zone.floorTint * 0.5f);
                mat.SetColor("_EdgeColor", zone.edgeColor);
                highlight.GetComponent<Renderer>().material = mat;
                highlight.GetComponent<Renderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

                var col = highlight.GetComponent<Collider>();
                if (col != null) Destroy(col);
            }

            // Zone 名稱立牌（原本的邊緣立牌）
            CreateZoneSign(zone.center + new Vector3(0, 2.5f, -zone.size * 0.5f + 0.5f), zone.name, zone.edgeColor);

            // 漂浮區域標題（高空可見的大字）
            CreateFloatingZoneLabel(zone.center, zone.name, zone.edgeColor, zone.size);

            // 半透明外牆
            BuildZoneTransparentWalls(zone.center, zone.size, zone.edgeColor);
        }
    }

    /// <summary>
    /// 建立漂浮在區域上方的大型標題文字，從遠處也能看到。
    /// </summary>
    private void CreateFloatingZoneLabel(Vector3 center, string text, Color color, float zoneSize)
    {
        // 取中文名（第一行）
        string displayName = text.Contains("\n") ? text.Split('\n')[0] : text;
        // 取英文名（第二行）
        string engName = text.Contains("\n") ? text.Split('\n')[1] : "";

        var go = new GameObject("FloatingLabel_" + displayName);
        go.transform.SetParent(transform, false);
        go.transform.position = center + Vector3.up * 6f;

        // 主標題（大字）
        var tm = go.AddComponent<TextMesh>();
        tm.text = displayName;
        tm.fontSize = 80;
        tm.characterSize = 0.15f;
        tm.anchor = TextAnchor.MiddleCenter;
        tm.alignment = TextAlignment.Center;
        tm.color = new Color(color.r, color.g, color.b, 0.85f);
        tm.fontStyle = FontStyle.Bold;

        // 英文副標（如果有）
        if (!string.IsNullOrEmpty(engName))
        {
            var subGo = new GameObject("FloatingLabel_Sub");
            subGo.transform.SetParent(go.transform, false);
            subGo.transform.localPosition = new Vector3(0, -1.2f, 0);

            var subTm = subGo.AddComponent<TextMesh>();
            subTm.text = engName;
            subTm.fontSize = 60;
            subTm.characterSize = 0.1f;
            subTm.anchor = TextAnchor.MiddleCenter;
            subTm.alignment = TextAlignment.Center;
            subTm.color = new Color(color.r, color.g, color.b, 0.5f);
        }
    }

    /// <summary>
    /// 為每個展區建立四面半透明外牆，讓玩家能看清區域界線但不阻擋視線。
    /// </summary>
    private void BuildZoneTransparentWalls(Vector3 center, float size, Color edgeColor)
    {
        float wallHeight = 3.5f;
        float half = size * 0.5f;
        float thickness = 0.05f;

        // 取得或建立半透明材質
        var shader = Shader.Find("Universal Render Pipeline/Unlit")
                     ?? Shader.Find("Unlit/Transparent");
        if (shader == null) return;

        var mat = new Material(shader);
        // 設為半透明
        mat.SetFloat("_Surface", 1); // URP Transparent
        mat.SetFloat("_Blend", 0);
        mat.SetFloat("_SrcBlend", (float)UnityEngine.Rendering.BlendMode.SrcAlpha);
        mat.SetFloat("_DstBlend", (float)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        mat.SetFloat("_ZWrite", 0);
        mat.renderQueue = 3000;
        mat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
        mat.color = new Color(edgeColor.r, edgeColor.g, edgeColor.b, 0.08f);

        // 北牆
        CreateZoneWall(center + new Vector3(0, wallHeight * 0.5f, half),
            new Vector3(size, wallHeight, thickness), mat);
        // 南牆
        CreateZoneWall(center + new Vector3(0, wallHeight * 0.5f, -half),
            new Vector3(size, wallHeight, thickness), mat);
        // 東牆
        CreateZoneWall(center + new Vector3(half, wallHeight * 0.5f, 0),
            new Vector3(thickness, wallHeight, size), mat);
        // 西牆
        CreateZoneWall(center + new Vector3(-half, wallHeight * 0.5f, 0),
            new Vector3(thickness, wallHeight, size), mat);
    }

    private void CreateZoneWall(Vector3 pos, Vector3 scale, Material mat)
    {
        var wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
        wall.name = "ZoneWall";
        wall.transform.SetParent(transform, false);
        wall.transform.position = pos;
        wall.transform.localScale = scale;
        wall.GetComponent<Renderer>().material = mat;
        wall.GetComponent<Renderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

        // 移除碰撞器，不阻擋玩家通行
        var col = wall.GetComponent<Collider>();
        if (col != null) Destroy(col);
    }

    private void CreateZoneSign(Vector3 pos, string text, Color color)
    {
        var go = new GameObject("ZoneSign");
        go.transform.SetParent(transform, false);
        go.transform.position = pos;

        var tm = go.AddComponent<TextMesh>();
        tm.text = text;
        tm.fontSize = 60;
        tm.characterSize = 0.08f;
        tm.anchor = TextAnchor.MiddleCenter;
        tm.alignment = TextAlignment.Center;
        tm.color = color;

        go.transform.localScale = Vector3.one;
    }

    // --------------------------------------------------------
    // 牆壁（外圍簡易）
    // --------------------------------------------------------

    private void BuildWalls()
    {
        float wallHeight = 4f;
        float halfExtent = 80f;
        var wallShader = Shader.Find("Universal Render Pipeline/Unlit")
                         ?? Shader.Find("Unlit/Color");
        if (wallShader == null)
        {
            Debug.LogWarning("[MathMuseum] No unlit shader found for walls");
            return;
        }
        var mat = new Material(wallShader);
        mat.color = new Color(0.18f, 0.18f, 0.22f);

        CreateWall(new Vector3(0, wallHeight / 2, halfExtent), new Vector3(halfExtent * 2, wallHeight, 0.5f), mat);
        CreateWall(new Vector3(0, wallHeight / 2, -halfExtent), new Vector3(halfExtent * 2, wallHeight, 0.5f), mat);
        CreateWall(new Vector3(halfExtent, wallHeight / 2, 0), new Vector3(0.5f, wallHeight, halfExtent * 2), mat);
        CreateWall(new Vector3(-halfExtent, wallHeight / 2, 0), new Vector3(0.5f, wallHeight, halfExtent * 2), mat);
    }

    private void CreateWall(Vector3 pos, Vector3 scale, Material mat)
    {
        var wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
        wall.name = "Wall";
        wall.transform.SetParent(transform, false);
        wall.transform.position = pos;
        wall.transform.localScale = scale;
        wall.GetComponent<Renderer>().material = mat;
    }

    // --------------------------------------------------------
    // 燈光
    // --------------------------------------------------------

    private void SetupLighting()
    {
        RenderSettings.ambientIntensity = 0.6f;
        RenderSettings.ambientLight = new Color(0.3f, 0.35f, 0.5f);

        var lightGo = new GameObject("Museum Directional Light");
        lightGo.transform.SetParent(transform, false);
        lightGo.transform.rotation = Quaternion.Euler(50, -30, 0);
        var light = lightGo.AddComponent<Light>();
        light.type = LightType.Directional;
        light.intensity = 0.8f;
        light.color = new Color(1f, 0.97f, 0.92f);

        // 各展區加點光源
        foreach (var zone in zones)
        {
            var pointGo = new GameObject("ZoneLight");
            pointGo.transform.SetParent(transform, false);
            pointGo.transform.position = zone.center + Vector3.up * 5f;
            var pl = pointGo.AddComponent<Light>();
            pl.type = LightType.Point;
            pl.range = zone.size * 0.8f;
            pl.intensity = 0.4f;
            pl.color = Color.Lerp(Color.white, zone.edgeColor, 0.3f);
        }
    }

    // --------------------------------------------------------
    // 玩家
    // --------------------------------------------------------

    private void SpawnPlayer()
    {
        var playerGo = new GameObject("Player");
        playerGo.tag = "Player";
        playerGo.transform.position = new Vector3(0, 1f, 0);

        var cc = playerGo.AddComponent<CharacterController>();
        cc.height = 1.8f;
        cc.radius = 0.3f;
        cc.center = new Vector3(0, 0.9f, 0);

        playerGo.AddComponent<PlayerController>();

        // 確保 Camera 有 URP 資料
        var cam = Camera.main;
        if (cam != null)
        {
            var urpData = cam.GetComponent<UniversalAdditionalCameraData>();
            if (urpData == null) cam.gameObject.AddComponent<UniversalAdditionalCameraData>();
        }
    }

    // --------------------------------------------------------
    // UI
    // --------------------------------------------------------

    private void BuildUI()
    {
        Font font = FontHelper.GetFont();

        var canvasGo = new GameObject("MuseumCanvas");
        canvasGo.transform.SetParent(transform, false);
        var canvas = canvasGo.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;
        var scaler = canvasGo.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        canvasGo.AddComponent<GraphicRaycaster>();

        // 十字準心
        CreateCrosshair(canvasGo.transform);

        // 資訊面板
        var infoPanel = canvasGo.AddComponent<ExhibitInfoPanel>();
        infoPanel.Initialize(canvasGo.transform, font);

        // 挑戰 HUD
        var hud = canvasGo.AddComponent<ChallengeHUD>();
        hud.Initialize(canvasGo.transform, font);

        // 互動提示
        var prompt = canvasGo.AddComponent<InteractionPrompt>();
        prompt.Initialize(canvasGo.transform, font);

        // EventSystem
        if (FindAnyObjectByType<EventSystem>() == null)
        {
            var esGo = new GameObject("EventSystem");
            esGo.transform.SetParent(transform, false);
            esGo.AddComponent<EventSystem>();
            esGo.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
        }
    }

    private void CreateCrosshair(Transform parent)
    {
        var go = new GameObject("Crosshair");
        go.transform.SetParent(parent, false);

        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = new Vector2(4, 4);

        var img = go.AddComponent<Image>();
        img.color = new Color(1, 1, 1, 0.5f);
        img.raycastTarget = false;
    }

    // --------------------------------------------------------
    // 系統
    // --------------------------------------------------------

    private void SetupSystems()
    {
        // 繪圖引擎
        var lineGo = new GameObject("MathLineRenderer");
        lineGo.transform.SetParent(transform, false);
        lineGo.AddComponent<MathLineRenderer>();

        // 挑戰系統
        var challengeGo = new GameObject("ChallengeSystem");
        challengeGo.transform.SetParent(transform, false);
        challengeGo.AddComponent<ChallengeSystem>();
    }

    // --------------------------------------------------------
    // 建立所有展品
    // --------------------------------------------------------

    private void BuildAllExhibits()
    {
        // Zone 0: 中央大廳 — 演算法展區
        var z0 = zones[0].center;
        PlaceExhibit<BinarySearchExhibit>(z0 + new Vector3(-7, 0, -5), 0);
        PlaceExhibit<MergeSortExhibit>(z0 + new Vector3(0, 0, -5), 0);
        PlaceExhibit<HeapExhibit>(z0 + new Vector3(7, 0, -5), 0);
        PlaceExhibit<BFSExhibit>(z0 + new Vector3(-7, 0, 0), 0);
        PlaceExhibit<DijkstraExhibit>(z0 + new Vector3(7, 0, 0), 0);
        PlaceExhibit<AStarExhibit>(z0 + new Vector3(-7, 0, 5), 0);
        PlaceExhibit<PathfindingCompareExhibit>(z0 + new Vector3(0, 0, 5), 0);
        PlaceExhibit<ECSEntityListExhibit>(z0 + new Vector3(7, 0, 5), 0);

        // Zone 1: 向量大廳
        var z1 = zones[1].center;
        PlaceExhibit<DotProductExhibit>(z1 + new Vector3(-5, 0, -5), 1);
        PlaceExhibit<CrossProductExhibit>(z1 + new Vector3(5, 0, -5), 1);
        PlaceExhibit<ProjectionExhibit>(z1 + new Vector3(-5, 0, 0), 1);
        PlaceExhibit<ReflectionExhibit>(z1 + new Vector3(5, 0, 0), 1);
        PlaceExhibit<DirectionExhibit>(z1 + new Vector3(-5, 0, 5), 1);
        PlaceExhibit<DistanceExhibit>(z1 + new Vector3(5, 0, 5), 1);
        PlaceExhibit<NormalsExhibit>(z1 + new Vector3(0, 0, -5), 1);
        PlaceExhibit<ClosestOnCircleExhibit>(z1 + new Vector3(0, 0, 5), 1);

        // Zone 2: 旋轉殿堂
        var z2 = zones[2].center;
        PlaceExhibit<QuaternionExhibit>(z2 + new Vector3(-5, 0, -5), 2);
        PlaceExhibit<EulerAngleExhibit>(z2 + new Vector3(5, 0, -5), 2);
        PlaceExhibit<AxisAngleExhibit>(z2 + new Vector3(-5, 0, 0), 2);
        PlaceExhibit<SlerpExhibit>(z2 + new Vector3(5, 0, 0), 2);
        PlaceExhibit<CoordinateTransformExhibit>(z2 + new Vector3(-5, 0, 5), 2);
        PlaceExhibit<HomogeneousExhibit>(z2 + new Vector3(5, 0, 5), 2);
        PlaceExhibit<ParentChildExhibit>(z2 + new Vector3(0, 0, 5), 2);

        // Zone 3: 曲線走廊
        var z3 = zones[3].center;
        PlaceExhibit<LerpRemapExhibit>(z3 + new Vector3(-5, 0, -3), 3);
        PlaceExhibit<CubicSplineExhibit>(z3 + new Vector3(0, 0, -3), 3);
        PlaceExhibit<EasingExhibit>(z3 + new Vector3(5, 0, -3), 3);
        PlaceExhibit<CubicVsLinearExhibit>(z3 + new Vector3(-5, 0, 3), 3);
        PlaceExhibit<HermiteExhibit>(z3 + new Vector3(0, 0, 3), 3);
        PlaceExhibit<SmoothDampExhibit>(z3 + new Vector3(5, 0, 3), 3);

        // Zone 4: 三角學塔
        var z4 = zones[4].center;
        PlaceExhibit<UnitCircleExhibit>(z4 + new Vector3(-5, 0, -3), 4);
        PlaceExhibit<EulerIdentityExhibit>(z4 + new Vector3(5, 0, -3), 4);
        PlaceExhibit<Atan2Exhibit>(z4 + new Vector3(-5, 0, 3), 4);
        PlaceExhibit<ComplexMultiplyExhibit>(z4 + new Vector3(5, 0, 3), 4);
        PlaceExhibit<SinCosTanWaveExhibit>(z4 + new Vector3(0, 0, 3), 4);

        // Zone 5: 碰撞競技場
        var z5 = zones[5].center;
        PlaceExhibit<PointInTriangleExhibit>(z5 + new Vector3(-5, 0, -3), 5);
        PlaceExhibit<RayTriangleExhibit>(z5 + new Vector3(5, 0, -3), 5);
        PlaceExhibit<AABBExhibit>(z5 + new Vector3(0, 0, -3), 5);
        PlaceExhibit<SegmentIntersectExhibit>(z5 + new Vector3(-5, 0, 3), 5);
        PlaceExhibit<MultiReflectionExhibit>(z5 + new Vector3(5, 0, 3), 5);
        PlaceExhibit<NewtonRootExhibit>(z5 + new Vector3(0, 0, 3), 5);

        // Zone 6: 物理實驗室
        var z6 = zones[6].center;
        PlaceExhibit<VelocityExhibit>(z6 + new Vector3(-5, 0, -3), 6);
        PlaceExhibit<SpringExhibit>(z6 + new Vector3(5, 0, -3), 6);
        PlaceExhibit<CausticsExhibit>(z6 + new Vector3(-5, 0, 3), 6);
        PlaceExhibit<IntegralDistanceExhibit>(z6 + new Vector3(5, 0, 3), 6);
        PlaceExhibit<MaxBounceHeightExhibit>(z6 + new Vector3(0, 0, 3), 6);

        // Zone 7: 渲染畫廊
        var z7 = zones[7].center;
        PlaceExhibit<FrustumCullingExhibit>(z7 + new Vector3(-5, 0, -5), 7);
        PlaceExhibit<BillboardExhibit>(z7 + new Vector3(5, 0, -5), 7);
        PlaceExhibit<CameraLookAtExhibit>(z7 + new Vector3(-5, 0, 0), 7);
        PlaceExhibit<PerspectiveMatrixExhibit>(z7 + new Vector3(5, 0, 0), 7);
        PlaceExhibit<RenderPipelineExhibit>(z7 + new Vector3(-5, 0, 5), 7);
        PlaceExhibit<TriangleMeshExhibit>(z7 + new Vector3(5, 0, 5), 7);
        PlaceExhibit<VBOExhibit>(z7 + new Vector3(0, 0, 5), 7);

        // Zone 8: 微積分花園
        var z8 = zones[8].center;
        PlaceExhibit<DerivativeExhibit>(z8 + new Vector3(-5, 0, -3), 8);
        PlaceExhibit<IntegralAreaExhibit>(z8 + new Vector3(5, 0, -3), 8);
        PlaceExhibit<SimpsonExhibit>(z8 + new Vector3(-5, 0, 3), 8);
        PlaceExhibit<TriangulationExhibit>(z8 + new Vector3(5, 0, 3), 8);

        // Zone 9: 程序生成 + 機率
        var z9 = zones[9].center;
        PlaceExhibit<FractalExhibit>(z9 + new Vector3(-5, 0, -3), 9);
        PlaceExhibit<LSystemExhibit>(z9 + new Vector3(5, 0, -3), 9);
        PlaceExhibit<VoronoiExhibit>(z9 + new Vector3(-5, 0, 3), 9);
        PlaceExhibit<WaveSuperpositionExhibit>(z9 + new Vector3(5, 0, 3), 9);
    }

    private T PlaceExhibit<T>(Vector3 worldPos, int zoneIndex) where T : ExhibitBase
    {
        var go = new GameObject(typeof(T).Name);
        go.transform.SetParent(transform, false);
        go.transform.position = worldPos;

        var exhibit = go.AddComponent<T>();
        exhibit.exhibitWorldCenter = worldPos;
        exhibit.BuildExhibit();

        // 觸發區域
        var triggerGo = new GameObject("Trigger");
        triggerGo.transform.SetParent(go.transform, false);
        triggerGo.transform.localPosition = Vector3.zero;
        triggerGo.layer = 0;
        var trigger = triggerGo.AddComponent<ExhibitTrigger>();
        trigger.exhibit = exhibit;
        trigger.triggerRadius = 4f;

        // 註冊挑戰
        if (ChallengeSystem.Instance != null && !string.IsNullOrEmpty(exhibit.challengeDescription))
        {
            ChallengeSystem.Instance.RegisterExhibit(exhibit.exhibitName);
        }

        allExhibits.Add(exhibit);
        return exhibit;
    }
}
