using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 任務提示系統：定位開始 5 秒後未掃描到目標，顯示「提示」按鈕；
/// 點擊後出現指向目標行星的箭頭，持續約 3 秒。
/// </summary>
public class QuestHintUI : MonoBehaviour
{
    public static QuestHintUI Instance { get; private set; }

    private GameObject _hintButton;
    private GameObject _arrowRoot;
    private RectTransform _arrowRT;

    private float _locatingTimer;
    private float _arrowTimer;
    private bool _arrowShowing;

    private const float HintDelay = 5f;
    private const float ArrowDuration = 3f;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(this); return; }
        Instance = this;
    }

    public void Init(GameObject hintButton, GameObject arrowRoot, RectTransform arrowRT)
    {
        _hintButton = hintButton;
        _arrowRoot = arrowRoot;
        _arrowRT = arrowRT;
        _hintButton.SetActive(false);
        _arrowRoot.SetActive(false);
    }

    private void Update()
    {
        Star target = QuestManager.Instance != null
            ? QuestManager.Instance.LocatingTarget : null;

        // 沒有定位目標 → 全部隱藏，重設計時
        if (target == null || target.IsScanned)
        {
            _hintButton.SetActive(false);
            _arrowRoot.SetActive(false);
            _locatingTimer = 0f;
            _arrowShowing = false;
            return;
        }

        // 箭頭顯示中
        if (_arrowShowing)
        {
            UpdateArrowDirection(target);
            _arrowTimer -= Time.deltaTime;
            if (_arrowTimer <= 0f)
            {
                _arrowShowing = false;
                _arrowRoot.SetActive(false);
                _locatingTimer = 0f; // 重新計時，可再次提示
            }
            return;
        }

        // 累計定位時間
        _locatingTimer += Time.deltaTime;
        if (_locatingTimer >= HintDelay)
        {
            _hintButton.SetActive(true);
        }
    }

    /// <summary>提示按鈕點擊 → 顯示箭頭。</summary>
    public void OnHintClicked()
    {
        _hintButton.SetActive(false);
        _arrowShowing = true;
        _arrowTimer = ArrowDuration;
        _arrowRoot.SetActive(true);
    }

    private void UpdateArrowDirection(Star target)
    {
        // 星星已被 PlanetFactory wrap 到鏡頭附近，直接用 transform 算方向
        Vector3 camPos = Camera.main.transform.position;
        Vector2 dir = ((Vector2)target.transform.position - new Vector2(camPos.x, camPos.y)).normalized;

        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        _arrowRT.rotation = Quaternion.Euler(0, 0, angle);
    }
}
