using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 行星詳情面板：掃描完成後自動彈出，顯示名稱(可編輯)、EMETH編號、描述、數值表。
/// </summary>
public class PlanetDetailUI : MonoBehaviour
{
    public static PlanetDetailUI Instance { get; private set; }

    [SerializeField] private GameObject panel;
    [SerializeField] private InputField nameInput;
    [SerializeField] private Text emethIdText;
    [SerializeField] private Text descriptionText;
    [SerializeField] private Text similarityText;
    [SerializeField] private Text radiusText;
    [SerializeField] private Text massText;
    [SerializeField] private Text temperatureText;
    [SerializeField] private Text waterText;
    [SerializeField] private Button closeButton;

    private Star _currentStar;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void Start()
    {
        panel.SetActive(false);
        if (closeButton != null)
            closeButton.onClick.AddListener(Hide);
        if (nameInput != null)
            nameInput.onEndEdit.AddListener(OnNameChanged);
    }

    public void Show(Star star)
    {
        _currentStar = star;
        panel.SetActive(true);

        var d = star.Data;
        if (d == null) return;

        nameInput.text = star.StarName;
        emethIdText.text = d.id;
        descriptionText.text = d.description;
        similarityText.text = $"{d.earthSimilarity:F2}%";
        radiusText.text = d.radius.ToString("N0");
        massText.text = d.mass.ToString("N0");
        temperatureText.text = $"{d.temperature}°C";
        waterText.text = $"{d.waterPercent}%";
    }

    public void Hide()
    {
        panel.SetActive(false);
        _currentStar = null;
    }

    private void OnNameChanged(string newName)
    {
        if (_currentStar == null || string.IsNullOrWhiteSpace(newName)) return;
        _currentStar.Rename(newName);

        // 通知任務面板更新
        if (QuestManager.Instance != null)
            QuestManager.Instance.NotifyScanComplete(_currentStar);
    }
}
