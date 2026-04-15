using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

/// <summary>
/// 掃描系統：螢幕中央方框偵測星星，顯示掃描按鈕，按下後 Log 星星名稱。
/// </summary>
public class ScanSystem : MonoBehaviour
{
    [Header("掃描框設定 (螢幕像素)")]
    [SerializeField] private float reticleWidth = 120f;
    [SerializeField] private float reticleHeight = 100f;

    [Header("UI 參照")]
    [SerializeField] private RectTransform reticleFrame;
    [SerializeField] private GameObject scanPanel;
    [SerializeField] private Button scanButton;
    [SerializeField] private Text scanButtonText;
    [SerializeField] private Text scanHintText;
    [SerializeField] private Text targetNameText;

    private Star _currentTarget;
    private int _scanCounter = 100;

    private Rect ReticleScreenRect
    {
        get
        {
            float cx = Screen.width * 0.5f;
            float cy = Screen.height * 0.5f;
            return new Rect(cx - reticleWidth * 0.5f, cy - reticleHeight * 0.5f, reticleWidth, reticleHeight);
        }
    }

    private void OnEnable()
    {
        if (scanButton != null)
            scanButton.onClick.AddListener(OnScanClicked);
    }

    private void OnDisable()
    {
        if (scanButton != null)
            scanButton.onClick.RemoveListener(OnScanClicked);
    }

    public void LateInit()
    {
        if (scanButton != null)
            scanButton.onClick.AddListener(OnScanClicked);
    }

    private void Start()
    {
        scanPanel.SetActive(false);
        SetupReticleSize();
    }

    private void Update()
    {
        _currentTarget = FindStarInReticle();

        bool hasTarget = _currentTarget != null;
        scanPanel.SetActive(hasTarget);

        if (hasTarget)
        {
            // 已掃描過的星顯示名稱，未掃描的顯示 "未知天體"
            targetNameText.text = _currentTarget.IsScanned ? _currentTarget.StarName : "未知天體";
        }

        if (hasTarget && Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
            PerformScan();
    }

    private Star FindStarInReticle()
    {
        if (PlanetFactory.Instance == null) return null;

        Camera cam = Camera.main;
        Rect rect = ReticleScreenRect;
        Star closest = null;
        float closestDist = float.MaxValue;

        foreach (Star star in PlanetFactory.Instance.ActiveStars)
        {
            Vector3 screenPos = cam.WorldToScreenPoint(star.transform.position);
            if (screenPos.z < 0) continue;

            Vector2 sp = new(screenPos.x, screenPos.y);
            if (!rect.Contains(sp)) continue;

            float dist = Vector2.Distance(sp, new Vector2(Screen.width * 0.5f, Screen.height * 0.5f));
            if (dist < closestDist)
            {
                closestDist = dist;
                closest = star;
            }
        }

        return closest;
    }
    private void OnScanClicked()
    {
        if (_currentTarget != null)
            PerformScan();
    }

    private void PerformScan()
    {
        if (_currentTarget.IsScanned)
        {
            Debug.Log($"已掃描過: {_currentTarget.StarName}");
            return;
        }

        string newName = $"EMETH-{_scanCounter++}";
        // 如果有預設名就用預設名，否則用 EMETH 編號
        string displayName = string.IsNullOrEmpty(_currentTarget.Data?.defaultName)
            ? newName
            : _currentTarget.Data.defaultName;

        _currentTarget.Scan(displayName);
        targetNameText.text = displayName;
        Debug.Log($"掃描完成: {displayName} ({newName})");

        // 通知任務系統
        if (QuestManager.Instance != null)
            QuestManager.Instance.NotifyScanComplete(_currentTarget);

        // 自動存檔
        if (SaveManager.Instance != null)
            SaveManager.Instance.Save();

        // 觸發詳情面板 (僅任務行星)
        if (_currentTarget.Data != null && _currentTarget.Data.isQuestTarget)
        {
            if (PlanetDetailUI.Instance != null)
                PlanetDetailUI.Instance.Show(_currentTarget);
        }
    }

    private void SetupReticleSize()
    {
        if (reticleFrame != null)
            reticleFrame.sizeDelta = new Vector2(reticleWidth, reticleHeight);
    }
}
