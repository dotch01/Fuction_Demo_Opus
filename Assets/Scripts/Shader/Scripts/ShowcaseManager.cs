using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;
using UnityEngine.InputSystem;


// ============================================================
// ShowcaseManager.cs
// 場景唯一 MonoBehaviour：Shader 展示畫廊控制器
//
// 使用方式：
// 1. 建立新場景（或用現有空場景）
// 2. 建立空 GameObject，掛上此腳本
// 3. Play — 所有展品、鏡頭、燈光、UI 都會自動建立
// ============================================================

public class ShowcaseManager : MonoBehaviour
{
    // --------------------------------------------------------
    // 展示資料
    // --------------------------------------------------------

    private struct SliderParam
    {
        public string label;
        public string shaderProperty;
        public float min, max, defaultValue;
    }

    private struct ShowcaseItem
    {
        public string name;
        public string description;
        public GameObject root;
        public List<SliderParam> sliders;
    }

    private readonly List<ShowcaseItem> items = new List<ShowcaseItem>();
    private int currentIndex = 0;

    // UI 參照
    private Text titleText;
    private Text descText;
    private Text indexText;
    private Transform sliderContainer;

    // Portal 專用
    private PortalRenderer portalRenderer;

    // --------------------------------------------------------
    // Unity 生命週期
    // --------------------------------------------------------

    void Start()
    {
        SetupCamera();
        SetupLighting();

        var stage = new GameObject("ShowcaseStage");
        // 不跟 ShowcaseManager 的位置，強制放在世界原點
        stage.transform.position = Vector3.zero;
        stage.transform.rotation = Quaternion.identity;

        CreateAllShowcases(stage.transform);
        CreateUI();

        if (items.Count > 0)
            ShowEffect(0);

        // === 新手提示 ===
        var hintCanvas = GetComponentInChildren<Canvas>();
        if (hintCanvas != null)
        {
            TutorialHint.Show(hintCanvas.transform,
                "← → 方向鍵 或 A/D：切換效果\n" +
                "右側滑桿：即時調整 Shader 參數\n" +
                "底部按鈕：上一個 / 下一個", this);
        }

        Debug.Log($"[ShaderShowcase] 初始化完成，共 {items.Count} 個展示。");
    }

    void OnDestroy()
    {
        if (portalRenderer != null)
            portalRenderer.Cleanup();
    }

    // --------------------------------------------------------
    // 鏡頭設定
    // --------------------------------------------------------

    private void SetupCamera()
    {
        var cam = Camera.main;
        if (cam == null)
        {
            var camGo = new GameObject("Main Camera");
            camGo.tag = "MainCamera";
            cam = camGo.AddComponent<Camera>();
        }

        cam.transform.position = new Vector3(0, 0, -6);
        cam.transform.rotation = Quaternion.identity;
        cam.fieldOfView = 60f;
        cam.nearClipPlane = 0.1f;
        cam.farClipPlane = 100f;
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0.08f, 0.08f, 0.12f);
        cam.cullingMask = ~(1 << PortalRenderer.PortalLayer);

        // URP 需要額外鏡頭資料
        var urpData = cam.GetComponent<UniversalAdditionalCameraData>();
        if (urpData == null) urpData = cam.gameObject.AddComponent<UniversalAdditionalCameraData>();
        urpData.renderType = CameraRenderType.Base;

