using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// 遊戲流程狀態機：控制太空船/望遠鏡模式切換 + 故事章節觸發。
///
/// 流程：太空船 → 對話 → 望遠鏡(星圖) → 掃描完成 → 對話 → 太空船 → ...
///
/// 狀態:
///   Spaceship  — 太空船場景
///   Dialogue   — 對話播放中
///   StarMap    — 望遠鏡觀測模式
/// </summary>
public class GameFlowManager : MonoBehaviour
{
    public static GameFlowManager Instance { get; private set; }

    public enum FlowState { Spaceship, Dialogue, StarMap }
    public FlowState CurrentState { get; private set; }

    private ChapterEntry[] _chapters;
    private readonly HashSet<string> _playedChapters = new();
    private int _lastCompletedQuest;

    // HUD 元素參照（由 GameSetup 注入）
    private GameObject _starMapHUD;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(this); return; }
        Instance = this;
    }

    public void Init(GameObject starMapHUD)
    {
        _starMapHUD = starMapHUD;

        var config = StoryConfigLoader.Load();
        _chapters = config?.chapters ?? new ChapterEntry[0];

        // 訂閱任務完成事件
        if (QuestManager.Instance != null)
            QuestManager.Instance.OnQuestUpdated += OnQuestUpdated;

        // 遊戲開始：先進太空船，然後播第 0 章
        EnterSpaceship();
        TryPlayChapter(0);
    }

    private void OnDestroy()
    {
        if (QuestManager.Instance != null)
            QuestManager.Instance.OnQuestUpdated -= OnQuestUpdated;
    }

    // ──────────────── 模式切換 ────────────────

    /// <summary>切換到太空船模式。</summary>
    public void EnterSpaceship()
    {
        CurrentState = FlowState.Spaceship;

        if (SpaceshipView.Instance != null) SpaceshipView.Instance.Show();
        if (_starMapHUD != null) _starMapHUD.SetActive(false);
        SetCameraControlEnabled(false);

        Debug.Log("FlowState → Spaceship");
    }

    /// <summary>切換到望遠鏡(星圖)模式。</summary>
    public void EnterStarMap()
    {
        CurrentState = FlowState.StarMap;

        if (SpaceshipView.Instance != null) SpaceshipView.Instance.Hide();
        if (_starMapHUD != null) _starMapHUD.SetActive(true);
        SetCameraControlEnabled(true);

        Debug.Log("FlowState → StarMap");
    }

    /// <summary>手動切換太空船/望遠鏡（按鈕用）。</summary>
    public void ToggleMode()
    {
        if (CurrentState == FlowState.Dialogue) return; // 對話中不能切換

        if (CurrentState == FlowState.Spaceship)
            EnterStarMap();
        else
            EnterSpaceship();
    }

    // ──────────────── 故事觸發 ────────────────

    private void OnQuestUpdated()
    {
        int completed = QuestManager.Instance != null ? QuestManager.Instance.CompletedCount : 0;
        if (completed > _lastCompletedQuest)
        {
            _lastCompletedQuest = completed;
            // 延一幀，等掃描 UI 關閉後再播對話
            StartCoroutine(DelayedChapterCheck(completed));
        }
    }

    private System.Collections.IEnumerator DelayedChapterCheck(int questCount)
    {
        yield return null;
        TryPlayChapter(questCount);
    }

    private void TryPlayChapter(int triggerAfterQuest)
    {
        var chapter = _chapters.FirstOrDefault(c =>
            c.triggerAfterQuest == triggerAfterQuest && !_playedChapters.Contains(c.id));

        if (chapter == null) return;

        _playedChapters.Add(chapter.id);
        CurrentState = FlowState.Dialogue;

        Debug.Log($"Playing chapter: {chapter.id}");

        if (DialogueUI.Instance != null)
        {
            DialogueUI.Instance.PlayChapter(chapter, () => OnChapterDone(chapter));
        }
        else
        {
            OnChapterDone(chapter);
        }
    }

    private void OnChapterDone(ChapterEntry chapter)
    {
        switch (chapter.onComplete)
        {
            case "openStarMap":
                EnterStarMap();
                break;
            case "returnToShip":
                EnterSpaceship();
                break;
            default:
                // 保持目前模式
                if (CurrentState == FlowState.Dialogue)
                    EnterSpaceship();
                break;
        }
    }

    // ──────────────── 存檔支援 ────────────────

    public List<string> GetPlayedChapterIds()
    {
        return new List<string>(_playedChapters);
    }

    public void RestorePlayedChapters(List<string> chapterIds)
    {
        if (chapterIds == null) return;
        foreach (var id in chapterIds)
            _playedChapters.Add(id);

        // 同步 _lastCompletedQuest
        if (QuestManager.Instance != null)
            _lastCompletedQuest = QuestManager.Instance.CompletedCount;
    }

    // ──────────────── Helpers ────────────────

    private static void SetCameraControlEnabled(bool enabled)
    {
        var cam = Camera.main;
        if (cam == null) return;
        var cc = cam.GetComponent<CameraController>();
        if (cc != null) cc.enabled = enabled;
    }
}
