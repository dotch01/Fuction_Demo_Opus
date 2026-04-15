using System.Collections.Generic;
using UnityEngine;

// ============================================================
// ChallengeSystem.cs
// 全域挑戰管理：追蹤各展品完成狀態 + 計分
// ============================================================

public class ChallengeSystem : MonoBehaviour
{
    public static ChallengeSystem Instance { get; private set; }

    private readonly Dictionary<string, bool> completedExhibits = new Dictionary<string, bool>();
    private int totalExhibits = 0;
    private int completedCount = 0;

    public int TotalExhibits => totalExhibits;
    public int CompletedCount => completedCount;
    public float Progress => totalExhibits > 0 ? (float)completedCount / totalExhibits : 0f;

    public System.Action<string, int, int> onChallengeCompleted; // exhibitName, completed, total

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public void RegisterExhibit(string exhibitName)
    {
        if (!completedExhibits.ContainsKey(exhibitName))
        {
            completedExhibits[exhibitName] = false;
            totalExhibits++;
        }
    }

    public void MarkCompleted(string exhibitName)
    {
        if (completedExhibits.ContainsKey(exhibitName) && !completedExhibits[exhibitName])
        {
            completedExhibits[exhibitName] = true;
            completedCount++;
            onChallengeCompleted?.Invoke(exhibitName, completedCount, totalExhibits);
            Debug.Log($"[Challenge] ★ {exhibitName} 完成！({completedCount}/{totalExhibits})");
        }
    }

    public bool IsCompleted(string exhibitName)
    {
        return completedExhibits.TryGetValue(exhibitName, out bool val) && val;
    }
}
