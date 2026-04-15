using UnityEngine;
using System.Collections;

// ============================================================
// BinarySearchExhibit.cs — 二分搜索
// 排好的柱子逐步二分，高亮搜索範圍
// ============================================================

public class BinarySearchExhibit : ExhibitBase
{
    private SortVisualizer sortVis;
    private DragHandle handleTarget;
    private TextMesh statusLabel;
    private TextMesh infoLabel;
    private bool isRunning;

    public override void BuildExhibit()
    {
        exhibitName = "二分搜索 Binary Search";
        description = "前提：陣列已排序！\n\n每次比較中間值：\n  target < mid → 淘汰右半\n  target > mid → 淘汰左半\n  target = mid → 找到！\n\n🎮 遊戲應用：\n• 排行榜即時查詢玩家排名\n• 物品資料庫的快速搜尋\n• 動畫曲線的關鍵幀查找\n• O(log n)：百萬筆資料只需 20 次比較\n\n拖曳滑桿選目標值，按 E 執行";
        formula = "T(n) = T(n/2) + O(1) → O(log n)";
        challengeDescription = "成功找到目標值";

        var visGo = new GameObject("SearchVis");
        visGo.transform.SetParent(transform, false);
        visGo.transform.localPosition = Vector3.up * 0.5f;
        sortVis = visGo.AddComponent<SortVisualizer>();
        sortVis.count = 16;
        sortVis.Build(visGo.transform);

        // 排好
        System.Array.Sort(sortVis.Values);
        sortVis.UpdateVisuals();

        // 目標值滑桿
        handleTarget = CreateDragHandle(new Vector3(0, -1f, 0), new Color(1f, 0.85f, 0.2f), 0.15f);
        handleTarget.minBounds = new Vector3(-2.5f, -1f, 0);
        handleTarget.maxBounds = new Vector3(2.5f, -1f, 0);

        statusLabel = CreateLabel(new Vector3(0, -1.8f, 0), "拖曳選目標，按 E 搜索", 32, Color.white);
        infoLabel = CreateLabel(new Vector3(0, -2.5f, 0), "", 26, new Color(0.7f, 0.7f, 0.8f));

        CreateLabel(new Vector3(-2.8f, -1f, 0), "1", 22, new Color(0.5f, 0.5f, 0.6f));
        CreateLabel(new Vector3(2.8f, -1f, 0), "16", 22, new Color(0.5f, 0.5f, 0.6f));
    }

    public override void UpdateVisualization()
    {
        if (!isRunning)
        {
            float t = Mathf.InverseLerp(-2.5f, 2.5f, handleTarget.LocalPosition.x);
            int target = Mathf.RoundToInt(Mathf.Lerp(1, 16, t));
            infoLabel.text = $"目標值 = {target}    按 E 開始搜索（最多 log₂(16) = 4 步）";
        }
    }

    protected override void OnChallengeStart()
    {
        if (isRunning) return;
        isRunning = true;
        float t = Mathf.InverseLerp(-2.5f, 2.5f, handleTarget.LocalPosition.x);
        int target = Mathf.RoundToInt(Mathf.Lerp(1, 16, t));
        statusLabel.text = $"搜索 {target} 中...";
        statusLabel.color = new Color(1f, 0.85f, 0.3f);
        StartCoroutine(RunSearch(target));
    }

    private IEnumerator RunSearch(int target)
    {
        yield return sortVis.BinarySearchCoroutine(target);
        statusLabel.text = $"✓ 搜索完成！按 E 再試";
        statusLabel.color = new Color(0.3f, 1f, 0.5f);
        isRunning = false;
        challengeCompleted = true;
        ChallengeSystem.Instance?.MarkCompleted(exhibitName);
    }

    public override bool CheckChallengeComplete() => challengeCompleted;
}
