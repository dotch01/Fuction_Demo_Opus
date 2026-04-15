using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// 任務管理器：追蹤 isQuestTarget 行星的掃描狀態，
/// 按 questOrder 線性鎖定（前一顆完成才解鎖下一顆）。
/// </summary>
public class QuestManager : MonoBehaviour
{
    public static QuestManager Instance { get; private set; }

    public event Action OnQuestUpdated;

    private readonly List<Star> _questStars = new();
    private Star _locatingTarget;

    public IReadOnlyList<Star> QuestStars => _questStars;
    public Star LocatingTarget => _locatingTarget;

    /// <summary>目前應完成的任務行星（第一顆尚未掃描的）。</summary>
    public Star CurrentQuestStar =>
        _questStars.FirstOrDefault(s => !s.IsScanned);

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public void Init()
    {
        if (PlanetFactory.Instance == null)
        {
            Debug.LogError("QuestManager.Init: PlanetFactory.Instance is null!");
            return;
        }

        CollectQuestStars(PlanetFactory.Instance.NormalStars);
        CollectQuestStars(PlanetFactory.Instance.FilterStars);

        // 按 questOrder 排序，確保線性順序
        _questStars.Sort((a, b) => a.Data.questOrder.CompareTo(b.Data.questOrder));

        Debug.Log($"QuestManager: {_questStars.Count} quest targets (sorted by questOrder)");
    }

    private void CollectQuestStars(IReadOnlyList<Star> stars)
    {
        foreach (var star in stars)
            if (star.Data != null && star.Data.isQuestTarget && star.Data.questOrder > 0)
                _questStars.Add(star);
    }

    /// <summary>該任務行星是否已解鎖（前一顆已完成或是第一顆）。</summary>
    public bool IsUnlocked(Star star)
    {
        int idx = _questStars.IndexOf(star);
        if (idx < 0) return false;
        if (idx == 0) return true;
        return _questStars[idx - 1].IsScanned;
    }

    public void NotifyScanComplete(Star star)
    {
        if (star.Data != null && star.Data.isQuestTarget)
        {
            if (_locatingTarget == star)
                StopLocating();
            OnQuestUpdated?.Invoke();
        }
    }

    public void StartLocating(Star target)
    {
        _locatingTarget = target;
    }

    public void StopLocating()
    {
        _locatingTarget = null;
    }

    public int CompletedCount => _questStars.Count(s => s.IsScanned);

    /// <summary>存檔載入後通知 UI 更新。</summary>
    public void NotifyLoadComplete()
    {
        OnQuestUpdated?.Invoke();
    }
}