        Debug.Log($"[ShaderShowcase] Camera pos={cam.transform.position}, fov={cam.fieldOfView}, mask={cam.cullingMask}");
    }

    // --------------------------------------------------------
    // 燈光設定
    // --------------------------------------------------------

    private void SetupLighting()
    {
        var existing = FindAnyObjectByType<Light>();
        if (existing != null && existing.type == LightType.Directional)
        {
            existing.transform.rotation = Quaternion.Euler(50, -30, 0);
            existing.intensity = 1f;
            return;
        }

        var lightGo = new GameObject("Directional Light");
        lightGo.transform.SetParent(transform, false);
        lightGo.transform.rotation = Quaternion.Euler(50, -30, 0);
        var light = lightGo.AddComponent<Light>();
        light.type = LightType.Directional;
        light.intensity = 1f;
    }

    // --------------------------------------------------------
    // 建立所有展示
    // --------------------------------------------------------

    private void CreateAllShowcases(Transform stage)
    {
        // Portal
        portalRenderer = new PortalRenderer();
        var portalRT = portalRenderer.Initialize(stage);
        AddItem("Portal 傳送門",
            "Stencil Buffer 遮罩 + 第二鏡頭 RenderTexture\n門框寫入 Stencil，內部採樣另一個世界的畫面",
            ShowcaseSetup.CreatePortal(stage, portalRT),
            new List<SliderParam> {
                new SliderParam { label = "Edge Width", shaderProperty = "_EdgeWidth", min = 0.01f, max = 0.15f, defaultValue = 0.05f },
                new SliderParam { label = "Edge Glow", shaderProperty = "_EdgeGlow", min = 0f, max = 5f, defaultValue = 2.5f }
            });

        // Outline
        AddItem("Outline 描邊",
            "兩 Pass 渲染\nPass 1: Cull Front + 法線外擴 → 描邊輪廓\nPass 2: 正常渲染物件本體",
            ShowcaseSetup.CreateOutline(stage),
            new List<SliderParam> {
                new SliderParam { label = "Outline Width", shaderProperty = "_OutlineWidth", min = 0.001f, max = 0.1f, defaultValue = 0.035f }
            });

        // Chromatic Aberration
        AddItem("RGB 色差分離 Chromatic Aberration",
            "三次 UV 採樣：R 偏移 +offset, G 不偏移, B 偏移 -offset\n效能極輕，本質上只是三次貼圖採樣",
            ShowcaseSetup.CreateChromaticAberration(stage),
            new List<SliderParam> {
                new SliderParam { label = "Intensity", shaderProperty = "_Intensity", min = 0f, max = 0.05f, defaultValue = 0.025f }
            });

        // Noise Collection
        AddItem("雜訊合集 Noise",
            "Hash: frac(sin(dot())) 最輕純數學\nPerlin: 梯度雜訊 + FBM 疊加\nVoronoi: 距離場 cell 計算",
            ShowcaseSetup.CreateNoise(stage),
            new List<SliderParam> {
                new SliderParam { label = "Scale", shaderProperty = "_Scale", min = 1f, max = 50f, defaultValue = 20f },
                new SliderParam { label = "Speed", shaderProperty = "_Speed", min = 0f, max = 5f, defaultValue = 1f }
            });

        // Water
        AddItem("Water 水面",
            "頂點位移：多層 sin 波疊加\n菲涅爾邊緣高光 + UV 動畫水流\n半透明混合",
            ShowcaseSetup.CreateWater(stage),
            new List<SliderParam> {
                new SliderParam { label = "Wave Height", shaderProperty = "_WaveHeight", min = 0f, max = 0.5f, defaultValue = 0.15f },
                new SliderParam { label = "Wave Speed", shaderProperty = "_WaveSpeed", min = 0f, max = 5f, defaultValue = 1.5f },
                new SliderParam { label = "Wave Freq", shaderProperty = "_WaveFreq", min = 1f, max = 20f, defaultValue = 6f }
            });

        // Fire
        AddItem("Fire 火焰",
            "FBM 雜訊驅動火焰形狀\nUV 向上滾動 + 雜訊扭曲\n色彩漸層：白→黃→橙→紅→黑",
            ShowcaseSetup.CreateFire(stage),
            new List<SliderParam> {
                new SliderParam { label = "Fire Speed", shaderProperty = "_FireSpeed", min = 0.1f, max = 5f, defaultValue = 2f },
                new SliderParam { label = "Distortion", shaderProperty = "_Distortion", min = 0f, max = 2f, defaultValue = 0.8f },
                new SliderParam { label = "Noise Scale", shaderProperty = "_NoiseScale", min = 1f, max = 20f, defaultValue = 6f }
            });

        // Lightning
        AddItem("Lightning 閃電",
            "1D Noise 沿 Y 軸產生鋸齒路徑\n多層雜訊累加 + 分支效果\nexp() 核心亮線 + 光暈 + 時間閃爍",
            ShowcaseSetup.CreateLightning(stage),
            new List<SliderParam> {
                new SliderParam { label = "Thickness", shaderProperty = "_Thickness", min = 0.001f, max = 0.1f, defaultValue = 0.015f },
                new SliderParam { label = "Branches", shaderProperty = "_Branches", min = 0f, max = 5f, defaultValue = 2f },
                new SliderParam { label = "Flicker Speed", shaderProperty = "_FlickerSpeed", min = 1f, max = 20f, defaultValue = 8f }
            });

        // Stencil
        AddItem("Stencil 遮罩",
            "StencilWrite: 球形 mesh 寫入 Ref=2\nStencilRead: Comp Equal → 只在遮罩區域內渲染\n方塊只在球形範圍內可見",
            ShowcaseSetup.CreateStencil(stage));

        // Fog + Rain
        AddItem("Fog 霧 + Rain 雨",
            "Fog: 高度漸變密度 + 雜訊飄動\nRain: 多層 cell 分割 + 隨機偏移雨滴\n兩者都是程序化生成，不需要貼圖",
            ShowcaseSetup.CreateFogRain(stage),
            new List<SliderParam> {
                new SliderParam { label = "Fog Density", shaderProperty = "_Density", min = 0f, max = 5f, defaultValue = 2f },
                new SliderParam { label = "Rain Speed", shaderProperty = "_Speed", min = 1f, max = 20f, defaultValue = 8f }
            });

        // Bubble + Glow
        AddItem("Bubble 泡泡 + Glow 光暈",
            "Bubble: 菲涅爾邊緣 + sin() 薄膜干涉彩虹色\nGlow: Additive Blend + exp() 距離衰減 + 脈動",
            ShowcaseSetup.CreateBubbleGlow(stage),
            new List<SliderParam> {
                new SliderParam { label = "Fresnel Power", shaderProperty = "_FresnelPower", min = 1f, max = 8f, defaultValue = 3f },
                new SliderParam { label = "Rainbow", shaderProperty = "_RainbowStrength", min = 0f, max = 2f, defaultValue = 1f },
                new SliderParam { label = "Glow Intensity", shaderProperty = "_Intensity", min = 0.5f, max = 10f, defaultValue = 3f }
            });

        // Particles
        AddItem("Particles 粒子",
            "Blend One One (Additive 疊加發光)\n支援頂點色（粒子系統顏色直接影響）\n軟邊圓形：1 - dist² 衰減",
            ShowcaseSetup.CreateParticles(stage));

        // SSS 次表面散射
        AddItem("SSS 次表面散射",
            "模擬光在物體內部散射後出來的效果\n皮膚、蠟燭、樹葉的透光暖色，AAA 皮膚渲染核心技術\nV·(-L+N*d) 計算背面穿透強度 + 菲涅爾邊緣",
            ShowcaseSetup.CreateSSS(stage),
            new List<SliderParam> {
                new SliderParam { label = "SSS Power", shaderProperty = "_SSSPower", min = 1f, max = 16f, defaultValue = 4f },
                new SliderParam { label = "SSS Strength", shaderProperty = "_SSSStrength", min = 0f, max = 3f, defaultValue = 1.2f },
                new SliderParam { label = "Distortion", shaderProperty = "_SSSDistortion", min = 0f, max = 1f, defaultValue = 0.3f },
                new SliderParam { label = "Thickness", shaderProperty = "_Thickness", min = 0f, max = 1f, defaultValue = 0.5f }
            });

        // POM 視差貼圖
        AddItem("POM 視差遮擋貼圖",
            "不增加面數，讓平面有真實深度凹凸\nRay Marching 從相機角度步進高度場\n每步檢查是否碰到表面 + 二分法精修交點",
            ShowcaseSetup.CreatePOM(stage),
            new List<SliderParam> {
                new SliderParam { label = "Height Scale", shaderProperty = "_HeightScale", min = 0f, max = 0.3f, defaultValue = 0.1f },
                new SliderParam { label = "Steps", shaderProperty = "_Steps", min = 4f, max = 64f, defaultValue = 32f },
                new SliderParam { label = "Brick Scale", shaderProperty = "_BrickScale", min = 1f, max = 10f, defaultValue = 4f }
            });

        // Volumetric Cloud 體積雲
        AddItem("Volumetric Cloud 體積雲",
            "Ray Marching 逐步採樣 3D 雜訊密度\nBeer-Lambert 光吸收模型\n光線步進計算自遮擋 + 風速飄動，技術含量最高",
            ShowcaseSetup.CreateVolumetricCloud(stage),
            new List<SliderParam> {
                new SliderParam { label = "Density", shaderProperty = "_Density", min = 0.1f, max = 5f, defaultValue = 1.5f },
                new SliderParam { label = "Absorption", shaderProperty = "_Absorption", min = 0f, max = 2f, defaultValue = 0.6f },
                new SliderParam { label = "Cloud Scale", shaderProperty = "_CloudScale", min = 1f, max = 10f, defaultValue = 3f },
                new SliderParam { label = "Wind Speed", shaderProperty = "_WindSpeed", min = 0f, max = 2f, defaultValue = 0.3f },
                new SliderParam { label = "Light", shaderProperty = "_LightIntensity", min = 0.5f, max = 5f, defaultValue = 2f }
            });

        // Cloth Flag 布料旗幟
        AddItem("Cloth / Flag 布料旗幟",
            "Vertex Shader 數學物理模擬\n多層 sin/cos 疊加風力，UV.x 控制固定端\n即時重算法線讓光照正確，不需 CPU 物理",
            ShowcaseSetup.CreateClothFlag(stage),
            new List<SliderParam> {
                new SliderParam { label = "Wind Strength", shaderProperty = "_WindStrength", min = 0f, max = 2f, defaultValue = 0.8f },
                new SliderParam { label = "Wind Speed", shaderProperty = "_WindSpeed", min = 0f, max = 10f, defaultValue = 3f },
                new SliderParam { label = "Wind Freq", shaderProperty = "_WindFreq", min = 1f, max = 10f, defaultValue = 3f },
                new SliderParam { label = "Wave Amount", shaderProperty = "_FlagWave", min = 0f, max = 1f, defaultValue = 0.5f }
            });

        // SSR 螢幕空間反射
        AddItem("SSR 螢幕空間反射",
            "地板反射場景物體，模擬現代引擎 SSR\n對每像素沿反射方向 Ray Trace\n菲涅爾控制反射強度 + 粗糙度擾動",
            ShowcaseSetup.CreateSSR(stage),
            new List<SliderParam> {
                new SliderParam { label = "Reflectivity", shaderProperty = "_Reflectivity", min = 0f, max = 1f, defaultValue = 0.6f },
                new SliderParam { label = "Fresnel Power", shaderProperty = "_FresnelPower", min = 1f, max = 8f, defaultValue = 3f },
                new SliderParam { label = "Roughness", shaderProperty = "_Roughness", min = 0f, max = 1f, defaultValue = 0.15f }
            });

        // Rim Light 邊緣光
        AddItem("Rim Light 邊緣光",
            "核心只有一行：rim = 1.0 - dot(normal, viewDir)\n法線和視角方向的點積，邊緣處接近 0\npow() 控制寬窄，乘上顏色就是電影級輪廓光\n角色、道具、Boss 登場必備",
            ShowcaseSetup.CreateRimLight(stage),
            new List<SliderParam> {
                new SliderParam { label = "Rim Power", shaderProperty = "_RimPower", min = 1f, max = 8f, defaultValue = 3f },
                new SliderParam { label = "Rim Intensity", shaderProperty = "_RimIntensity", min = 0f, max = 5f, defaultValue = 1.5f }
            });

        // Fresnel 菲涅爾
        AddItem("Fresnel 菲涅爾",
            "和 Rim Light 原理一樣：1 - dot(N, V)\n但用在透明/反射材質上：越斜著看越反光\n這是物理上真實存在的現象\n玻璃、水面、泡泡、能量護盾都靠它",
            ShowcaseSetup.CreateFresnel(stage),
            new List<SliderParam> {
                new SliderParam { label = "Fresnel Power", shaderProperty = "_FresnelPower", min = 1f, max = 8f, defaultValue = 3f },
                new SliderParam { label = "Opacity", shaderProperty = "_Opacity", min = 0f, max = 1f, defaultValue = 0.15f },
                new SliderParam { label = "Env Reflect", shaderProperty = "_EnvReflect", min = 0f, max = 1f, defaultValue = 0.5f }
            });

        // Color Grading 顏色分級
        AddItem("Color Grading 顏色分級",
            "把畫面的顏色重新映射：pow(color, gamma)\n對 RGB 三個通道分別做曲線調整\n加上對比度和飽和度控制\n電影感、復古、賽博龐克，本質都是顏色數學",
            ShowcaseSetup.CreateColorGrading(stage),
            new List<SliderParam> {
                new SliderParam { label = "Gamma R", shaderProperty = "_GammaR", min = 0.2f, max = 3f, defaultValue = 1f },
                new SliderParam { label = "Gamma G", shaderProperty = "_GammaG", min = 0.2f, max = 3f, defaultValue = 1f },
                new SliderParam { label = "Gamma B", shaderProperty = "_GammaB", min = 0.2f, max = 3f, defaultValue = 1f },
                new SliderParam { label = "Contrast", shaderProperty = "_Contrast", min = 0.5f, max = 2f, defaultValue = 1f },
                new SliderParam { label = "Saturation", shaderProperty = "_Saturation", min = 0f, max = 2f, defaultValue = 1f },
                new SliderParam { label = "Brightness", shaderProperty = "_Brightness", min = 0.5f, max = 2f, defaultValue = 1f }
            });

        // Normal Map 法線貼圖
        AddItem("Normal Map 法線貼圖",
            "模型面數完全沒變，只是欺騙光照計算\n用程序化磚牆法線改變表面方向\nTBN 矩陣：切線空間 → 世界空間\n最高 CP 值的細節技術，幾乎所有 3D 遊戲都在用",
            ShowcaseSetup.CreateNormalMap(stage),
            new List<SliderParam> {
                new SliderParam { label = "Normal Strength", shaderProperty = "_NormalStrength", min = 0f, max = 3f, defaultValue = 1.5f },
                new SliderParam { label = "Brick Scale", shaderProperty = "_BrickScale", min = 1f, max = 10f, defaultValue = 4f },
                new SliderParam { label = "Light Angle", shaderProperty = "_LightAngle", min = 0f, max = 360f, defaultValue = 45f }
            });

        // Dithering 抖動
        AddItem("Dithering 抖動",
            "用 Bayer 4×4 矩陣的規律點陣圖案\n根據像素位置決定要不要顯示 → clip()\n模擬半透明但不需要排序，效能極好\n早期遊戲大量使用，現在用在風格化渲染有復古美感",
            ShowcaseSetup.CreateDithering(stage),
            new List<SliderParam> {
                new SliderParam { label = "Opacity", shaderProperty = "_Opacity", min = 0f, max = 1f, defaultValue = 0.5f },
                new SliderParam { label = "Dither Scale", shaderProperty = "_DitherScale", min = 1f, max = 8f, defaultValue = 1f }
            });
    }

    private void AddItem(string name, string description, GameObject root, List<SliderParam> sliders = null)
    {
        items.Add(new ShowcaseItem
        {
            name = name,
            description = description,
            root = root,
            sliders = sliders ?? new List<SliderParam>()
        });
    }

    // --------------------------------------------------------
    // 切換展示
    // --------------------------------------------------------

    public void ShowEffect(int index)
    {
        if (index < 0 || index >= items.Count) return;

        for (int i = 0; i < items.Count; i++)
            items[i].root.SetActive(i == index);

        currentIndex = index;

        // Portal 鏡頭只在 Portal 展示時開啟
        portalRenderer?.SetActive(index == 0);

        UpdateUI();
        BuildSliders();
    }

    public void NextEffect()
    {
        ShowEffect((currentIndex + 1) % items.Count);
    }

    public void PrevEffect()
    {
        ShowEffect((currentIndex - 1 + items.Count) % items.Count);
    }

    // --------------------------------------------------------
    // UI 建立
    // --------------------------------------------------------

    private void CreateUI()
    {
        Font font = FontHelper.GetFont();

        // Canvas
        var canvasGo = new GameObject("ShowcaseCanvas");
        canvasGo.transform.SetParent(transform, false);
        var canvas = canvasGo.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;
        var scaler = canvasGo.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        canvasGo.AddComponent<GraphicRaycaster>();

        // 標題（頂部小條）
        titleText = CreateUIText(canvasGo.transform, font, "標題",
            new Vector2(0.15f, 0.92f), new Vector2(0.85f, 0.99f), 28, TextAnchor.MiddleCenter);

        // 說明文字（緊貼標題下方）
        descText = CreateUIText(canvasGo.transform, font, "說明",
            new Vector2(0.15f, 0.82f), new Vector2(0.85f, 0.92f), 16, TextAnchor.UpperCenter);
        descText.color = new Color(0.8f, 0.85f, 0.9f);

        // 頁碼指示（底部中央）
        indexText = CreateUIText(canvasGo.transform, font, "1/1",
            new Vector2(0.4f, 0.01f), new Vector2(0.6f, 0.06f), 18, TextAnchor.MiddleCenter);
        indexText.color = new Color(0.6f, 0.6f, 0.6f);

        // 上一個按鈕（左下）
        CreateButton(canvasGo.transform, font, "← 上一個",
            new Vector2(0.02f, 0.01f), new Vector2(0.18f, 0.07f), PrevEffect);

        // 下一個按鈕（右下）
        CreateButton(canvasGo.transform, font, "下一個 →",
            new Vector2(0.82f, 0.01f), new Vector2(0.98f, 0.07f), NextEffect);

        // EventSystem（沒有的話按鈕無法點擊）
        if (FindAnyObjectByType<EventSystem>() == null)
        {
            var esGo = new GameObject("EventSystem");
            esGo.transform.SetParent(transform, false);
            esGo.AddComponent<EventSystem>();
            esGo.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
        }

        // 右側滑桿面板
        var sliderPanel = new GameObject("SliderPanel");
        sliderPanel.transform.SetParent(canvasGo.transform, false);
        var panelRt = sliderPanel.AddComponent<RectTransform>();
        panelRt.anchorMin = new Vector2(0.78f, 0.10f);
        panelRt.anchorMax = new Vector2(0.99f, 0.80f);
        panelRt.offsetMin = Vector2.zero;
        panelRt.offsetMax = Vector2.zero;

        var panelBg = sliderPanel.AddComponent<Image>();
        panelBg.color = new Color(0.1f, 0.12f, 0.18f, 0.75f);
        panelBg.raycastTarget = false;

        var vlg = sliderPanel.AddComponent<VerticalLayoutGroup>();
        vlg.padding = new RectOffset(8, 8, 8, 8);
        vlg.spacing = 6;
        vlg.childForceExpandWidth = true;
        vlg.childForceExpandHeight = false;
        vlg.childControlWidth = true;
        vlg.childControlHeight = true;

        sliderContainer = sliderPanel.transform;
    }

    private void UpdateUI()
    {
        if (currentIndex < 0 || currentIndex >= items.Count) return;

        var item = items[currentIndex];
        if (titleText != null) titleText.text = item.name;
        if (descText != null) descText.text = item.description;
        if (indexText != null) indexText.text = $"{currentIndex + 1} / {items.Count}";
    }

    // --------------------------------------------------------
    // UI 工具
    // --------------------------------------------------------

    private Text CreateUIText(Transform parent, Font font, string defaultText,
        Vector2 anchorMin, Vector2 anchorMax, int fontSize, TextAnchor alignment)
    {
        var go = new GameObject(defaultText);
        go.transform.SetParent(parent, false);

        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        var text = go.AddComponent<Text>();
        text.font = font;
        text.fontSize = fontSize;
        text.color = Color.white;
        text.alignment = alignment;
        text.horizontalOverflow = HorizontalWrapMode.Wrap;
        text.verticalOverflow = VerticalWrapMode.Overflow;
        text.text = defaultText;
        text.raycastTarget = false; // 不擋住 3D 場景的點擊

        // 文字陰影增加可讀性
        var shadow = go.AddComponent<Shadow>();
        shadow.effectColor = new Color(0, 0, 0, 0.8f);
        shadow.effectDistance = new Vector2(1, -1);

        return text;
    }

    private void CreateButton(Transform parent, Font font, string label,
        Vector2 anchorMin, Vector2 anchorMax, UnityEngine.Events.UnityAction onClick)
    {
        var go = new GameObject(label);
        go.transform.SetParent(parent, false);

        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        var bg = go.AddComponent<Image>();
        bg.color = new Color(0.2f, 0.25f, 0.35f, 0.9f);

        var btn = go.AddComponent<Button>();
        btn.targetGraphic = bg;
        btn.onClick.AddListener(onClick);

        // 按鈕文字
        var textGo = new GameObject("Text");
        textGo.transform.SetParent(go.transform, false);
        var textRt = textGo.AddComponent<RectTransform>();
        textRt.anchorMin = Vector2.zero;
        textRt.anchorMax = Vector2.one;
        textRt.offsetMin = Vector2.zero;
        textRt.offsetMax = Vector2.zero;

        var text = textGo.AddComponent<Text>();
        text.font = font;
        text.fontSize = 20;
        text.color = Color.white;
        text.text = label;
        text.alignment = TextAnchor.MiddleCenter;
    }

    // --------------------------------------------------------
    // 滑桿建立
    // --------------------------------------------------------

    private void BuildSliders()
    {
        if (sliderContainer == null) return;

        // 清除舊滑桿
        for (int i = sliderContainer.childCount - 1; i >= 0; i--)
            Destroy(sliderContainer.GetChild(i).gameObject);

        var item = items[currentIndex];
        if (item.sliders == null || item.sliders.Count == 0)
        {
            sliderContainer.gameObject.SetActive(false);
            return;
        }
        sliderContainer.gameObject.SetActive(true);

        // 收集此展品的所有材質
        var renderers = item.root.GetComponentsInChildren<Renderer>(true);

        Font font = FontHelper.GetFont();

        foreach (var sp in item.sliders)
        {
            CreateSliderRow(sliderContainer, font, sp, renderers);
        }
    }

    private void CreateSliderRow(Transform parent, Font font, SliderParam sp, Renderer[] renderers)
    {
        // 整行容器
        var row = new GameObject(sp.label);
        row.transform.SetParent(parent, false);
        var rowLe = row.AddComponent<LayoutElement>();
        rowLe.preferredHeight = 50;

        var rowRt = row.AddComponent<RectTransform>();

        // 標籤
        var labelGo = new GameObject("Label");
        labelGo.transform.SetParent(row.transform, false);
        var labelRt = labelGo.AddComponent<RectTransform>();
        labelRt.anchorMin = new Vector2(0, 0.5f);
        labelRt.anchorMax = new Vector2(1, 1);
        labelRt.offsetMin = Vector2.zero;
        labelRt.offsetMax = Vector2.zero;

        var labelText = labelGo.AddComponent<Text>();
        labelText.font = font;
        labelText.fontSize = 14;
        labelText.color = new Color(0.85f, 0.9f, 0.95f);
        labelText.text = $"{sp.label}: {sp.defaultValue:F2}";
        labelText.alignment = TextAnchor.MiddleLeft;
        labelText.raycastTarget = false;

        // 滑桿
        var sliderGo = new GameObject("Slider");
        sliderGo.transform.SetParent(row.transform, false);
        var sliderRt = sliderGo.AddComponent<RectTransform>();
        sliderRt.anchorMin = new Vector2(0, 0);
        sliderRt.anchorMax = new Vector2(1, 0.5f);
        sliderRt.offsetMin = Vector2.zero;
        sliderRt.offsetMax = Vector2.zero;

        // 背景
        var bgGo = new GameObject("Background");
        bgGo.transform.SetParent(sliderGo.transform, false);
        var bgRt = bgGo.AddComponent<RectTransform>();
        bgRt.anchorMin = new Vector2(0, 0.35f);
        bgRt.anchorMax = new Vector2(1, 0.65f);
        bgRt.offsetMin = Vector2.zero;
        bgRt.offsetMax = Vector2.zero;
        var bgImg = bgGo.AddComponent<Image>();
        bgImg.color = new Color(0.2f, 0.22f, 0.28f, 1f);

        // 填充區域
        var fillArea = new GameObject("Fill Area");
        fillArea.transform.SetParent(sliderGo.transform, false);
        var fillAreaRt = fillArea.AddComponent<RectTransform>();
        fillAreaRt.anchorMin = new Vector2(0, 0.35f);
        fillAreaRt.anchorMax = new Vector2(1, 0.65f);
        fillAreaRt.offsetMin = Vector2.zero;
        fillAreaRt.offsetMax = Vector2.zero;

        var fillGo = new GameObject("Fill");
        fillGo.transform.SetParent(fillArea.transform, false);
        var fillRt = fillGo.AddComponent<RectTransform>();
        fillRt.anchorMin = Vector2.zero;
        fillRt.anchorMax = Vector2.one;
        fillRt.offsetMin = Vector2.zero;
        fillRt.offsetMax = Vector2.zero;
        var fillImg = fillGo.AddComponent<Image>();
        fillImg.color = new Color(0.35f, 0.55f, 0.85f, 1f);

        // 拖曳手柄
        var handleArea = new GameObject("Handle Slide Area");
        handleArea.transform.SetParent(sliderGo.transform, false);
        var handleAreaRt = handleArea.AddComponent<RectTransform>();
        handleAreaRt.anchorMin = Vector2.zero;
        handleAreaRt.anchorMax = Vector2.one;
        handleAreaRt.offsetMin = Vector2.zero;
        handleAreaRt.offsetMax = Vector2.zero;

        var handleGo = new GameObject("Handle");
        handleGo.transform.SetParent(handleArea.transform, false);
        var handleRt = handleGo.AddComponent<RectTransform>();
        handleRt.sizeDelta = new Vector2(14, 0);
        var handleImg = handleGo.AddComponent<Image>();
        handleImg.color = new Color(0.9f, 0.9f, 0.95f, 1f);

        // Slider 組件
        var slider = sliderGo.AddComponent<Slider>();
        slider.fillRect = fillRt;
        slider.handleRect = handleRt;
        slider.targetGraphic = handleImg;
        slider.minValue = sp.min;
        slider.maxValue = sp.max;
        slider.value = sp.defaultValue;
        slider.direction = Slider.Direction.LeftToRight;

        // 改值時即時更新所有材質
        string prop = sp.shaderProperty;
        slider.onValueChanged.AddListener((float val) =>
        {
            labelText.text = $"{sp.label}: {val:F2}";
            foreach (var r in renderers)
            {
                if (r != null && r.material != null && r.material.HasFloat(prop))
                    r.material.SetFloat(prop, val);
            }
        });
    }

    // --------------------------------------------------------
    // 鍵盤快捷鍵
    // --------------------------------------------------------

    void Update()
    {
        var kb = Keyboard.current;
        if (kb == null) return;
        if (kb.rightArrowKey.wasPressedThisFrame || kb.dKey.wasPressedThisFrame)
            NextEffect();
        if (kb.leftArrowKey.wasPressedThisFrame || kb.aKey.wasPressedThisFrame)
            PrevEffect();
    }
}
