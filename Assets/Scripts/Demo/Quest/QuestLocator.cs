using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 定位提示系統：顯示漸進式條件（象限、星系、座標、濾鏡），
/// 未達成=白字，達成=金黃字。由 GameSetup 透過 Init() 注入 UI 參照。
/// </summary>
public class QuestLocator : MonoBehaviour
{
    public static QuestLocator Instance { get; private set; }

    private GameObject _panel;
    private Text _condQuadrant;
    private Text _condStarSystem;
    private Text _condCoord;
    private Text _condFilter;

    private static readonly Color ColorMet = new(1f, 0.84f, 0.2f, 1f);
    private static readonly Color ColorUnmet = new(0.85f, 0.85f, 0.85f, 0.7f);

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(this); return; }
        Instance = this;
    }

    public void Init(GameObject panel, Text condQ, Text condS, Text condC, Text condF)
    {
        _panel = panel;
        _condQuadrant = condQ;
        _condStarSystem = condS;
        _condCoord = condC;
        _condFilter = condF;
        _panel.SetActive(false);
    }

    private void Update()
    {
        if (_panel == null || QuestManager.Instance == null)
        {
            if (_panel != null) _panel.SetActive(false);
            return;
        }

        Star target = QuestManager.Instance.LocatingTarget;
        if (target == null || target.IsScanned)
        {
            _panel.SetActive(false);
            return;
        }

        _panel.SetActive(true);
        EvaluateConditions(target);
    }

    private void EvaluateConditions(Star target)
    {
        Vector2 camPos = QuadrantManager.Instance != null
            ? QuadrantManager.Instance.CameraPosition : Vector2.zero;
        Vector2 targetPos = target.Data.position.ToVector2();

        int targetQ = QuadrantManager.Instance != null
            ? QuadrantManager.Instance.GetQuadrant(targetPos) : 0;
        int currentQ = QuadrantManager.Instance != null
            ? QuadrantManager.Instance.CurrentQuadrant : 0;
        SetCondition(_condQuadrant, $"第{targetQ}象限", targetQ == currentQ);

        string sysName = StarSystemManager.Instance != null
            ? StarSystemManager.Instance.GetStarSystemForPlanet(target.Data) ?? "未知" : "未知";
        string currentSys = StarSystemManager.Instance != null
            ? StarSystemManager.Instance.GetStarSystemName(camPos) : null;
        SetCondition(_condStarSystem, $"{sysName} 星域",
            !string.IsNullOrEmpty(currentSys) && currentSys == sysName);

        // 用原始鏡頭位置和 wrap 後的星星 transform 計算距離（跨邊界也正確）
        Vector3 rawCam = Camera.main.transform.position;
        float dist = Vector2.Distance(new Vector2(rawCam.x, rawCam.y), (Vector2)target.transform.position);
        SetCondition(_condCoord, $"座標範圍 ({targetPos.x:F0}, {targetPos.y:F0})", dist <= 10f);

        bool needFilter = target.Data.filterOnly;
        bool filterOn = FilterManager.Instance != null && FilterManager.Instance.IsFilterActive;
        SetCondition(_condFilter, needFilter ? "需要開啟濾鏡" : "無需開啟濾鏡",
            needFilter ? filterOn : !filterOn);
    }

    private static void SetCondition(Text text, string label, bool met)
    {
        text.text = $"[{(met ? "✓" : " ")}] {label}";
        text.color = met ? ColorMet : ColorUnmet;
    }
}
