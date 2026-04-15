using System;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 太空船視圖：從 SceneConfig.json 載入場景物件並動態建立可互動 UI。
/// Show() / Hide() 控制顯隱。企劃透過 generate_scene.py 輸出 JSON 即可修改內容。
/// </summary>
public class SpaceshipView : MonoBehaviour
{
    public static SpaceshipView Instance { get; private set; }

    private GameObject _root;
    private Canvas _canvas;

    public bool IsActive => _root != null && _root.activeSelf;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(this); return; }
        Instance = this;
    }

    public void Init()
    {
        BuildScene();
        _root.SetActive(false);
    }

    public void Show() => _root.SetActive(true);
    public void Hide() => _root.SetActive(false);

    private void BuildScene()
    {
        // ── 根 Canvas ──
        _root = new GameObject("SpaceshipView");
        _root.transform.SetParent(transform, false);
        _canvas = _root.AddComponent<Canvas>();
        _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        _canvas.sortingOrder = 50;
        var scaler = _root.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        _root.AddComponent<GraphicRaycaster>();

        var rootRT = _root.GetComponent<RectTransform>();

        // ── 背景 ──
        var bgGo = new GameObject("Background");
        bgGo.transform.SetParent(rootRT, false);
        Stretch(bgGo.AddComponent<RectTransform>());
        bgGo.AddComponent<Image>().color = new Color(0.06f, 0.06f, 0.1f, 1f);

        // ── 標題 ──
        CreateText(rootRT, "Title", "太空船艦橋",
            new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -40), 28);

        // ── 從 SceneConfig.json 載入場景物件 ──
        var sceneEntry = SceneConfigLoader.GetScene("spaceship");
        if (sceneEntry != null && sceneEntry.objects != null)
        {
            foreach (var obj in sceneEntry.objects)
                CreateInteractableFromData(rootRT, obj);
        }
        else
        {
            Debug.LogWarning("SpaceshipView: SceneConfig.json 找不到 'spaceship' 場景，使用預設物件。");
            CreateInteractableFallback(rootRT);
        }
    }

    // ── 由 JSON 資料建立互動物件 ──
    private void CreateInteractableFromData(RectTransform parent, SceneObjectEntry obj)
    {
        var color = new Color(obj.colorR, obj.colorG, obj.colorB);
        CreateInteractable(parent, obj.label,
            new Vector2(obj.posX, obj.posY),
            new Vector2(obj.width, obj.height),
            color, obj.dialogues);
    }

    // ── 預設物件（SceneConfig.json 不存在時使用）──
    private void CreateInteractableFallback(RectTransform parent)
    {
        CreateInteractable(parent, "控制台", new Vector2(-300, 0), new Vector2(120, 120),
            new Color(0.3f, 0.4f, 0.6f), new[]
            {
                new DialogueLine { speaker = "系統", portrait = "", text = "控制台運作正常，所有系統綠燈。" }
            });

        CreateInteractable(parent, "觀測窗", new Vector2(0, 50), new Vector2(200, 150),
            new Color(0.1f, 0.15f, 0.3f), new[]
            {
                new DialogueLine { speaker = "系統", portrait = "", text = "窗外是無盡的星海..." },
                new DialogueLine { speaker = "系統", portrait = "", text = "也許該用望遠鏡仔細觀察一下。" }
            });

        CreateInteractable(parent, "通訊設備", new Vector2(300, 0), new Vector2(100, 100),
            new Color(0.5f, 0.3f, 0.3f), new[]
            {
                new DialogueLine { speaker = "通訊員", portrait = "operator", text = "通訊頻道暢通，等待總部進一步指示。" }
            });
    }

    private void CreateInteractable(RectTransform parent, string label,
        Vector2 pos, Vector2 size, Color color, DialogueLine[] lines)
    {
        var go = new GameObject("Interactable_" + label);
        go.transform.SetParent(parent, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;

        var img = go.AddComponent<Image>();
        img.color = color;

        CreateText(rt, "Label", label,
            new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0, -20), 16);

        var btn = go.AddComponent<Button>();
        btn.targetGraphic = img;
        DialogueLine[] captured = lines;
        btn.onClick.AddListener(() =>
        {
            if (DialogueUI.Instance != null)
                DialogueUI.Instance.PlayLines(captured);
        });
    }

    // ── Helpers ──

    private static GameObject CreateText(RectTransform parent, string name, string content,
        Vector2 anchorMin, Vector2 anchorMax, Vector2 pos, int fontSize)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.anchoredPosition = pos;
        rt.sizeDelta = new Vector2(400, 40);

        var text = go.AddComponent<Text>();
        text.text = content;
        text.fontSize = fontSize;
        text.alignment = TextAnchor.MiddleCenter;
        text.color = Color.white;
        text.font = FontHelper.GetFont();
        text.raycastTarget = false;

        var outline = go.AddComponent<Outline>();
        outline.effectColor = new Color(0, 0, 0, 0.7f);
        outline.effectDistance = new Vector2(1, -1);

        return go;
    }

    private static void Stretch(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }
}
